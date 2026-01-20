// =============================================================================
// ASP.NET Core 整合測試 - 自訂 WebApplicationFactory 範本
// =============================================================================
// 用途：建立測試專用的應用程式工廠，配置記憶體資料庫與測試服務
// 使用方式：繼承此類別或直接修改以符合專案需求
// =============================================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace YourProject.IntegrationTests.Infrastructure;

/// <summary>
/// 自訂 WebApplicationFactory，用於整合測試
/// </summary>
/// <typeparam name="TProgram">應用程式入口點類別</typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ========================================
            // 1. 移除原本的資料庫設定
            // ========================================
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 也可以使用 RemoveAll 一次移除所有相關服務
            // services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // ========================================
            // 2. 加入記憶體資料庫
            // ========================================
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDatabase");
            });

            // ========================================
            // 3. 替換外部服務為測試版本
            // ========================================
            // 郵件服務
            services.Replace(ServiceDescriptor.Scoped<IEmailService, TestEmailService>());
            
            // 外部 API 服務
            services.Replace(ServiceDescriptor.Scoped<IExternalApiService, MockExternalApiService>());
            
            // 檔案服務
            services.Replace(ServiceDescriptor.Scoped<IFileService, InMemoryFileService>());

            // ========================================
            // 4. 初始化資料庫
            // ========================================
            var serviceProvider = services.BuildServiceProvider();
            
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // 確保資料庫已建立
            context.Database.EnsureCreated();
            
            // 可選：加入種子資料
            // SeedTestData(context);
        });

        // ========================================
        // 5. 設定測試環境
        // ========================================
        builder.UseEnvironment("Testing");

        // ========================================
        // 6. 覆寫設定值
        // ========================================
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Logging:LogLevel:Default", "Warning"),
                new KeyValuePair<string, string?>("ConnectionStrings:TestDb", "InMemory"),
            });
        });
    }

    /// <summary>
    /// 加入測試種子資料
    /// </summary>
    private static void SeedTestData(AppDbContext context)
    {
        // 範例：加入測試用貨運商資料
        if (!context.Shippers.Any())
        {
            context.Shippers.AddRange(
                new Shipper
                {
                    CompanyName = "測試物流A",
                    Phone = "02-12345678",
                    CreatedAt = DateTime.UtcNow
                },
                new Shipper
                {
                    CompanyName = "測試物流B",
                    Phone = "02-87654321",
                    CreatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
        }
    }
}

// =============================================================================
// 測試用服務實作範例
// =============================================================================

/// <summary>
/// 測試用郵件服務 - 不實際發送郵件
/// </summary>
public class TestEmailService : IEmailService
{
    public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // 記錄郵件內容，但不實際發送
        SentEmails.Add((to, subject, body));
        return Task.CompletedTask;
    }
}

/// <summary>
/// 測試用外部 API 服務 - 返回預設資料
/// </summary>
public class MockExternalApiService : IExternalApiService
{
    public Task<string> GetDataAsync()
    {
        return Task.FromResult("Mock API Response");
    }
}

/// <summary>
/// 測試用檔案服務 - 使用記憶體儲存
/// </summary>
public class InMemoryFileService : IFileService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public Task SaveFileAsync(string path, byte[] content)
    {
        _files[path] = content;
        return Task.CompletedTask;
    }

    public Task<byte[]?> GetFileAsync(string path)
    {
        return Task.FromResult(_files.GetValueOrDefault(path));
    }

    public Task<bool> FileExistsAsync(string path)
    {
        return Task.FromResult(_files.ContainsKey(path));
    }
}
