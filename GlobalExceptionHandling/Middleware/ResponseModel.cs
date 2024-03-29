namespace GlobalExceptionHandling.Middleware;

public class ResponseModel
{
    public int ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public object? Data { get; set; }
}