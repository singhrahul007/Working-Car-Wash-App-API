namespace CarWash.Api.Utilities
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public static ServiceResult<T> SuccessResult(T data, string message = null)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> FailureResult(string message, string errorCode = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public static ServiceResult SuccessResult(string message = null)
        {
            return new ServiceResult
            {
                Success = true,
                Message = message
            };
        }

        public static ServiceResult FailureResult(string message, string errorCode = null)
        {
            return new ServiceResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}