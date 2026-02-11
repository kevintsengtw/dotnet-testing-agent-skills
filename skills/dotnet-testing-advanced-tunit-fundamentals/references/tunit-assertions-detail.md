# TUnit.Assertions 斷言系統

TUnit 採用流暢式（Fluent）斷言設計，所有斷言都是非同步的：

## 基本相等性斷言

```csharp
[Test]
public async Task 基本相等性斷言範例()
{
    var expected = 42;
    var actual = 40 + 2;
    
    await Assert.That(actual).IsEqualTo(expected);
    await Assert.That(actual).IsNotEqualTo(43);
    
    // Null 檢查
    string? nullValue = null;
    await Assert.That(nullValue).IsNull();
    await Assert.That("test").IsNotNull();
}
```

## 布林值斷言

```csharp
[Test]
public async Task 布林值斷言範例()
{
    var condition = 1 + 1 == 2;
    
    await Assert.That(condition).IsTrue();
    await Assert.That(1 + 1 == 3).IsFalse();
    
    var number = 10;
    await Assert.That(number > 5).IsTrue();
}
```

## 數值比較斷言

```csharp
[Test]
public async Task 數值比較斷言範例()
{
    var actual = 10;
    
    await Assert.That(actual).IsGreaterThan(5);
    await Assert.That(actual).IsGreaterThanOrEqualTo(10);
    await Assert.That(actual).IsLessThan(15);
    await Assert.That(actual).IsBetween(5, 15);
}

[Test]
[Arguments(3.14159, 3.14, 0.01)]
public async Task 浮點數精確度控制(double actual, double expected, double tolerance)
{
    await Assert.That(actual)
        .IsEqualTo(expected)
        .Within(tolerance);
}
```

## 字串斷言

```csharp
[Test]
public async Task 字串斷言範例()
{
    var email = "user@example.com";
    
    await Assert.That(email).Contains("@");
    await Assert.That(email).StartsWith("user");
    await Assert.That(email).EndsWith(".com");
    await Assert.That(email).DoesNotContain(" ");
    await Assert.That("").IsEmpty();
    await Assert.That(email).IsNotEmpty();
}
```

## 集合斷言

```csharp
[Test]
public async Task 集合斷言範例()
{
    var numbers = new List<int> { 1, 2, 3, 4, 5 };
    
    await Assert.That(numbers).HasCount(5);
    await Assert.That(numbers).IsNotEmpty();
    await Assert.That(numbers).Contains(3);
    await Assert.That(numbers).DoesNotContain(10);
    await Assert.That(numbers.First()).IsEqualTo(1);
    await Assert.That(numbers.Last()).IsEqualTo(5);
}
```

## 例外斷言

```csharp
[Test]
public async Task 例外斷言範例()
{
    var calculator = new Calculator();
    
    // 檢查特定例外類型
    await Assert.That(() => calculator.Divide(10, 0))
        .Throws<DivideByZeroException>();
    
    // 檢查例外訊息
    await Assert.That(() => calculator.Divide(10, 0))
        .Throws<DivideByZeroException>()
        .WithMessage("除數不能為零");
    
    // 檢查不拋出例外
    await Assert.That(() => calculator.Add(1, 2))
        .DoesNotThrow();
}
```

## And / Or 條件組合

```csharp
[Test]
public async Task 條件組合範例()
{
    var number = 10;
    
    // And：所有條件都必須成立
    await Assert.That(number)
        .IsGreaterThan(5)
        .And.IsLessThan(15)
        .And.IsEqualTo(10);
    
    // Or：任一條件成立即可
    await Assert.That(number)
        .IsEqualTo(5)
        .Or.IsEqualTo(10)
        .Or.IsEqualTo(15);
}
```
