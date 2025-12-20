using System.Net.Http.Headers;
using System.Text.Json;
using UUNATEK.API.Helpers;
using UUNATEK.API.Models;

namespace UUNATEK.API.Services
{
    public class UunaTekPlotter(IHttpClientFactory httpClientFactory) : IUunaTekPlotter
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IAuto");

        public async Task<Response> Login(string username, string password)
        {
            try
            {
                var result = await SendRequest(HttpMethod.Get, $"login?uin={username}&passwd={password}");
                return result;
            }
            catch (Exception ex)
            {
                return Response.CreateFailure(ex.Message, -1);
            }
        }

        public async Task<Response> Status()
        {
            try
            {
                var result = await SendRequest(HttpMethod.Get, $"machine_status");
                return result;
            }
            catch (Exception ex)
            {
                return Response.CreateFailure(ex.Message, -1);
            }
        }

        public async Task<Response> WriteSingle(WriteRequest request)
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


                var result = await SendRequest(HttpMethod.Post, "write_svg", content);

                return result;
            }
            catch (Exception ex)
            {
                return Response.CreateFailure(ex.Message, -1);
            }
            
        }

        public async Task<Response> WriteBatch(List<WriteRequest> requests)
        {
            List<Response> results = [];

            foreach (var request in requests)
            {
                var result = await WriteSingle(request);

                results.Add(result);

                await Task.Delay(1000);
            }

            return Response.CreateSuccess();
        }

        public async Task<Response> WriteTemplate(IFormFile json)
        {
            try
            {
                if (json == null || json.Length == 0)
                {
                    return Response.CreateFailure("No file provided", -1);
                }

                if (!json.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
                    && !json.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    return Response.CreateFailure("File must be JSON", -1);
                }

                using var content = new MultipartFormDataContent();

                using var stream = json.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                content.Add(fileContent, "file", json.FileName);

                var result = await SendRequest(HttpMethod.Post, "write_template", content);

                return result;
            }
            catch (Exception ex)
            {
                return Response.CreateFailure(ex.Message, -1);
            }
        }

        private async Task<Response> SendRequest(HttpMethod http, string endpoint, MultipartFormDataContent? content = null,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = new(http, endpoint);

            if (http is not HttpMethod { Method: "GET" })
            {
                request.Content = JsonContent.Create(content);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);


            if (!response.IsSuccessStatusCode)
            {
                return ErrorHandler.FailedToGenerate();
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<MachineReposne>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Error == 0)
            {
                return Response.CreateSuccess();
            }
            else
            {
                return ErrorHandler.GetErrorResponse(result.Error);
            }
        }
    }
}
