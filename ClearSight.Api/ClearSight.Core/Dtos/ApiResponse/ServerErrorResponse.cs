namespace ClearSight.Core.Dtos.ApiResponse
{
    public class ServerErrorResponse
    {
        public int StatusCode { get; set; } = 500;
        public string err_message { get; set; }
    }
}
