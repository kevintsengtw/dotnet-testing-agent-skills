// =============================================================================
// ASP.NET Core 整合測試 - 測試基底類別範本
// =============================================================================
// 用途：提供整合測試的共用功能，包含資料庫操作、HttpClient 管理
// 使用方式：讓測試類別繼承此基底類別
// =============================================================================

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace YourProject.IntegrationTests;

/// <summary>
/// 整合測試基底類別
/// 提供共用的測試設定、資料庫操作、HttpClient 管理
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CustomWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    // ========================================
    // 資料庫輔助方法
    // ========================================

    /// <summary>
    /// 新增測試用貨運商資料
    /// </summary>
    /// <param name="companyName">公司名稱</param>
    /// <param name="phone">電話號碼</param>
    /// <returns>新增的貨運商 ID</returns>
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

    /// <summary>
    /// 批次新增多個測試用貨運商
    /// </summary>
    /// <param name="shippers">貨運商資料列表</param>
    /// <returns>新增的貨運商 ID 列表</returns>
    protected async Task<List<int>> SeedShippersAsync(
        params (string CompanyName, string Phone)[] shippers)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entities = shippers.Select(s => new Shipper
        {
            CompanyName = s.CompanyName,
            Phone = s.Phone,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        context.Shippers.AddRange(entities);
        await context.SaveChangesAsync();

        return entities.Select(e => e.ShipperId).ToList();
    }

    /// <summary>
    /// 清理資料庫中的所有貨運商資料
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Shippers.RemoveRange(context.Shippers);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 取得資料庫中的貨運商數量
    /// </summary>
    protected async Task<int> GetShipperCountAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Shippers.CountAsync();
    }

    // ========================================
    // HTTP 請求輔助方法
    // ========================================

    /// <summary>
    /// 發送 GET 請求並取得結果
    /// </summary>
    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>
    /// 發送 POST 請求並取得結果
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await Client.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    /// <summary>
    /// 發送 PUT 請求並取得結果
    /// </summary>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await Client.PutAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    /// <summary>
    /// 發送 DELETE 請求
    /// </summary>
    protected async Task DeleteAsync(string url)
    {
        var response = await Client.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }

    // ========================================
    // 資源清理
    // ========================================

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Client?.Dispose();
            Factory?.Dispose();
        }
    }
}

// =============================================================================
// 使用範例
// =============================================================================

/// <summary>
/// 貨運商控制器整合測試範例
/// </summary>
public class ShippersControllerTests : IntegrationTestBase
{
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

    [Fact]
    public async Task GetAllShippers_應回傳所有貨運商()
    {
        // Arrange
        await CleanupDatabaseAsync();
        await SeedShippersAsync(
            ("公司A", "02-1111-1111"),
            ("公司B", "02-2222-2222"),
            ("公司C", "02-3333-3333")
        );

        // Act
        var response = await Client.GetAsync("/api/shippers");

        // Assert
        response.Should().Be200Ok()
                .And
                .Satisfy<SuccessResultOutputModel<List<ShipperOutputModel>>>(result =>
                {
                    result.Data!.Count.Should().Be(3);
                    result.Data.Should().Contain(s => s.CompanyName == "公司A");
                    result.Data.Should().Contain(s => s.CompanyName == "公司B");
                    result.Data.Should().Contain(s => s.CompanyName == "公司C");
                });
    }
}
