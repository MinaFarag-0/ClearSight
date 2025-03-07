namespace ClearSight.Core.Dtos.ApiResponse
{
    public class ModelStateErrorResponse
    {
        public int StatusCode { get; set; } = 400;
        public List<string> Errors { get; set; }
    }
}
