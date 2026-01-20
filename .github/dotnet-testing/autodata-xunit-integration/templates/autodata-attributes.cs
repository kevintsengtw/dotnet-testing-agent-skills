// =============================================================================
// AutoData 屬性家族使用範例
// 展示 AutoData、InlineAutoData、MemberAutoData、CompositeAutoData 的使用方式
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace AutoDataXunitIntegration.Templates;

// -----------------------------------------------------------------------------
// 1. 測試模型類別
// -----------------------------------------------------------------------------

public class Person
{
    public Guid Id { get; set; }

    [StringLength(10)]
    public string Name { get; set; } = string.Empty;

    [Range(18, 80)]
    public int Age { get; set; }

    public string Email { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}

public class Product
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class Customer
{
    public Person Person { get; set; } = new();
    public string Type { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }

    public bool CanPlaceOrder(decimal orderAmount)
    {
        return orderAmount <= CreditLimit;
    }
}

public class Order
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }

    public void TransitionTo(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}

public enum OrderStatus
{
    Created,
    Confirmed,
    Shipped,
    Delivered,
    Completed,
    Cancelled
}

// -----------------------------------------------------------------------------
// 2. AutoData：完全自動產生參數
// -----------------------------------------------------------------------------

public class AutoDataBasicTests
{
    /// <summary>
    /// AutoData 自動為所有參數產生測試資料
    /// </summary>
    [Theory]
    [AutoData]
    public void AutoData_應能自動產生所有參數(Person person, string message, int count)
    {
        // Arrange & Act - 參數已由 AutoData 自動產生

        // Assert
        person.Should().NotBeNull();
        person.Id.Should().NotBe(Guid.Empty);
        person.Name.Should().HaveLength(10);     // 遵循 StringLength(10) 限制
        person.Age.Should().BeInRange(18, 80);   // 遵循 Range(18, 80) 限制
        message.Should().NotBeNullOrEmpty();
        count.Should().NotBe(0);
    }

    /// <summary>
    /// 透過 DataAnnotation 約束參數
    /// </summary>
    [Theory]
    [AutoData]
    public void AutoData_透過DataAnnotation約束參數(
        [StringLength(5, MinimumLength = 3)] string shortName,
        [Range(1, 100)] int percentage,
        Person person)
    {
        // Assert
        shortName.Length.Should().BeInRange(3, 5);
        percentage.Should().BeInRange(1, 100);
        person.Should().NotBeNull();
    }

    /// <summary>
    /// AutoData 自動處理複雜物件
    /// </summary>
    [Theory]
    [AutoData]
    public void AutoData_處理複雜物件(Customer customer, Order order)
    {
        // Assert
        customer.Should().NotBeNull();
        customer.Person.Should().NotBeNull();
        customer.Person.Name.Should().NotBeNullOrEmpty();
        customer.CreditLimit.Should().NotBe(0);

        order.Should().NotBeNull();
        order.OrderNumber.Should().NotBeNullOrEmpty();
    }
}

// -----------------------------------------------------------------------------
// 3. InlineAutoData：混合固定值與自動產生
// -----------------------------------------------------------------------------

public class InlineAutoDataTests
{
    /// <summary>
    /// InlineAutoData 混合固定值與自動產生
    /// 固定值的順序必須與方法參數順序一致
    /// </summary>
    [Theory]
    [InlineAutoData("VIP客戶", 100000)]
    [InlineAutoData("一般客戶", 50000)]
    [InlineAutoData("新客戶", 10000)]
    public void InlineAutoData_混合固定值與自動產生(
        string customerType,    // 對應第 1 個固定值
        decimal creditLimit,    // 對應第 2 個固定值
        Person person)          // 由 AutoFixture 產生
    {
        // Arrange
        var customer = new Customer
        {
            Person = person,
            Type = customerType,
            CreditLimit = creditLimit
        };

        // Assert
        customer.Type.Should().BeOneOf("VIP客戶", "一般客戶", "新客戶");
        customer.CreditLimit.Should().BeOneOf(100000, 50000, 10000);
        customer.Person.Should().NotBeNull();
        customer.Person.Age.Should().BeInRange(18, 80);
    }

    /// <summary>
    /// InlineAutoData 參數順序一致性
    /// </summary>
    [Theory]
    [InlineAutoData("產品A", 100)]
    [InlineAutoData("產品B", 200)]
    [InlineAutoData("產品C", 300)]
    public void InlineAutoData_參數順序一致性(
        string name,        // 對應第 1 個固定值
        decimal price,      // 對應第 2 個固定值
        string description, // 由 AutoFixture 產生
        Product product)    // 由 AutoFixture 產生
    {
        // Assert
        name.Should().BeOneOf("產品A", "產品B", "產品C");
        price.Should().BeOneOf(100, 200, 300);
        description.Should().NotBeNullOrEmpty();
        product.Should().NotBeNull();
    }

    /// <summary>
    /// InlineAutoData 與 DataAnnotation 協作
    /// </summary>
    [Theory]
    [InlineAutoData("電子產品")]
    [InlineAutoData("服飾用品")]
    [InlineAutoData("生活用品")]
    public void InlineAutoData_與DataAnnotation協作(
        string category,
        [Range(100, 10000)] decimal price,
        [StringLength(50)] string description,
        Product product)
    {
        // Assert
        category.Should().NotBeNullOrEmpty();
        price.Should().BeInRange(100, 10000);
        description.Length.Should().BeLessOrEqualTo(50);
        product.Should().NotBeNull();
    }

    /// <summary>
    /// InlineAutoData 用於邊界測試
    /// </summary>
    [Theory]
    [InlineAutoData(0)]
    [InlineAutoData(1)]
    [InlineAutoData(100)]
    [InlineAutoData(int.MaxValue)]
    public void InlineAutoData_邊界測試(int boundary, string message)
    {
        // Assert
        boundary.Should().BeOneOf(0, 1, 100, int.MaxValue);
        message.Should().NotBeNullOrEmpty();
    }
}

// -----------------------------------------------------------------------------
// 4. MemberAutoData：結合外部資料來源
// -----------------------------------------------------------------------------

public class MemberAutoDataTests
{
    /// <summary>
    /// 使用靜態方法提供測試資料
    /// </summary>
    public static IEnumerable<object[]> GetProductCategories()
    {
        yield return new object[] { "3C產品", "TECH" };
        yield return new object[] { "服飾配件", "FASHION" };
        yield return new object[] { "居家生活", "HOME" };
        yield return new object[] { "運動健身", "SPORTS" };
    }

    [Theory]
    [MemberAutoData(nameof(GetProductCategories))]
    public void MemberAutoData_使用靜態方法資料(
        string categoryName,   // 來自 GetProductCategories
        string categoryCode,   // 來自 GetProductCategories
        Product product)       // 由 AutoFixture 產生
    {
        // Arrange
        var categorizedProduct = new CategorizedProduct
        {
            Product = product,
            CategoryName = categoryName,
            CategoryCode = categoryCode
        };

        // Assert
        categorizedProduct.CategoryName.Should().BeOneOf("3C產品", "服飾配件", "居家生活", "運動健身");
        categorizedProduct.CategoryCode.Should().BeOneOf("TECH", "FASHION", "HOME", "SPORTS");
        categorizedProduct.Product.Should().NotBeNull();
    }

    /// <summary>
    /// 使用靜態屬性提供測試資料
    /// </summary>
    public static IEnumerable<object[]> StatusTransitions => new[]
    {
        new object[] { OrderStatus.Created, OrderStatus.Confirmed },
        new object[] { OrderStatus.Confirmed, OrderStatus.Shipped },
        new object[] { OrderStatus.Shipped, OrderStatus.Delivered },
        new object[] { OrderStatus.Delivered, OrderStatus.Completed }
    };

    [Theory]
    [MemberAutoData(nameof(StatusTransitions))]
    public void MemberAutoData_使用靜態屬性_訂單狀態轉換(
        OrderStatus fromStatus,
        OrderStatus toStatus,
        Order order)
    {
        // Arrange
        order.Status = fromStatus;

        // Act
        order.TransitionTo(toStatus);

        // Assert
        order.Status.Should().Be(toStatus);
    }

    /// <summary>
    /// 使用動態計算的資料（MemberAutoData 的優勢）
    /// </summary>
    public static IEnumerable<object[]> GetDynamicPriceData()
    {
        var basePrice = 1000m;
        yield return new object[] { "基本方案", basePrice };
        yield return new object[] { "進階方案", basePrice * 2 };          // 可使用運算式
        yield return new object[] { "企業方案", CalculateEnterprisePrice() }; // 可使用方法
    }

    private static decimal CalculateEnterprisePrice()
    {
        return 5000m * 1.2m; // 複雜計算邏輯
    }

    [Theory]
    [MemberAutoData(nameof(GetDynamicPriceData))]
    public void MemberAutoData_動態計算資料(
        string planName,
        decimal price,
        Customer customer)
    {
        // Assert
        planName.Should().NotBeNullOrEmpty();
        price.Should().BeOneOf(1000m, 2000m, 6000m);
        customer.Should().NotBeNull();
    }
}

public class CategorizedProduct
{
    public Product Product { get; set; } = new();
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
}

// -----------------------------------------------------------------------------
// 5. 自訂 AutoData 屬性
// -----------------------------------------------------------------------------

/// <summary>
/// 領域專用的 AutoData 屬性
/// </summary>
public class DomainAutoDataAttribute : AutoDataAttribute
{
    public DomainAutoDataAttribute() : base(() => CreateFixture())
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        // 設定 Person 的自訂規則
        fixture.Customize<Person>(composer => composer
            .With(p => p.Age, () => Random.Shared.Next(18, 65))
            .With(p => p.Email, () => $"user{Random.Shared.Next(1000)}@example.com")
            .With(p => p.Name, () => $"測試用戶{Random.Shared.Next(100)}"));

        // 設定 Product 的自訂規則
        fixture.Customize<Product>(composer => composer
            .With(p => p.Price, () => Random.Shared.Next(100, 10000))
            .With(p => p.IsAvailable, true)
            .With(p => p.Name, () => $"產品{Random.Shared.Next(1000)}"));

        return fixture;
    }
}

/// <summary>
/// 業務邏輯專用的 AutoData 屬性
/// </summary>
public class BusinessAutoDataAttribute : AutoDataAttribute
{
    public BusinessAutoDataAttribute() : base(() => CreateFixture())
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        // 設定 Order 的業務規則
        fixture.Customize<Order>(composer => composer
            .With(o => o.Status, OrderStatus.Created)
            .With(o => o.Amount, () => Random.Shared.Next(1000, 50000))
            .With(o => o.OrderNumber, () => $"ORD{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000):D4}"));

        return fixture;
    }
}

public class CustomAutoDataTests
{
    /// <summary>
    /// 使用 DomainAutoData
    /// </summary>
    [Theory]
    [DomainAutoData]
    public void 使用DomainAutoData(Person person, Product product)
    {
        // Assert - 驗證 DomainAutoData 的設定
        person.Age.Should().BeInRange(18, 64);
        person.Email.Should().EndWith("@example.com");
        person.Name.Should().StartWith("測試用戶");

        product.IsAvailable.Should().BeTrue();
        product.Price.Should().BeInRange(100, 9999);
        product.Name.Should().StartWith("產品");
    }

    /// <summary>
    /// 使用 BusinessAutoData
    /// </summary>
    [Theory]
    [BusinessAutoData]
    public void 使用BusinessAutoData(Order order)
    {
        // Assert - 驗證 BusinessAutoData 的設定
        order.Status.Should().Be(OrderStatus.Created);
        order.Amount.Should().BeInRange(1000, 49999);
        order.OrderNumber.Should().StartWith("ORD");
    }
}

// -----------------------------------------------------------------------------
// 6. CompositeAutoData：多重資料來源整合
// -----------------------------------------------------------------------------

/// <summary>
/// 組合多個自訂 AutoData 配置
/// </summary>
public class CompositeAutoDataAttribute : AutoDataAttribute
{
    public CompositeAutoDataAttribute(params Type[] autoDataAttributeTypes)
        : base(() => CreateFixture(autoDataAttributeTypes))
    {
    }

    private static IFixture CreateFixture(Type[] autoDataAttributeTypes)
    {
        var fixture = new Fixture();

        foreach (var attributeType in autoDataAttributeTypes)
        {
            var createFixtureMethod = attributeType.GetMethod(
                "CreateFixture",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (createFixtureMethod != null)
            {
                var sourceFixture = (IFixture)createFixtureMethod.Invoke(null, null)!;

                foreach (var customization in sourceFixture.Customizations)
                {
                    fixture.Customizations.Add(customization);
                }
            }
        }

        return fixture;
    }
}

public class CompositeAutoDataTests
{
    /// <summary>
    /// 使用 CompositeAutoData 整合多重資料來源
    /// </summary>
    [Theory]
    [CompositeAutoData(typeof(DomainAutoDataAttribute), typeof(BusinessAutoDataAttribute))]
    public void CompositeAutoData_整合多重資料來源(
        Person person,
        Product product,
        Order order)
    {
        // Assert - DomainAutoData 的設定
        person.Age.Should().BeInRange(18, 64);
        person.Email.Should().EndWith("@example.com");
        product.IsAvailable.Should().BeTrue();

        // Assert - BusinessAutoData 的設定
        order.Status.Should().Be(OrderStatus.Created);
        order.OrderNumber.Should().StartWith("ORD");
    }
}
