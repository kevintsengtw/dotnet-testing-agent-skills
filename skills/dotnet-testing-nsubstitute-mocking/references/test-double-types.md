# Test Double 五大類型

> 本文件從 [SKILL.md](../SKILL.md) 提取，包含完整的 Test Double 分類說明與程式碼範例。

根據 Gerard Meszaros 在《xUnit Test Patterns》中的定義：

## 1. Dummy - 填充物件

僅用於滿足方法簽章，不會被實際使用。

```csharp
public interface IEmailService
{
    void SendEmail(string to, string subject, string body, ILogger logger);
}

[Fact]
public void ProcessOrder_不使用Logger_應成功處理訂單()
{
    // Dummy：只是為了滿足參數要求
    var dummyLogger = Substitute.For<ILogger>();
    
    var service = new OrderService();
    var result = service.ProcessOrder(order, dummyLogger);
    
    result.Success.Should().BeTrue();
    // 不關心 logger 是否被調用
}
```

## 2. Stub - 預設回傳值

提供預先定義的回傳值，用於測試特定情境。

```csharp
[Fact]
public void GetUser_有效的使用者ID_應回傳使用者資料()
{
    // Arrange - Stub：預設回傳值
    var stubRepository = Substitute.For<IUserRepository>();
    stubRepository.GetById(123).Returns(new User { Id = 123, Name = "John" });
    
    var service = new UserService(stubRepository);
    
    // Act
    var actual = service.GetUser(123);
    
    // Assert
    actual.Name.Should().Be("John");
    // 不關心 GetById 被呼叫了幾次
}
```

## 3. Fake - 簡化實作

有實際功能但簡化的實作，通常用於整合測試。

```csharp
public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    
    public User GetById(int id) => _users.TryGetValue(id, out var user) ? user : null;
    public void Save(User user) => _users[user.Id] = user;
    public void Delete(int id) => _users.Remove(id);
}

[Fact]
public void CreateUser_建立使用者_應儲存並可查詢()
{
    // Fake：有真實邏輯的簡化實作
    var fakeRepository = new FakeUserRepository();
    var service = new UserService(fakeRepository);
    
    service.CreateUser(new User { Id = 1, Name = "John" });
    var actual = service.GetUser(1);
    
    actual.Name.Should().Be("John");
}
```

## 4. Spy - 記錄呼叫

記錄被如何呼叫，可以事後驗證。

```csharp
[Fact]
public void CreateUser_建立使用者_應記錄建立資訊()
{
    // Arrange
    var spyLogger = Substitute.For<ILogger<UserService>>();
    var repository = Substitute.For<IUserRepository>();
    var service = new UserService(repository, spyLogger);
    
    // Act
    service.CreateUser(new User { Name = "John" });
    
    // Assert - Spy：驗證呼叫記錄
    spyLogger.Received(1).LogInformation("User created: {Name}", "John");
}
```

## 5. Mock - 行為驗證

預設期望的互動行為，測試失敗如果期望沒有滿足。

```csharp
[Fact]
public void RegisterUser_註冊使用者_應發送歡迎郵件()
{
    // Arrange
    var mockEmailService = Substitute.For<IEmailService>();
    var repository = Substitute.For<IUserRepository>();
    var service = new UserService(repository, mockEmailService);
    
    // Act
    service.RegisterUser("john@example.com", "John");
    
    // Assert - Mock：驗證特定的互動行為
    mockEmailService.Received(1).SendWelcomeEmail("john@example.com", "John");
}
```
