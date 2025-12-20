using UUNATEK.API.Models;

namespace UUNATEK.API.Services
{
    public interface IHandWriteService
    {
        Task<MachineResponse> WriteSingle(WriteRequest request);
        Task<MachineResponse> WriteBatch(List<WriteRequest> requests);
    }
}
