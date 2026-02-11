# 常見任務映射表

> 此文件從 `SKILL.md` 提取，提供 7 個常見測試任務的技能組合推薦與實施步驟。

---

### 任務 1：從零建立測試專案

**情境**：全新專案，需要建立完整的測試基礎設施

**推薦技能組合**：
1. `dotnet-testing-xunit-project-setup` - 建立專案結構
2. `dotnet-testing-test-naming-conventions` - 設定命名規範
3. `dotnet-testing-unit-test-fundamentals` - 撰寫第一個測試

**實施步驟**：
1. 使用 xunit-project-setup 建立測試專案
2. 配置 .csproj 檔案與 NuGet 套件
3. 學習命名規範，制定團隊標準
4. 按照 3A Pattern 撰寫第一個測試

**提示詞範例**：
```
請使用 dotnet-testing-xunit-project-setup skill 為我的專案建立測試結構，
專案名稱是 MyProject，然後使用 dotnet-testing-unit-test-fundamentals skill
為 Calculator 類別建立第一個測試。
```

---

### 任務 2：為有依賴的服務類別寫測試

**情境**：UserService 依賴 IUserRepository 和 IEmailService

**推薦技能組合**：
1. `dotnet-testing-unit-test-fundamentals` - 測試結構
2. `dotnet-testing-nsubstitute-mocking` - 模擬依賴
3. `dotnet-testing-autofixture-basics` - 產生測試資料
4. `dotnet-testing-awesome-assertions-guide` - 清晰斷言

**實施步驟**：
1. 使用 NSubstitute 建立 Repository 和 EmailService 的 Mock
2. 用 AutoFixture 產生 User 測試資料
3. 按照 3A Pattern 撰寫測試
4. 使用 FluentAssertions 驗證結果

**提示詞範例**：
```
請使用 dotnet-testing-nsubstitute-mocking 和 dotnet-testing-autofixture-basics skills
為 UserService 建立測試。UserService 的建構函式需要 IUserRepository 和 IEmailService。
請測試 CreateUser 方法。
```

**預期程式碼結構**：
```csharp
[Fact]
public void CreateUser_ValidUser_ShouldSaveAndSendEmail()
{
    // Arrange - 使用 NSubstitute 建立 Mock
    var repository = Substitute.For<IUserRepository>();
    var emailService = Substitute.For<IEmailService>();

    // 使用 AutoFixture 產生測試資料
    var fixture = new Fixture();
    var user = fixture.Create<User>();

    var sut = new UserService(repository, emailService);

    // Act
    sut.CreateUser(user);

    // Assert - 使用 FluentAssertions
    repository.Received(1).Save(Arg.Is<User>(u => u.Email == user.Email));
    emailService.Received(1).SendWelcomeEmail(user.Email);
}
```

---

### 任務 3：測試有時間邏輯的程式碼

**情境**：OrderService 判斷訂單是否過期

**推薦技能組合**：
1. `dotnet-testing-datetime-testing-timeprovider` - TimeProvider 抽象化
2. `dotnet-testing-unit-test-fundamentals` - 測試基礎
3. `dotnet-testing-nsubstitute-mocking` - Mock TimeProvider

**實施步驟**：
1. 重構程式碼，注入 TimeProvider
2. 在測試中使用 FakeTimeProvider
3. 控制時間進行測試

**提示詞範例**：
```
請使用 dotnet-testing-datetime-testing-timeprovider skill 協助重構 OrderService，
使其可測試時間相關邏輯。訂單在建立 30 天後視為過期。
```

**預期程式碼結構**：
```csharp
// 重構前
public class OrderService
{
    public bool IsExpired(Order order)
    {
        return DateTime.Now > order.CreatedDate.AddDays(30);  // 無法測試
    }
}

// 重構後
public class OrderService
{
    private readonly TimeProvider _timeProvider;

    public OrderService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsExpired(Order order)
    {
        return _timeProvider.GetUtcNow() > order.CreatedDate.AddDays(30);
    }
}

// 測試
[Fact]
public void IsExpired_Order31DaysOld_ShouldReturnTrue()
{
    // Arrange
    var fakeTime = new FakeTimeProvider();
    fakeTime.SetUtcNow(new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero));

    var order = new Order
    {
        CreatedDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
    };

    var sut = new OrderService(fakeTime);

    // Act
    var result = sut.IsExpired(order);

    // Assert
    result.Should().BeTrue();
}
```

---

### 任務 4：改善測試可讀性

**情境**：現有測試難以理解，維護困難

**推薦技能組合**：
1. `dotnet-testing-test-naming-conventions` - 命名規範
2. `dotnet-testing-awesome-assertions-guide` - 流暢斷言
3. `dotnet-testing-test-data-builder-pattern` - 清晰的測試資料

**實施步驟**：
1. 重新命名測試方法，遵循三段式命名
2. 使用 FluentAssertions 改寫斷言
3. 使用 Builder Pattern 建立測試資料

**提示詞範例**：
```
請使用 dotnet-testing-awesome-assertions-guide 和 dotnet-testing-test-naming-conventions skills
檢視並改善這些測試的可讀性：
[貼上測試程式碼]
```

---

### 任務 5：產生大量測試資料

**情境**：效能測試需要 1000 筆 Customer 資料

**推薦技能組合**：
1. `dotnet-testing-autofixture-basics` - 自動產生
2. `dotnet-testing-bogus-fake-data` - 擬真資料（可選）
3. `dotnet-testing-autofixture-bogus-integration` - 結合兩者優勢（可選）

**實施步驟**：
1. 使用 AutoFixture.CreateMany<T>() 產生大量資料
2. （可選）使用 Bogus 讓資料更真實
3. （可選）整合兩者，自動化 + 擬真

**提示詞範例**：
```
請使用 dotnet-testing-autofixture-basics skill 為效能測試產生 1000 筆 Customer 資料，
每筆資料需要有 Name、Email、PhoneNumber 等欄位。
```

---

### 任務 6：測試 FluentValidation 驗證器

**情境**：CreateUserValidator 有多條驗證規則

**推薦技能**：
- `dotnet-testing-fluentvalidation-testing`

**實施步驟**：
1. 使用 FluentValidation.TestHelper
2. 測試每條驗證規則
3. 測試組合情境

**提示詞範例**：
```
請使用 dotnet-testing-fluentvalidation-testing skill 為 CreateUserValidator 建立測試。
驗證器的規則包括：Name 必填、Email 格式驗證、Age 必須大於 18。
```

---

### 任務 7：測試檔案上傳功能

**情境**：FileUploadService 需要儲存上傳的檔案

**推薦技能**：
- `dotnet-testing-filesystem-testing-abstractions`

**實施步驟**：
1. 重構程式碼，使用 IFileSystem
2. 在測試中使用 MockFileSystem
3. 驗證檔案操作

**提示詞範例**：
```
請使用 dotnet-testing-filesystem-testing-abstractions skill 協助測試 FileUploadService。
該服務會將上傳的檔案儲存到 uploads/ 目錄。
```
