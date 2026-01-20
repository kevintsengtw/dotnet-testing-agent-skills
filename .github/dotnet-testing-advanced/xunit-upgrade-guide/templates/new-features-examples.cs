// =============================================================================
// xUnit 3.x 新功能使用範例
// =============================================================================

using System.Globalization;
using System.Runtime.InteropServices;
using Xunit;

namespace XunitUpgradeGuide.Examples;

// =============================================================================
// 1. [Test] 屬性 - 統一的測試標記
// =============================================================================

public class TestAttributeExamples
{
    // ✅ 新的 [Test] 屬性，功能等同於 [Fact]
    [Test]
    public void 使用Test屬性的測試()
    {
        Assert.True(true);
    }
    
    // ✅ [Fact] 仍然可用
    [Fact]
    public void 使用Fact屬性的測試()
    {
        Assert.True(true);
    }
    
    // ✅ [Theory] 用於參數化測試
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    public void 參數化測試(int a, int b, int expected)
    {
        Assert.Equal(expected, a + b);
    }
}

// =============================================================================
// 2. 明確測試 (Explicit Tests)
// =============================================================================

public class ExplicitTestExamples
{
    // ✅ 明確測試：預設不執行，除非明確要求
    [Fact(Explicit = true)]
    public void 昂貴的整合測試()
    {
        // 這個測試只有在明確選擇執行時才會執行
        // 適用於：效能測試、長時間執行的測試、需要特殊環境的測試
        Thread.Sleep(1000); // 模擬耗時操作
        Assert.True(true);
    }
    
    [Fact(Explicit = true)]
    public void 需要特殊環境的測試()
    {
        // 例如：需要特定資料庫、外部服務等
        Assert.True(true);
    }
}

// =============================================================================
// 3. 動態跳過測試 (Dynamic Skip)
// =============================================================================

public class DynamicSkipExamples
{
    // ✅ 使用 Assert.Skip (命令式)
    [Fact]
    public void 根據功能開關跳過測試()
    {
        var featureEnabled = GetFeatureFlag("NEW_CALCULATION_ENGINE");
        
        if (!featureEnabled)
        {
            Assert.Skip("新計算引擎功能尚未啟用");
        }
        
        // 測試新功能...
        Assert.True(true);
    }
    
    // ✅ 使用 SkipUnless (聲明式)
    [Fact(SkipUnless = nameof(IsLinuxEnvironment), 
          Skip = "此測試只在 Linux 環境執行")]
    public void Linux專屬測試()
    {
        Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
    }
    
    // ✅ 使用 SkipWhen (聲明式)
    [Fact(SkipWhen = nameof(IsDebugBuild), 
          Skip = "此測試在 Debug 建置中跳過")]
    public void Release建置專屬測試()
    {
        Assert.True(true);
    }
    
    // 靜態屬性
    public static bool IsLinuxEnvironment 
        => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    
    public static bool IsDebugBuild
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
    
    private bool GetFeatureFlag(string flagName)
        => bool.TryParse(Environment.GetEnvironmentVariable($"FEATURE_{flagName}"), 
                         out var result) && result;
}

// =============================================================================
// 4. 矩陣理論資料 (Matrix Theory Data)
// =============================================================================

public class MatrixTheoryDataExamples
{
    // ✅ 矩陣組合：產生所有可能的組合
    public static TheoryData<int, string> MatrixData =>
        new MatrixTheoryData<int, string>(
            [1, 2, 3],                      // 3 個數字
            ["Hello", "World", "Test"]       // 3 個字串
        );
        // 這會產生 3×3=9 個測試案例:
        // (1, "Hello"), (1, "World"), (1, "Test")
        // (2, "Hello"), (2, "World"), (2, "Test")
        // (3, "Hello"), (3, "World"), (3, "Test")

    [Theory]
    [MemberData(nameof(MatrixData))]
    public void 矩陣測試範例(int number, string text)
    {
        Assert.True(number > 0);
        Assert.NotNull(text);
        Assert.NotEmpty(text);
    }
    
    // ✅ 更複雜的矩陣組合
    public static TheoryData<string, int, bool> ComplexMatrixData =>
        new MatrixTheoryData<string, int, bool>(
            ["Admin", "User", "Guest"],     // 角色
            [1, 5, 10],                      // 權限等級
            [true, false]                    // 是否啟用
        );
        // 產生 3×3×2=18 個測試案例

    [Theory]
    [MemberData(nameof(ComplexMatrixData))]
    public void 複雜矩陣測試(string role, int level, bool enabled)
    {
        Assert.NotNull(role);
        Assert.InRange(level, 1, 10);
    }
}

// =============================================================================
// 5. Assembly Fixtures (全組件層級設定)
// =============================================================================

/// <summary>
/// Assembly Fixture：在整個測試組件中共享的資源
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = string.Empty;
    
    public async Task InitializeAsync()
    {
        // 在所有測試執行前初始化
        ConnectionString = "Server=localhost;Database=TestDb;";
        await Task.CompletedTask;
        
        Console.WriteLine("Assembly Fixture 初始化完成");
    }
    
    public async Task DisposeAsync()
    {
        // 在所有測試執行後清理
        await Task.CompletedTask;
        
        Console.WriteLine("Assembly Fixture 清理完成");
    }
}

// 註冊 Assembly Fixture (通常放在 AssemblyInfo.cs 或專案根目錄)
// [assembly: AssemblyFixture(typeof(TestDatabaseFixture))]

// 在測試中使用 Assembly Fixture
public class AssemblyFixtureExamples
{
    private readonly TestDatabaseFixture _dbFixture;
    
    public AssemblyFixtureExamples(TestDatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }
    
    [Fact]
    public void 使用Assembly Fixture的測試()
    {
        Assert.NotNull(_dbFixture.ConnectionString);
    }
}

// =============================================================================
// 6. Test Pipeline Startup (測試前期執行)
// =============================================================================

/// <summary>
/// Test Pipeline Startup：在任何測試執行前進行全域初始化
/// </summary>
public class CustomTestPipelineStartup : ITestPipelineStartup
{
    public async ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        // 全域初始化邏輯
        diagnosticMessageSink.OnMessage(
            new DiagnosticMessage("正在初始化測試環境..."));
        
        // 例如：設定環境變數、初始化共享資源
        Environment.SetEnvironmentVariable("TEST_MODE", "true");
        
        await Task.CompletedTask;
    }
}

// 註冊 Test Pipeline Startup (通常放在 AssemblyInfo.cs)
// [assembly: TestPipelineStartup(typeof(CustomTestPipelineStartup))]

// =============================================================================
// 7. Culture 設定 (多語系測試)
// =============================================================================

public class CultureTestExamples
{
    [Fact]
    public void 使用英文文化的貨幣格式測試()
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var result = 123.45m.ToString("C");
            Assert.Equal("$123.45", result);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
    
    [Fact]
    public void 使用繁體中文文化的日期格式測試()
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var testDate = new DateTime(2024, 12, 31);
        
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-TW");
            var result = testDate.ToString("yyyy年MM月dd日");
            Assert.Equal("2024年12月31日", result);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
    
    [Theory]
    [InlineData("en-US", "$123.45")]
    [InlineData("zh-TW", "NT$123.45")]
    [InlineData("ja-JP", "￥123")]
    public void 多文化貨幣格式測試(string cultureName, string expected)
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            var result = cultureName == "ja-JP" 
                ? 123m.ToString("C") 
                : 123.45m.ToString("C");
            Assert.Equal(expected, result);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}

// =============================================================================
// 8. 改進的測試診斷
// =============================================================================

public class DiagnosticsExamples
{
    private readonly ITestOutputHelper _output;
    
    public DiagnosticsExamples(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void 詳細診斷資訊測試()
    {
        // xUnit 3.x 自動提供更詳細的測試執行資訊
        _output.WriteLine("測試開始執行");
        _output.WriteLine($"執行時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        var startTime = DateTime.Now;
        
        // 執行測試邏輯
        var result = PerformCalculation(5, 3);
        
        var duration = DateTime.Now - startTime;
        _output.WriteLine($"執行耗時：{duration.TotalMilliseconds:F2} ms");
        _output.WriteLine($"計算結果：{result}");
        
        Assert.Equal(8, result);
    }
    
    private int PerformCalculation(int a, int b) => a + b;
}
