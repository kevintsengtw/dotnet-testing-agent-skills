// =============================================================================
// AutoData 進階模式與 CollectionSizeAttribute 範例
// 展示 CollectionSizeAttribute 實作、階層式資料組織、可重用資料集
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace AutoDataXunitIntegration.Templates;

// -----------------------------------------------------------------------------
// 1. 測試模型類別
// -----------------------------------------------------------------------------

public class Product
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class Customer
{
    public string Name { get; set; } = string.Empty;
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
}

// -----------------------------------------------------------------------------
// 2. CollectionSizeAttribute：控制集合產生數量
// -----------------------------------------------------------------------------

/// <summary>
/// 自訂屬性，用於控制 AutoData 產生集合的大小
/// 預設 AutoData 產生的集合大小是 3，此屬性可覆寫該行為
/// </summary>
public class CollectionSizeAttribute : CustomizeAttribute
{
    private readonly int _size;

    /// <summary>
    /// 建立 CollectionSizeAttribute
    /// </summary>
    /// <param name="size">集合大小</param>
    public CollectionSizeAttribute(int size)
    {
        _size = size;
    }

    public override ICustomization GetCustomization(ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        // 取得集合元素型別
        var objectType = parameter.ParameterType.GetGenericArguments()[0];

        // 驗證型別相容性
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

        // 建立對應的客製化
        var customizationType = typeof(CollectionSizeCustomization<>).MakeGenericType(objectType);
        return (ICustomization)Activator.CreateInstance(customizationType, parameter, _size)!;
    }

    /// <summary>
    /// 集合大小客製化實作
    /// </summary>
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

// -----------------------------------------------------------------------------
// 3. CollectionSizeAttribute 使用範例
// -----------------------------------------------------------------------------

public class CollectionSizeTests
{
    /// <summary>
    /// 使用 CollectionSizeAttribute 控制集合大小
    /// </summary>
    [Theory]
    [AutoData]
    public void CollectionSize_控制自動產生集合大小(
        [CollectionSize(5)] List<Product> products,
        [CollectionSize(3)] List<Order> orders,
        Customer customer)
    {
        // Assert - 驗證集合大小
        products.Should().HaveCount(5);
        orders.Should().HaveCount(3);
        customer.Should().NotBeNull();

        // Assert - 驗證每個 Product 都有合理的值
        products.Should().AllSatisfy(product =>
        {
            product.Name.Should().NotBeNullOrEmpty();
            product.Price.Should().BeGreaterOrEqualTo(0);
        });

        // Assert - 驗證每個 Order 都有合理的值
        orders.Should().AllSatisfy(order =>
        {
            order.OrderNumber.Should().NotBeNullOrEmpty();
        });
    }

    /// <summary>
    /// 使用不同大小的集合進行測試
    /// </summary>
    [Theory]
    [AutoData]
    public void CollectionSize_多個不同大小集合(
        [CollectionSize(1)] List<Customer> singleCustomer,
        [CollectionSize(10)] List<Product> manyProducts,
        [CollectionSize(2)] List<Order> twoOrders)
    {
        // Assert
        singleCustomer.Should().HaveCount(1);
        manyProducts.Should().HaveCount(10);
        twoOrders.Should().HaveCount(2);
    }

    /// <summary>
    /// 大量資料測試
    /// </summary>
    [Theory]
    [AutoData]
    public void CollectionSize_大量資料測試(
        [CollectionSize(100)] List<Product> products)
    {
        // Assert
        products.Should().HaveCount(100);

        // 驗證所有產品都是獨立的
        var distinctNames = products.Select(p => p.Name).Distinct().Count();
        distinctNames.Should().BeGreaterThan(1);
    }
}

// -----------------------------------------------------------------------------
// 4. 階層式資料組織策略
// -----------------------------------------------------------------------------

namespace DataSources
{
    /// <summary>
    /// 測試資料來源基底類別
    /// </summary>
    public abstract class BaseTestData
    {
        protected static string GetTestDataPath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
        }
    }

    /// <summary>
    /// 產品測試資料來源
    /// </summary>
    public class ProductTestDataSource : BaseTestData
    {
        public static IEnumerable<object[]> BasicProducts()
        {
            yield return new object[] { "iPhone", 35900m, true };
            yield return new object[] { "MacBook", 89900m, true };
            yield return new object[] { "AirPods", 7490m, false };
        }

        public static IEnumerable<object[]> ElectronicsProducts()
        {
            yield return new object[] { "手機", "MOBILE", 35000m };
            yield return new object[] { "筆電", "LAPTOP", 80000m };
            yield return new object[] { "平板", "TABLET", 25000m };
        }

        public static IEnumerable<object[]> DiscountedProducts()
        {
            yield return new object[] { "清倉商品A", 1000m, 0.5m };  // 50% 折扣
            yield return new object[] { "清倉商品B", 2000m, 0.3m };  // 30% 折扣
            yield return new object[] { "季節商品", 3000m, 0.2m };   // 20% 折扣
        }
    }

    /// <summary>
    /// 客戶測試資料來源
    /// </summary>
    public class CustomerTestDataSource : BaseTestData
    {
        public static IEnumerable<object[]> VipCustomers()
        {
            yield return new object[] { "張三", "VIP", 100000m };
            yield return new object[] { "李四", "VIP", 150000m };
        }

        public static IEnumerable<object[]> AllCustomerTypes()
        {
            yield return new object[] { "VIP", 100000m, 0.15m };
            yield return new object[] { "Premium", 50000m, 0.10m };
            yield return new object[] { "Regular", 20000m, 0.05m };
        }
    }
}

// -----------------------------------------------------------------------------
// 5. 可重用資料集
// -----------------------------------------------------------------------------

/// <summary>
/// 可重用的測試資料集
/// </summary>
public static class ReusableTestDataSets
{
    /// <summary>
    /// 產品類別資料集
    /// </summary>
    public static class ProductCategories
    {
        public static IEnumerable<object[]> All()
        {
            yield return new object[] { "3C產品", "TECH" };
            yield return new object[] { "服飾配件", "FASHION" };
            yield return new object[] { "居家生活", "HOME" };
            yield return new object[] { "運動健身", "SPORTS" };
        }

        public static IEnumerable<object[]> Electronics()
        {
            yield return new object[] { "手機", "MOBILE" };
            yield return new object[] { "筆電", "LAPTOP" };
            yield return new object[] { "平板", "TABLET" };
        }

        public static IEnumerable<object[]> Fashion()
        {
            yield return new object[] { "上衣", "TOP" };
            yield return new object[] { "褲裝", "BOTTOM" };
            yield return new object[] { "配件", "ACCESSORY" };
        }
    }

    /// <summary>
    /// 客戶類型資料集
    /// </summary>
    public static class CustomerTypes
    {
        public static IEnumerable<object[]> All()
        {
            yield return new object[] { "VIP", 100000m, 0.15m };
            yield return new object[] { "Premium", 50000m, 0.10m };
            yield return new object[] { "Regular", 20000m, 0.05m };
        }

        public static IEnumerable<object[]> HighValue()
        {
            yield return new object[] { "VIP", 100000m, 0.15m };
            yield return new object[] { "Premium", 50000m, 0.10m };
        }
    }

    /// <summary>
    /// 訂單狀態轉換資料集
    /// </summary>
    public static class OrderTransitions
    {
        public static IEnumerable<object[]> ValidTransitions()
        {
            yield return new object[] { "Created", "Confirmed" };
            yield return new object[] { "Confirmed", "Shipped" };
            yield return new object[] { "Shipped", "Delivered" };
            yield return new object[] { "Delivered", "Completed" };
        }

        public static IEnumerable<object[]> CancelTransitions()
        {
            yield return new object[] { "Created", "Cancelled" };
            yield return new object[] { "Confirmed", "Cancelled" };
        }
    }
}

// -----------------------------------------------------------------------------
// 6. 使用可重用資料集的測試
// -----------------------------------------------------------------------------

public class ReusableDataSetTests
{
    // 代理方法：讓 MemberAutoData 能找到可重用資料集
    public static IEnumerable<object[]> AllProductCategories() =>
        ReusableTestDataSets.ProductCategories.All();

    public static IEnumerable<object[]> AllCustomerTypes() =>
        ReusableTestDataSets.CustomerTypes.All();

    public static IEnumerable<object[]> ValidOrderTransitions() =>
        ReusableTestDataSets.OrderTransitions.ValidTransitions();

    [Theory]
    [MemberAutoData(nameof(AllProductCategories))]
    public void 產品類別測試(
        string categoryName,
        string categoryCode,
        Product product)
    {
        // Assert
        categoryName.Should().NotBeNullOrEmpty();
        categoryCode.Should().NotBeNullOrEmpty();
        product.Should().NotBeNull();

        // 驗證類別在預期範圍內
        categoryName.Should().BeOneOf("3C產品", "服飾配件", "居家生活", "運動健身");
    }

    [Theory]
    [MemberAutoData(nameof(AllCustomerTypes))]
    public void 客戶類型折扣測試(
        string customerType,
        decimal creditLimit,
        decimal discountRate,
        Order order)
    {
        // Arrange
        var customer = new Customer
        {
            Type = customerType,
            CreditLimit = creditLimit
        };

        // Assert
        customerType.Should().BeOneOf("VIP", "Premium", "Regular");
        creditLimit.Should().BePositive();
        discountRate.Should().BeInRange(0.05m, 0.15m);
        order.Should().NotBeNull();
    }
}

// -----------------------------------------------------------------------------
// 7. 與 Awesome Assertions 協作的進階範例
// -----------------------------------------------------------------------------

public class AwesomeAssertionsIntegrationTests
{
    /// <summary>
    /// 結合 InlineAutoData 與 CollectionSize 進行複雜驗證
    /// </summary>
    [Theory]
    [InlineAutoData("VIP", 100000)]
    [InlineAutoData("Premium", 50000)]
    [InlineAutoData("Regular", 20000)]
    public void 複雜驗證_客戶訂單處理(
        string customerType,
        decimal creditLimit,
        [Range(1000, 15000)] decimal orderAmount,
        [CollectionSize(3)] List<Product> products,
        Customer customer,
        Order order)
    {
        // Arrange
        customer.Type = customerType;
        customer.CreditLimit = creditLimit;
        order.Amount = orderAmount;

        // Act
        var canPlaceOrder = customer.CanPlaceOrder(order.Amount);
        var discountRate = CalculateDiscount(customer.Type);

        // Assert - 使用 Awesome Assertions 語法
        customer.Type.Should().Be(customerType);
        customer.CreditLimit.Should().Be(creditLimit);

        order.Amount.Should().BeInRange(1000m, 15000m);
        canPlaceOrder.Should().BeTrue("訂單金額應在信用額度內");

        discountRate.Should().BeInRange(0.05m, 0.15m);

        products.Should().HaveCount(3);
        products.Should().AllSatisfy(p =>
        {
            p.Name.Should().NotBeNullOrEmpty();
        });
    }

    /// <summary>
    /// 批量驗證測試
    /// </summary>
    [Theory]
    [AutoData]
    public void 批量驗證_產品清單(
        [CollectionSize(10)] List<Product> products,
        Customer customer)
    {
        // Assert - 使用 Should().AllSatisfy() 進行批量驗證
        products.Should().AllSatisfy(product =>
        {
            product.Should().NotBeNull();
            product.Name.Should().NotBeNullOrEmpty();
            product.Price.Should().BeGreaterOrEqualTo(0);
        });

        // 使用 Should().BeEquivalentTo() 進行部分比對
        var productNames = products.Select(p => p.Name);
        productNames.Should().OnlyContain(name => !string.IsNullOrEmpty(name));
    }

    private static decimal CalculateDiscount(string customerType)
    {
        return customerType switch
        {
            "VIP" => 0.15m,
            "Premium" => 0.10m,
            "Regular" => 0.05m,
            _ => 0m
        };
    }
}

// -----------------------------------------------------------------------------
// 8. 效能測試資料產生
// -----------------------------------------------------------------------------

public class PerformanceTestDataTests
{
    /// <summary>
    /// 大量資料效能測試
    /// </summary>
    [Theory]
    [AutoData]
    public void 效能測試_大量產品處理(
        [CollectionSize(1000)] List<Product> products)
    {
        // Arrange
        var availableProducts = products.Where(p => p.IsAvailable).ToList();

        // Act
        var totalValue = products.Sum(p => p.Price);
        var averagePrice = products.Average(p => p.Price);

        // Assert
        products.Should().HaveCount(1000);
        totalValue.Should().BePositive();
        averagePrice.Should().BePositive();
    }
}
