using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagementSystem.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
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
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;

            var response = new { message = exception.Message };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

