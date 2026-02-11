# ASP.NET Core 整合測試

> 本文件從 [SKILL.md](../SKILL.md) 提煉，提供 WebApplicationFactory 與 TUnit 整合測試的完整範例與細節。

## WebApplicationFactory 與 TUnit 的整合

```csharp
public class WebApiIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddLogging();
                });
            });

        _client = _factory.CreateClient();
    }

    [Test]
    public async Task WeatherForecast_Get_應回傳正確格式的資料()
    {
        var response = await _client.GetAsync("/weatherforecast");

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotNull();
        await Assert.That(content.Length).IsGreaterThan(0);
    }

    [Test]
    [Property("Category", "Integration")]
    public async Task WeatherForecast_ResponseHeaders_應包含ContentType標頭()
    {
        var response = await _client.GetAsync("/weatherforecast");

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        
        var contentType = response.Content.Headers.ContentType?.MediaType;
        await Assert.That(contentType).IsEqualTo("application/json");
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
```

## 效能測試與負載測試

```csharp
[Test]
[Property("Category", "Performance")]
[Timeout(10000)]
public async Task WeatherForecast_ResponseTime_應在合理範圍內()
{
    var stopwatch = Stopwatch.StartNew();

    var response = await _client.GetAsync("/weatherforecast");
    stopwatch.Stop();

    await Assert.That(response.IsSuccessStatusCode).IsTrue();
    await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000);
}

[Test]
[Property("Category", "Load")]
[Timeout(30000)]
public async Task WeatherForecast_並行請求_應能正確處理()
{
    const int concurrentRequests = 50;
    var tasks = new List<Task<HttpResponseMessage>>();

    for (int i = 0; i < concurrentRequests; i++)
    {
        tasks.Add(_client.GetAsync("/weatherforecast"));
    }

    var responses = await Task.WhenAll(tasks);

    await Assert.That(responses.Length).IsEqualTo(concurrentRequests);
    await Assert.That(responses.All(r => r.IsSuccessStatusCode)).IsTrue();

    foreach (var response in responses)
    {
        response.Dispose();
    }
}
```
