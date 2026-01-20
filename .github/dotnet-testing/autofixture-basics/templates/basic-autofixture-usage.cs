// =============================================================================
// AutoFixture 基礎使用方式
// 展示 Fixture 類別、Create<T>()、CreateMany<T>() 和 Build<T>() 的基本用法
// =============================================================================

using AutoFixture;
using FluentAssertions;
using System.Net.Mail;

namespace TestProject.AutoFixtureBasics;

/// <summary>
/// 展示 AutoFixture 基礎功能
/// </summary>
public class BasicAutoFixtureUsageTests
{
    #region Fixture 類別與 Create<T>()

    [Fact]
    public void Create_基本型別_應產生有效值()
    {
        // Arrange
        var fixture = new Fixture();

        // Act - 產生各種基本型別
        var id = fixture.Create<int>();
        var name = fixture.Create<string>();
        var price = fixture.Create<decimal>();
        var isActive = fixture.Create<bool>();
        var date = fixture.Create<DateTime>();
        var guid = fixture.Create<Guid>();

        // Assert
        id.Should().BePositive();
        name.Should().NotBeNullOrEmpty();
        price.Should().BePositive();
        date.Should().BeAfter(DateTime.MinValue);
        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_特殊型別_應產生有效格式()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var email = fixture.Create<MailAddress>();
        var uri = fixture.Create<Uri>();
        var version = fixture.Create<Version>();
        var timeSpan = fixture.Create<TimeSpan>();

        // Assert
        email.Address.Should().Contain("@");
        uri.IsAbsoluteUri.Should().BeTrue();
        version.Major.Should().BePositive();
    }

    [Fact]
    public void Create_每次產生不同值()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var name1 = fixture.Create<string>();
        var name2 = fixture.Create<string>();
        var id1 = fixture.Create<int>();
        var id2 = fixture.Create<int>();

        // Assert - 每次產生的值都不同
        name1.Should().NotBe(name2);
        id1.Should().NotBe(id2);
    }

    #endregion

    #region CreateMany<T>() 產生集合

    [Fact]
    public void CreateMany_預設數量_應產生三個元素()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var items = fixture.CreateMany<string>().ToList();

        // Assert
        items.Should().HaveCount(3);
        items.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void CreateMany_指定數量_應產生指定個數()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var items = fixture.CreateMany<int>(10).ToList();

        // Assert
        items.Should().HaveCount(10);
        items.Should().AllSatisfy(x => x.Should().BePositive());
    }

    [Fact]
    public void CreateMany_複雜物件_應完整建構()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var products = fixture.CreateMany<Product>(5).ToList();

        // Assert
        products.Should().HaveCount(5);
        products.Should().AllSatisfy(p =>
        {
            p.Id.Should().BePositive();
            p.Name.Should().NotBeNullOrEmpty();
            p.Price.Should().BePositive();
        });
    }

    #endregion

    #region Build<T>() 精確控制

    [Fact]
    public void Build_With_指定特定屬性值()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var customer = fixture.Build<Customer>()
            .With(x => x.Name, "測試客戶")
            .With(x => x.Age, 25)
            .Create();

        // Assert
        customer.Name.Should().Be("測試客戶");
        customer.Age.Should().Be(25);
        // 其他屬性仍會自動產生
        customer.Id.Should().BePositive();
        customer.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Build_Without_排除特定屬性()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var customer = fixture.Build<Customer>()
            .Without(x => x.InternalId)
            .Without(x => x.CreatedDate)
            .Create();

        // Assert
        customer.InternalId.Should().BeNullOrEmpty();
        customer.CreatedDate.Should().Be(default);
        // 其他屬性正常產生
        customer.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Build_動態值產生()
    {
        // Arrange
        var fixture = new Fixture();

        // Act - 使用 lambda 動態產生值
        var products = fixture.Build<Product>()
            .With(x => x.Price, () => Math.Round((decimal)Random.Shared.NextDouble() * 1000, 2))
            .CreateMany(10)
            .ToList();

        // Assert
        products.Should().AllSatisfy(p =>
        {
            p.Price.Should().BeInRange(0, 1000);
        });
    }

    #endregion

    #region OmitAutoProperties() 控制

    [Fact]
    public void OmitAutoProperties_僅設定必要屬性()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var customer = fixture.Build<Customer>()
            .OmitAutoProperties()       // 不自動設定任何屬性
            .With(x => x.Id, 123)       // 只設定關心的屬性
            .With(x => x.Name, "測試客戶")
            .Create();

        // Assert
        customer.Id.Should().Be(123);
        customer.Name.Should().Be("測試客戶");
        // 未設定的屬性保持預設值
        customer.Email.Should().BeNullOrEmpty();
        customer.Age.Should().Be(0);
        customer.Address.Should().BeNull();
    }

    [Fact]
    public void OmitAutoProperties_結合自動產生()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var customer = fixture.Build<Customer>()
            .OmitAutoProperties()
            .With(x => x.Id)            // 啟用 Id 的自動產生
            .With(x => x.Name)          // 啟用 Name 的自動產生
            .With(x => x.Email, "test@example.com")  // 手動設定
            .Create();

        // Assert
        customer.Id.Should().NotBe(0);                   // 自動產生
        customer.Name.Should().NotBeNullOrEmpty();       // 自動產生
        customer.Email.Should().Be("test@example.com");  // 手動設定
        customer.Age.Should().Be(0);                     // 預設值
    }

    #endregion

    #region 測試用的 Model 類別

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public Address? Address { get; set; }
        public string? InternalId { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    #endregion
}
