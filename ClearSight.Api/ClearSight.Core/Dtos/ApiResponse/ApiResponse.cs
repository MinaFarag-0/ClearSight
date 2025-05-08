using System.Net;

namespace ClearSight.Core.Dtos.ApiResponse
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, HttpStatusCode statusCode = HttpStatusCode.OK, string? message = null)
        {
            return new()
            {
                Success = true,
                StatusCode = statusCode,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> FailureResponse(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new()
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
