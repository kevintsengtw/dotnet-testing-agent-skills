using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using YourProject.Application.DTOs;
using YourProject.Tests.Integration.Fixtures;

namespace YourProject.Tests.Integration.Controllers;

/// <summary>
/// ProductsController 整合測試
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region 建立產品測試

    [Fact]
    public async Task CreateProduct_使用有效資料_應成功建立產品()
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
                product.CreatedAt.Should().Be(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
                product.UpdatedAt.Should().Be(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
            });
    }

    [Fact]
    public async Task CreateProduct_當產品名稱為空_應回傳400BadRequest()
    {
        // Arrange
        var invalidRequest = new ProductCreateRequest
        {
            Name = "",
            Price = 100.00m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

        // Assert
        response.Should().Be400BadRequest()
            .And.Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
                problem.Title.Should().Be("One or more validation errors occurred.");
                problem.Status.Should().Be(400);
                problem.Errors.Should().ContainKey("Name");
                problem.Errors["Name"].Should().Contain("產品名稱不能為空");
            });
    }

    [Fact]
    public async Task CreateProduct_當價格小於零_應回傳400BadRequest()
    {
        // Arrange
        var invalidRequest = new ProductCreateRequest
        {
            Name = "測試產品",
            Price = -10.00m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

        // Assert
        response.Should().Be400BadRequest()
            .And.Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Errors.Should().ContainKey("Price");
                problem.Errors["Price"].Should().Contain("產品價格必須大於 0");
            });
    }

    [Fact]
    public async Task CreateProduct_當多個欄位無效_應回傳所有驗證錯誤()
    {
        // Arrange
        var invalidRequest = new ProductCreateRequest
        {
            Name = "",
            Price = -10.00m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

        // Assert
        response.Should().Be400BadRequest()
            .And.Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Errors.Should().ContainKey("Name");
                problem.Errors.Should().ContainKey("Price");
            });
    }

    #endregion

    #region 查詢產品測試

    [Fact]
    public async Task GetById_當產品存在_應回傳產品資料()
    {
        // Arrange
        var productId = await DatabaseManager.SeedProductAsync("測試產品", 199.99m);

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
    public async Task GetById_當產品不存在_應回傳404且包含ProblemDetails()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/products/{nonExistentId}");

        // Assert
        response.Should().Be404NotFound();

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

        problemDetails.GetProperty("type").GetString().Should().Be("https://httpstatuses.com/404");
        problemDetails.GetProperty("title").GetString().Should().Be("產品不存在");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain($"找不到 ID 為 {nonExistentId} 的產品");
    }

    #endregion

    #region 分頁查詢測試

    [Fact]
    public async Task GetProducts_使用分頁參數_應回傳正確的分頁結果()
    {
        // Arrange
        await DatabaseManager.SeedProductsAsync(15);

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
                result.Items.Should().AllSatisfy(product =>
                {
                    product.Id.Should().NotBeEmpty();
                    product.Name.Should().NotBeNullOrEmpty();
                    product.Price.Should().BeGreaterThan(0);
                });
            });
    }

    [Fact]
    public async Task GetProducts_使用搜尋參數_應回傳符合條件的產品()
    {
        // Arrange
        await DatabaseManager.SeedProductsAsync(5);
        await DatabaseManager.SeedProductAsync("特殊產品", 199.99m);

        // Act - 使用 Flurl 建構複雜查詢
        var url = "/products"
            .SetQueryParam("keyword", "特殊")
            .SetQueryParam("pageSize", 10);

        var response = await HttpClient.GetAsync(url);

        // Assert
        response.Should().Be200Ok()
            .And.Satisfy<PagedResult<ProductResponse>>(result =>
            {
                result.Total.Should().Be(1);
                result.Items.Should().HaveCount(1);

                var product = result.Items.First();
                product.Name.Should().Be("特殊產品");
                product.Price.Should().Be(199.99m);
            });
    }

    #endregion

    #region 更新產品測試

    [Fact]
    public async Task UpdateProduct_使用有效資料_應成功更新產品()
    {
        // Arrange
        var productId = await DatabaseManager.SeedProductAsync("原始產品", 100.00m);
        var updateRequest = new ProductUpdateRequest
        {
            Name = "更新後產品",
            Price = 299.99m
        };

        // 前進時間 1 小時
        AdvanceTime(TimeSpan.FromHours(1));

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/products/{productId}", updateRequest);

        // Assert
        response.Should().Be200Ok()
            .And.Satisfy<ProductResponse>(product =>
            {
                product.Name.Should().Be("更新後產品");
                product.Price.Should().Be(299.99m);
            });
    }

    #endregion

    #region 刪除產品測試

    [Fact]
    public async Task DeleteProduct_當產品存在_應成功刪除()
    {
        // Arrange
        var productId = await DatabaseManager.SeedProductAsync("待刪除產品", 100.00m);

        // Act
        var response = await HttpClient.DeleteAsync($"/products/{productId}");

        // Assert
        response.Should().Be204NoContent();

        // 確認產品已被刪除
        var getResponse = await HttpClient.GetAsync($"/products/{productId}");
        getResponse.Should().Be404NotFound();
    }

    [Fact]
    public async Task DeleteProduct_當產品不存在_應回傳404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/products/{nonExistentId}");

        // Assert
        response.Should().Be404NotFound();
    }

    #endregion
}

#region DTO 類別 (範例)

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

#endregion
