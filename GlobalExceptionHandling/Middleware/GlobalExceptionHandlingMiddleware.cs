using System.Net;
using System.Text.Json;

namespace GlobalExceptionHandling.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        finally
        {
            if (!context.Response.HasStarted)
            {
                //Eğer bir exception oluşmazsa ve response başlatılmamışsa 
                context.Response.ContentType = "application/json";
                ResponseModel successModel = new ResponseModel()
                {
                    ResponseCode = (int)HttpStatusCode.OK,
                    ResponseMessage = "Request processed successfully.",
                    Data = null
                };
                
                var successResult = JsonSerializer.Serialize(successModel);
                await context.Response.WriteAsync(successResult);
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;
        ResponseModel exModel = new ResponseModel();

        switch (exception)
        {
            case ApplicationException ex:
                exModel.ResponseCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                exModel.ResponseMessage = "Application Exception Occured, please retry after sometime.";
                break;
            case FileNotFoundException ex:
                exModel.ResponseCode = (int)HttpStatusCode.NotFound;
                response.StatusCode = (int)HttpStatusCode.NotFound;
                exModel.ResponseMessage = "The requested resource is not found.";
                break;
            default:
                exModel.ResponseCode = (int)HttpStatusCode.InternalServerError;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                exModel.ResponseMessage = "Internal Server Error, Please retry after sometime";
                break;
        }

        var exResult = JsonSerializer.Serialize(exModel);
        await context.Response.WriteAsync(exResult);
    }
}