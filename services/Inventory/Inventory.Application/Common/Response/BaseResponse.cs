namespace Inventory.Application.Common.Models;

public class BaseResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
    
    public BaseResponse() { }

    public BaseResponse(T data, string message = "Success")
    {
        Success = true;
        Message = message;
        Data = data;
    }

    public BaseResponse(string message)
    {
        Success = false;
        Message = message;
    }
}