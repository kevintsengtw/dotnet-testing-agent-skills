using AwesomeAssertions;
using Flurl;

namespace MyApp.Tests.Integration.Controllers;

/// <summary>
/// ProductsController 整合測試 - 使用 Aspire Testing
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(AspireAppFixture fixture) : base(fixture)
    {
    }

    #region GET /products 測試

    [Fact]
    public async Task GetProducts_當沒有產品時_應回傳空的分頁結果()
    {
        // Arrange
        // 資料庫應為空（每個測試執行前會清理）

        // Act
        var response = await HttpClient.GetAsync("/products");

        // Assert
        response.Should().Be200Ok()
                .And.Satisfy<PagedResult<ProductResponse>>(result =>
                {
                    result.Total.Should().Be(0);
                    result.PageSize.Should().Be(10);
                    result.Page.Should().Be(1);
                    result.Items.Should().BeEmpty();
                });
    }

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
                    result.PageCount.Should().Be(3);
                    result.Items.Should().HaveCount(5);
                });
    }

    [Fact]
    public async Task GetProducts_使用搜尋參數_應回傳符合條件的產品()
    {
        // Arrange
        await TestHelpers.SeedProductsAsync(DatabaseManager, 5);
        await TestHelpers.SeedSpecificProductAsync(DatabaseManager, "特殊產品", 199.99m);

        // Act - 使用 Flurl 建構 QueryString
        var url = "/products"
                  .SetQueryParam("keyword", "特殊");

        var response = await HttpClient.GetAsync(url);

        // Assert
        response.Should().Be200Ok()
                .And.Satisfy<PagedResult<ProductResponse>>(result =>
                {
                    result.Total.Should().Be(1);
                    result.Items.Should().HaveCount(1);
                    result.Items.First().Name.Should().Be("特殊產品");
                    result.Items.First().Price.Should().Be(199.99m);
                });
    }

    #endregion

    #region POST /products 測試

    [Fact]
    public async Task CreateProduct_有效請求_應回傳201與產品資訊()
    {
        // Arrange
        var request = new ProductCreateRequest
        {
            Name = "新產品",
            Price = 299.99m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", request);

        // Assert
        response.Should().Be201Created()
                .And.Satisfy<ProductResponse>(product =>
                {
                    product.Id.Should().NotBeEmpty();
                    product.Name.Should().Be("新產品");
                    product.Price.Should().Be(299.99m);
                    product.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
                });
    }

    [Fact]
    public async Task CreateProduct_無效請求_應回傳400與驗證錯誤()
    {
        // Arrange
        var request = new ProductCreateRequest
        {
            Name = "", // 名稱不可為空
            Price = -100m // 價格不可為負
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", request);

        // Assert
        response.Should().Be400BadRequest()
                .And.Satisfy<ValidationProblemDetails>(problem =>
                {
                    problem.Errors.Should().ContainKey("Name");
                    problem.Errors.Should().ContainKey("Price");
                });
    }

    #endregion

    #region GET /products/{id} 測試

    [Fact]
    public async Task GetProductById_存在的產品_應回傳200與產品資訊()
    {
        // Arrange
        var productId = await TestHelpers.SeedSpecificProductAsync(
            DatabaseManager, "測試產品", 199.99m);

        // Act
        var response = await HttpClient.GetAsync($"/products/{productId}");

        // Assert
        response.Should().Be200Ok()
                .And.Satisfy<ProductResponse>(product =>
                {
                    product.Id.Should().Be(productId);
                    product.Name.Should().Be("測試產品");
                    product.Price.Should().Be(199.99m);
                });
    }

    [Fact]
    public async Task GetProductById_不存在的產品_應回傳404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/products/{nonExistentId}");

        // Assert
        response.Should().Be404NotFound();
    }

    #endregion

    #region PUT /products/{id} 測試

    [Fact]
    public async Task UpdateProduct_存在的產品_應回傳200與更新後的產品()
    {
        // Arrange
        var productId = await TestHelpers.SeedSpecificProductAsync(
            DatabaseManager, "原始產品", 100m);

        var updateRequest = new ProductUpdateRequest
        {
            Name = "更新後產品",
            Price = 150m
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/products/{productId}", updateRequest);

        // Assert
        response.Should().Be200Ok()
                .And.Satisfy<ProductResponse>(product =>
                {
                    product.Id.Should().Be(productId);
                    product.Name.Should().Be("更新後產品");
                    product.Price.Should().Be(150m);
                    product.UpdatedAt.Should().BeAfter(product.CreatedAt);
                });
    }

    #endregion

    #region DELETE /products/{id} 測試

    [Fact]
    public async Task DeleteProduct_存在的產品_應回傳204()
    {
        // Arrange
        var productId = await TestHelpers.SeedSpecificProductAsync(
            DatabaseManager, "待刪除產品", 99.99m);

        // Act
        var response = await HttpClient.DeleteAsync($"/products/{productId}");

        // Assert
        response.Should().Be204NoContent();

        // 驗證產品已被刪除
        var getResponse = await HttpClient.GetAsync($"/products/{productId}");
        getResponse.Should().Be404NotFound();
    }

    #endregion
}

#region 測試用 DTO 類別

public class ProductCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class PagedResult<T>
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}

public class ValidationProblemDetails
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

#endregion
