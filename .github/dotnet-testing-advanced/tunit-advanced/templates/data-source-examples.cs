// TUnit 資料來源範例 - MethodDataSource 與 ClassDataSource

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Text.Json;

namespace TUnit.Advanced.DataSource.Examples;

#region Domain Models

public enum CustomerLevel
{
    一般會員 = 0,
    VIP會員 = 1,
    白金會員 = 2,
    鑽石會員 = 3
}

public class Order
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public CustomerLevel CustomerLevel { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal SubTotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount => SubTotal - DiscountAmount + ShippingFee;
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class OrderValidationScenario
{
    public string Name { get; set; } = "";
    public Order Order { get; set; } = new();
    public bool ExpectedValid { get; set; }
    public string? ExpectedErrorKeyword { get; set; }
}

#endregion

#region MethodDataSource Examples

/// <summary>
/// MethodDataSource 基本使用範例
/// 展示如何使用方法作為資料來源進行參數化測試
/// </summary>
public class MethodDataSourceBasicTests
{
    /// <summary>
    /// 使用 MethodDataSource 進行參數化測試
    /// 支援複雜物件類型的資料傳遞
    /// </summary>
    [Test]
    [MethodDataSource(nameof(GetOrderTestData))]
    public async Task CreateOrder_各種情況_應正確處理(
        string customerId, 
        CustomerLevel level, 
        List<OrderItem> items, 
        decimal expectedTotal)
    {
        // Arrange
        var order = new Order
        {
            CustomerId = customerId,
            CustomerLevel = level,
            Items = items
        };

        // Assert
        await Assert.That(order).IsNotNull();
        await Assert.That(order.CustomerId).IsEqualTo(customerId);
        await Assert.That(order.CustomerLevel).IsEqualTo(level);
        await Assert.That(order.SubTotal).IsEqualTo(expectedTotal);
    }

    /// <summary>
    /// 資料提供方法 - 使用 yield return 產生測試資料
    /// </summary>
    public static IEnumerable<object[]> GetOrderTestData()
    {
        // 一般會員訂單
        yield return new object[]
        {
            "CUST001",
            CustomerLevel.一般會員,
            new List<OrderItem>
            {
                new() { ProductId = "PROD001", ProductName = "商品A", UnitPrice = 100m, Quantity = 2 }
            },
            200m
        };

        // VIP會員訂單
        yield return new object[]
        {
            "CUST002", 
            CustomerLevel.VIP會員,
            new List<OrderItem>
            {
                new() { ProductId = "PROD002", ProductName = "商品B", UnitPrice = 500m, Quantity = 1 }
            },
            500m
        };

        // 多商品訂單
        yield return new object[]
        {
            "CUST003",
            CustomerLevel.白金會員,
            new List<OrderItem>
            {
                new() { ProductId = "PROD001", ProductName = "商品A", UnitPrice = 100m, Quantity = 1 },
                new() { ProductId = "PROD002", ProductName = "商品B", UnitPrice = 200m, Quantity = 2 }
            },
            500m
        };
    }
}

/// <summary>
/// 從 JSON 檔案載入測試資料範例
/// </summary>
public class MethodDataSourceFromFileTests
{
    /// <summary>
    /// 從 JSON 檔案讀取測試資料的情境
    /// </summary>
    [Test]
    [MethodDataSource(nameof(GetDiscountTestDataFromFile))]
    public async Task CalculateDiscount_從檔案讀取_應套用正確折扣(
        string scenario, 
        decimal originalAmount, 
        CustomerLevel level, 
        string discountCode, 
        decimal expectedDiscount)
    {
        // Arrange
        // 這裡使用模擬的折扣計算邏輯
        decimal discount = CalculateMockDiscount(originalAmount, level, discountCode);

        // Assert
        await Assert.That(discount).IsEqualTo(expectedDiscount);
    }

    /// <summary>
    /// 從 JSON 檔案載入測試資料
    /// 需要在專案中建立 TestData/discount-scenarios.json
    /// </summary>
    public static IEnumerable<object[]> GetDiscountTestDataFromFile()
    {
        // 模擬 JSON 資料（實際專案中會從檔案載入）
        var scenarios = new List<DiscountScenario>
        {
            new() { Scenario = "一般會員無折扣碼", Amount = 1000, Level = 0, Code = "", Expected = 0 },
            new() { Scenario = "VIP會員使用VIP折扣碼", Amount = 1000, Level = 1, Code = "VIP50", Expected = 50 },
            new() { Scenario = "白金會員使用SAVE20折扣碼", Amount = 1000, Level = 2, Code = "SAVE20", Expected = 250 }
        };

        foreach (var s in scenarios)
        {
            yield return new object[] { s.Scenario, s.Amount, (CustomerLevel)s.Level, s.Code, s.Expected };
        }
    }

    private static decimal CalculateMockDiscount(decimal amount, CustomerLevel level, string code)
    {
        return level switch
        {
            CustomerLevel.一般會員 => 0,
            CustomerLevel.VIP會員 when code == "VIP50" => 50,
            CustomerLevel.白金會員 when code == "SAVE20" => 250,
            _ => 0
        };
    }

    /// <summary>
    /// JSON 資料結構
    /// </summary>
    private class DiscountScenario
    {
        public string Scenario { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Level { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Expected { get; set; }
    }
}

/// <summary>
/// TestDataHelper - 統一管理測試資料載入
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// 從 JSON 檔案載入測試資料的通用方法
    /// </summary>
    public static IEnumerable<object[]> LoadFromJson<T>(string fileName, Func<T, object[]> converter)
    {
        var filePath = Path.Combine("TestData", fileName);
        
        if (!File.Exists(filePath))
        {
            yield break;
        }

        var jsonData = File.ReadAllText(filePath);
        var items = JsonSerializer.Deserialize<T[]>(jsonData);
        
        if (items == null) yield break;

        foreach (var item in items)
        {
            yield return converter(item);
        }
    }
}

#endregion

#region ClassDataSource Examples

/// <summary>
/// ClassDataSource 基本使用範例
/// 使用類別作為資料提供者，適合共享資料和可重用的測試情境
/// </summary>
public class ClassDataSourceTests
{
    /// <summary>
    /// 使用 ClassDataSource 進行訂單驗證測試
    /// </summary>
    [Test]
    [ClassDataSource<OrderValidationTestData>]
    public async Task ValidateOrder_各種驗證情況_應回傳正確結果(OrderValidationScenario scenario)
    {
        // Arrange
        var isValid = ValidateOrder(scenario.Order);

        // Assert
        await Assert.That(isValid).IsEqualTo(scenario.ExpectedValid);
    }

    private static bool ValidateOrder(Order order)
    {
        if (string.IsNullOrEmpty(order.CustomerId))
            return false;
        if (order.Items.Count == 0)
            return false;
        return true;
    }
}

/// <summary>
/// 訂單驗證測試資料提供類別
/// 實作 IEnumerable<T> 介面
/// </summary>
public class OrderValidationTestData : IEnumerable<OrderValidationScenario>
{
    public IEnumerator<OrderValidationScenario> GetEnumerator()
    {
        // 有效訂單
        yield return new OrderValidationScenario
        {
            Name = "有效的一般訂單",
            Order = CreateValidOrder(),
            ExpectedValid = true,
            ExpectedErrorKeyword = null
        };

        // 客戶ID為空
        yield return new OrderValidationScenario
        {
            Name = "客戶ID為空",
            Order = CreateOrderWithEmptyCustomerId(),
            ExpectedValid = false,
            ExpectedErrorKeyword = "客戶ID"
        };

        // 商品清單為空
        yield return new OrderValidationScenario
        {
            Name = "沒有商品",
            Order = CreateOrderWithNoItems(),
            ExpectedValid = false,
            ExpectedErrorKeyword = "商品"
        };
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private static Order CreateValidOrder() => new()
    {
        CustomerId = "CUST001",
        CustomerLevel = CustomerLevel.一般會員,
        Items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "測試商品", UnitPrice = 100m, Quantity = 1 }
        }
    };

    private static Order CreateOrderWithEmptyCustomerId() => new()
    {
        CustomerId = "",
        CustomerLevel = CustomerLevel.一般會員,
        Items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "測試商品", UnitPrice = 100m, Quantity = 1 }
        }
    };

    private static Order CreateOrderWithNoItems() => new()
    {
        CustomerId = "CUST001",
        CustomerLevel = CustomerLevel.一般會員,
        Items = new List<OrderItem>()
    };
}

#endregion

#region AutoFixture Integration Example

/*
 * AutoFixture 整合範例（需要安裝 AutoFixture 套件）
 * 
 * 安裝套件：dotnet add package AutoFixture
 * 
 * public class AutoFixtureOrderTestData : IEnumerable<Order>
 * {
 *     private readonly Fixture _fixture;
 * 
 *     public AutoFixtureOrderTestData()
 *     {
 *         _fixture = new Fixture();
 *         
 *         // 自訂 Order 的產生規則
 *         _fixture.Customize<Order>(composer => composer
 *             .With(o => o.CustomerId, () => $"CUST{_fixture.Create<int>() % 1000:D3}")
 *             .With(o => o.CustomerLevel, () => _fixture.Create<CustomerLevel>())
 *             .With(o => o.Items, () => _fixture.CreateMany<OrderItem>(Random.Shared.Next(1, 5)).ToList()));
 * 
 *         // 自訂 OrderItem 的產生規則
 *         _fixture.Customize<OrderItem>(composer => composer
 *             .With(oi => oi.ProductId, () => $"PROD{_fixture.Create<int>() % 1000:D3}")
 *             .With(oi => oi.ProductName, () => $"測試商品{_fixture.Create<int>() % 100}")
 *             .With(oi => oi.UnitPrice, () => Math.Round(_fixture.Create<decimal>() % 1000 + 1, 2))
 *             .With(oi => oi.Quantity, () => _fixture.Create<int>() % 10 + 1));
 *     }
 * 
 *     public IEnumerator<Order> GetEnumerator()
 *     {
 *         for (int i = 0; i < 5; i++)
 *         {
 *             yield return _fixture.Create<Order>();
 *         }
 *     }
 * 
 *     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
 * }
 * 
 * // 使用方式
 * [Test]
 * [ClassDataSource(typeof(AutoFixtureOrderTestData))]
 * public async Task ProcessOrder_自動產生測試資料_應正確計算訂單金額(Order order)
 * {
 *     await Assert.That(order).IsNotNull();
 *     await Assert.That(order.CustomerId).IsNotEmpty();
 *     await Assert.That(order.Items).IsNotEmpty();
 * }
 */

#endregion
