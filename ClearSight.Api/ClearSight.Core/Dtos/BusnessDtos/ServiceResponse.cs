namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class ServiceResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T Response { get; set; }

        public static ServiceResponse<T> Success(T response)
        {
            return new ServiceResponse<T>
            {
                IsSuccess = true,
                Response = response
            };
        }
        public static ServiceResponse<T> Fail(T response)
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Response = response
            };
        }
    }
}
