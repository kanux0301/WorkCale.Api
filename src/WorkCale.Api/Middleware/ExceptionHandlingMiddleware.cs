using System.Net;
using System.Text.Json;
using FluentValidation;

namespace WorkCale.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, type, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "ValidationError",
                "Validation failed.",
                validationEx.Errors.Select(e => e.ErrorMessage).ToArray()),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                exception.Message,
                Array.Empty<string>()),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "NotFound",
                exception.Message,
                Array.Empty<string>()),

            InvalidOperationException => (
                HttpStatusCode.Conflict,
                "Conflict",
                exception.Message,
                Array.Empty<string>()),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                "BadRequest",
                exception.Message,
                Array.Empty<string>()),

            _ => (
                HttpStatusCode.InternalServerError,
                "ServerError",
                env.IsDevelopment()
                    ? $"{exception.GetType().Name}: {exception.Message}"
                    : "An unexpected error occurred.",
                Array.Empty<string>())
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception: {Type} - {Message}",
                exception.GetType().Name, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            type,
            message,
            errors = errors.Length > 0 ? errors : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
