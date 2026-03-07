---
name: dotnet-testing-nsubstitute-mocking
description: |
  使用 NSubstitute 建立測試替身（Mock、Stub、Spy）的專門技能。當需要隔離外部依賴、模擬介面行為、驗證方法呼叫時使用。涵蓋 Substitute.For、Returns、Received、Throws 等完整指引。
  Make sure to use this skill whenever the user mentions mock, stub, spy, NSubstitute, test double, 測試替身, Substitute.For, Returns, Received, or dependency isolation, even if they don't explicitly ask for mocking guidance.
  Keywords: mock, stub, spy, nsubstitute, 模擬, test double, 測試替身, IRepository, IService, Substitute.For, Returns, Received, Throws, Arg.Any, Arg.Is, 隔離依賴, 模擬外部服務, dependency injection testing
---

# NSubstitute 測試替身指南

## 為什麼需要測試替身？

真實世界的程式碼通常依賴外部資源，這些依賴會讓測試變得：

1. **緩慢** - 需要實際操作資料庫、檔案系統、網路
2. **不穩定** - 外部服務異常導致測試失敗
3. **難以重複** - 時間、隨機數導致結果不一致
4. **環境依賴** - 需要特定的外部環境設定
5. **開發阻塞** - 必須等待外部系統準備就緒

測試替身（Test Double）讓我們能夠隔離這些依賴，專注測試業務邏輯。

## 前置需求

### 套件安裝

```xml
<PackageReference Include="NSubstitute" Version="5.3.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="AwesomeAssertions" Version="9.1.0" />
```

### 基本 using 指令

```csharp
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
```

## Test Double 五大類型

根據 Gerard Meszaros 在《xUnit Test Patterns》中的定義，測試替身分為五種類型：

| 類型 | 用途 | NSubstitute 對應 |
|------|------|-------------------|
| **Dummy** | 填充物件，僅滿足方法簽章 | `Substitute.For<T>()` 不設定任何行為 |
| **Stub** | 提供預設回傳值，設定測試情境 | `.Returns(value)` |
| **Fake** | 簡化實作，有真實邏輯 | 手動實作介面（如 `FakeUserRepository`） |
| **Spy** | 記錄呼叫，事後驗證 | `.Received()` 驗證 |
| **Mock** | 預設期望互動，未滿足則測試失敗 | `.Received(n)` 嚴格驗證 |

> 各類型的完整程式碼範例請參閱 [references/test-double-types.md](references/test-double-types.md)

## NSubstitute 核心功能

### 基本替代語法

```csharp
// 建立介面替代
var substitute = Substitute.For<IUserRepository>();

// 建立類別替代（需要虛擬成員）
var classSubstitute = Substitute.For<BaseService>();

// 建立多重介面替代
var multiSubstitute = Substitute.For<IService, IDisposable>();
```

### 回傳值設定

#### 基本回傳值

```csharp
// 精確參數匹配
_repository.GetById(1).Returns(new User { Id = 1, Name = "John" });

// 任意參數匹配
_service.Process(Arg.Any<string>()).Returns("processed");

// 回傳序列值
_generator.GetNext().Returns(1, 2, 3, 4, 5);
```

#### 條件回傳值

```csharp
// 使用委派計算回傳值
_calculator.Add(Arg.Any<int>(), Arg.Any<int>())
           .Returns(x => (int)x[0] + (int)x[1]);

// 條件匹配
_service.Process(Arg.Is<string>(x => x.StartsWith("test")))
        .Returns("test-result");
```

#### 拋出例外

```csharp
// 同步方法拋出例外
_service.RiskyOperation()
        .Throws(new InvalidOperationException("Something went wrong"));

// 非同步方法拋出例外
_service.RiskyOperationAsync()
        .Throws(new InvalidOperationException("Async operation failed"));
```

### 引數匹配器

```csharp
// 任意值
_service.Process(Arg.Any<string>()).Returns("result");

// 特定條件
_service.Process(Arg.Is<string>(x => x.Length > 5)).Returns("long-result");

// 引數擷取
string capturedArg = null;
_service.Process(Arg.Do<string>(x => capturedArg = x)).Returns("result");
_service.Process("test");
capturedArg.Should().Be("test");

// 引數檢查
_service.Process(Arg.Is<string>(x =>
{
    x.Should().StartWith("prefix");
    return true;
})).Returns("result");
```

### 呼叫驗證

```csharp
// 驗證被呼叫（至少一次）
_service.Received().Process("test");

// 驗證呼叫次數
_service.Received(2).Process(Arg.Any<string>());

// 驗證未被呼叫
_service.DidNotReceive().Delete(Arg.Any<int>());

// 驗證任意參數呼叫
_service.ReceivedWithAnyArgs().Process(default);

// 驗證呼叫順序
Received.InOrder(() =>
{
    _service.Start();
    _service.Process();
    _service.Stop();
});
```

## 實戰模式

涵蓋五種常見的 NSubstitute 實戰模式，包含完整程式碼範例：

| 模式 | 說明 |
|------|------|
| 模式 1：依賴注入與測試設定 | FileBackupService 完整範例，含建構式注入與 SUT 設定 |
| 模式 2：Mock vs Stub 差異 | Stub 關注狀態回傳值 vs Mock 關注互動行為驗證 |
| 模式 3：非同步方法測試 | `Returns(Task.FromResult(...))` 與 `.Throws()` 模式 |
| 模式 4：ILogger 驗證 | 驗證底層 `Log` 方法繞過擴展方法限制 |
| 模式 5：複雜設定管理 | 基底測試類別管理共用 Substitute 設定 |

> 完整程式碼範例請參閱 [references/practical-patterns.md](references/practical-patterns.md)

## 引數匹配進階技巧

### 複雜物件匹配

```csharp
[Fact]
public void CreateOrder_建立訂單_應儲存正確的訂單資料()
{
    var repository = Substitute.For<IOrderRepository>();
    var service = new OrderService(repository);

    service.CreateOrder("Product A", 5, 100);

    // 驗證物件屬性
    repository.Received(1).Save(Arg.Is<Order>(o =>
        o.ProductName == "Product A" &&
        o.Quantity == 5 &&
        o.Price == 100));
}
```

### 引數擷取與驗證

```csharp
[Fact]
public void RegisterUser_註冊使用者_應產生正確的雜湊密碼()
{
    var repository = Substitute.For<IUserRepository>();
    var service = new UserService(repository);

    User capturedUser = null;
    repository.Save(Arg.Do<User>(u => capturedUser = u));

    service.RegisterUser("john@example.com", "password123");

    capturedUser.Should().NotBeNull();
    capturedUser.Email.Should().Be("john@example.com");
    capturedUser.PasswordHash.Should().NotBe("password123"); // 應該被雜湊
    capturedUser.PasswordHash.Length.Should().BeGreaterThan(20);
}
```

## 常見陷阱與最佳實踐

### 推薦做法

1. **針對介面而非實作建立 Substitute**

    ```csharp
    // 正確：針對介面
    var repository = Substitute.For<IUserRepository>();

    // 錯誤：針對具體類別（除非有虛擬成員）
    var repository = Substitute.For<UserRepository>();
    ```

2. **使用有意義的測試資料**

    ```csharp
    // 正確：清楚表達意圖
    var user = new User { Id = 123, Name = "John Doe", Email = "john@example.com" };

    // 錯誤：無意義的資料
    var user = new User { Id = 1, Name = "test", Email = "a@b.c" };
    ```

3. **避免過度驗證**

    ```csharp
    // 正確：只驗證重要的行為
    _emailService.Received(1).SendWelcomeEmail(Arg.Any<string>());

    // 錯誤：驗證所有內部實作細節
    _repository.Received(1).GetById(123);
    _repository.Received(1).Update(Arg.Any<User>());
    _validator.Received(1).Validate(Arg.Any<User>());
    ```

4. **Mock 與 Stub 的明確區分**

    ```csharp
    // 正確：Stub 用於設定情境，Mock 用於驗證行為
    var stubRepository = Substitute.For<IUserRepository>(); // Stub
    var mockLogger = Substitute.For<ILogger>(); // Mock

    stubRepository.GetById(123).Returns(user);
    service.ProcessUser(123);
    mockLogger.Received(1).LogInformation(Arg.Any<string>());
    ```

### 避免做法

1. **避免模擬值類型**

    ```csharp
    // 錯誤：DateTime 是值類型
    var badDate = Substitute.For<DateTime>();

    // 正確：抽象時間提供者
    var dateTimeProvider = Substitute.For<IDateTimeProvider>();
    dateTimeProvider.Now.Returns(new DateTime(2024, 1, 1));
    ```

2. **避免測試與實作強耦合**

    ```csharp
    // 錯誤：測試實作細節
    _repository.Received(1).Query(Arg.Any<string>());
    _repository.Received(1).Filter(Arg.Any<Expression<Func<User, bool>>>());

    // 正確：測試行為結果
    var users = service.GetActiveUsers();
    users.Should().HaveCount(2);
    ```

3. **避免設定過於複雜**

    ```csharp
    // 錯誤：過多的 Substitute（可能違反 SRP）
    var sub1 = Substitute.For<IService1>();
    var sub2 = Substitute.For<IService2>();
    var sub3 = Substitute.For<IService3>();
    var sub4 = Substitute.For<IService4>();

    // 正確：重新思考類別職責
    // 考慮是否違反單一職責原則，需要重構
    ```

## 識別需要替代的相依性

### 應該替代的

- 外部 API 呼叫（IHttpClient、IApiClient）
- 資料庫操作（IRepository、IDbContext）
- 檔案系統操作（IFileSystem）
- 網路通訊（IEmailService、IMessageQueue）
- 時間依賴（IDateTimeProvider、TimeProvider）
- 隨機數產生（IRandom）
- 昂貴的計算（IComplexCalculator）
- 記錄服務（ILogger<T>）

### 不應該替代的

- 值物件（DateTime、string、int）
- 簡單的資料傳輸物件（DTO）
- 純函數工具（如 AutoMapper 的 IMapper，考慮使用真實實例）
- 框架核心類別（除非有明確需求）

## 疑難排解

### Q1: 如何測試沒有介面的類別？

**A:** 確保要模擬的成員是 virtual：

```csharp
public class BaseService
{
    public virtual string GetData() => "real data";
}

var substitute = Substitute.For<BaseService>();
substitute.GetData().Returns("test data");
```

### Q2: 如何驗證方法被呼叫的順序？

**A:** 使用 Received.InOrder()：

```csharp
Received.InOrder(() =>
{
    _service.Start();
    _service.Process();
    _service.Stop();
});
```

### Q3: 如何處理 out 參數？

**A:** 使用 Returns() 配合委派：

```csharp
_service.TryGetValue("key", out Arg.Any<string>())
        .Returns(x =>
        {
            x[1] = "value";
            return true;
        });
```

### Q4: NSubstitute 與 Moq 該如何選擇？

**A:** NSubstitute 優勢：

- 語法更簡潔直觀
- 學習曲線平緩
- 沒有隱私爭議
- 對多數測試場景足夠

選擇 NSubstitute，除非：

- 專案已使用 Moq
- 需要 Moq 特有的進階功能
- 團隊已熟悉 Moq 語法

## 與其他技能整合

此技能可與以下技能組合使用：

- **unit-test-fundamentals**: 單元測試基礎與 3A 模式
- **dependency-injection-testing**: 依賴注入測試策略
- **test-naming-conventions**: 測試命名規範
- **test-output-logging**: ITestOutputHelper 與 ILogger 整合
- **datetime-testing-timeprovider**: TimeProvider 抽象化時間依賴
- **filesystem-testing-abstractions**: 檔案系統依賴抽象化

## 範本檔案參考

本技能提供以下範本檔案：

- `templates/mock-patterns.cs`: 完整的 Mock/Stub/Spy 模式範例
- `templates/verification-examples.cs`: 行為驗證與引數匹配範例
- `references/practical-patterns.md`: 五種實戰模式完整程式碼
- `references/test-double-types.md`: Test Double 五大類型詳細範例

## 輸出格式

- 產生使用 NSubstitute 的 xUnit 測試類別檔案（`*Tests.cs`）
- 測試方法遵循 3A（Arrange-Act-Assert）模式
- Substitute 設定集中於測試建構函式或 Arrange 區段
- 驗證呼叫使用 `Received()` / `DidNotReceive()` 明確斷言
- 搭配 AwesomeAssertions 進行狀態驗證

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 07 - 依賴替代入門：使用 NSubstitute**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10374593
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day07

### NSubstitute 官方

- [NSubstitute 官方網站](https://nsubstitute.github.io/)
- [NSubstitute GitHub](https://github.com/nsubstitute/NSubstitute)
- [NSubstitute NuGet](https://www.nuget.org/packages/NSubstitute/)

### Test Double 理論

- [XUnit Test Patterns](http://xunitpatterns.com/Test%20Double.html)
- [Martin Fowler - Test Double](https://martinfowler.com/bliki/TestDouble.html)
