namespace MyApp.Tests;

/// <summary>
/// TUnit 測試生命週期管理範例
/// 展示 Before/After 屬性與建構式/Dispose 模式
/// </summary>
/// 
#region 基本生命週期：建構式與 Dispose

/// <summary>
/// 使用建構式與 Dispose 的基本生命週期管理
/// 與 xUnit 完全相容
/// </summary>
public class BasicLifecycleTests : IDisposable
{
    private readonly Calculator _calculator;

    // 每個測試執行前都會呼叫建構式
    public BasicLifecycleTests()
    {
        _calculator = new Calculator();
        Console.WriteLine("建構式：建立 Calculator 實例");
    }

    [Test]
    public async Task Add_基本測試()
    {
        Console.WriteLine("執行測試：Add_基本測試");
        await Assert.That(_calculator.Add(1, 2)).IsEqualTo(3);
    }

    [Test]
    public async Task Multiply_基本測試()
    {
        Console.WriteLine("執行測試：Multiply_基本測試");
        await Assert.That(_calculator.Multiply(3, 4)).IsEqualTo(12);
    }

    // 每個測試執行後都會呼叫 Dispose
    public void Dispose()
    {
        Console.WriteLine("Dispose：清理資源");
        // 進行必要的清理工作
    }
}

#endregion

#region 進階生命週期：Before/After 屬性

/// <summary>
/// 使用 Before 和 After 屬性的進階生命週期管理
/// </summary>
public class DatabaseLifecycleTests
{
    private static TestDatabase? _database;

    // 類別層級：所有測試執行前只執行一次（靜態方法）
    [Before(Class)]
    public static async Task ClassSetup()
    {
        _database = new TestDatabase();
        await _database.InitializeAsync();
        Console.WriteLine("資料庫初始化完成");
    }

    // 測試層級：每個測試執行前都會執行（實例方法）
    [Before(Test)]
    public async Task TestSetup()
    {
        Console.WriteLine("測試準備：清理資料庫狀態");
        await _database!.ClearDataAsync();
    }

    [Test]
    public async Task 測試使用者建立()
    {
        // Arrange
        var userService = new UserService(_database!);

        // Act
        var user = await userService.CreateUserAsync("test@example.com");

        // Assert
        await Assert.That(user.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(user.Email).IsEqualTo("test@example.com");
    }

    [Test]
    public async Task 測試使用者查詢()
    {
        // Arrange
        var userService = new UserService(_database!);
        await userService.CreateUserAsync("query@example.com");

        // Act
        var user = await userService.GetUserByEmailAsync("query@example.com");

        // Assert
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Email).IsEqualTo("query@example.com");
    }

    // 測試層級：每個測試執行後都會執行
    [After(Test)]
    public async Task TestTearDown()
    {
        Console.WriteLine("測試清理：記錄測試結果");
        await Task.CompletedTask;
    }

    // 類別層級：所有測試執行後只執行一次
    [After(Class)]
    public static async Task ClassTearDown()
    {
        if (_database != null)
        {
            await _database.DisposeAsync();
            Console.WriteLine("資料庫連線關閉");
        }
    }
}

#endregion

#region 生命週期執行順序示範

/// <summary>
/// 展示完整的生命週期執行順序
/// 執行順序：Before(Class) → 建構式 → Before(Test) → 測試 → After(Test) → Dispose → After(Class)
/// </summary>
public class LifecycleOrderDemoTests : IDisposable
{
    public LifecycleOrderDemoTests()
    {
        Console.WriteLine("2. 建構式執行");
    }

    [Before(Class)]
    public static void ClassSetup()
    {
        Console.WriteLine("1. Before(Class) 執行");
    }

    [Before(Test)]
    public async Task TestSetup()
    {
        Console.WriteLine("3. Before(Test) 執行");
        await Task.CompletedTask;
    }

    [Test]
    public async Task 示範測試()
    {
        Console.WriteLine("4. 測試方法執行");
        await Assert.That(true).IsTrue();
    }

    [After(Test)]
    public async Task TestTearDown()
    {
        Console.WriteLine("5. After(Test) 執行");
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Console.WriteLine("6. Dispose 執行");
    }

    [After(Class)]
    public static void ClassTearDown()
    {
        Console.WriteLine("7. After(Class) 執行");
    }
}

#endregion

#region 非同步生命週期

/// <summary>
/// 展示非同步生命週期方法
/// </summary>
public class AsyncLifecycleTests
{
    private HttpClient? _httpClient;

    [Before(Test)]
    public async Task SetupAsync()
    {
        // 非同步設定
        _httpClient = new HttpClient();
        await Task.Delay(100); // 模擬非同步初始化
    }

    [Test]
    public async Task Http請求測試()
    {
        // 使用已設定的 HttpClient
        var response = await _httpClient!.GetAsync("https://httpbin.org/status/200");
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        // 非同步清理
        if (_httpClient != null)
        {
            _httpClient.Dispose();
        }
        await Task.CompletedTask;
    }
}

#endregion

#region 輔助類別

public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Multiply(int a, int b) => a * b;
}

public class TestDatabase : IAsyncDisposable
{
    public Task InitializeAsync() => Task.CompletedTask;
    public Task ClearDataAsync() => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class UserService
{
    private readonly TestDatabase _database;

    public UserService(TestDatabase database)
    {
        _database = database;
    }

    public Task<User> CreateUserAsync(string email)
    {
        return Task.FromResult(new User
        {
            Id = Guid.NewGuid(),
            Email = email
        });
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return Task.FromResult<User?>(new User
        {
            Id = Guid.NewGuid(),
            Email = email
        });
    }
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

#endregion
