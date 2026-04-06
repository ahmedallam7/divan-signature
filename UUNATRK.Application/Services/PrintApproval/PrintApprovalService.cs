using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UUNATEK.Domain.Entities;
using UUNATEK.Domain.Enums;
using UUNATRK.Application.Data;
using UUNATRK.Application.Enums;
using UUNATRK.Application.Models;
using UUNATRK.Application.Repositories;
using UUNATRK.Application.Services.Approval;
using UUNATRK.Application.Services.Printer;

namespace UUNATRK.Application.Services.PrintApproval;

public class PrintApprovalService : IPrintApprovalService
{
    private readonly IRequestLogRepository _requestLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPrinterService _printerService;
    private readonly IApprovalService _approvalService;
    private readonly PrintRetrySettings _printRetrySettings;
    private readonly ILogger<PrintApprovalService> _logger;

    public PrintApprovalService(
        IRequestLogRepository requestLogRepository,
        IUnitOfWork unitOfWork,
        IPrinterService printerService,
        IApprovalService approvalService,
        IOptions<PrintRetrySettings> printRetrySettings,
        ILogger<PrintApprovalService> logger)
    {
        _requestLogRepository = requestLogRepository;
        _unitOfWork = unitOfWork;
        _printerService = printerService;
        _approvalService = approvalService;
        _printRetrySettings = printRetrySettings.Value;
        _logger = logger;
    }

    public async Task<PrintWithApprovalResponse> PrintWithApprovalAsync(PrintApprovalRequest request)
    {
        var requestId = Guid.NewGuid();
        RequestLog? log = null;

        try
        {
            _logger.LogInformation("Starting print with approval workflow. RequestId: {RequestId}", requestId);

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

            _logger.LogInformation("Request log created. RequestId: {RequestId}", requestId);

            var paperImageBytes = await ReadStreamToByteArrayAsync(request.PaperImageStream);
            _logger.LogInformation("Paper image read: {Size} bytes. RequestId: {RequestId}", paperImageBytes.Length, requestId);

            log.Status = RequestStatus.WaitingForApproval;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Requesting approval. RequestId: {RequestId}", requestId);
            var approvalResponse = await _approvalService.RequestApprovalAsync(paperImageBytes, requestId);
            
            bool shouldApprove = request.ShouldApprove;
            if (!approvalResponse.IsApproved)
                shouldApprove = false;

            log.ApprovalResponse = approvalResponse.Message;
            log.Status = shouldApprove ? RequestStatus.Approved : RequestStatus.Rejected;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Approval decision: {Decision}. RequestId: {RequestId}", shouldApprove ? "Approved" : "Rejected", requestId);

            if (!shouldApprove)
            {
                return await ExecuteRejectedPrintAsync(log, requestId);
            }

            return await ExecuteApprovedPrintAsync(log, request, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in print with approval workflow. RequestId: {RequestId}", requestId);

            if (log != null)
            {
                log.Status = RequestStatus.Failed;
                log.ErrorMessage = ex.Message;
                log.UpdatedAt = DateTime.UtcNow;
                _requestLogRepository.Update(log);
                await _unitOfWork.SaveChangesAsync();
            }
            
            throw;
        }
    }

    public async Task<RequestLog?> GetRequestLogAsync(Guid requestId)
    {
        return await _requestLogRepository.GetByRequestIdAsync(requestId);
    }

    public async Task<List<RequestLog>> GetRecentRequestsAsync(int count = 10)
    {
        return await _requestLogRepository.GetRecentAsync(count);
    }

    private async Task<PrintWithApprovalResponse> ExecuteRejectedPrintAsync(RequestLog log, Guid requestId)
    {
        _logger.LogInformation("Executing void print (rejected). RequestId: {RequestId}", requestId);

        log.Status = RequestStatus.Voided;
        log.UpdatedAt = DateTime.UtcNow;
        _requestLogRepository.Update(log);
        await _unitOfWork.SaveChangesAsync();

        var voidResult = await _printerService.VoidPrint();
        
        log.Status = RequestStatus.Completed;
        log.CompletedAt = DateTime.UtcNow;
        log.UpdatedAt = DateTime.UtcNow;
        _requestLogRepository.Update(log);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Void print completed. RequestId: {RequestId}", requestId);

        return new PrintWithApprovalResponse
        {
            RequestId = requestId,
            Status = "Completed",
            WasApproved = false,
            WasPrinted = false,
            Message = "Request rejected - void print executed (paper ejected without printing).",
            CommandsSent = 0
        };
    }

    private async Task<PrintWithApprovalResponse> ExecuteApprovedPrintAsync(
        RequestLog log, 
        PrintApprovalRequest request, 
        Guid requestId)
    {
        _logger.LogInformation("Executing approved print. RequestId: {RequestId}", requestId);

        var gcode = ConvertSvgToGCode(request.SignatureSvgStream, request.PrintSettings);

        if (gcode == null || gcode.Count == 0)
        {
            var errorMessage = "Failed to convert SVG to G-code. No drawable paths found.";
            _logger.LogError(errorMessage + " RequestId: {RequestId}", requestId);

            log.Status = RequestStatus.Failed;
            log.ErrorMessage = errorMessage;
            log.UpdatedAt = DateTime.UtcNow;
            _requestLogRepository.Update(log);
            await _unitOfWork.SaveChangesAsync();

            throw new InvalidOperationException(errorMessage);
        }

        _logger.LogInformation("G-code generated: {Count} commands. RequestId: {RequestId}", gcode.Count, requestId);

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
            _logger.LogInformation("Print attempt {Attempt} of {MaxRetries}. RequestId: {RequestId}", 
                attempt, _printRetrySettings.MaxRetries, requestId);

            try
            {
                printResult = await _printerService.Print(gcode);
                _logger.LogInformation("Print successful on attempt {Attempt}. RequestId: {RequestId}", attempt, requestId);
                break;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Print attempt {Attempt} failed. RequestId: {RequestId}", attempt, requestId);
                
                if (attempt < _printRetrySettings.MaxRetries)
                {
                    _logger.LogInformation("Retrying in {Delay}ms. RequestId: {RequestId}", 
                        _printRetrySettings.RetryDelayMs, requestId);
                    await Task.Delay(_printRetrySettings.RetryDelayMs);
                }
            }
        }

        if (printResult == null)
        {
            var errorMessage = $"Print failed after {attempt} attempts: {lastException?.Message}";
            _logger.LogError(errorMessage + " RequestId: {RequestId}", requestId);

            log.Status = RequestStatus.Failed;
            log.ErrorMessage = errorMessage;
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

        _logger.LogInformation("Print completed successfully. RequestId: {RequestId}", requestId);

        return new PrintWithApprovalResponse
        {
            RequestId = requestId,
            Status = "Completed",
            WasApproved = true,
            WasPrinted = true,
            Message = "Print completed successfully.",
            CommandsSent = printResult.CommandsSent
        };
    }

    private async Task<byte[]> ReadStreamToByteArrayAsync(Stream? stream)
    {
        if (stream == null)
        {
            return Array.Empty<byte>();
        }

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private List<string>? ConvertSvgToGCode(Stream svgStream, PrintRequest printSettings)
    {
        if (printSettings.Scale < 1)
        {
            throw new ArgumentException("Scale must be at least 1.");
        }

        if (printSettings.Rotation < 0 || printSettings.Rotation > 360)
        {
            throw new ArgumentException("Rotation must be between 0 and 360.");
        }

        if (printSettings.Paper.HasValue)
        {
            var (pw, ph) = PaperSizes.GetSizeMm(printSettings.Paper.Value);
            printSettings.Width = $"{pw}mm";
            printSettings.Height = $"{ph}mm";
        }

        var gcode = SvgConverter.ConvertToGCode(svgStream, printSettings);

        return gcode.Count == 0 ? null : gcode;
    }
}
