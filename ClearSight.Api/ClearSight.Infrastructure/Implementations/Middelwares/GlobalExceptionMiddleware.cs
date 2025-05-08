using ClearSight.Core.Dtos.ApiResponse;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Net;
using System.Text.Json;

namespace ClearSight.Infrastructure.Implementations.Middelwares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
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
            Log.Error(exception, "An unhandled exception occurred.");
            var response = ApiResponse<string>
                    .FailureResponse("An unexpected error occurred. Please try again later.",
                    HttpStatusCode.InternalServerError);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}