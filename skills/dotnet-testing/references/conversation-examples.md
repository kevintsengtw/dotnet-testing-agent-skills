# 引導對話範例

> 此文件從 `SKILL.md` 提取，展示 AI 如何與使用者互動並推薦正確的技能組合。

以下範例展示 AI 如何與您互動，幫助您選擇正確的技能。

---

### 範例 1：新手入門

**您**：我想學習 .NET 測試

**AI**：我很樂意協助您學習 .NET 測試！為了提供最適合的指引，請告訴我：

1. **您的測試經驗**：
   - [ ] 完全沒寫過測試
   - [ ] 寫過一些基礎測試
   - [ ] 有一定經驗，想深入學習

2. **您的專案情況**：
   - [ ] 新專案，從零開始
   - [ ] 現有專案，想加入測試
   - [ ] 改善現有測試品質

請告訴我您的情況，我會推薦適合的學習路徑。

**您**：完全沒寫過測試，是新專案

**AI**：太好了！我會為您規劃一個循序漸進的學習路徑。建議您從以下三個技能開始：

**第一步：理解測試基礎（1-2 天）**
使用 `dotnet-testing-unit-test-fundamentals` skill
學習重點：FIRST 原則、3A Pattern、基本測試結構

**第二步：學習命名規範（半天）**
使用 `dotnet-testing-test-naming-conventions` skill
學習重點：三段式命名法、提升可讀性

**第三步：建立測試專案（1 天）**
使用 `dotnet-testing-xunit-project-setup` skill
學習重點：專案結構、套件設定

需要我使用這些 skills 協助您開始嗎？

---

### 範例 2：處理依賴

**您**：我需要為 UserService 寫測試，但它依賴資料庫和外部 API

**AI**：了解！您的情況需要模擬（Mock）外部依賴。我推薦以下技能組合：

**必要技能**：
1. `dotnet-testing-nsubstitute-mocking` - 模擬資料庫與 API 依賴
   用途：建立 IUserRepository 和 IApiClient 的 Mock 物件

**建議搭配**：
2. `dotnet-testing-autofixture-basics` - 自動產生測試資料
   用途：快速產生 User 測試物件

3. `dotnet-testing-awesome-assertions-guide` - 清晰的斷言
   用途：讓測試更易讀

**實施建議**：
1. 先使用 NSubstitute 為依賴建立 Mock
2. 用 AutoFixture 產生 User 測試資料
3. 按照 3A Pattern 撰寫測試
4. 使用 FluentAssertions 驗證結果

需要我使用這些 skills 協助您建立測試嗎？

---

### 範例 3：特定問題

**您**：我的程式碼用 DateTime.Now，要怎麼測試？

**AI**：這是時間相關測試的經典問題！我推薦使用 `dotnet-testing-datetime-testing-timeprovider` skill。

**問題分析**：
`DateTime.Now` 每次執行都不同，無法寫出可重複的測試（違反 FIRST 原則中的 Repeatable）。

**解決方案**：
1. 重構程式碼，注入 `TimeProvider`
2. 在測試中使用 `FakeTimeProvider` 控制時間
3. 這樣就能測試特定時間點的行為

**範例**：
```csharp
// 重構前（難以測試）
public bool IsExpired()
{
    return DateTime.Now > ExpiryDate;
}

// 重構後（可測試）
public bool IsExpired(TimeProvider timeProvider)
{
    return timeProvider.GetUtcNow() > ExpiryDate;
}
```

需要我使用 `dotnet-testing-datetime-testing-timeprovider` skill 協助您重構並建立測試嗎？

---

### 範例 4：改善現有測試

**您**：我的測試很難讀，維護困難

**AI**：測試可讀性問題很常見！我建議從以下三個方向改善：

**1. 命名規範**
使用 `dotnet-testing-test-naming-conventions` skill
- 採用三段式命名：`[方法]_[情境]_[預期]`
- 讓測試名稱自我說明

**2. 流暢斷言**
使用 `dotnet-testing-awesome-assertions-guide` skill
- 使用 FluentAssertions 取代傳統斷言
- 大幅提升可讀性

**3. 清晰的測試資料**
使用 `dotnet-testing-test-data-builder-pattern` skill
- 使用 Builder Pattern 建立測試資料
- 讓測試意圖更明確

**快速範例**：
```csharp
// 改善前
[Fact]
public void Test1()
{
    var u = new User { Name = "test", Age = 20 };
    var r = svc.Validate(u);
    Assert.True(r);
}

// 改善後
[Fact]
public void Validate_AdultUser_ShouldReturnTrue()
{
    // Arrange
    var user = new UserBuilder()
        .WithName("test")
        .WithAge(20)
        .Build();

    // Act
    var result = _service.Validate(user);

    // Assert
    result.Should().BeTrue();
}
```

要不要我協助您改善特定的測試？請貼上您的測試程式碼。
