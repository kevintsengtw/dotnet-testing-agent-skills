using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace YourProject.Api.Handlers;

/// <summary>
/// FluentValidation 專用異常處理器
/// </summary>
public class FluentValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<FluentValidationExceptionHandler> _logger;

    public FluentValidationExceptionHandler(ILogger<FluentValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 嘗試處理 ValidationException
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 只處理 FluentValidation 的 ValidationException
        if (exception is not ValidationException validationException)
        {
            return false; // 讓下一個處理器處理
        }

        _logger.LogWarning(validationException, "驗證失敗: {Message}", validationException.Message);

        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "輸入的資料包含驗證錯誤，請檢查後重新提交。",
            Instance = httpContext.Request.Path
        };

        // 將驗證錯誤轉換為 ValidationProblemDetails 格式
        foreach (var error in validationException.Errors)
        {
            var propertyName = error.PropertyName;
            var errorMessage = error.ErrorMessage;

            if (problemDetails.Errors.ContainsKey(propertyName))
            {
                // 如果已存在該屬性的錯誤，則新增到陣列
                var existingErrors = problemDetails.Errors[propertyName].ToList();
                existingErrors.Add(errorMessage);
                problemDetails.Errors[propertyName] = existingErrors.ToArray();
            }
            else
            {
                problemDetails.Errors.Add(propertyName, new[] { errorMessage });
            }
        }

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }
}
