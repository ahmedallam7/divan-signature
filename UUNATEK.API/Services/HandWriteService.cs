using System.Net.Http.Headers;
using UUNATEK.API.Helpers;
using UUNATEK.API.Models;

namespace UUNATEK.API.Services
{
    public class HandWriteService(IHttpClientFactory httpClientFactory) : IHandWriteService
    {
        private readonly HttpClient _httpClientFactory = httpClientFactory.CreateClient("IAuto");

        public async Task<MachineResponse> WriteSingle(WriteRequest request)
        {
            try
            {
                if (!File.Exists(request.FilePath))
                {
                    return ErrorHandler.FileNotExist();
                }

                using var content = new MultipartFormDataContent();
                var fileBytes = await File.ReadAllBytesAsync(request.FilePath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/svg+xml");
                content.Add(fileContent, "file", Path.GetFileName(request.FilePath));

                content.Add(new StringContent(request.Width), "width");
                content.Add(new StringContent(request.Height), "height");
                content.Add(new StringContent(request.XPosition), "xpos");
                content.Add(new StringContent(request.YPosition), "ypos");
                content.Add(new StringContent(request.Scale.ToString()), "scale");
                content.Add(new StringContent(request.Rotation.ToString()), "rotation");
                content.Add(new StringContent(request.Clear.ToString()), "clear");
                content.Add(new StringContent(request.Start.ToString()), "start");

                var response = await _httpClientFactory.PostAsync(string.Empty, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<MachineResponse>(responseContent);

                if (result?.Error == 0)
                {
                    return MachineResponse.CreateSuccess();
                }
                else
                {
                    return ErrorHandler.GetErrorResponse(result.Error);
                }
            }
            catch (Exception ex)
            {
                return MachineResponse.CreateFailure(ex.Message, -1);
            }
            
        }

        public async Task<MachineResponse> WriteBatch(List<WriteRequest> requests)
        {
            List<MachineResponse> results = [];

            foreach (var request in requests)
            {
                var result = await WriteSingle(request);

                results.Add(result);

                await Task.Delay(1000);
            }

            return MachineResponse.CreateSuccess();
        }
    }
}
