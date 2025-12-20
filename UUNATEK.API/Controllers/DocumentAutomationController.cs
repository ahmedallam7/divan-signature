using Microsoft.AspNetCore.Mvc;
using UUNATEK.API.Models;
using UUNATEK.API.Services;

namespace UUNATEK.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentAutomationController(IHandWriteService handWriteService) : ControllerBase
    {
        private readonly IHandWriteService _handWriteService = handWriteService;

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
    }
}
