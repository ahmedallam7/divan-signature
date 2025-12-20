using UUNATEK.API.Models;

namespace UUNATEK.API.Services
{
    public interface IUunaTekPlotter
    {
        Task<Response> Login(string username, string password);
        Task<Response> WriteTemplate(IFormFile json);
        Task<Response> WriteSingle(WriteRequest request);
        Task<Response> WriteBatch(List<WriteRequest> requests);
        Task<Response> Status();
    }
}
