using API.Errors;
using System.Net;
using System.Text.Json;

namespace API.MiddleWare
{
    public class ExceptionMiddleWare
    {
        public RequestDelegate Next { get; }
        public ILogger<ExceptionMiddleWare> Logger { get; }
        public IHostEnvironment Env { get; }
        public ExceptionMiddleWare(RequestDelegate next, ILogger<ExceptionMiddleWare> logger,IHostEnvironment env)
        {
            Next = next;
            Logger = logger;
            Env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await Next(context);
            }
            catch (Exception ex) 
            {
                Logger.LogError(ex,ex.Message);
                context.Response.ContentType= "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = Env.IsDevelopment()
                               ? new ApiExtensions(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                               : new ApiExtensions(context.Response.StatusCode, ex.Message, "Internal Server Error");
                var options = new JsonSerializerOptions { PropertyNamingPolicy= JsonNamingPolicy.CamelCase };   
                var json = JsonSerializer.Serialize(response, options); 
                await context.Response.WriteAsync(json);    
            }
        }
    }
}
