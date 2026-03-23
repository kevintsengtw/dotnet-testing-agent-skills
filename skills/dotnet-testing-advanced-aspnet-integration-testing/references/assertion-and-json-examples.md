# AwesomeAssertions.Web 與 JSON 操作範例

## HTTP 狀態碼斷言

```csharp
response.Should().Be200Ok();          // HTTP 200
response.Should().Be201Created();     // HTTP 201
response.Should().Be204NoContent();   // HTTP 204
response.Should().Be400BadRequest();  // HTTP 400
response.Should().Be404NotFound();    // HTTP 404
response.Should().Be500InternalServerError();  // HTTP 500
```

## Satisfy<T> 強型別驗證

```csharp
[Fact]
public async Task GetShipper_當貨運商存在_應回傳成功結果()
{
    // Arrange
    await CleanupDatabaseAsync();
    var shipperId = await SeedShipperAsync("順豐速運", "02-2345-6789");

    // Act
    var response = await Client.GetAsync($"/api/shippers/{shipperId}");

    // Assert
    response.Should().Be200Ok()
            .And
            .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
            {
                result.Status.Should().Be("Success");
                result.Data.Should().NotBeNull();
                result.Data!.ShipperId.Should().Be(shipperId);
                result.Data.CompanyName.Should().Be("順豐速運");
                result.Data.Phone.Should().Be("02-2345-6789");
            });
}
```

## 與傳統方式的比較

```csharp
// ❌ 傳統方式 - 冗長且容易出錯
response.IsSuccessStatusCode.Should().BeTrue();
var content = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<SuccessResultOutputModel<ShipperOutputModel>>(content,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
result.Should().NotBeNull();
result!.Status.Should().Be("Success");

// ✅ 使用 Satisfy<T> - 簡潔且直觀
response.Should().Be200Ok()
        .And
        .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
        {
            result.Status.Should().Be("Success");
            result.Data!.CompanyName.Should().Be("測試公司");
        });
```

## System.Net.Http.Json 簡化 JSON 操作

### PostAsJsonAsync 簡化 POST 請求

```csharp
// ❌ 傳統方式
var createParameter = new ShipperCreateParameter { CompanyName = "測試公司", Phone = "02-1234-5678" };
var jsonContent = JsonSerializer.Serialize(createParameter);
var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
var response = await client.PostAsync("/api/shippers", content);

// ✅ 現代化方式
var createParameter = new ShipperCreateParameter { CompanyName = "測試公司", Phone = "02-1234-5678" };
var response = await client.PostAsJsonAsync("/api/shippers", createParameter);
```

### ReadFromJsonAsync 簡化回應讀取

```csharp
// ❌ 傳統方式
var responseContent = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<SuccessResultOutputModel<ShipperOutputModel>>(responseContent,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

// ✅ 現代化方式
var result = await response.Content.ReadFromJsonAsync<SuccessResultOutputModel<ShipperOutputModel>>();
```
