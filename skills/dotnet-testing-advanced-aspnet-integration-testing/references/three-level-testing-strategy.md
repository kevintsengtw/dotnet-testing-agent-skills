# 三個層級的整合測試策略

## Level 1：簡單的 WebApi 專案

**特色**：沒有資料庫、Service 與 Repository 依賴，直接使用 `WebApplicationFactory<Program>` 進行測試。

```csharp
public class BasicApiControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BasicApiControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStatus_應回傳OK()
    {
        // Act
        var response = await _client.GetAsync("/api/status");

        // Assert
        response.Should().Be200Ok();
    }
}
```

## Level 2：相依 Service 的 WebApi 專案

**特色**：沒有資料庫，但有 Service 依賴，使用 NSubstitute 建立 Service stub，在測試中配置依賴注入。

```csharp
public class ServiceStubWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IExampleService _serviceStub;

    public ServiceStubWebApplicationFactory(IExampleService serviceStub)
    {
        _serviceStub = serviceStub;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IExampleService>();
            services.AddScoped(_ => _serviceStub);
        });
    }
}

public class ServiceDependentControllerTests
{
    [Fact]
    public async Task GetData_應回傳服務資料()
    {
        // Arrange
        var serviceStub = Substitute.For<IExampleService>();
        serviceStub.GetDataAsync().Returns("測試資料");

        var factory = new ServiceStubWebApplicationFactory(serviceStub);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data");

        // Assert
        response.Should().Be200Ok();
    }
}
```

## Level 3：完整的 WebApi 專案

**特色**：完整的 Solution 架構，包含真實的資料庫操作，使用 InMemory 或真實測試資料庫。

```csharp
public class FullDatabaseWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 移除原本的資料庫設定
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 加入記憶體資料庫
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // 建立資料庫並加入測試資料
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Database.EnsureCreated();
        });
    }
}
```
