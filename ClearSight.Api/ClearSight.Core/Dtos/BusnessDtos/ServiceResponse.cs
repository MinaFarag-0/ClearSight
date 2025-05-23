namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class ServiceResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T Response { get; set; }
    }
}
