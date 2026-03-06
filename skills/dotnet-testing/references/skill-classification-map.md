# 技能分類地圖

> 此文件從 `SKILL.md` 提取，提供 19 個基礎技能的完整分類說明。

---

### 1. 測試基礎（3 個技能）- 必學基礎

| 技能名稱 | 核心價值 | 適合新手 | 何時使用 |
|---------|---------|---------|---------|
| `dotnet-testing-unit-test-fundamentals` | 理解 FIRST 原則、3A Pattern、測試金字塔等基礎概念 | ⭐⭐⭐ | 所有測試的起點，建立正確的測試觀念 |
| `dotnet-testing-test-naming-conventions` | 學習三段式命名法、中文命名建議 | ⭐⭐⭐ | 提升測試可讀性與可維護性 |
| `dotnet-testing-xunit-project-setup` | 建立標準的 xUnit 測試專案結構 | ⭐⭐⭐ | 建立新測試專案時 |

**學習順序建議**：fundamentals → naming-conventions → xunit-project-setup

**核心收穫**：
- **FIRST 原則**：Fast、Independent、Repeatable、Self-Validating、Timely
- **3A Pattern**：Arrange-Act-Assert 結構化測試
- **命名規範**：`[被測方法]_[測試情境]_[預期行為]`
- **專案結構**：tests/ 目錄、.csproj 設定、NuGet 套件管理

---

### 2. 測試資料生成（5 個技能）- 提升效率

| 技能名稱 | 特點 | 優勢 | 何時使用 |
|---------|------|------|---------|
| `dotnet-testing-autofixture-basics` | 自動產生匿名測試資料 | 減少樣板程式碼、快速建立測試物件 | 需要大量測試資料、測試資料內容不重要時 |
| `dotnet-testing-autofixture-customization` | 客製化 AutoFixture 行為 | 控制資料生成規則、符合業務邏輯 | 需要特定規則的測試資料 |
| `dotnet-testing-bogus-fake-data` | 產生擬真假資料（姓名、地址、Email 等） | 資料看起來真實、易於除錯 | 需要真實感的測試資料、展示用途 |
| `dotnet-testing-test-data-builder-pattern` | 使用 Builder Pattern 建立測試資料 | 語意清晰、表達測試意圖 | 需要高可讀性、複雜物件建構 |
| `dotnet-testing-autofixture-bogus-integration` | 結合 AutoFixture 與 Bogus | 兩者優勢互補 | 同時需要自動化和擬真資料 |

**選擇指南**：

```
需要大量資料 + 不在乎真實感？
  → autofixture-basics

需要看起來真實的資料？
  → bogus-fake-data

需要高度可讀性和語意清晰？
  → test-data-builder-pattern

需要靈活控制生成規則？
  → autofixture-basics + autofixture-customization

需要自動化 + 擬真？
  → autofixture-basics + autofixture-bogus-integration
```

**學習路徑**：
- 入門：autofixture-basics 或 bogus-fake-data
- 進階：autofixture-customization + test-data-builder-pattern
- 整合：autofixture-bogus-integration

---

### 3. 測試替身（2 個技能）- 處理依賴

| 技能名稱 | 用途 | 涵蓋範圍 | 何時使用 |
|---------|------|---------|---------|
| `dotnet-testing-nsubstitute-mocking` | NSubstitute Mock 框架 | Mock、Stub、Spy、驗證呼叫 | 有外部依賴需要模擬 |
| `dotnet-testing-autofixture-nsubstitute-integration` | AutoFixture + NSubstitute 整合 | 自動建立 Mock 物件 | 使用 AutoFixture 且有大量依賴 |

**核心概念**：
- **Mock**：模擬物件並驗證互動
- **Stub**：提供預設回應
- **Spy**：記錄呼叫並驗證

**學習順序建議**：
1. 先學 nsubstitute-mocking（理解 Mock 基礎）
2. 如果已使用 AutoFixture，再學 integration（提升效率）

**常見用途**：
- 模擬資料庫存取層（Repository）
- 模擬外部 API 呼叫
- 模擬第三方服務
- 驗證方法是否被正確呼叫

---

### 4. 斷言驗證（3 個技能）- 寫出清晰測試

| 技能名稱 | 特色 | 提升幅度 | 何時使用 |
|---------|------|---------|---------|
| `dotnet-testing-awesome-assertions-guide` | AwesomeAssertions 流暢斷言 | ⭐⭐⭐ 高 | 所有測試（強烈推薦） |
| `dotnet-testing-complex-object-comparison` | 深層物件比對技巧 | ⭐⭐⭐ 高 | DTO、Entity、複雜物件驗證 |
| `dotnet-testing-fluentvalidation-testing` | 測試 FluentValidation 驗證器 | ⭐⭐ 中 | 使用 FluentValidation 的專案 |

**AwesomeAssertions 範例對比**：

```csharp
// 傳統斷言
Assert.Equal(expected.Name, actual.Name);
Assert.Equal(expected.Age, actual.Age);
Assert.True(actual.IsActive);

// AwesomeAssertions（更易讀）
actual.Should().BeEquivalentTo(expected, options => options
    .Including(x => x.Name)
    .Including(x => x.Age));
actual.IsActive.Should().BeTrue();
```

**學習路徑**：
1. awesome-assertions-guide（所有專案必學）
2. complex-object-comparison（處理複雜比對）
3. fluentvalidation-testing（特定需求）

---

### 5. 特殊場景（3 個技能）- 解決棘手問題

| 技能名稱 | 解決問題 | 實務價值 | 學習難度 |
|---------|---------|---------|---------|
| `dotnet-testing-datetime-testing-timeprovider` | 時間相關測試（使用 .NET 8+ TimeProvider） | ⭐⭐⭐ 高 | 中等 |
| `dotnet-testing-filesystem-testing-abstractions` | 檔案系統抽象化測試 | ⭐⭐⭐ 高 | 中等 |
| `dotnet-testing-private-internal-testing` | 測試私有/內部成員 | ⭐⭐ 中 | 簡單 |

**時間測試常見問題**：
```csharp
// 問題：難以測試
public bool IsExpired()
{
    return DateTime.Now > ExpiryDate;  // DateTime.Now 無法控制
}

// 解決：使用 TimeProvider
public bool IsExpired(TimeProvider timeProvider)
{
    return timeProvider.GetUtcNow() > ExpiryDate;  // 可在測試中注入假時間
}
```

**檔案系統測試常見問題**：
```csharp
// 問題：難以測試
public void SaveToFile(string content)
{
    File.WriteAllText("data.txt", content);  // 真實檔案操作
}

// 解決：使用 IFileSystem 抽象
public void SaveToFile(string content, IFileSystem fileSystem)
{
    fileSystem.File.WriteAllText("data.txt", content);  // 可在測試中使用記憶體檔案系統
}
```

**何時需要這些技能**：
- **TimeProvider**：排程系統、有效期檢查、時間計算
- **FileSystem**：檔案上傳、報表產生、設定檔讀寫
- **Private Testing**：重構遺留程式碼（但應優先考慮重構設計）

---

### 6. 測試度量（1 個技能）- 品質監控

| 技能名稱 | 用途 | 工具 | 何時使用 |
|---------|------|------|---------|
| `dotnet-testing-code-coverage-analysis` | 程式碼覆蓋率分析與報告 | Coverlet、ReportGenerator | 評估測試完整性、CI/CD 整合 |

**涵蓋內容**：
- 使用 Coverlet 收集覆蓋率資料
- 產生 HTML 報告
- 設定覆蓋率門檻
- CI/CD 整合

**重要提醒**：
- 高覆蓋率 ≠ 高品質測試
- 目標是有意義的測試，而非追求 100% 覆蓋率
- 覆蓋率是參考指標，不是絕對標準

---

### 7. 框架整合（2 個技能）- 進階整合

| 技能名稱 | 整合對象 | 價值 | 何時使用 |
|---------|---------|------|---------|
| `dotnet-testing-autodata-xunit-integration` | AutoFixture + xUnit Theory | 簡化參數化測試 | 使用 xUnit Theory 且需要測試資料 |
| `dotnet-testing-test-output-logging` | ITestOutputHelper + ILogger | 測試輸出與除錯 | 需要查看測試執行過程、除錯複雜測試 |

**AutoData 範例**：

```csharp
// 傳統寫法
[Theory]
[InlineData("user1", "pass1")]
[InlineData("user2", "pass2")]
public void Login_ValidCredentials_Success(string username, string password)
{
    // ...
}

// 使用 AutoData
[Theory, AutoData]
public void Login_ValidCredentials_Success(string username, string password)
{
    // username 和 password 自動產生
}
```

**Test Output 範例**：

```csharp
public class MyTests
{
    private readonly ITestOutputHelper _output;

    public MyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Test()
    {
        _output.WriteLine("Debug information");  // 在測試輸出中顯示
    }
}
```
