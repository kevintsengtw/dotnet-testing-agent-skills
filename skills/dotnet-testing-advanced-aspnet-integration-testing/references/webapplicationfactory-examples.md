# WebApplicationFactory 使用範例

## 基本使用方式

```csharp
public class BasicIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_首頁_應回傳成功()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

## 自訂 WebApplicationFactory

```csharp
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 移除原本的資料庫設定
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // 加入記憶體資料庫
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // 替換外部服務為測試版本
            services.Replace(ServiceDescriptor.Scoped<IEmailService, TestEmailService>());
        });

        // 設定測試環境
        builder.UseEnvironment("Testing");
    }
}
```

## 測試基底類別

```csharp
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory();
        Client = Factory.CreateClient();
    }

    protected async Task<int> SeedShipperAsync(string companyName, string phone = "02-12345678")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var shipper = new Shipper
        {
            CompanyName = companyName,
            Phone = phone,
            CreatedAt = DateTime.UtcNow
        };

        context.Shippers.Add(shipper);
        await context.SaveChangesAsync();

        return shipper.ShipperId;
    }

    protected async Task CleanupDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Shippers.RemoveRange(context.Shippers);
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}
```
