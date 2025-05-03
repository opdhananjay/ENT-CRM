using ENT.Helpers;
using Serilog;
using System.Text.Json;

namespace ENT.Midderlwares
{

    // 0 .this is register in progra.cs
    // when we throw from controller it will get catch over here so =>
    // it will return => internal server error with error message 
    public class GlobalExceptionMiddleware
    {
        public readonly RequestDelegate _next;
        public GlobalExceptionMiddleware(RequestDelegate next)
        {
                this._next = next;     
        }
       
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch(Exception e)
            {
                Log.Error(e, "Global Exception =>");
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = 500;
                var response = new Res<string>(
                    StatusCode:500,
                    Message:$"Internal Server Error - {e.Message.ToString().Trim()}",
                    Data:string.Empty
                );
                var json = JsonSerializer.Serialize(response);
                await httpContext.Response.WriteAsync(json);
            }
        }

    }
}
