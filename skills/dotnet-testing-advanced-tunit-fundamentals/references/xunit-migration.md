# xUnit 與 TUnit 遷移指南 - 完整範例

## 並行執行控制

### NotInParallel 屬性

```csharp
// 預設並行執行
[Test]
public async Task 並行測試1() { }

[Test]
public async Task 並行測試2() { }

// 控制特定測試不要並行
[Test]
[NotInParallel("DatabaseTests")]
public async Task 資料庫測試1_不並行執行()
{
    // 這個測試不會與其他 "DatabaseTests" 群組並行執行
}

[Test]
[NotInParallel("DatabaseTests")]
public async Task 資料庫測試2_不並行執行()
{
    // 與資料庫測試1 依序執行
}
```

## xUnit 與 TUnit 語法對照

| 功能           | xUnit                                 | TUnit                                            |
| -------------- | ------------------------------------- | ------------------------------------------------ |
| **基本測試**   | `[Fact]`                              | `[Test]`                                         |
| **參數化測試** | `[Theory]` + `[InlineData]`           | `[Test]` + `[Arguments]`                         |
| **基本斷言**   | `Assert.Equal(expected, actual)`      | `await Assert.That(actual).IsEqualTo(expected)`  |
| **布林斷言**   | `Assert.True(condition)`              | `await Assert.That(condition).IsTrue()`          |
| **例外測試**   | `Assert.Throws<T>(() => action())`    | `await Assert.That(() => action()).Throws<T>()`  |
| **Null 檢查**  | `Assert.Null(value)`                  | `await Assert.That(value).IsNull()`              |
| **字串檢查**   | `Assert.Contains("text", fullString)` | `await Assert.That(fullString).Contains("text")` |

## 遷移範例

**xUnit 原始程式碼：**

```csharp
[Theory]
[InlineData("test@example.com", true)]
[InlineData("invalid", false)]
public void IsValidEmail_各種輸入_應回傳正確驗證結果(string email, bool expected)
{
    var result = _validator.IsValidEmail(email);
    Assert.Equal(expected, result);
}
```

**TUnit 轉換後：**

```csharp
[Test]
[Arguments("test@example.com", true)]
[Arguments("invalid", false)]
public async Task IsValidEmail_各種輸入_應回傳正確驗證結果(string email, bool expected)
{
    var result = _validator.IsValidEmail(email);
    await Assert.That(result).IsEqualTo(expected);
}
```

**主要變更：**

1. `[Theory]` → `[Test]`
2. `[InlineData]` → `[Arguments]`
3. 方法改為 `async Task`
4. 所有斷言加上 `await`
5. 流暢式斷言語法

## 執行與偵錯

### CLI 執行

```bash
# 建置專案
dotnet build

# 執行所有測試
dotnet run

# 詳細輸出
dotnet run -- --verbosity Detailed

# 產生覆蓋率報告
dotnet run -- --coverage

# 產生 TRX 報告
dotnet run -- --report-trx

# 過濾特定測試（使用 treenode-filter）
dotnet run -- --treenode-filter "*/*/CalculatorTests/*"
dotnet run -- --treenode-filter "*/*/CalculatorTests/Add*"
```

### AOT 編譯執行

```bash
# 發佈為 AOT 編譯版本
dotnet publish -c Release -p:PublishAot=true

# 執行 AOT 編譯的測試
./bin/Release/net9.0/publish/MyApp.Tests.exe
```

### IDE 整合

**Visual Studio 2022：**

- 版本需 17.13+
- 啟用 "Use testing platform server mode"

**VS Code：**

- 安裝 C# Dev Kit 擴充套件
- 啟用 "Use Testing Platform Protocol"

**JetBrains Rider：**

- 啟用 "Testing Platform support"
