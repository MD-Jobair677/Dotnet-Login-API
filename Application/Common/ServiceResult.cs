namespace BulkMail.Application.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int? ErrorStatusCode { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "Success")
            => new() { Success = true, Message = message, Data = data };

        public static ServiceResult<T> Ok(string message)
            => new() { Success = true, Message = message };

        public static ServiceResult<T> Fail(string message, int statusCode = 400)
            => new() { Success = false, Message = message, ErrorStatusCode = statusCode };
    }
}
