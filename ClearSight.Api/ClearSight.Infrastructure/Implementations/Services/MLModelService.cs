using ClearSight.Core.Dtos.BusnessDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class MLModelService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MLModelService> _logger;

        public MLModelService(HttpClient httpClient, IConfiguration configuration, ILogger<MLModelService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<MLModelDto> Predict(IFormFile file)
        {
            //return new MLModelDto
            //{
            //    Result = new MLModelResult
            //    {
            //        Prediction = "Diabetic Retinopathy",
            //        Confidence = 97,
            //    },
            //    IsSuccess = true,
            //    ArabicName = _configuration["DiseasesMSG:Diabetic Retinopathy:0"],
            //    DiseaseMsg = _configuration["DiseasesMSG:Diabetic Retinopathy:1"],
            //};
            using var form = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "image", file.FileName);
            try
            {
                var flaskApiUrl = _configuration["ModelURL"];
                var response = await _httpClient.PostAsync(flaskApiUrl, form);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<MLModelResult>();
                    jsonResponse.Confidence = Math.Round(jsonResponse.Confidence * 100, 2);
                    return new MLModelDto
                    {
                        Result = jsonResponse,
                        IsSuccess = true,
                        ArabicName = _configuration[$"DiseasesMSG:{jsonResponse.Prediction}:0"],
                        DiseaseMsg = _configuration[$"DiseasesMSG:{jsonResponse.Prediction}:1"],
                    };
                }
                else
                {
                    var jsonErrorResponse = await response.Content.ReadFromJsonAsync<MLModelError>();

                    return new MLModelDto
                    {
                        Result = new MLModelResult
                        {
                            Prediction = jsonErrorResponse.error
                        },
                        IsSuccess = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Flask API.");
                return new MLModelDto
                {
                    Result = null,
                    IsSuccess = false
                };
            }
        }


    }


}
