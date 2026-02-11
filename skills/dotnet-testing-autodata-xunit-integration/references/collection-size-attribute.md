# CollectionSizeAttribute：控制集合大小

> 本文件從 [SKILL.md](../SKILL.md) 提取，提供 CollectionSizeAttribute 的完整實作與使用範例。

AutoData 預設的集合大小是 3，可透過自訂屬性控制：

## CollectionSizeAttribute 實作

```csharp
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using System.Reflection;

public class CollectionSizeAttribute : CustomizeAttribute
{
    private readonly int _size;

    public CollectionSizeAttribute(int size)
    {
        _size = size;
    }

    public override ICustomization GetCustomization(ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        var objectType = parameter.ParameterType.GetGenericArguments()[0];

        var isTypeCompatible = parameter.ParameterType.IsGenericType &&
            parameter.ParameterType.GetGenericTypeDefinition()
                .MakeGenericType(objectType)
                .IsAssignableFrom(typeof(List<>).MakeGenericType(objectType));

        if (!isTypeCompatible)
        {
            throw new InvalidOperationException(
                $"{nameof(CollectionSizeAttribute)} 指定的型別與 List 不相容: " +
                $"{parameter.ParameterType} {parameter.Name}");
        }

        var customizationType = typeof(CollectionSizeCustomization<>).MakeGenericType(objectType);
        return (ICustomization)Activator.CreateInstance(customizationType, parameter, _size)!;
    }

    private class CollectionSizeCustomization<T> : ICustomization
    {
        private readonly ParameterInfo _parameter;
        private readonly int _repeatCount;

        public CollectionSizeCustomization(ParameterInfo parameter, int repeatCount)
        {
            _parameter = parameter;
            _repeatCount = repeatCount;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(fixture.CreateMany<T>(_repeatCount).ToList()),
                    new EqualRequestSpecification(_parameter)));
        }
    }
}
```

## 使用 CollectionSizeAttribute

```csharp
[Theory]
[AutoData]
public void CollectionSize_控制自動產生集合大小(
    [CollectionSize(5)] List<Product> products,
    [CollectionSize(3)] List<Order> orders,
    Customer customer)
{
    // Assert
    products.Should().HaveCount(5);
    orders.Should().HaveCount(3);
    customer.Should().NotBeNull();

    products.Should().AllSatisfy(product =>
    {
        product.Name.Should().NotBeNullOrEmpty();
        product.Price.Should().BeGreaterOrEqualTo(0);
    });
}
```
