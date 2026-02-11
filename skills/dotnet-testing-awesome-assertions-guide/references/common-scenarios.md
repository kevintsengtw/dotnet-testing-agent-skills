# 常見情境與解決方案

> 本文件從 [SKILL.md](../SKILL.md) 提取，提供各種常見測試情境的斷言範例。

---

## 情境 1：API 回應驗證

```csharp
[Fact]
public void API_使用者資料_應符合規格()
{
    var response = apiClient.GetUserProfile(userId);
    
    response.StatusCode.Should().Be(200);
    response.Content.Should().NotBeNullOrEmpty();
    
    var user = JsonSerializer.Deserialize<User>(response.Content);
    
    user.Should().BeEquivalentTo(new
    {
        Id = userId,
        Email = expectedEmail
    }, options => options
        .Including(u => u.Id)
        .Including(u => u.Email)
    );
}
```

## 情境 2：資料庫實體驗證

```csharp
[Fact]
public void Database_儲存實體_應正確持久化()
{
    var user = new User 
    { 
        Name = "John", 
        Email = "john@example.com" 
    };
    
    dbContext.Users.Add(user);
    dbContext.SaveChanges();
    
    var saved = dbContext.Users.Find(user.Id);
    
    saved.Should().BeEquivalentTo(user, options => options
        .Excluding(u => u.CreatedAt)
        .Excluding(u => u.UpdatedAt)
        .Excluding(u => u.RowVersion)
    );
}
```

## 情境 3：事件驗證

```csharp
[Fact]
public void Event_發佈事件_應包含正確資料()
{
    var eventRaised = false;
    OrderCreatedEvent? capturedEvent = null;
    
    eventBus.Subscribe<OrderCreatedEvent>(e => 
    {
        eventRaised = true;
        capturedEvent = e;
    });
    
    orderService.CreateOrder(orderRequest);
    
    eventRaised.Should().BeTrue("Order creation should raise event");
    capturedEvent.Should().NotBeNull();
    capturedEvent!.OrderId.Should().BeGreaterThan(0);
    capturedEvent.TotalAmount.Should().Be(expectedAmount);
}
```
