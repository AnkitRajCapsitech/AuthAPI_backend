namespace AuthAPI.DTOs
{
    public class ApiResponse<T>
    {

        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Result { get; set; }



        public ApiResponse() { }

        public ApiResponse(bool status, string message, T? result = default)
        {
            Status = status;
            Message = message;
            Result = result;
        }
    }

   

    
}
