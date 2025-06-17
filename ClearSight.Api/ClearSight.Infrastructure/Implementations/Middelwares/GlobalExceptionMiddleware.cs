using ClearSight.Core.Dtos.ApiResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ClearSight.Infrastructure.Implementations.Middelwares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(exception);

            LogException(exception);

            var response = CreateErrorResponse(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)response.StatusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private void LogException(Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred.");
        }

        private ApiResponse<string> CreateErrorResponse(Exception exception)
        {
            return ApiResponse<string>.FailureResponse(
                "An unexpected error occurred. Please try again later.",
                        HttpStatusCode.InternalServerError);
        }
    }
}
