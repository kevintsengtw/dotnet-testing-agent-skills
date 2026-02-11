# 異常處理器詳細實作

> 本文件從 SKILL.md `## 核心概念` 提取，包含 FluentValidation 異常處理器的完整實作與註冊順序說明。

## FluentValidation 異常處理器

```csharp
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

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false; // 讓下一個處理器處理
        }

        _logger.LogWarning(validationException, "驗證失敗: {Message}", validationException.Message);

        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = "輸入的資料包含驗證錯誤",
            Instance = httpContext.Request.Path
        };

        foreach (var error in validationException.Errors)
        {
            if (problemDetails.Errors.ContainsKey(error.PropertyName))
            {
                var errors = problemDetails.Errors[error.PropertyName].ToList();
                errors.Add(error.ErrorMessage);
                problemDetails.Errors[error.PropertyName] = errors.ToArray();
            }
            else
            {
                problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
            }
        }

        httpContext.Response.StatusCode = 400;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

## 註冊順序很重要

異常處理器按照註冊順序執行，特定處理器必須在全域處理器之前：

```csharp
// Program.cs
builder.Services.AddProblemDetails();

// 順序很重要！特定處理器先註冊
builder.Services.AddExceptionHandler<FluentValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Middleware
app.UseExceptionHandler();
```
