using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class MLModelService
    {
        private readonly HttpClient _httpClient;

        public MLModelService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Predict(IFormFile file)
        {
            return "Diabitics";
            using var form = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "file", file.FileName);
            try
            {
                var flaskApiUrl = "http://127.0.0.1:5000//predict";
                var response = await _httpClient.PostAsync(flaskApiUrl, form);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return jsonResponse;
                }
                else
                {
                    return "Error calling Flask API.";
                }
            }
            catch (Exception ex)
            {
                return $"Internal server error: {ex.Message}";
            }
        }
    }
}
