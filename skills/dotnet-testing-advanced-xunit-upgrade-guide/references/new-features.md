# xUnit 3.x 新功能完整範例

## 動態跳過測試

### 聲明式 (SkipUnless/SkipWhen)

```csharp
[Fact(SkipUnless = nameof(IsWindowsEnvironment),
      Skip = "此測試只在 Windows 環境執行")]
public void 只在Windows上執行的測試()
{
    // 測試邏輯
}

public static bool IsWindowsEnvironment =>
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
```

### 命令式 (Assert.Skip)

```csharp
[Fact]
public void 根據環境變數跳過的測試()
{
    var enableTests = Environment.GetEnvironmentVariable("ENABLE_INTEGRATION_TESTS");

    if (string.IsNullOrEmpty(enableTests) || enableTests.ToLower() != "true")
    {
        Assert.Skip("整合測試已停用。設定 ENABLE_INTEGRATION_TESTS=true 來執行");
    }

    // 測試邏輯...
}
```

## 明確測試 (Explicit Tests)

```csharp
[Fact(Explicit = true)]
public void 昂貴的整合測試()
{
    // 這個測試預設不會執行，除非明確要求
    // 適用於效能測試、長時間執行的測試
}
```

## [Test] 屬性

```csharp
// 三種寫法功能相同
[Test]
public void 使用Test屬性的測試() { Assert.True(true); }

[Fact]
public void 使用Fact屬性的測試() { Assert.True(true); }
```

## 矩陣理論資料 (Matrix Theory Data)

```csharp
public static TheoryData<int, string> TestData =>
    new MatrixTheoryData<int, string>(
        [1, 2, 3],                    // 數字資料
        ["Hello", "World", "Test"]    // 字串資料
    );
    // 這會產生 3×3=9 個測試案例

[Theory]
[MemberData(nameof(TestData))]
public void 矩陣測試範例(int number, string text)
{
    number.Should().BePositive();
    text.Should().NotBeNullOrEmpty();
}
```

## Assembly Fixtures

```csharp
public class DatabaseAssemblyFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        // 建立測試資料庫
        ConnectionString = await CreateTestDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        // 清理測試資料庫
        await DropTestDatabaseAsync();
    }
}

// 註冊 Assembly Fixture
[assembly: AssemblyFixture(typeof(DatabaseAssemblyFixture))]

// 在測試中使用
public class UserServiceTests
{
    private readonly DatabaseAssemblyFixture _dbFixture;

    public UserServiceTests(DatabaseAssemblyFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public void Test1() { /* 使用 _dbFixture.ConnectionString */ }
}
```

## Test Pipeline Startup

```csharp
public class TestPipelineStartup : ITestPipelineStartup
{
    public async Task ConfigureAsync(ITestPipelineBuilder builder,
                                     CancellationToken cancellationToken)
    {
        // 全域初始化邏輯
        Console.WriteLine("初始化測試環境...");
        await InitializeDatabaseAsync();
    }
}

// 註冊
[assembly: TestPipelineStartup(typeof(TestPipelineStartup))]
```

## 測試報告格式

xUnit 3.x 支援多種報告格式：

```bash
# 產生 CTRF 格式報告
dotnet run -- -ctrf results.json

# 產生 TRX 格式報告
dotnet run -- -trx results.trx

# 產生 XML 格式報告
dotnet run -- -xml results.xml

# 產生多種格式報告
dotnet run -- -xml results.xml -ctrf results.json -trx results.trx
```

## 效能改進

xUnit 3.x 帶來的效能改進：

1. **獨立進程執行**：測試在獨立進程中執行，更好的隔離性
2. **改進的並行演算法**：更智慧的負載平衡
3. **更快的啟動時間**：可執行檔直接執行
4. **更好的記憶體隔離**：減少測試之間的干擾
