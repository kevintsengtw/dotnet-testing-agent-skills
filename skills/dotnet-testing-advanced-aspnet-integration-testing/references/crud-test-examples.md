# CRUD 操作測試範例

> 本文件從 [SKILL.md](../SKILL.md) 提取，提供完整的 CRUD 操作整合測試程式碼範例。

## GET 請求測試

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
                result.Data!.ShipperId.Should().Be(shipperId);
                result.Data.CompanyName.Should().Be("順豐速運");
            });
}

[Fact]
public async Task GetShipper_當貨運商不存在_應回傳404NotFound()
{
    // Arrange
    var nonExistentShipperId = 9999;

    // Act
    var response = await Client.GetAsync($"/api/shippers/{nonExistentShipperId}");

    // Assert
    response.Should().Be404NotFound();
}
```

## POST 請求測試

```csharp
[Fact]
public async Task CreateShipper_輸入有效資料_應建立成功()
{
    // Arrange
    await CleanupDatabaseAsync();
    var createParameter = new ShipperCreateParameter
    {
        CompanyName = "黑貓宅急便",
        Phone = "02-1234-5678"
    };

    // Act
    var response = await Client.PostAsJsonAsync("/api/shippers", createParameter);

    // Assert
    response.Should().Be201Created()
            .And
            .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
            {
                result.Status.Should().Be("Success");
                result.Data!.ShipperId.Should().BeGreaterThan(0);
                result.Data.CompanyName.Should().Be("黑貓宅急便");
            });
}
```

## 驗證錯誤測試

```csharp
[Fact]
public async Task CreateShipper_當公司名稱為空_應回傳400BadRequest()
{
    // Arrange
    var createParameter = new ShipperCreateParameter
    {
        CompanyName = "",
        Phone = "02-1234-5678"
    };

    // Act
    var response = await Client.PostAsJsonAsync("/api/shippers", createParameter);

    // Assert
    response.Should().Be400BadRequest()
            .And
            .Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Status.Should().Be(400);
                problem.Errors.Should().ContainKey("CompanyName");
            });
}
```

## 集合資料測試

```csharp
[Fact]
public async Task GetAllShippers_應回傳所有貨運商()
{
    // Arrange
    await CleanupDatabaseAsync();
    await SeedShipperAsync("公司A", "02-1111-1111");
    await SeedShipperAsync("公司B", "02-2222-2222");

    // Act
    var response = await Client.GetAsync("/api/shippers");

    // Assert
    response.Should().Be200Ok()
            .And
            .Satisfy<SuccessResultOutputModel<List<ShipperOutputModel>>>(result =>
            {
                result.Data!.Count.Should().Be(2);
                result.Data.Should().Contain(s => s.CompanyName == "公司A");
                result.Data.Should().Contain(s => s.CompanyName == "公司B");
            });
}
```
