using Microsoft.AspNetCore.Mvc;
using UUNATEK.API.Models;
using UUNATEK.API.Services;

namespace UUNATEK.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlotterController(IUunaTekPlotter handWriteService) : ControllerBase
    {
        private readonly IUunaTekPlotter _handWriteService = handWriteService;

        [HttpGet("login")]
        public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
        {
            var result = await _handWriteService.Login(username, password);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("write-single")]
        public async Task<IActionResult> WriteSingle([FromBody] WriteRequest request)
        {
            var result = await _handWriteService.WriteSingle(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("write-template")]
        public async Task<IActionResult> WriteTemplate([FromForm] IFormFile file)
        {
            var result = await _handWriteService.WriteTemplate(file);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("write-batch")]
        public async Task<IActionResult> WriteBatch([FromBody] List<WriteRequest> requests)
        {
            var result = await _handWriteService.WriteBatch(requests);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var result = await _handWriteService.Status();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
