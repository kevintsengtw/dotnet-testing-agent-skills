// =============================================================================
// AutoFixture 複雜物件與循環參考處理
// 展示巢狀物件建構、集合處理、循環參考解決方案
// =============================================================================

using AutoFixture;
using FluentAssertions;

namespace TestProject.AutoFixtureBasics;

/// <summary>
/// 展示 AutoFixture 處理複雜物件結構的能力
/// </summary>
public class ComplexObjectScenariosTests
{
    #region 巢狀物件自動建構

    [Fact]
    public void 巢狀物件_應完整建構所有層級()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var customer = fixture.Create<Customer>();

        // Assert - 所有層級都被建構
        customer.Should().NotBeNull();
        customer.Id.Should().BePositive();
        customer.Name.Should().NotBeNullOrEmpty();
        
        // 巢狀物件
        customer.Address.Should().NotBeNull();
        customer.Address.Street.Should().NotBeNullOrEmpty();
        customer.Address.City.Should().NotBeNullOrEmpty();
        customer.Address.Location.Should().NotBeNull();
        customer.Address.Location.Latitude.Should().NotBe(0);
        
        // 另一個巢狀物件
        customer.ContactInfo.Should().NotBeNull();
        customer.ContactInfo.Phone.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void 深層巢狀_應正確處理()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var order = fixture.Create<Order>();

        // Assert - 多層巢狀結構
        order.Customer.Address.Location.Should().NotBeNull();
        order.Customer.ContactInfo.Should().NotBeNull();
    }

    #endregion

    #region 集合與陣列處理

    [Fact]
    public void List集合_應建立包含元素()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var order = fixture.Create<Order>();

        // Assert
        order.Items.Should().NotBeNull();
        order.Items.Should().NotBeEmpty();
        order.Items.Should().AllSatisfy(item =>
        {
            item.ProductId.Should().BePositive();
            item.ProductName.Should().NotBeNullOrEmpty();
            item.Quantity.Should().BePositive();
            item.UnitPrice.Should().BePositive();
        });
    }

    [Fact]
    public void 陣列屬性_應建立包含元素()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var order = fixture.Create<Order>();

        // Assert
        order.Tags.Should().NotBeNull();
        order.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public void Dictionary屬性_應建立包含鍵值對()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var order = fixture.Create<Order>();

        // Assert
        order.Metadata.Should().NotBeNull();
        order.Metadata.Should().NotBeEmpty();
    }

    [Fact]
    public void HashSet屬性_應建立包含元素()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var order = fixture.Create<Order>();

        // Assert
        order.CategoryIds.Should().NotBeNull();
        order.CategoryIds.Should().NotBeEmpty();
    }

    #endregion

    #region 循環參考處理

    [Fact]
    public void 循環參考_預設行為_應拋出例外()
    {
        // Arrange
        var fixture = new Fixture();

        // Act & Assert - 預設會拋出例外
        Action act = () => fixture.Create<Category>();

        act.Should().Throw<ObjectCreationException>();
    }

    [Fact]
    public void 循環參考_使用OmitOnRecursion_應成功建立()
    {
        // Arrange
        var fixture = CreateRecursionSafeFixture();

        // Act
        var category = fixture.Create<Category>();

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().BePositive();
        category.Name.Should().NotBeNullOrEmpty();
        // 循環參考屬性會被設為 null
    }

    [Fact]
    public void 雙向關聯_使用OmitOnRecursion_應成功建立()
    {
        // Arrange - Customer 和 Order 互相參考
        var fixture = CreateRecursionSafeFixture();

        // Act
        var customer = fixture.Create<CustomerWithOrders>();

        // Assert
        customer.Should().NotBeNull();
        customer.Orders.Should().NotBeNull();
    }

    #endregion

    #region 共用基底類別模式

    /// <summary>
    /// 建立處理循環參考的 Fixture
    /// </summary>
    private static Fixture CreateRecursionSafeFixture()
    {
        var fixture = new Fixture();

        // 移除預設的拋出例外行為
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        // 加入忽略循環參考行為
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    #endregion

    #region 控制集合大小

    [Fact]
    public void RepeatCount_控制集合元素數量()
    {
        // Arrange
        var fixture = CreateRecursionSafeFixture();
        fixture.RepeatCount = 5;  // 設定集合預設包含 5 個元素

        // Act
        var order = fixture.Create<Order>();

        // Assert
        order.Items.Should().HaveCount(5);
    }

    [Fact]
    public void Build_With_控制特定集合大小()
    {
        // Arrange
        var fixture = CreateRecursionSafeFixture();

        // Act
        var order = fixture.Build<Order>()
            .With(x => x.Items, fixture.CreateMany<OrderItem>(10).ToList())
            .Create();

        // Assert
        order.Items.Should().HaveCount(10);
    }

    #endregion

    #region 列舉型別處理

    [Fact]
    public void 列舉屬性_應產生有效值()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var customer = fixture.Create<Customer>();

        // Assert
        customer.Type.Should().BeOneOf(
            CustomerType.Regular,
            CustomerType.Premium,
            CustomerType.VIP
        );
    }

    [Fact]
    public void 列舉_CreateMany_應產生不同值()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var types = fixture.CreateMany<CustomerType>(10).ToList();

        // Assert - 應該包含多種列舉值
        types.Distinct().Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region 測試用的 Model 類別

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Address Address { get; set; } = null!;
        public ContactInfo ContactInfo { get; set; } = null!;
        public CustomerType Type { get; set; }
    }

    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public GeoLocation Location { get; set; } = null!;
    }

    public class GeoLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class ContactInfo
    {
        public string Phone { get; set; } = string.Empty;
        public string MobilePhone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
    }

    public enum CustomerType
    {
        Regular,
        Premium,
        VIP
    }

    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();
        public string[] Tags { get; set; } = Array.Empty<string>();
        public Dictionary<string, string> Metadata { get; set; } = new();
        public HashSet<int> CategoryIds { get; set; } = new();
        public DateTime OrderDate { get; set; }
    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // 循環參考範例
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Category? Parent { get; set; }           // 自我參考
        public List<Category> Children { get; set; } = new();  // 自我參考集合
    }

    // 雙向關聯範例
    public class CustomerWithOrders
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<OrderWithCustomer> Orders { get; set; } = new();
    }

    public class OrderWithCustomer
    {
        public int Id { get; set; }
        public CustomerWithOrders Customer { get; set; } = null!;  // 反向參考
    }

    #endregion
}

/// <summary>
/// 建議的基底類別：統一處理循環參考
/// </summary>
public abstract class AutoFixtureTestBase
{
    protected Fixture CreateFixture()
    {
        var fixture = new Fixture();

        // 移除預設的拋出例外行為
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        // 加入忽略循環參考行為
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}

/// <summary>
/// 繼承基底類別的範例測試
/// </summary>
public class SampleServiceTests : AutoFixtureTestBase
{
    [Fact]
    public void 使用基底類別_簡化Fixture建立()
    {
        // Arrange
        var fixture = CreateFixture();
        var customer = fixture.Create<ComplexObjectScenariosTests.Customer>();

        // Act & Assert
        customer.Should().NotBeNull();
    }
}
