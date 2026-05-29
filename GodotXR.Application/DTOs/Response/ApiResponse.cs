namespace GodotXR.Application.DTOs.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public List<string>? Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = "Success")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        public ApiResponse(string message, List<string>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
            Data = default;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>(message, errors);
        }
    }


    public class ApiResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<string>? Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(string message = "Success")
        {
            Success = true;
            Message = message;
        }

        public ApiResponse(string message, List<string>? errors)
        {
            Success = false;
            Message = message;
            Errors = errors;
        }

        public static ApiResponse SuccessResponse(string message = "Success")
        {
            return new ApiResponse(message);
        }

        public static ApiResponse FailureResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse(message, errors);
        }
    }
}
