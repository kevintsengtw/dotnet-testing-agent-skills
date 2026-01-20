// =============================================================================
// xUnit 2.x → 3.x 程式碼遷移範例
// =============================================================================

using System.Runtime.InteropServices;
using Xunit;

namespace XunitUpgradeGuide.Examples;

// =============================================================================
// 1. async void → async Task 修正
// =============================================================================

/// <summary>
/// 修正前：async void 測試 (xUnit 3.x 不支援)
/// </summary>
public class AsyncVoidTests_Before
{
    // ❌ 這在 xUnit 3.x 中會失敗
    // [Fact]
    // public async void TestAsyncMethod()
    // {
    //     var result = await SomeAsyncOperation();
    //     Assert.True(result);
    // }
}

/// <summary>
/// 修正後：async Task 測試 (正確寫法)
/// </summary>
public class AsyncVoidTests_After
{
    // ✅ 正確的 xUnit 3.x 寫法
    [Fact]
    public async Task TestAsyncMethod()
    {
        var result = await SomeAsyncOperation();
        Assert.True(result);
    }

    private Task<bool> SomeAsyncOperation() => Task.FromResult(true);
}

// =============================================================================
// 2. IAsyncLifetime + IDisposable 修正
// =============================================================================

/// <summary>
/// 修正前：同時實作 IAsyncLifetime 和 IDisposable
/// </summary>
public class AsyncLifetimeTests_Before // : IAsyncLifetime, IDisposable
{
    // ⚠️ 在 xUnit 2.x 中，Dispose 和 DisposeAsync 都會被呼叫
    // ⚠️ 在 xUnit 3.x 中，只有 DisposeAsync 會被呼叫
    
    // public async Task InitializeAsync() { /* 初始化 */ }
    // public async Task DisposeAsync() { /* 非同步清理 */ }
    // public void Dispose() { /* 同步清理 - 3.x 中不會被呼叫 */ }
}

/// <summary>
/// 修正後：只使用 IAsyncLifetime
/// </summary>
public class AsyncLifetimeTests_After : IAsyncLifetime
{
    private IDisposable? _resource;
    
    public async Task InitializeAsync()
    {
        // 初始化資源
        _resource = await CreateResourceAsync();
    }
    
    public async Task DisposeAsync()
    {
        // ✅ 將所有清理邏輯統一放在這裡
        _resource?.Dispose();
        await Task.CompletedTask;
    }
    
    [Fact]
    public void Test1()
    {
        Assert.NotNull(_resource);
    }
    
    private Task<IDisposable> CreateResourceAsync() 
        => Task.FromResult<IDisposable>(new MemoryStream());
}

// =============================================================================
// 3. SkippableFact → Assert.Skip 修正
// =============================================================================

/// <summary>
/// 修正前：使用 SkippableFact (xUnit 3.x 已移除)
/// </summary>
public class SkippableTests_Before
{
    // ❌ xUnit 3.x 中已移除
    // [SkippableFact]
    // public void 可跳過的測試()
    // {
    //     Skip.If(!IsWindowsEnvironment, "只在 Windows 執行");
    //     // 測試邏輯
    // }
}

/// <summary>
/// 修正後：使用 Assert.Skip (命令式)
/// </summary>
public class SkippableTests_After_Imperative
{
    [Fact]
    public void 根據條件動態跳過的測試()
    {
        // ✅ 使用 Assert.Skip (命令式)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Skip("此測試只在 Windows 環境執行");
        }
        
        // 測試邏輯
        Assert.True(true);
    }
    
    [Fact]
    public void 根據環境變數跳過的測試()
    {
        var enableTests = Environment.GetEnvironmentVariable("ENABLE_INTEGRATION_TESTS");
        
        if (string.IsNullOrEmpty(enableTests) || enableTests.ToLower() != "true")
        {
            Assert.Skip("整合測試已停用。設定 ENABLE_INTEGRATION_TESTS=true 來執行");
        }
        
        // 整合測試邏輯
        Assert.True(true);
    }
}

/// <summary>
/// 修正後：使用 SkipUnless 屬性 (聲明式)
/// </summary>
public class SkippableTests_After_Declarative
{
    // ✅ 使用 SkipUnless 屬性 (聲明式)
    // 注意：必須同時設定 Skip 屬性提供跳過訊息
    [Fact(SkipUnless = nameof(IsWindowsEnvironment), 
          Skip = "此測試只在 Windows 環境執行")]
    public void 只在Windows上執行的測試()
    {
        // 測試邏輯
        Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    }
    
    // ✅ SkipWhen：條件為 true 時跳過
    [Fact(SkipWhen = nameof(IsCIEnvironment), 
          Skip = "此測試在 CI 環境中跳過")]
    public void 不在CI環境執行的測試()
    {
        // 測試邏輯
        Assert.True(true);
    }
    
    // 靜態屬性用於 SkipUnless/SkipWhen
    public static bool IsWindowsEnvironment 
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    
    public static bool IsCIEnvironment 
        => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
}

// =============================================================================
// 4. 自訂 DataAttribute 修正
// =============================================================================

/// <summary>
/// 修正前：xUnit 2.x 的 DataAttribute 實作
/// </summary>
// public class CustomDataAttribute_Before : DataAttribute
// {
//     // ❌ xUnit 2.x 的同步方法
//     public override IEnumerable<object[]> GetData(MethodInfo testMethod)
//     {
//         yield return new object[] { 1, "test1" };
//         yield return new object[] { 2, "test2" };
//     }
// }

/// <summary>
/// 修正後：xUnit 3.x 的 DataAttribute 實作
/// </summary>
public class CustomDataAttribute_After : DataAttribute
{
    // ✅ xUnit 3.x 的非同步方法
    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod, 
        DisposalTracker disposalTracker)
    {
        // 支援非同步資料載入
        var data = await LoadDataAsync();
        
        return data.Select(item => new TheoryDataRow(item.Id, item.Name))
                   .ToList();
    }
    
    private Task<List<(int Id, string Name)>> LoadDataAsync()
    {
        var data = new List<(int, string)>
        {
            (1, "test1"),
            (2, "test2"),
            (3, "test3")
        };
        return Task.FromResult(data);
    }
}

// 使用自訂屬性
public class CustomDataAttributeTests
{
    [Theory]
    [CustomData_After]
    public void 使用自訂資料屬性的測試(int id, string name)
    {
        Assert.True(id > 0);
        Assert.NotNullOrEmpty(name);
    }
}

// =============================================================================
// 5. ITestOutputHelper 遷移
// =============================================================================

/// <summary>
/// ITestOutputHelper 在 xUnit 3.x 中仍然可用
/// 但命名空間已變更
/// </summary>
public class TestOutputTests
{
    private readonly ITestOutputHelper _output;
    
    public TestOutputTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void 輸出測試資訊()
    {
        // ✅ ITestOutputHelper 用法不變
        _output.WriteLine("測試開始執行");
        
        var result = 1 + 1;
        _output.WriteLine($"計算結果：{result}");
        
        Assert.Equal(2, result);
    }
}
