// =============================================================================
// AutoFixture 自訂 ISpecimenBuilder 範例
// 展示如何建立精確控制特定屬性的自訂建構器
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

public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
}

public class Order
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipDate { get; set; }
    public int Quantity { get; set; }
    public int Priority { get; set; }
}

// -----------------------------------------------------------------------------
// 2. RandomRangedDateTimeBuilder：精確控制特定 DateTime 屬性
// -----------------------------------------------------------------------------

/// <summary>
/// 自訂 DateTime 範圍建構器
/// 只控制指定名稱的 DateTime 屬性，其他屬性不受影響
/// </summary>
public class RandomRangedDateTimeBuilder : ISpecimenBuilder
{
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    private readonly HashSet<string> _targetProperties;

    /// <summary>
    /// 建立 DateTime 範圍建構器
    /// </summary>
    /// <param name="minDate">最小日期</param>
    /// <param name="maxDate">最大日期</param>
    /// <param name="targetProperties">要控制的屬性名稱</param>
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
        // 步驟 1：檢查是否為 PropertyInfo
        if (request is not PropertyInfo propertyInfo)
            return new NoSpecimen();

        // 步驟 2：檢查型別是否為 DateTime
        if (propertyInfo.PropertyType != typeof(DateTime))
            return new NoSpecimen();

        // 步驟 3：檢查屬性名稱是否在目標清單中
        if (!_targetProperties.Contains(propertyInfo.Name))
            return new NoSpecimen();

        // 步驟 4：產生範圍內的隨機 DateTime
        return GenerateRandomDateTime();
    }

    private DateTime GenerateRandomDateTime()
    {
        var range = _maxDate - _minDate;
        var randomTicks = (long)(Random.Shared.NextDouble() * range.Ticks);
        return _minDate.AddTicks(randomTicks);
    }
}

// -----------------------------------------------------------------------------
// 3. RandomRangedNumericSequenceBuilder：精確控制特定 int 屬性
// -----------------------------------------------------------------------------

/// <summary>
/// 自訂數值範圍建構器（簡單版本）
/// 只控制指定名稱的 int 屬性
/// </summary>
public class RandomRangedNumericSequenceBuilder : ISpecimenBuilder
{
    private readonly int _min;
    private readonly int _max;
    private readonly HashSet<string> _targetProperties;

    public RandomRangedNumericSequenceBuilder(
        int min, 
        int max, 
        params string[] targetProperties)
    {
        _min = min;
        _max = max;
        _targetProperties = new HashSet<string>(targetProperties);
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not PropertyInfo propertyInfo)
            return new NoSpecimen();

        if (propertyInfo.PropertyType != typeof(int))
            return new NoSpecimen();

        if (!_targetProperties.Contains(propertyInfo.Name))
            return new NoSpecimen();

        // 注意：Random.Next(min, max) 的 max 是不包含的
        return Random.Shared.Next(_min, _max);
    }
}

// -----------------------------------------------------------------------------
// 4. ImprovedRandomRangedNumericSequenceBuilder：使用 Predicate 的改進版本
// -----------------------------------------------------------------------------

/// <summary>
/// 使用 Predicate 的數值範圍建構器
/// 可以更精確地控制哪些屬性要被處理
/// </summary>
public class ImprovedRandomRangedNumericSequenceBuilder : ISpecimenBuilder
{
    private readonly int _min;
    private readonly int _max;
    private readonly Func<PropertyInfo, bool> _predicate;

    /// <summary>
    /// 建立改進版數值範圍建構器
    /// </summary>
    /// <param name="min">最小值（包含）</param>
    /// <param name="max">最大值（不包含）</param>
    /// <param name="predicate">決定是否處理該屬性的條件</param>
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
        if (request is not PropertyInfo propertyInfo)
            return new NoSpecimen();

        if (propertyInfo.PropertyType != typeof(int))
            return new NoSpecimen();

        // 使用 predicate 進行精確判斷
        if (!_predicate(propertyInfo))
            return new NoSpecimen();

        return Random.Shared.Next(_min, _max);
    }
}

// -----------------------------------------------------------------------------
// 5. 測試範例
// -----------------------------------------------------------------------------

public class CustomSpecimenBuilderTests
{
    /// <summary>
    /// 使用 RandomRangedDateTimeBuilder 控制特定 DateTime 屬性
    /// </summary>
    [Fact]
    public void RandomRangedDateTimeBuilder_只控制指定屬性()
    {
        // Arrange
        var fixture = new Fixture();
        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 12, 31);

        // 只控制 UpdateTime 屬性
        fixture.Customizations.Add(
            new RandomRangedDateTimeBuilder(minDate, maxDate, "UpdateTime"));

        // Act
        var member = fixture.Create<Member>();

        // Assert
        // UpdateTime 應該在指定範圍內
        member.UpdateTime.Should().BeOnOrAfter(minDate);
        member.UpdateTime.Should().BeOnOrBefore(maxDate);

        // CreateTime 不受影響（由 AutoFixture 預設產生）
        member.CreateTime.Should().NotBe(default);
    }

    /// <summary>
    /// 使用 RandomRangedDateTimeBuilder 控制多個 DateTime 屬性
    /// </summary>
    [Fact]
    public void RandomRangedDateTimeBuilder_控制多個屬性()
    {
        // Arrange
        var fixture = new Fixture();
        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 6, 30);

        // 同時控制 OrderDate 和 ShipDate
        fixture.Customizations.Add(
            new RandomRangedDateTimeBuilder(minDate, maxDate, "OrderDate", "ShipDate"));

        // Act
        var orders = fixture.CreateMany<Order>(10).ToList();

        // Assert
        orders.Should().AllSatisfy(order =>
        {
            order.OrderDate.Should().BeOnOrAfter(minDate);
            order.OrderDate.Should().BeOnOrBefore(maxDate);
            order.ShipDate.Should().BeOnOrAfter(minDate);
            order.ShipDate.Should().BeOnOrBefore(maxDate);
        });
    }

    /// <summary>
    /// 使用 Add() 的數值建構器可能被內建建構器覆蓋
    /// </summary>
    [Fact]
    public void 使用Add_可能被內建建構器覆蓋()
    {
        // Arrange
        var fixture = new Fixture();

        // 使用 Add() 新增建構器
        fixture.Customizations.Add(
            new RandomRangedNumericSequenceBuilder(30, 50, "Age"));

        // Act
        var member = fixture.Create<Member>();

        // Assert
        // 注意：這個測試可能會失敗！
        // 因為內建的 NumericSequenceGenerator 可能有更高優先順序
        // 如果失敗，應該改用 Insert(0)
        member.Age.Should().BePositive();
    }

    /// <summary>
    /// 使用 Insert(0) 確保建構器有最高優先順序
    /// </summary>
    [Fact]
    public void 使用Insert0_確保最高優先順序()
    {
        // Arrange
        var fixture = new Fixture();

        // 使用 Insert(0) 確保最高優先順序
        fixture.Customizations.Insert(0,
            new ImprovedRandomRangedNumericSequenceBuilder(
                30, 50,
                prop => prop.Name == "Age" && prop.DeclaringType == typeof(Member)));

        // Act
        var members = fixture.CreateMany<Member>(20).ToList();

        // Assert
        members.Should().AllSatisfy(m => m.Age.Should().BeInRange(30, 49));
    }

    /// <summary>
    /// 使用 Predicate 精確控制多個屬性
    /// </summary>
    [Fact]
    public void 使用Predicate_精確控制多個屬性()
    {
        // Arrange
        var fixture = new Fixture();

        // 控制 Order 的 Quantity（1-100）
        fixture.Customizations.Insert(0,
            new ImprovedRandomRangedNumericSequenceBuilder(
                1, 100,
                prop => prop.Name == "Quantity" && prop.DeclaringType == typeof(Order)));

        // 控制 Order 的 Priority（1-5）
        fixture.Customizations.Insert(0,
            new ImprovedRandomRangedNumericSequenceBuilder(
                1, 5,
                prop => prop.Name == "Priority" && prop.DeclaringType == typeof(Order)));

        // Act
        var orders = fixture.CreateMany<Order>(20).ToList();

        // Assert
        orders.Should().AllSatisfy(order =>
        {
            order.Quantity.Should().BeInRange(1, 99);
            order.Priority.Should().BeInRange(1, 4);
        });
    }

    /// <summary>
    /// 結合 DateTime 和數值範圍建構器
    /// </summary>
    [Fact]
    public void 結合多種自訂建構器()
    {
        // Arrange
        var fixture = new Fixture();

        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 12, 31);

        // DateTime 範圍
        fixture.Customizations.Add(
            new RandomRangedDateTimeBuilder(minDate, maxDate, "OrderDate"));

        // 數值範圍（使用 Insert(0) 確保優先順序）
        fixture.Customizations.Insert(0,
            new ImprovedRandomRangedNumericSequenceBuilder(
                1, 100,
                prop => prop.Name == "Quantity" && prop.DeclaringType == typeof(Order)));

        // Act
        var order = fixture.Create<Order>();

        // Assert
        order.OrderDate.Should().BeOnOrAfter(minDate);
        order.OrderDate.Should().BeOnOrBefore(maxDate);
        order.Quantity.Should().BeInRange(1, 99);
    }
}

// -----------------------------------------------------------------------------
// 6. NoSpecimen 的重要性說明
// -----------------------------------------------------------------------------

/// <summary>
/// 展示 NoSpecimen 在責任鏈模式中的作用
/// </summary>
public class NoSpecimenExplanation
{
    /// <summary>
    /// 錯誤示範：不回傳 NoSpecimen 會導致問題
    /// </summary>
    public class BadSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo propertyInfo &&
                propertyInfo.Name == "Age")
            {
                return 25;
            }

            // ❌ 錯誤：回傳 null 會導致其他屬性也變成 null
            return null!;
        }
    }

    /// <summary>
    /// 正確示範：回傳 NoSpecimen 讓責任鏈繼續
    /// </summary>
    public class GoodSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo propertyInfo &&
                propertyInfo.Name == "Age")
            {
                return 25;
            }

            // ✅ 正確：回傳 NoSpecimen 讓其他建構器處理
            return new NoSpecimen();
        }
    }
}
