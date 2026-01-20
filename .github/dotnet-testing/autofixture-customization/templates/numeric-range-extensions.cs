// =============================================================================
// AutoFixture 泛型化數值範圍建構器與擴充方法
// 展示如何建立可重用的泛型建構器和流暢介面
// =============================================================================

using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Xunit;

namespace AutoFixtureCustomization.Templates;

// -----------------------------------------------------------------------------
// 1. 測試模型類別
// -----------------------------------------------------------------------------

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public double Rating { get; set; }
    public float Discount { get; set; }
    public long ViewCount { get; set; }
    public short StockLevel { get; set; }
}

// -----------------------------------------------------------------------------
// 2. NumericRangeBuilder<TValue>：泛型數值範圍建構器
// -----------------------------------------------------------------------------

/// <summary>
/// 泛型化的數值範圍建構器
/// 支援 int, long, short, byte, float, double, decimal 等數值型別
/// </summary>
/// <typeparam name="TValue">數值型別</typeparam>
public class NumericRangeBuilder<TValue> : ISpecimenBuilder
    where TValue : struct, IComparable, IConvertible
{
    private readonly TValue _min;
    private readonly TValue _max;
    private readonly Func<PropertyInfo, bool> _predicate;

    /// <summary>
    /// 建立泛型數值範圍建構器
    /// </summary>
    /// <param name="min">最小值（包含）</param>
    /// <param name="max">最大值（不包含）</param>
    /// <param name="predicate">決定是否處理該屬性的條件</param>
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
        // 檢查是否為 PropertyInfo
        if (request is not PropertyInfo propertyInfo)
            return new NoSpecimen();

        // 檢查型別是否匹配
        if (propertyInfo.PropertyType != typeof(TValue))
            return new NoSpecimen();

        // 使用 predicate 進行精確判斷
        if (!_predicate(propertyInfo))
            return new NoSpecimen();

        return GenerateRandomValue();
    }

    /// <summary>
    /// 根據型別產生對應範圍的隨機值
    /// </summary>
    private TValue GenerateRandomValue()
    {
        var minDecimal = Convert.ToDecimal(_min);
        var maxDecimal = Convert.ToDecimal(_max);
        var range = maxDecimal - minDecimal;
        var randomValue = minDecimal + (decimal)Random.Shared.NextDouble() * range;

        // 根據實際型別轉換
        return typeof(TValue).Name switch
        {
            nameof(Int32) => (TValue)(object)(int)randomValue,
            nameof(Int64) => (TValue)(object)(long)randomValue,
            nameof(Int16) => (TValue)(object)(short)randomValue,
            nameof(Byte) => (TValue)(object)(byte)randomValue,
            nameof(Single) => (TValue)(object)(float)randomValue,
            nameof(Double) => (TValue)(object)(double)randomValue,
            nameof(Decimal) => (TValue)(object)randomValue,
            _ => throw new NotSupportedException(
                $"Type {typeof(TValue).Name} is not supported. " +
                $"Supported types: int, long, short, byte, float, double, decimal")
        };
    }
}

// -----------------------------------------------------------------------------
// 3. FixtureRangedNumericExtensions：流暢介面擴充方法
// -----------------------------------------------------------------------------

/// <summary>
/// IFixture 的擴充方法，提供流暢介面設定數值範圍
/// </summary>
public static class FixtureRangedNumericExtensions
{
    /// <summary>
    /// 為指定的屬性新增數值範圍限制
    /// </summary>
    /// <typeparam name="T">目標類別型別</typeparam>
    /// <typeparam name="TValue">數值型別</typeparam>
    /// <param name="fixture">Fixture 實例</param>
    /// <param name="min">最小值（包含）</param>
    /// <param name="max">最大值（不包含）</param>
    /// <param name="predicate">屬性選擇條件</param>
    /// <returns>Fixture 實例（支援鏈式呼叫）</returns>
    public static IFixture AddRandomRange<T, TValue>(
        this IFixture fixture,
        TValue min,
        TValue max,
        Func<PropertyInfo, bool> predicate)
        where TValue : struct, IComparable, IConvertible
    {
        // 使用 Insert(0) 確保優先順序
        fixture.Customizations.Insert(0,
            new NumericRangeBuilder<TValue>(min, max, predicate));

        return fixture;
    }

    /// <summary>
    /// 為指定名稱的屬性新增數值範圍限制
    /// </summary>
    /// <typeparam name="T">目標類別型別</typeparam>
    /// <typeparam name="TValue">數值型別</typeparam>
    /// <param name="fixture">Fixture 實例</param>
    /// <param name="min">最小值（包含）</param>
    /// <param name="max">最大值（不包含）</param>
    /// <param name="propertyName">屬性名稱</param>
    /// <returns>Fixture 實例（支援鏈式呼叫）</returns>
    public static IFixture AddRandomRange<T, TValue>(
        this IFixture fixture,
        TValue min,
        TValue max,
        string propertyName)
        where TValue : struct, IComparable, IConvertible
    {
        return fixture.AddRandomRange<T, TValue>(
            min, max,
            prop => prop.Name == propertyName && prop.DeclaringType == typeof(T));
    }
}

// -----------------------------------------------------------------------------
// 4. 測試範例
// -----------------------------------------------------------------------------

public class NumericRangeExtensionTests
{
    /// <summary>
    /// 使用擴充方法控制多種數值型別
    /// </summary>
    [Fact]
    public void 多重數值型別範圍控制()
    {
        // Arrange
        var fixture = new Fixture();

        fixture
            // decimal 價格：50-500
            .AddRandomRange<Product, decimal>(
                50m, 500m,
                prop => prop.Name == "Price" && prop.DeclaringType == typeof(Product))
            // int 數量：1-50
            .AddRandomRange<Product, int>(
                1, 50,
                prop => prop.Name == "Quantity" && prop.DeclaringType == typeof(Product))
            // double 評分：1.0-5.0
            .AddRandomRange<Product, double>(
                1.0, 5.0,
                prop => prop.Name == "Rating" && prop.DeclaringType == typeof(Product))
            // float 折扣：0.0-0.5
            .AddRandomRange<Product, float>(
                0.0f, 0.5f,
                prop => prop.Name == "Discount" && prop.DeclaringType == typeof(Product));

        // Act
        var products = fixture.CreateMany<Product>(10).ToList();

        // Assert
        products.Should().AllSatisfy(product =>
        {
            product.Price.Should().BeInRange(50m, 500m);
            product.Quantity.Should().BeInRange(1, 49);
            product.Rating.Should().BeInRange(1.0, 5.0);
            product.Discount.Should().BeInRange(0.0f, 0.5f);
        });
    }

    /// <summary>
    /// 使用簡化的屬性名稱指定方式
    /// </summary>
    [Fact]
    public void 使用屬性名稱簡化設定()
    {
        // Arrange
        var fixture = new Fixture();

        fixture
            .AddRandomRange<Product, decimal>(100m, 1000m, "Price")
            .AddRandomRange<Product, int>(10, 100, "Quantity")
            .AddRandomRange<Product, long>(1000L, 100000L, "ViewCount")
            .AddRandomRange<Product, short>((short)1, (short)100, "StockLevel");

        // Act
        var product = fixture.Create<Product>();

        // Assert
        product.Price.Should().BeInRange(100m, 1000m);
        product.Quantity.Should().BeInRange(10, 99);
        product.ViewCount.Should().BeInRange(1000L, 99999L);
        product.StockLevel.Should().BeInRange((short)1, (short)99);
    }

    /// <summary>
    /// 單一屬性範圍控制
    /// </summary>
    [Fact]
    public void 單一屬性範圍控制()
    {
        // Arrange
        var fixture = new Fixture();

        fixture.AddRandomRange<Product, decimal>(
            99.99m, 999.99m,
            "Price");

        // Act
        var products = fixture.CreateMany<Product>(20).ToList();

        // Assert
        products.Should().AllSatisfy(p =>
            p.Price.Should().BeInRange(99.99m, 999.99m));
    }

    /// <summary>
    /// 結合 Build() 方法使用
    /// </summary>
    [Fact]
    public void 結合Build方法使用()
    {
        // Arrange
        var fixture = new Fixture();

        fixture
            .AddRandomRange<Product, decimal>(50m, 200m, "Price")
            .AddRandomRange<Product, int>(1, 10, "Quantity");

        // Act - 結合 Build() 設定其他屬性
        var product = fixture.Build<Product>()
            .With(x => x.Name, "Test Product")
            .With(x => x.Rating, () => Math.Round(Random.Shared.NextDouble() * 4 + 1, 1))
            .Create();

        // Assert
        product.Name.Should().Be("Test Product");
        product.Price.Should().BeInRange(50m, 200m);
        product.Quantity.Should().BeInRange(1, 9);
        product.Rating.Should().BeInRange(1.0, 5.0);
    }
}

// -----------------------------------------------------------------------------
// 5. 進階：DateTimeRangeBuilder 泛型版本
// -----------------------------------------------------------------------------

/// <summary>
/// DateTime 範圍建構器（使用 Predicate 模式）
/// </summary>
public class DateTimeRangeBuilder : ISpecimenBuilder
{
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    private readonly Func<PropertyInfo, bool> _predicate;

    public DateTimeRangeBuilder(
        DateTime minDate,
        DateTime maxDate,
        Func<PropertyInfo, bool> predicate)
    {
        _minDate = minDate;
        _maxDate = maxDate;
        _predicate = predicate;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not PropertyInfo propertyInfo)
            return new NoSpecimen();

        if (propertyInfo.PropertyType != typeof(DateTime))
            return new NoSpecimen();

        if (!_predicate(propertyInfo))
            return new NoSpecimen();

        var range = _maxDate - _minDate;
        var randomTicks = (long)(Random.Shared.NextDouble() * range.Ticks);
        return _minDate.AddTicks(randomTicks);
    }
}

/// <summary>
/// DateTime 範圍擴充方法
/// </summary>
public static class FixtureDateTimeExtensions
{
    public static IFixture AddDateTimeRange<T>(
        this IFixture fixture,
        DateTime minDate,
        DateTime maxDate,
        string propertyName)
    {
        fixture.Customizations.Add(
            new DateTimeRangeBuilder(
                minDate, maxDate,
                prop => prop.Name == propertyName && prop.DeclaringType == typeof(T)));

        return fixture;
    }
}

// -----------------------------------------------------------------------------
// 6. 整合測試：結合數值和日期範圍
// -----------------------------------------------------------------------------

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipDate { get; set; }
}

public class IntegrationTests
{
    /// <summary>
    /// 結合所有擴充方法建立完整的測試資料
    /// </summary>
    [Fact]
    public void 完整整合測試()
    {
        // Arrange
        var fixture = new Fixture();

        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 12, 31);

        fixture
            // 數值範圍
            .AddRandomRange<Order, decimal>(100m, 10000m, "TotalAmount")
            .AddRandomRange<Order, int>(1, 20, "ItemCount")
            // 日期範圍
            .AddDateTimeRange<Order>(minDate, maxDate, "OrderDate")
            .AddDateTimeRange<Order>(minDate, maxDate, "ShipDate");

        // Act
        var orders = fixture.CreateMany<Order>(50).ToList();

        // Assert
        orders.Should().HaveCount(50);
        orders.Should().AllSatisfy(order =>
        {
            order.Id.Should().NotBeEmpty();
            order.CustomerName.Should().NotBeNullOrEmpty();
            order.TotalAmount.Should().BeInRange(100m, 10000m);
            order.ItemCount.Should().BeInRange(1, 19);
            order.OrderDate.Should().BeOnOrAfter(minDate);
            order.OrderDate.Should().BeOnOrBefore(maxDate);
            order.ShipDate.Should().BeOnOrAfter(minDate);
            order.ShipDate.Should().BeOnOrBefore(maxDate);
        });
    }
}
