# 測試範例與資料管理 - 完整範例

## 成功建立產品測試

```csharp
[Fact]
public async Task CreateProduct_使用有效資料_應成功建立產品()
{
    // Arrange
    var request = new ProductCreateRequest { Name = "新產品", Price = 299.99m };

    // Act
    var response = await HttpClient.PostAsJsonAsync("/products", request);

    // Assert
    response.Should().Be201Created()
        .And.Satisfy<ProductResponse>(product =>
        {
            product.Id.Should().NotBeEmpty();
            product.Name.Should().Be("新產品");
            product.Price.Should().Be(299.99m);
        });
}
```

## 驗證錯誤測試

```csharp
[Fact]
public async Task CreateProduct_當產品名稱為空_應回傳400BadRequest()
{
    // Arrange
    var invalidRequest = new ProductCreateRequest { Name = "", Price = 100.00m };

    // Act
    var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

    // Assert
    response.Should().Be400BadRequest()
        .And.Satisfy<ValidationProblemDetails>(problem =>
        {
            problem.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
            problem.Title.Should().Be("One or more validation errors occurred.");
            problem.Errors.Should().ContainKey("Name");
            problem.Errors["Name"].Should().Contain("產品名稱不能為空");
        });
}
```

## 資源不存在測試

```csharp
[Fact]
public async Task GetById_當產品不存在_應回傳404且包含ProblemDetails()
{
    // Arrange
    var nonExistentId = Guid.NewGuid();

    // Act
    var response = await HttpClient.GetAsync($"/Products/{nonExistentId}");

    // Assert
    response.Should().Be404NotFound()
        .And.Satisfy<ProblemDetails>(problem =>
        {
            problem.Type.Should().Be("https://httpstatuses.com/404");
            problem.Title.Should().Be("產品不存在");
            problem.Status.Should().Be(404);
        });
}
```

## 分頁查詢測試

```csharp
[Fact]
public async Task GetProducts_使用分頁參數_應回傳正確的分頁結果()
{
    // Arrange
    await TestHelpers.SeedProductsAsync(DatabaseManager, 15);

    // Act - 使用 Flurl 建構 QueryString
    var url = "/products"
        .SetQueryParam("pageSize", 5)
        .SetQueryParam("page", 2);

    var response = await HttpClient.GetAsync(url);

    // Assert
    response.Should().Be200Ok()
        .And.Satisfy<PagedResult<ProductResponse>>(result =>
        {
            result.Total.Should().Be(15);
            result.PageSize.Should().Be(5);
            result.Page.Should().Be(2);
            result.Items.Should().HaveCount(5);
        });
}
```

## 資料管理策略

### TestHelpers 設計

```csharp
public static class TestHelpers
{
    public static ProductCreateRequest CreateProductRequest(
        string name = "測試產品",
        decimal price = 100.00m)
    {
        return new ProductCreateRequest { Name = name, Price = price };
    }

    public static async Task SeedProductsAsync(DatabaseManager dbManager, int count)
    {
        var tasks = Enumerable.Range(1, count)
            .Select(i => SeedSpecificProductAsync(dbManager, $"產品 {i:D2}", i * 10.0m));
        await Task.WhenAll(tasks);
    }
}
```

### SQL 指令碼外部化

```text
tests/Integration/
└── SqlScripts/
    └── Tables/
        └── CreateProductsTable.sql
```
