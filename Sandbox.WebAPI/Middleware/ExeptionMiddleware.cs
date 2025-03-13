using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Sandbox.WebAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string message;

            switch (exception)
            {
                case ApplicationException appEx: 
                    statusCode = HttpStatusCode.NotFound; 
                    message = appEx.Message;
                    break;

                default: 
                    statusCode = HttpStatusCode.InternalServerError; 
                    message = exception.Message;
                    break;
            }

            var response = new { message };
            var json = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(json);
        }
    }
}
