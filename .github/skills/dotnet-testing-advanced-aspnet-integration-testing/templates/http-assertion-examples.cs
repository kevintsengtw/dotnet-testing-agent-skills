// =============================================================================
// ASP.NET Core 整合測試 - HTTP 回應斷言範例
// =============================================================================
// 用途：展示如何使用 AwesomeAssertions.Web 進行 HTTP 回應驗證
// 套件需求：AwesomeAssertions.Web (配合 AwesomeAssertions 使用)
// =============================================================================

using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Web;
using Microsoft.AspNetCore.Mvc;

namespace YourProject.IntegrationTests.Examples;

/// <summary>
/// HTTP 回應斷言範例
/// 展示 AwesomeAssertions.Web 的各種斷言方法
/// </summary>
public class HttpAssertionExamples : IntegrationTestBase
{
    // ========================================
    // HTTP 狀態碼斷言
    // ========================================

    [Fact]
    public async Task 狀態碼斷言_200OK()
    {
        var response = await Client.GetAsync("/api/shippers");
        
        // 驗證 HTTP 200 OK
        response.Should().Be200Ok();
    }

    [Fact]
    public async Task 狀態碼斷言_201Created()
    {
        var request = new ShipperCreateParameter
        {
            CompanyName = "新公司",
            Phone = "02-1234-5678"
        };
        
        var response = await Client.PostAsJsonAsync("/api/shippers", request);
        
        // 驗證 HTTP 201 Created
        response.Should().Be201Created();
    }

    [Fact]
    public async Task 狀態碼斷言_204NoContent()
    {
        var shipperId = await SeedShipperAsync("待刪除公司");
        
        var response = await Client.DeleteAsync($"/api/shippers/{shipperId}");
        
        // 驗證 HTTP 204 No Content
        response.Should().Be204NoContent();
    }

    [Fact]
    public async Task 狀態碼斷言_400BadRequest()
    {
        var invalidRequest = new ShipperCreateParameter
        {
            CompanyName = "", // 空值，應該驗證失敗
            Phone = ""
        };
        
        var response = await Client.PostAsJsonAsync("/api/shippers", invalidRequest);
        
        // 驗證 HTTP 400 Bad Request
        response.Should().Be400BadRequest();
    }

    [Fact]
    public async Task 狀態碼斷言_404NotFound()
    {
        var response = await Client.GetAsync("/api/shippers/99999");
        
        // 驗證 HTTP 404 Not Found
        response.Should().Be404NotFound();
    }

    // ========================================
    // Satisfy<T> 強型別驗證
    // ========================================

    [Fact]
    public async Task Satisfy_驗證成功回應內容()
    {
        await CleanupDatabaseAsync();
        var shipperId = await SeedShipperAsync("測試公司", "02-9876-5432");

        var response = await Client.GetAsync($"/api/shippers/{shipperId}");

        // 使用 Satisfy<T> 進行強型別驗證
        response.Should().Be200Ok()
                .And
                .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                {
                    // 驗證整體結構
                    result.Status.Should().Be("Success");
                    result.Data.Should().NotBeNull();
                    
                    // 驗證資料內容
                    result.Data!.ShipperId.Should().Be(shipperId);
                    result.Data.CompanyName.Should().Be("測試公司");
                    result.Data.Phone.Should().Be("02-9876-5432");
                });
    }

    [Fact]
    public async Task Satisfy_驗證集合回應()
    {
        await CleanupDatabaseAsync();
        await SeedShipperAsync("公司A", "02-1111-1111");
        await SeedShipperAsync("公司B", "02-2222-2222");

        var response = await Client.GetAsync("/api/shippers");

        response.Should().Be200Ok()
                .And
                .Satisfy<SuccessResultOutputModel<List<ShipperOutputModel>>>(result =>
                {
                    result.Data!.Count.Should().Be(2);
                    result.Data.Should().Contain(s => s.CompanyName == "公司A");
                    result.Data.Should().Contain(s => s.CompanyName == "公司B");
                    
                    // 驗證順序 (如果有排序需求)
                    result.Data.Should().BeInAscendingOrder(s => s.CompanyName);
                });
    }

    [Fact]
    public async Task Satisfy_驗證錯誤回應詳情()
    {
        var invalidRequest = new ShipperCreateParameter
        {
            CompanyName = "",
            Phone = "02-1234-5678"
        };

        var response = await Client.PostAsJsonAsync("/api/shippers", invalidRequest);

        response.Should().Be400BadRequest()
                .And
                .Satisfy<ValidationProblemDetails>(problem =>
                {
                    problem.Status.Should().Be(400);
                    problem.Title.Should().Contain("validation");
                    problem.Errors.Should().ContainKey("CompanyName");
                });
    }

    // ========================================
    // BeAs 匿名型別驗證
    // ========================================

    [Fact]
    public async Task BeAs_使用匿名型別驗證()
    {
        await CleanupDatabaseAsync();
        var shipperId = await SeedShipperAsync("匿名驗證公司", "02-5555-5555");

        var response = await Client.GetAsync($"/api/shippers/{shipperId}");

        // 使用匿名型別進行快速驗證
        response.Should().Be200Ok()
                .And
                .BeAs(new
                {
                    Status = "Success",
                    Data = new
                    {
                        CompanyName = "匿名驗證公司",
                        Phone = "02-5555-5555"
                    }
                });
    }

    // ========================================
    // 組合斷言
    // ========================================

    [Fact]
    public async Task 組合斷言_完整CRUD流程驗證()
    {
        await CleanupDatabaseAsync();

        // Create
        var createRequest = new ShipperCreateParameter
        {
            CompanyName = "CRUD測試公司",
            Phone = "02-1234-5678"
        };
        
        var createResponse = await Client.PostAsJsonAsync("/api/shippers", createRequest);
        createResponse.Should().Be201Created();
        
        var created = await createResponse.Content
            .ReadFromJsonAsync<SuccessResultOutputModel<ShipperOutputModel>>();
        var shipperId = created!.Data!.ShipperId;

        // Read
        var readResponse = await Client.GetAsync($"/api/shippers/{shipperId}");
        readResponse.Should().Be200Ok()
                   .And
                   .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                   {
                       result.Data!.CompanyName.Should().Be("CRUD測試公司");
                   });

        // Update
        var updateRequest = new ShipperCreateParameter
        {
            CompanyName = "更新後的公司名稱",
            Phone = "02-8765-4321"
        };
        
        var updateResponse = await Client.PutAsJsonAsync($"/api/shippers/{shipperId}", updateRequest);
        updateResponse.Should().Be200Ok()
                     .And
                     .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                     {
                         result.Data!.CompanyName.Should().Be("更新後的公司名稱");
                         result.Data.Phone.Should().Be("02-8765-4321");
                     });

        // Delete
        var deleteResponse = await Client.DeleteAsync($"/api/shippers/{shipperId}");
        deleteResponse.Should().Be204NoContent();

        // Verify deletion
        var verifyResponse = await Client.GetAsync($"/api/shippers/{shipperId}");
        verifyResponse.Should().Be404NotFound();
    }
}

// =============================================================================
// DTO 模型範例 (根據專案需求調整)
// =============================================================================

/// <summary>
/// 成功回應模型
/// </summary>
public class SuccessResultOutputModel<T>
{
    public string Status { get; set; } = "Success";
    public T? Data { get; set; }
}

/// <summary>
/// 貨運商輸出模型
/// </summary>
public class ShipperOutputModel
{
    public int ShipperId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// 貨運商建立參數
/// </summary>
public class ShipperCreateParameter
{
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
