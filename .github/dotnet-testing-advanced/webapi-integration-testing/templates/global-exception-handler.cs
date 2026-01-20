using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace YourProject.Api.Handlers;

/// <summary>
/// 全域異常處理器 - 處理所有未被特定處理器處理的異常
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 嘗試處理異常
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "發生未處理的異常: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }

    /// <summary>
    /// 根據異常類型建立對應的 ProblemDetails
    /// </summary>
    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "參數錯誤",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = exception.Message
            },
            KeyNotFoundException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "資源不存在",
                Status = (int)HttpStatusCode.NotFound,
                Detail = exception.Message
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/401",
                Title = "未授權",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "您沒有權限執行此操作"
            },
            TimeoutException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/408",
                Title = "請求超時",
                Status = (int)HttpStatusCode.RequestTimeout,
                Detail = "操作執行超時，請稍後再試"
            },
            InvalidOperationException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/422",
                Title = "操作無效",
                Status = (int)HttpStatusCode.UnprocessableEntity,
                Detail = exception.Message
            },
            _ => new ProblemDetails
            {
                Type = "https://httpstatuses.com/500",
                Title = "內部伺服器錯誤",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "發生未預期的錯誤，請聯絡系統管理員"
            }
        };
    }
}
