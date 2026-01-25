# xUnit 與 TUnit 語法對照遷移指南

本文件提供 xUnit 到 TUnit 的語法對照，幫助開發團隊快速進行遷移評估與轉換。

---

## 核心差異總覽

| 功能           | xUnit                       | TUnit                          |
| -------------- | --------------------------- | ------------------------------ |
| **基本測試**   | `[Fact]`                    | `[Test]`                       |
| **參數化測試** | `[Theory]` + `[InlineData]` | `[Test]` + `[Arguments]`       |
| **測試方法**   | `void` 或 `Task`            | 必須是 `async Task`            |
| **斷言前綴**   | `Assert.Method()`           | `await Assert.That().Method()` |

---

## 測試屬性對照

### 基本測試

```csharp
// xUnit
[Fact]
public void TestMethod()
{
    Assert.True(true);
}

// TUnit
[Test]
public async Task TestMethod()
{
    await Assert.That(true).IsTrue();
}
```

### 參數化測試

```csharp
// xUnit
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
public void Add_多組輸入(int a, int b, int expected)
{
    Assert.Equal(expected, _calculator.Add(a, b));
}

// TUnit
[Test]
[Arguments(1, 2, 3)]
[Arguments(-1, 1, 0)]
public async Task Add_多組輸入(int a, int b, int expected)
{
    await Assert.That(_calculator.Add(a, b)).IsEqualTo(expected);
}
```

---

## 斷言語法對照

### 相等性斷言

```csharp
// xUnit
Assert.Equal(expected, actual);
Assert.NotEqual(unexpected, actual);

// TUnit
await Assert.That(actual).IsEqualTo(expected);
await Assert.That(actual).IsNotEqualTo(unexpected);
```

### 布林斷言

```csharp
// xUnit
Assert.True(condition);
Assert.False(condition);

// TUnit
await Assert.That(condition).IsTrue();
await Assert.That(condition).IsFalse();
```

### Null 檢查

```csharp
// xUnit
Assert.Null(value);
Assert.NotNull(value);

// TUnit
await Assert.That(value).IsNull();
await Assert.That(value).IsNotNull();
```

### 字串斷言

```csharp
// xUnit
Assert.Contains("substring", fullString);
Assert.StartsWith("prefix", fullString);
Assert.EndsWith("suffix", fullString);
Assert.Empty(emptyString);

// TUnit
await Assert.That(fullString).Contains("substring");
await Assert.That(fullString).StartsWith("prefix");
await Assert.That(fullString).EndsWith("suffix");
await Assert.That(emptyString).IsEmpty();
```

### 集合斷言

```csharp
// xUnit
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);

// TUnit
await Assert.That(collection).Contains(item);
await Assert.That(collection).DoesNotContain(item);
await Assert.That(collection).IsEmpty();
await Assert.That(collection).IsNotEmpty();
await Assert.That(collection).HasCount(1);
```

### 數值比較

```csharp
// xUnit
Assert.InRange(actual, low, high);
Assert.True(actual > compareValue);  // 沒有直接的 API

// TUnit
await Assert.That(actual).IsBetween(low, high);
await Assert.That(actual).IsGreaterThan(compareValue);
await Assert.That(actual).IsLessThan(compareValue);
```

### 例外斷言

```csharp
// xUnit
Assert.Throws<ArgumentException>(() => SomeMethod());
var ex = Assert.Throws<ArgumentException>(() => SomeMethod());
Assert.Equal("message", ex.Message);

// TUnit
await Assert.That(() => SomeMethod()).Throws<ArgumentException>();
await Assert.That(() => SomeMethod())
    .Throws<ArgumentException>()
    .WithMessage("message");
```

### 非同步例外斷言

```csharp
// xUnit
await Assert.ThrowsAsync<ArgumentException>(async () => await SomeAsyncMethod());

// TUnit
await Assert.That(async () => await SomeAsyncMethod())
    .Throws<ArgumentException>();
```

---

## 生命週期對照

### 類別層級設定/清理

```csharp
// xUnit
public class MyTests : IClassFixture<MyFixture>
{
    private readonly MyFixture _fixture;
    public MyTests(MyFixture fixture) => _fixture = fixture;
}

// TUnit
public class MyTests
{
    private static MyResource? _resource;

    [Before(Class)]
    public static async Task ClassSetup()
    {
        _resource = new MyResource();
        await _resource.InitializeAsync();
    }

    [After(Class)]
    public static async Task ClassTearDown()
    {
        await _resource?.DisposeAsync();
    }
}
```

### 測試層級設定/清理

```csharp
// xUnit（使用建構式和 Dispose）
public class MyTests : IDisposable
{
    public MyTests() { /* 設定 */ }
    public void Dispose() { /* 清理 */ }
}

// TUnit（可使用建構式/Dispose，或 Before/After）
public class MyTests
{
    [Before(Test)]
    public async Task Setup() { /* 設定 */ }

    [After(Test)]
    public async Task TearDown() { /* 清理 */ }
}
```

---

## 並行控制對照

### 停用並行

```csharp
// xUnit（使用 Collection）
[Collection("NonParallel")]
public class MyTests { }

// TUnit（使用 NotInParallel）
public class MyTests
{
    [Test]
    [NotInParallel("DatabaseTests")]
    public async Task DatabaseTest() { }
}
```

---

## 完整遷移範例

### xUnit 原始程式碼

```csharp
using Xunit;

public class EmailValidatorTests
{
    private readonly EmailValidator _validator;

    public EmailValidatorTests()
    {
        _validator = new EmailValidator();
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_各種輸入_應回傳正確驗證結果(string? email, bool expected)
    {
        var result = _validator.IsValidEmail(email);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetDomain_輸入無效Email_應拋出ArgumentException()
    {
        var invalidEmail = "invalid-email";
        Assert.Throws<ArgumentException>(() => _validator.GetDomain(invalidEmail));
    }

    [Fact]
    public void GetDomain_輸入有效Email_應回傳正確網域()
    {
        var email = "user@example.com";
        var domain = _validator.GetDomain(email);
        Assert.Equal("example.com", domain);
        Assert.Contains(".", domain);
    }
}
```

### TUnit 轉換後

```csharp
using TUnit.Core;
using TUnit.Assertions;

public class EmailValidatorTests
{
    private readonly EmailValidator _validator;

    public EmailValidatorTests()
    {
        _validator = new EmailValidator();
    }

    [Test]
    [Arguments("test@example.com", true)]
    [Arguments("invalid-email", false)]
    [Arguments("", false)]
    [Arguments(null, false)]
    public async Task IsValidEmail_各種輸入_應回傳正確驗證結果(string? email, bool expected)
    {
        var result = _validator.IsValidEmail(email);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task GetDomain_輸入無效Email_應拋出ArgumentException()
    {
        var invalidEmail = "invalid-email";
        await Assert.That(() => _validator.GetDomain(invalidEmail))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task GetDomain_輸入有效Email_應回傳正確網域()
    {
        var email = "user@example.com";
        var domain = _validator.GetDomain(email);
        await Assert.That(domain)
            .IsEqualTo("example.com")
            .And.Contains(".");
    }
}
```

---

## 遷移檢查清單

- [ ] 移除 `Microsoft.NET.Test.Sdk` 套件參考
- [ ] 安裝 `TUnit` 套件
- [ ] 更新 GlobalUsings 加入 TUnit 命名空間
- [ ] 將 `[Fact]` 改為 `[Test]`
- [ ] 將 `[Theory]` 改為 `[Test]`
- [ ] 將 `[InlineData]` 改為 `[Arguments]`
- [ ] 將測試方法改為 `async Task`
- [ ] 在所有斷言前加上 `await`
- [ ] 轉換斷言語法為流暢式風格
- [ ] 確認 IDE 版本支援 Microsoft.Testing.Platform
