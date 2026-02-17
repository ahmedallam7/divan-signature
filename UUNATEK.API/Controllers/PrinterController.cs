using Microsoft.AspNetCore.Mvc;
using UUNATRK.Application.Models;
using UUNATRK.Application.Services.Printer;

namespace UUNATEK.API.Controllers;

[ApiController]
[Route("printer")]
public class PrinterController : ControllerBase
{
    private readonly PrinterService _printer;

    public PrinterController(PrinterService printer)
    {
        _printer = printer;
    }


    [HttpPost("print")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PrintResponse>> Print(
        IFormFile svg,
        [FromForm] PrintRequest req)
    {
        Console.WriteLine($"=== PRINT REQUEST ===");
        Console.WriteLine($"File: {svg.FileName}, Size: {svg.Length} bytes");
        Console.WriteLine($"Settings: X={req.XPosition}, Y={req.YPosition}, Scale={req.Scale}, Rotation={req.Rotation}");
        Console.WriteLine($"Flips: InvertX={req.InvertX}, InvertY={req.InvertY}");

        if (_printer.IsPrinting)
            return Conflict(new { Message = "Printer is busy." });

        using var stream = new MemoryStream();
        await svg.CopyToAsync(stream);
        stream.Position = 0;

        var gcode = SvgConverter.ConvertToGCode(stream, req);

        Console.WriteLine($"Generated {gcode.Count} G-code lines");
        if (gcode.Count > 0)
        {
            Console.WriteLine($"First command: {gcode[0]}");
            Console.WriteLine($"Last command: {gcode[^1]}");
        }
        else
        {
            Console.WriteLine("!!! WARNING: G-code list is EMPTY");
        }

        var result = await _printer.Print(gcode);

        return Ok(result);
    }



    [HttpPost("print/bulk")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PrintResponse>> BulkPrint(
        IFormFile svg,
        [FromForm] int copies,
        [FromForm] PrintRequest req)
    {
        if (_printer.IsPrinting)
            return Conflict(new { Message = "Printer is busy." });

        if (copies < 1 || copies > 100)
            return BadRequest(new { Message = "Copies must be between 1 and 100." });

        using var stream = new MemoryStream();
        await svg.CopyToAsync(stream);
        stream.Position = 0;

        var gcode = SvgConverter.ConvertToGCode(stream, req);
        var result = await _printer.BulkPrint(gcode, copies);

        return Ok(result);
    }



    [HttpGet("status")]
    public ActionResult<PrinterStatus> GetStatus()
    {
        return Ok(_printer.GetStatus());
    }
}
