# 自訂 ISpecimenBuilder 完整範例

## RandomRangedDateTimeBuilder：精確控制 DateTime 屬性

`RandomDateTimeSequenceGenerator` 會影響**所有** DateTime 屬性。若需控制特定屬性，需自訂建構器：

```csharp
using AutoFixture.Kernel;
using System.Reflection;

public class RandomRangedDateTimeBuilder : ISpecimenBuilder
{
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    private readonly HashSet<string> _targetProperties;

    public RandomRangedDateTimeBuilder(
        DateTime minDate,
        DateTime maxDate,
        params string[] targetProperties)
    {
        _minDate = minDate;
        _maxDate = maxDate;
        _targetProperties = new HashSet<string>(targetProperties);
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo &&
            propertyInfo.PropertyType == typeof(DateTime) &&
            _targetProperties.Contains(propertyInfo.Name))
        {
            var range = _maxDate - _minDate;
            var randomTicks = (long)(Random.Shared.NextDouble() * range.Ticks);
            return _minDate.AddTicks(randomTicks);
        }

        return new NoSpecimen();
    }
}
```

### 使用範例

```csharp
[Fact]
public void 只控制特定DateTime屬性()
{
    var fixture = new Fixture();

    var minDate = new DateTime(2025, 1, 1);
    var maxDate = new DateTime(2025, 12, 31);

    // 只控制 UpdateTime 屬性
    fixture.Customizations.Add(
        new RandomRangedDateTimeBuilder(minDate, maxDate, "UpdateTime"));

    var member = fixture.Create<Member>();

    // UpdateTime 在指定範圍
    member.UpdateTime.Should().BeOnOrAfter(minDate).And.BeOnOrBefore(maxDate);

    // CreateTime 不受影響
}
```

### NoSpecimen 的重要性

`NoSpecimen` 表示此建構器無法處理請求，交由責任鏈中下一個建構器處理：

```csharp
public object Create(object request, ISpecimenContext context)
{
    // 不是我們的目標 → 回傳 NoSpecimen
    if (request is not PropertyInfo propertyInfo)
        return new NoSpecimen();

    if (propertyInfo.PropertyType != typeof(DateTime))
        return new NoSpecimen();

    if (!_targetProperties.Contains(propertyInfo.Name))
        return new NoSpecimen();

    // 是我們的目標 → 產生值
    return GenerateRandomDateTime();
}
```

---

## 改進版數值範圍建構器

```csharp
public class ImprovedRandomRangedNumericSequenceBuilder : ISpecimenBuilder
{
    private readonly int _min;
    private readonly int _max;
    private readonly Func<PropertyInfo, bool> _predicate;

    public ImprovedRandomRangedNumericSequenceBuilder(
        int min,
        int max,
        Func<PropertyInfo, bool> predicate)
    {
        _min = min;
        _max = max;
        _predicate = predicate;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo &&
            propertyInfo.PropertyType == typeof(int) &&
            _predicate(propertyInfo))
        {
            return Random.Shared.Next(_min, _max);
        }

        return new NoSpecimen();
    }
}
```

### 使用 Insert(0) 確保優先順序

```csharp
[Fact]
public void 使用Insert0確保優先順序()
{
    var fixture = new Fixture();

    // 使用 Insert(0) 確保最高優先順序
    fixture.Customizations.Insert(0,
        new ImprovedRandomRangedNumericSequenceBuilder(
            30, 50,
            prop => prop.Name == "Age" && prop.DeclaringType == typeof(Member)));

    var members = fixture.CreateMany<Member>(20).ToList();

    members.Should().AllSatisfy(m => m.Age.Should().BeInRange(30, 49));
}
```

---

## 泛型化數值範圍建構器 NumericRangeBuilder<TValue>

```csharp
public class NumericRangeBuilder<TValue> : ISpecimenBuilder
    where TValue : struct, IComparable, IConvertible
{
    private readonly TValue _min;
    private readonly TValue _max;
    private readonly Func<PropertyInfo, bool> _predicate;

    public NumericRangeBuilder(
        TValue min,
        TValue max,
        Func<PropertyInfo, bool> predicate)
    {
        _min = min;
        _max = max;
        _predicate = predicate;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo &&
            propertyInfo.PropertyType == typeof(TValue) &&
            _predicate(propertyInfo))
        {
            return GenerateRandomValue();
        }

        return new NoSpecimen();
    }

    private TValue GenerateRandomValue()
    {
        var minDecimal = Convert.ToDecimal(_min);
        var maxDecimal = Convert.ToDecimal(_max);
        var range = maxDecimal - minDecimal;
        var randomValue = minDecimal + (decimal)Random.Shared.NextDouble() * range;

        return typeof(TValue).Name switch
        {
            nameof(Int32) => (TValue)(object)(int)randomValue,
            nameof(Int64) => (TValue)(object)(long)randomValue,
            nameof(Int16) => (TValue)(object)(short)randomValue,
            nameof(Byte) => (TValue)(object)(byte)randomValue,
            nameof(Single) => (TValue)(object)(float)randomValue,
            nameof(Double) => (TValue)(object)(double)randomValue,
            nameof(Decimal) => (TValue)(object)randomValue,
            _ => throw new NotSupportedException($"Type {typeof(TValue).Name} is not supported")
        };
    }
}
```

### 流暢介面擴充方法

```csharp
public static class FixtureRangedNumericExtensions
{
    public static IFixture AddRandomRange<T, TValue>(
        this IFixture fixture,
        TValue min,
        TValue max,
        Func<PropertyInfo, bool> predicate)
        where TValue : struct, IComparable, IConvertible
    {
        fixture.Customizations.Insert(0,
            new NumericRangeBuilder<TValue>(min, max, predicate));
        return fixture;
    }
}
```

### 完整使用範例

```csharp
public class Product
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public double Rating { get; set; }
    public float Discount { get; set; }
}

[Fact]
public void 多重數值型別範圍控制()
{
    var fixture = new Fixture();

    fixture
        .AddRandomRange<Product, decimal>(
            50m, 500m,
            prop => prop.Name == "Price" && prop.DeclaringType == typeof(Product))
        .AddRandomRange<Product, int>(
            1, 50,
            prop => prop.Name == "Quantity" && prop.DeclaringType == typeof(Product))
        .AddRandomRange<Product, double>(
            1.0, 5.0,
            prop => prop.Name == "Rating" && prop.DeclaringType == typeof(Product))
        .AddRandomRange<Product, float>(
            0.0f, 0.5f,
            prop => prop.Name == "Discount" && prop.DeclaringType == typeof(Product));

    var products = fixture.CreateMany<Product>(10).ToList();

    products.Should().AllSatisfy(product =>
    {
        product.Price.Should().BeInRange(50m, 500m);
        product.Quantity.Should().BeInRange(1, 49);
        product.Rating.Should().BeInRange(1.0, 5.0);
        product.Discount.Should().BeInRange(0.0f, 0.5f);
    });
}
```
