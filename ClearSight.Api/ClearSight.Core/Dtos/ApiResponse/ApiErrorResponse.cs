namespace ClearSight.Core.Dtos.ApiResponse
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; } = 400;
        public string err_message { get; set; } = "error happen";
    }
}
