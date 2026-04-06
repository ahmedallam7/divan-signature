using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using UUNATEK.Domain.Entities;
using UUNATEK.Domain.Enums;
using UUNATRK.Application.Data;
using UUNATRK.Application.Enums;
using UUNATRK.Application.Models;
using UUNATRK.Application.Repositories;
using UUNATRK.Application.Services.Approval;
using UUNATRK.Application.Services.Printer;

namespace UUNATEK.API.Controllers;

[ApiController]
[Route("printer")]
public class PrinterController : ControllerBase
{
    private readonly PrinterService _printer;
    private readonly IRequestLogRepository _requestLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApprovalService _approvalService;
    private readonly FileStorageSettings _fileStorageSettings;
    private readonly PrintRetrySettings _printRetrySettings;

    public PrinterController(
        PrinterService printer,
        IRequestLogRepository requestLogRepository,
        IUnitOfWork unitOfWork,
        IApprovalService approvalService,
        IOptions<FileStorageSettings> fileStorageSettings,
        IOptions<PrintRetrySettings> printRetrySettings)
    {
        _printer = printer;
        _requestLogRepository = requestLogRepository;
        _unitOfWork = unitOfWork;
        _approvalService = approvalService;
        _fileStorageSettings = fileStorageSettings.Value;
        _printRetrySettings = printRetrySettings.Value;
    }


    [HttpPost("connect")]
    public ActionResult Connect(
        [FromQuery] string? comPort = null,
        [FromQuery] int? baudRate = null)
    {
        if (_printer.IsOpen)
            return Conflict(new { Message = $"Already connected to {_printer.PortName}. Disconnect first." });

        var port = comPort ?? _printer.DefaultComPort;
        var baud = baudRate ?? _printer.DefaultBaudRate;

        try
        {
            _printer.OpenPort(port, baud);
            return Ok(new { Message = $"Connected to {port} at {baud} baud." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = $"Failed to connect: {ex.Message}" });
        }
    }

    [HttpPost("disconnect")]
    public ActionResult Disconnect()
    {
        if (!_printer.IsOpen)
            return Conflict(new { Message = "Not connected." });

        if (_printer.IsPrinting)
            return Conflict(new { Message = "Cannot disconnect while printing." });

        _printer.ClosePort();
        return Ok(new { Message = "Disconnected." });
    }



    [HttpGet("status")]
    public ActionResult<PrinterStatus> GetStatus()
    {
        return Ok(_printer.GetStatus());
    }



    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> Generate(
        IFormFile svg,
        [FromForm] PrintRequest req)
    {
        var (gcode, error) = await ConvertSvg(svg, req);

        if (error != null)
            return error;

        return Ok(new
        {
            Message = $"Generated {gcode!.Count} G-code commands.",
            CommandCount = gcode.Count,
            GCode = gcode
        });
    }



    [HttpPost("print")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PrintResponse>> Print(
        IFormFile svg,
        [FromForm] PrintRequest req)
    {
        if (!_printer.IsOpen)
            return BadRequest(new { Message = "Printer is not connected. Call POST /printer/connect first." });

        if (_printer.IsPrinting)
            return Conflict(new { Message = "Printer is busy." });

        var (gcode, error) = await ConvertSvg(svg, req);

        if (error != null)
            return error;

        var result = await _printer.Print(gcode!);
        return Ok(result);
    }

    [HttpPost("print/bulk")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PrintResponse>> BulkPrint(
        IFormFile svg,
        [FromForm] int copies,
        [FromForm] PrintRequest req)
    {
        if (!_printer.IsOpen)
            return BadRequest(new { Message = "Printer is not connected. Call POST /printer/connect first." });

        if (_printer.IsPrinting)
            return Conflict(new { Message = "Printer is busy." });

        if (copies < 1 || copies > 100)
            return BadRequest(new { Message = "Copies must be between 1 and 100." });

        var (gcode, error) = await ConvertSvg(svg, req);

        if (error != null)
            return error;

        var result = await _printer.BulkPrint(gcode!, copies);
        return Ok(result);
    }

    [HttpPost("print-with-approval")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PrintWithApprovalResponse>> PrintWithApproval(
        IFormFile? paperImage,
        string? paperImageBase64,
        IFormFile signatureSvg,
        [FromForm] string printRequestJson)
    {
        var requestId = Guid.NewGuid();
        RequestLog? log = null;

        try
        {
            if (!_printer.IsOpen)
                return BadRequest(new { Message = "Printer is not connected. Call POST /printer/connect first." });

            if (_printer.IsPrinting)
                return Conflict(new { Message = "Printer is busy." });

            var printWithApprovalReq = JsonSerializer.Deserialize<PrintWithApprovalRequest>(printRequestJson);
            if (printWithApprovalReq == null)
                return BadRequest(new { Message = "Invalid printRequest JSON." });

            log = new RequestLog
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                Status = RequestStatus.New,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _requestLogRepository.Add(log);
            await _unitOfWork.SaveChangesAsync();

            string? imagePath = null;
            if (paperImage != null && paperImage.Length > 0)
            {
                imagePath = await SaveFile(paperImage, requestId, "images", "jpg");
            }
            else if (!string.IsNullOrEmpty(paperImageBase64))
            {
                imagePath = await SaveBase64File(paperImageBase64, requestId, "images", "jpg");
            }

            if (signatureSvg == null || signatureSvg.Length == 0)
                return BadRequest(new { Message = "No signature SVG file provided." });

            var svgPath = await SaveFile(signatureSvg, requestId, "svgs", "svg");

            log.PaperImagePath = imagePath;
            log.SignatureSvgPath = svgPath;
            log.Status = RequestStatus.WaitingForApproval;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            var approvalResponse = await _approvalService.RequestApprovalAsync(imagePath ?? "N/A", requestId);
            
            bool shouldApprove = printWithApprovalReq.ShouldApprove;
            if (!approvalResponse.IsApproved)
                shouldApprove = false;

            log.ApprovalResponse = approvalResponse.Message;
            log.Status = shouldApprove ? RequestStatus.Approved : RequestStatus.Rejected;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            if (!shouldApprove)
            {
                log.Status = RequestStatus.Voided;
                log.UpdatedAt = DateTime.UtcNow;
                _requestLogRepository.Update(log);
                await _unitOfWork.SaveChangesAsync();

                var voidResult = await _printer.VoidPrint();
                
                log.Status = RequestStatus.Completed;
                log.CompletedAt = DateTime.UtcNow;
                log.UpdatedAt = DateTime.UtcNow;
                _requestLogRepository.Update(log);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new PrintWithApprovalResponse
                {
                    RequestId = requestId,
                    Status = "Completed",
                    WasApproved = false,
                    WasPrinted = false,
                    Message = "Request rejected - void print executed (paper ejected without printing).",
                    CommandsSent = 0
                });
            }

            using var svgStream = System.IO.File.OpenRead(svgPath);
            var (gcode, error) = await ConvertSvgStream(svgStream, printWithApprovalReq.PrintSettings);
            
            if (error != null)
            {
                log.Status = RequestStatus.Failed;
                log.ErrorMessage = "Failed to convert SVG to G-code.";
                log.UpdatedAt = DateTime.UtcNow;
                _requestLogRepository.Update(log);
                await _unitOfWork.SaveChangesAsync();
                return error;
            }

            log.Status = RequestStatus.Printing;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            PrintResponse? printResult = null;
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < _printRetrySettings.MaxRetries)
            {
                attempt++;
                try
                {
                    printResult = await _printer.Print(gcode!);
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attempt < _printRetrySettings.MaxRetries)
                    {
                        await Task.Delay(_printRetrySettings.RetryDelayMs);
                    }
                }
            }

            if (printResult == null)
            {
                log.Status = RequestStatus.Failed;
                log.ErrorMessage = $"Print failed after {attempt} attempts: {lastException?.Message}";
                log.UpdatedAt = DateTime.UtcNow;
                _requestLogRepository.Update(log);
                await _unitOfWork.SaveChangesAsync();
                throw lastException!;
            }

            log.Status = RequestStatus.Printed;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            log.Status = RequestStatus.Completed;
            log.CompletedAt = DateTime.UtcNow;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new PrintWithApprovalResponse
            {
                RequestId = requestId,
                Status = "Completed",
                WasApproved = true,
                WasPrinted = true,
                Message = "Print completed successfully.",
                CommandsSent = printResult.CommandsSent
            });
        }
        catch (Exception ex)
        {
            if (log != null)
            {
                log.Status = RequestStatus.Failed;
                log.ErrorMessage = ex.Message;
                log.UpdatedAt = DateTime.UtcNow;
                _requestLogRepository.Update(log);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return StatusCode(500, new { Message = $"Error: {ex.Message}" });
        }
    }

    [HttpGet("requests/{requestId}")]
    public async Task<ActionResult<RequestLog>> GetRequestLog(Guid requestId)
    {
        var log = await _requestLogRepository.GetByRequestIdAsync(requestId);
        if (log == null)
            return NotFound(new { Message = $"Request log with ID {requestId} not found." });

        return Ok(log);
    }


    private async Task<(List<string>? gcode, ActionResult? error)> ConvertSvg(
        IFormFile svg, PrintRequest req)
    {
        if (svg == null || svg.Length == 0)
            return (null, BadRequest(new { Message = "No SVG file provided." }));

        if (req.Scale < 1)
            return (null, BadRequest(new { Message = "Scale must be at least 1." }));

        if (req.Rotation < 0 || req.Rotation > 360)
            return (null, BadRequest(new { Message = "Rotation must be between 0 and 360." }));

        if (req.Paper.HasValue)
        {
            var (pw, ph) = PaperSizes.GetSizeMm(req.Paper.Value);
            req.Width = $"{pw}mm";
            req.Height = $"{ph}mm";
        }

        using var stream = new MemoryStream();
        await svg.CopyToAsync(stream);
        stream.Position = 0;

        var gcode = SvgConverter.ConvertToGCode(stream, req);

        if (gcode.Count == 0)
            return (null, BadRequest(new { Message = "No drawable paths found. If SVG contains text, convert to path first (Inkscape: Path > Object to Path)." }));

        return (gcode, null);
    }

    private async Task<(List<string>? gcode, ActionResult? error)> ConvertSvgStream(
        Stream svgStream, PrintRequest req)
    {
        if (req.Scale < 1)
            return (null, BadRequest(new { Message = "Scale must be at least 1." }));

        if (req.Rotation < 0 || req.Rotation > 360)
            return (null, BadRequest(new { Message = "Rotation must be between 0 and 360." }));

        if (req.Paper.HasValue)
        {
            var (pw, ph) = PaperSizes.GetSizeMm(req.Paper.Value);
            req.Width = $"{pw}mm";
            req.Height = $"{ph}mm";
        }

        var gcode = SvgConverter.ConvertToGCode(svgStream, req);

        if (gcode.Count == 0)
            return (null, BadRequest(new { Message = "No drawable paths found. If SVG contains text, convert to path first (Inkscape: Path > Object to Path)." }));

        return (gcode, null);
    }

    private async Task<string> SaveFile(IFormFile file, Guid requestId, string subfolder, string extension)
    {
        var uploadDir = Path.Combine(_fileStorageSettings.UploadPath, subfolder);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{requestId}.{extension}";
        var filePath = Path.Combine(uploadDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }

    private async Task<string> SaveBase64File(string base64String, Guid requestId, string subfolder, string extension)
    {
        var uploadDir = Path.Combine(_fileStorageSettings.UploadPath, subfolder);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{requestId}.{extension}";
        var filePath = Path.Combine(uploadDir, fileName);

        var base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;
        var bytes = Convert.FromBase64String(base64Data);

        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

        return filePath;
    }
}
