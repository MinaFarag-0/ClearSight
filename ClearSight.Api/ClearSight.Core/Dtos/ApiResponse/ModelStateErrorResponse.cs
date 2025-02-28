namespace ClearSight.Core.Dtos.ApiResponse
{
    public class ModelStateErrorResponse
    {
        public int StatusCode { get; set; }
        public List<string> Errors { get; set; }
    }
}
