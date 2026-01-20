// TUnit 生命週期管理與依賴注入範例

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace TUnit.Advanced.Lifecycle.Examples;

#region Domain Models and Interfaces

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
    public decimal TotalAmount => SubTotal;
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public interface IOrderRepository
{
    Task<bool> SaveOrderAsync(Order order);
}

public interface IDiscountCalculator
{
    Task<decimal> CalculateDiscountAsync(Order order, string discountCode);
}

public interface IShippingCalculator
{
    decimal CalculateShippingFee(Order order);
}

public interface ILogger<T>
{
    void LogInformation(string message);
}

#endregion

#region Mock Implementations

public class MockOrderRepository : IOrderRepository
{
    public Task<bool> SaveOrderAsync(Order order)
    {
        order.OrderId = Guid.NewGuid().ToString();
        return Task.FromResult(true);
    }
}

public class MockDiscountCalculator : IDiscountCalculator
{
    public Task<decimal> CalculateDiscountAsync(Order order, string discountCode)
    {
        var baseDiscount = order.CustomerLevel == CustomerLevel.VIP會員 ? 
            order.TotalAmount * 0.1m : 0m;
        return Task.FromResult(baseDiscount);
    }
}

public class MockShippingCalculator : IShippingCalculator
{
    public decimal CalculateShippingFee(Order order)
    {
        if (order.CustomerLevel == CustomerLevel.鑽石會員) return 0m;
        if (order.SubTotal >= 1000m) return 0m;
        return 80m;
    }
}

public class MockLogger<T> : ILogger<T>
{
    public void LogInformation(string message)
    {
        Console.WriteLine($"[{typeof(T).Name}] {message}");
    }
}

#endregion

#region Order Service

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IDiscountCalculator _discountCalculator;
    private readonly IShippingCalculator _shippingCalculator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IDiscountCalculator discountCalculator,
        IShippingCalculator shippingCalculator,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _discountCalculator = discountCalculator;
        _shippingCalculator = shippingCalculator;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(string customerId, CustomerLevel level, List<OrderItem> items)
    {
        var order = new Order
        {
            CustomerId = customerId,
            CustomerLevel = level,
            Items = items
        };

        await _repository.SaveOrderAsync(order);
        _logger.LogInformation($"Order created: {order.OrderId}");
        return order;
    }
}

#endregion

#region Lifecycle Examples

/// <summary>
/// TUnit 生命週期完整範例
/// 展示 Before/After 屬性的執行順序
/// </summary>
public class LifecycleCompleteExample
{
    private readonly StringBuilder _logBuilder;
    private static readonly List<string> ClassLog = [];

    public LifecycleCompleteExample()
    {
        Console.WriteLine("1. 建構式執行 - 測試實例建立");
        _logBuilder = new StringBuilder();
        _logBuilder.AppendLine("建構式執行");
    }

    [Before(Class)]
    public static async Task BeforeClass()
    {
        Console.WriteLine("2. BeforeClass 執行 - 類別層級初始化");
        ClassLog.Add("BeforeClass 執行");
        await Task.Delay(10); // 模擬非同步初始化
    }

    [Before(Test)]
    public async Task BeforeTest()
    {
        Console.WriteLine("3. BeforeTest 執行 - 測試前置設定");
        _logBuilder.AppendLine("BeforeTest 執行");
        await Task.Delay(5); // 模擬非同步設定
    }

    [Test]
    public async Task FirstTest_應按正確順序執行生命週期方法()
    {
        Console.WriteLine($"4. FirstTest 執行 - 驗證生命週期順序 [{DateTime.Now:HH:mm:ss.fff}]");
        _logBuilder.AppendLine("FirstTest 執行");

        var log = _logBuilder.ToString();
        await Assert.That(log).Contains("建構式執行");
        await Assert.That(log).Contains("BeforeTest 執行");
        await Assert.That(ClassLog).Contains("BeforeClass 執行");
    }

    [Test]
    public async Task SecondTest_應有獨立的實例()
    {
        Console.WriteLine($"4. SecondTest 執行 - 驗證實例獨立性 [{DateTime.Now:HH:mm:ss.fff}]");
        _logBuilder.AppendLine("SecondTest 執行");

        // 每個測試都有新的實例，所以建構式會重新執行
        var log = _logBuilder.ToString();
        await Assert.That(log).Contains("建構式執行");
        await Assert.That(log).Contains("BeforeTest 執行");
    }

    [After(Test)]
    public async Task AfterTest()
    {
        Console.WriteLine("5. AfterTest 執行 - 測試後清理");
        _logBuilder.AppendLine("AfterTest 執行");
        await Task.Delay(5); // 模擬非同步清理
    }

    [After(Class)]
    public static async Task AfterClass()
    {
        Console.WriteLine("6. AfterClass 執行 - 類別層級清理");
        ClassLog.Add("AfterClass 執行");
        await Task.Delay(10); // 模擬非同步清理
    }
}

/// <summary>
/// IDisposable 支援範例
/// 展示如何正確釋放測試資源
/// </summary>
public class DisposableLifecycleExample : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _tempFiles = [];

    public DisposableLifecycleExample()
    {
        _httpClient = new HttpClient();
        Console.WriteLine("資源已建立: HttpClient");
    }

    [Test]
    public async Task TestWithResources_應正確管理資源()
    {
        // 模擬建立暫存檔案
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);
        
        await Assert.That(_httpClient).IsNotNull();
        await Assert.That(File.Exists(tempFile)).IsTrue();
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("DisposeAsync 執行 - 釋放所有資源");
        
        _httpClient.Dispose();
        
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        
        await Task.CompletedTask;
    }
}

#endregion

#region Dependency Injection

/// <summary>
/// TUnit 依賴注入資料來源屬性
/// 基於 Microsoft.Extensions.DependencyInjection 實作
/// </summary>
public class MicrosoftDependencyInjectionDataSourceAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    private static readonly IServiceProvider ServiceProvider = CreateSharedServiceProvider();

    public override IServiceScope CreateScope(DataGeneratorMetadata dataGeneratorMetadata)
    {
        return ServiceProvider.CreateScope();
    }

    public override object? Create(IServiceScope scope, Type type)
    {
        return scope.ServiceProvider.GetService(type);
    }

    private static IServiceProvider CreateSharedServiceProvider()
    {
        return new ServiceCollection()
            .AddSingleton<IOrderRepository, MockOrderRepository>()
            .AddSingleton<IDiscountCalculator, MockDiscountCalculator>()
            .AddSingleton<IShippingCalculator, MockShippingCalculator>()
            .AddSingleton<ILogger<OrderService>, MockLogger<OrderService>>()
            .AddTransient<OrderService>()
            .BuildServiceProvider();
    }
}

/// <summary>
/// 使用 TUnit 依賴注入進行測試
/// 展示透過建構式自動注入服務
/// </summary>
[MicrosoftDependencyInjectionDataSource]
public class DependencyInjectionTests(OrderService orderService)
{
    [Test]
    public async Task CreateOrder_使用TUnit依賴注入_應正確運作()
    {
        // Arrange - 依賴已經透過 TUnit DI 自動注入
        var items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "測試商品", UnitPrice = 100m, Quantity = 2 }
        };

        // Act
        var order = await orderService.CreateOrderAsync("CUST001", CustomerLevel.VIP會員, items);

        // Assert
        await Assert.That(order).IsNotNull();
        await Assert.That(order.CustomerId).IsEqualTo("CUST001");
        await Assert.That(order.CustomerLevel).IsEqualTo(CustomerLevel.VIP會員);
        await Assert.That(order.Items).HasCount().EqualTo(1);
    }

    [Test]
    public async Task TUnitDependencyInjection_驗證自動注入_服務應為正確類型()
    {
        // Assert - 驗證 TUnit 已正確注入 OrderService 實例
        await Assert.That(orderService).IsNotNull();
        await Assert.That(orderService.GetType().Name).IsEqualTo("OrderService");
    }
}

/// <summary>
/// 手動依賴建立的對比範例
/// 展示傳統方式與 TUnit DI 的差異
/// </summary>
public class ManualDependencyTests
{
    [Test]
    public async Task CreateOrder_手動建立依賴_傳統方式對比()
    {
        // Arrange - 手動建立測試所需的依賴（傳統方式）
        var mockRepository = new MockOrderRepository();
        var mockDiscountCalculator = new MockDiscountCalculator();
        var mockShippingCalculator = new MockShippingCalculator();
        var mockLogger = new MockLogger<OrderService>();

        var orderService = new OrderService(
            mockRepository, 
            mockDiscountCalculator, 
            mockShippingCalculator, 
            mockLogger);

        var items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "測試商品", UnitPrice = 100m, Quantity = 2 }
        };

        // Act
        var order = await orderService.CreateOrderAsync("CUST001", CustomerLevel.VIP會員, items);

        // Assert
        await Assert.That(order).IsNotNull();
        await Assert.That(order.CustomerId).IsEqualTo("CUST001");
        await Assert.That(order.CustomerLevel).IsEqualTo(CustomerLevel.VIP會員);
        await Assert.That(order.Items).HasCount().EqualTo(1);
    }
}

#endregion

#region Properties Examples

/// <summary>
/// Properties 屬性標記與測試過濾範例
/// </summary>
public class PropertiesExamples
{
    /// <summary>
    /// 建立一致的屬性命名規範
    /// </summary>
    public static class TestProperties
    {
        // 測試類別
        public const string CATEGORY_UNIT = "Unit";
        public const string CATEGORY_INTEGRATION = "Integration";
        public const string CATEGORY_E2E = "E2E";
        
        // 優先級
        public const string PRIORITY_CRITICAL = "Critical";
        public const string PRIORITY_HIGH = "High";
        public const string PRIORITY_MEDIUM = "Medium";
        public const string PRIORITY_LOW = "Low";
        
        // 環境
        public const string ENV_DEVELOPMENT = "Development";
        public const string ENV_STAGING = "Staging";
        public const string ENV_PRODUCTION = "Production";
    }

    [Test]
    [Property("Category", "Database")]
    [Property("Priority", "High")]
    public async Task DatabaseTest_高優先級_應能透過屬性過濾()
    {
        await Assert.That(true).IsTrue();
    }

    [Test]
    [Property("Category", "Unit")]
    [Property("Priority", "Medium")]
    public async Task UnitTest_中等優先級_基本驗證()
    {
        await Assert.That(1 + 1).IsEqualTo(2);
    }

    [Test]
    [Property("Category", "Integration")]
    [Property("Priority", "Low")]
    [Property("Environment", "Development")]
    public async Task IntegrationTest_低優先級_僅開發環境執行()
    {
        await Assert.That("Hello World").Contains("World");
    }

    // 使用常數確保一致性
    [Test]
    [Property("Category", "Unit")]
    [Property("Priority", "High")]
    public async Task ExampleTest_使用常數_確保一致性()
    {
        await Assert.That(1 + 1).IsEqualTo(2);
    }
}

#endregion

#region Test Filtering Commands

/*
 * TUnit 測試過濾執行指令範例
 * 
 * TUnit 使用 dotnet run 而不是 dotnet test：
 * 
 * # 只執行單元測試
 * dotnet run --treenode-filter "/*/*/*/*[Category=Unit]"
 * 
 * # 只執行高優先級測試
 * dotnet run --treenode-filter "/*/*/*/*[Priority=High]"
 * 
 * # 組合條件：執行高優先級的單元測試
 * dotnet run --treenode-filter "/*/*/*/*[(Category=Unit)&(Priority=High)]"
 * 
 * # 執行冒煙測試套件
 * dotnet run --treenode-filter "/*/*/*/*[Suite=Smoke]"
 * 
 * # 執行特定功能的測試
 * dotnet run --treenode-filter "/*/*/*/*[Feature=OrderProcessing]"
 * 
 * # 複雜組合：執行高優先級的單元測試或冒煙測試
 * dotnet run --treenode-filter "/*/*/*/*[((Category=Unit)&(Priority=High))|(Suite=Smoke)]"
 * 
 * 過濾語法注意事項：
 * 1. 路徑模式 /*/*/*/* 是固定格式，代表 Assembly/Namespace/Class/Method 的層級
 * 2. 屬性名稱大小寫敏感
 * 3. 值的大小寫敏感
 * 4. 括號的使用：組合條件必須用括號正確包圍
 * 5. 引號的使用：整個過濾字串需要用引號包圍
 */

#endregion
