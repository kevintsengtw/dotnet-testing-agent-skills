# 核心 Assertions 語法完整參考

> 本文件從 [SKILL.md](../SKILL.md) 提取，提供 AwesomeAssertions 各類斷言的完整程式碼範例。

---

## 1. 物件斷言（Object Assertions）

### 基本檢查

```csharp
[Fact]
public void Object_基本斷言_應正常運作()
{
    var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    
    // 空值檢查
    user.Should().NotBeNull();
    
    // 類型檢查
    user.Should().BeOfType<User>();
    user.Should().BeAssignableTo<IUser>();
    
    // 相等性檢查
    var anotherUser = new User { Id = 1, Name = "John", Email = "john@example.com" };
    user.Should().BeEquivalentTo(anotherUser);
}
```

### 屬性驗證

```csharp
[Fact]
public void Object_屬性驗證_應正常運作()
{
    var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    
    // 單一屬性驗證
    user.Id.Should().Be(1);
    user.Name.Should().Be("John");
    user.Email.Should().Contain("@");
    
    // 多屬性驗證
    user.Should().BeEquivalentTo(new 
    { 
        Id = 1, 
        Name = "John" 
    });
}
```

---

## 2. 字串斷言（String Assertions）

### 內容驗證

```csharp
[Fact]
public void String_內容驗證_應正常運作()
{
    var text = "Hello World";
    
    // 基本檢查
    text.Should().NotBeNullOrEmpty();
    text.Should().NotBeNullOrWhiteSpace();
    
    // 內容檢查
    text.Should().Contain("Hello");
    text.Should().StartWith("Hello");
    text.Should().EndWith("World");
    
    // 精確匹配
    text.Should().Be("Hello World");
    text.Should().BeEquivalentTo("hello world"); // 忽略大小寫
}
```

### 模式匹配

```csharp
[Fact]
public void String_模式匹配_應正常運作()
{
    var email = "user@example.com";
    
    // 正規表示式匹配
    email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    
    // 長度驗證
    email.Should().HaveLength(16);
    email.Should().HaveLengthGreaterThan(10);
    email.Should().HaveLengthLessThanOrEqualTo(50);
}
```

---

## 3. 數值斷言（Numeric Assertions）

### 範圍與比較

```csharp
[Fact]
public void Numeric_範圍檢查_應正常運作()
{
    var value = 10;
    
    // 比較運算
    value.Should().BeGreaterThan(5);
    value.Should().BeLessThan(15);
    value.Should().BeGreaterThanOrEqualTo(10);
    value.Should().BeLessThanOrEqualTo(10);
    
    // 範圍檢查
    value.Should().BeInRange(5, 15);
    value.Should().BeOneOf(8, 9, 10, 11);
}
```

### 浮點數處理

```csharp
[Fact]
public void Numeric_浮點數精度_應正常運作()
{
    var pi = 3.14159;
    
    // 精度比較
    pi.Should().BeApproximately(3.14, 0.01);
    
    // 特殊值檢查
    double.NaN.Should().Be(double.NaN);
    double.PositiveInfinity.Should().BePositiveInfinity();
    
    // 符號檢查
    pi.Should().BePositive();
    (-5.5).Should().BeNegative();
}
```

---

## 4. 集合斷言（Collection Assertions）

### 基本檢查

```csharp
[Fact]
public void Collection_基本驗證_應正常運作()
{
    var numbers = new[] { 1, 2, 3, 4, 5 };
    
    // 數量檢查
    numbers.Should().NotBeEmpty();
    numbers.Should().HaveCount(5);
    numbers.Should().HaveCountGreaterThan(3);
    
    // 內容檢查
    numbers.Should().Contain(3);
    numbers.Should().ContainSingle(x => x == 3);
    numbers.Should().NotContain(0);
    
    // 完整比對
    numbers.Should().Equal(1, 2, 3, 4, 5);
    numbers.Should().BeEquivalentTo(new[] { 5, 4, 3, 2, 1 }); // 忽略順序
}
```

### 順序與唯一性

```csharp
[Fact]
public void Collection_順序驗證_應正常運作()
{
    var numbers = new[] { 1, 2, 3, 4, 5 };
    
    // 順序檢查
    numbers.Should().BeInAscendingOrder();
    numbers.Should().BeInDescendingOrder();
    
    // 唯一性檢查
    numbers.Should().OnlyHaveUniqueItems();
    
    // 子集檢查
    numbers.Should().BeSubsetOf(new[] { 1, 2, 3, 4, 5, 6, 7 });
    numbers.Should().Contain(x => x > 3);
}
```

### 複雜物件集合

```csharp
[Fact]
public void Collection_複雜物件_應正常運作()
{
    var users = new[]
    {
        new User { Id = 1, Name = "John", Age = 30 },
        new User { Id = 2, Name = "Jane", Age = 25 },
        new User { Id = 3, Name = "Bob", Age = 35 }
    };
    
    // 條件過濾
    users.Should().Contain(u => u.Name == "John");
    users.Should().OnlyContain(u => u.Age >= 18);
    
    // 全部滿足
    users.Should().AllSatisfy(u => 
    {
        u.Id.Should().BeGreaterThan(0);
        u.Name.Should().NotBeNullOrEmpty();
    });
    
    // LINQ 整合
    users.Where(u => u.Age > 30).Should().HaveCount(1);
}
```

---

## 5. 例外斷言（Exception Assertions）

### 基本例外處理

```csharp
[Fact]
public void Exception_基本驗證_應正常運作()
{
    var service = new UserService();
    
    // 預期拋出例外
    Action act = () => service.GetUser(-1);
    
    act.Should().Throw<ArgumentException>()
       .WithMessage("*User ID*")
       .And.ParamName.Should().Be("userId");
}
```

### 不應拋出例外

```csharp
[Fact]
public void Exception_不應拋出_應正常運作()
{
    var calculator = new Calculator();
    
    // 不應拋出任何例外
    Action act = () => calculator.Add(1, 2);
    act.Should().NotThrow();
    
    // 不應拋出特定例外
    act.Should().NotThrow<DivideByZeroException>();
}
```

### 巢狀例外

```csharp
[Fact]
public void Exception_巢狀例外_應正常運作()
{
    var service = new DatabaseService();
    
    Action act = () => service.Connect("invalid");
    
    act.Should().Throw<DatabaseConnectionException>()
       .WithInnerException<ArgumentException>()
       .WithMessage("*connection string*");
}
```

---

## 6. 非同步斷言（Async Assertions）

### Task 完成驗證

```csharp
[Fact]
public async Task Async_任務完成_應正常運作()
{
    var service = new UserService();
    
    // 等待任務完成
    var task = service.GetUserAsync(1);
    await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));
    
    // 驗證結果
    task.Result.Should().NotBeNull();
    task.Result.Id.Should().Be(1);
}
```

### 非同步例外

```csharp
[Fact]
public async Task Async_例外處理_應正常運作()
{
    var service = new ApiService();
    
    Func<Task> act = async () => await service.CallInvalidEndpointAsync();
    
    await act.Should().ThrowAsync<HttpRequestException>()
             .WithMessage("*404*");
}
```
