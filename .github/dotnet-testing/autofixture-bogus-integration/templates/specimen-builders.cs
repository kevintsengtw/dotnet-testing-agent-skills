// =============================================================================
// AutoFixture 與 Bogus 整合 - SpecimenBuilder 實作範例
// 展示如何透過 ISpecimenBuilder 將 Bogus 整合到 AutoFixture
// =============================================================================

using AutoFixture;
using AutoFixture.Kernel;
using Bogus;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace AutoFixtureBogusIntegration.Templates;

#region 測試模型類別

// =============================================================================
// 測試模型類別
// =============================================================================

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public Address? HomeAddress { get; set; }
    public Company? Company { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public class Address
{
    public Guid Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public List<User> Employees { get; set; } = new();
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class Order
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public User? Customer { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

#endregion

#region 屬性層級 SpecimenBuilder

// =============================================================================
// 屬性層級 SpecimenBuilder
// 根據屬性名稱決定是否使用 Bogus 產生資料
// =============================================================================

/// <summary>
/// Email 屬性的 Bogus 整合
/// 匹配所有名稱包含 "Email" 的屬性
/// </summary>
public class EmailSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && 
            property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType == typeof(string))
        {
            return _faker.Internet.Email();
        }
        
        return new NoSpecimen();
    }
}

/// <summary>
/// 電話號碼屬性的 Bogus 整合
/// 匹配所有名稱包含 "Phone" 的屬性
/// </summary>
public class PhoneSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && 
            property.Name.Contains("Phone", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType == typeof(string))
        {
            return _faker.Phone.PhoneNumber();
        }
        
        return new NoSpecimen();
    }
}

/// <summary>
/// 姓名屬性的 Bogus 整合
/// 匹配 FirstName、LastName、FullName 等屬性
/// </summary>
public class NameSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            return property.Name.ToLower() switch
            {
                var name when name.Contains("firstname") => _faker.Person.FirstName,
                var name when name.Contains("lastname") => _faker.Person.LastName,
                var name when name == "fullname" => _faker.Person.FullName,
                _ => new NoSpecimen()
            };
        }
        
        return new NoSpecimen();
    }
}

/// <summary>
/// 地址相關屬性的 Bogus 整合
/// 匹配 Street、City、PostalCode、Country 等屬性
/// </summary>
public class AddressSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            return property.Name.ToLower() switch
            {
                var name when name.Contains("street") => _faker.Address.StreetAddress(),
                var name when name.Contains("city") => _faker.Address.City(),
                var name when name.Contains("postal") || name.Contains("zip") => _faker.Address.ZipCode(),
                var name when name.Contains("country") => _faker.Address.Country(),
                var name when name.Contains("state") || name.Contains("province") => _faker.Address.State(),
                _ => new NoSpecimen()
            };
        }

        return new NoSpecimen();
    }
}

/// <summary>
/// 網站 URL 屬性的 Bogus 整合
/// 匹配名稱包含 "Website" 或 "Url" 的屬性
/// </summary>
public class WebsiteSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property &&
            property.PropertyType == typeof(string) &&
            (property.Name.Contains("Website", StringComparison.OrdinalIgnoreCase) ||
             property.Name.Contains("Url", StringComparison.OrdinalIgnoreCase)))
        {
            return _faker.Internet.Url();
        }

        return new NoSpecimen();
    }
}

/// <summary>
/// 公司名稱屬性的 Bogus 整合
/// 專門處理 Company 類型的 Name 屬性
/// </summary>
public class CompanyNameSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && 
            property.PropertyType == typeof(string) &&
            property.DeclaringType?.Name == "Company" &&
            property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
        {
            return _faker.Company.CompanyName();
        }

        return new NoSpecimen();
    }
}

/// <summary>
/// 產品相關屬性的 Bogus 整合
/// </summary>
public class ProductSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            // 只處理 Product 類型的屬性
            if (property.DeclaringType?.Name != "Product")
                return new NoSpecimen();

            return property.Name.ToLower() switch
            {
                "name" => _faker.Commerce.ProductName(),
                "description" => _faker.Commerce.ProductDescription(),
                "category" => _faker.Commerce.Department(),
                _ => new NoSpecimen()
            };
        }

        return new NoSpecimen();
    }
}

/// <summary>
/// 產業屬性的 Bogus 整合
/// </summary>
public class IndustrySpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && 
            property.PropertyType == typeof(string) &&
            property.Name.Equals("Industry", StringComparison.OrdinalIgnoreCase))
        {
            return _faker.Commerce.Department();
        }

        return new NoSpecimen();
    }
}

#endregion

#region 類型層級 SpecimenBuilder

// =============================================================================
// 類型層級 SpecimenBuilder
// 為整個類型建立完整的 Bogus 產生器
// =============================================================================

/// <summary>
/// 整合 Bogus 的 AutoFixture SpecimenBuilder
/// 為多個類型註冊完整的 Faker 產生器
/// </summary>
public class BogusSpecimenBuilder : ISpecimenBuilder
{
    private readonly Dictionary<Type, object> _fakers;

    public BogusSpecimenBuilder()
    {
        _fakers = new Dictionary<Type, object>();
        RegisterFakers();
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && _fakers.TryGetValue(type, out var faker))
        {
            return GenerateWithFaker(faker);
        }
        
        return new NoSpecimen();
    }

    private void RegisterFakers()
    {
        // 註冊使用者 Faker
        _fakers[typeof(User)] = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.LastName, f => f.Person.LastName)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.BirthDate, f => f.Person.DateOfBirth)
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80))
            // 忽略關聯屬性以避免循環參考
            .Ignore(u => u.HomeAddress)
            .Ignore(u => u.Company)
            .Ignore(u => u.Orders);

        // 註冊地址 Faker
        _fakers[typeof(Address)] = new Faker<Address>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.Street, f => f.Address.StreetAddress())
            .RuleFor(a => a.City, f => f.Address.City())
            .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
            .RuleFor(a => a.Country, f => f.Address.Country());

        // 註冊公司 Faker
        _fakers[typeof(Company)] = new Faker<Company>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Industry, f => f.Commerce.Department())
            .RuleFor(c => c.Website, f => f.Internet.Url())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .Ignore(c => c.Address)
            .Ignore(c => c.Employees);

        // 註冊產品 Faker
        _fakers[typeof(Product)] = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000))
            .RuleFor(p => p.Category, f => f.Commerce.Department())
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.8f));

        // 註冊訂單項目 Faker
        _fakers[typeof(OrderItem)] = new Faker<OrderItem>()
            .RuleFor(oi => oi.Id, f => f.Random.Guid())
            .RuleFor(oi => oi.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(oi => oi.UnitPrice, f => f.Random.Decimal(1, 500))
            .Ignore(oi => oi.Product);

        // 註冊訂單 Faker
        _fakers[typeof(Order)] = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.OrderDate, f => f.Date.Recent(30))
            .RuleFor(o => o.TotalAmount, f => f.Random.Decimal(10, 5000))
            .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatus>())
            .Ignore(o => o.Customer)
            .Ignore(o => o.Items);
    }

    private object GenerateWithFaker(object faker)
    {
        var generateMethod = faker.GetType().GetMethod("Generate", Type.EmptyTypes);
        return generateMethod?.Invoke(faker, null) ?? new NoSpecimen();
    }
}

/// <summary>
/// 泛型類型 SpecimenBuilder
/// 允許為特定類型註冊自訂的 Faker
/// </summary>
public class TypedBogusSpecimenBuilder<T> : ISpecimenBuilder where T : class
{
    private readonly Faker<T> _faker;

    public TypedBogusSpecimenBuilder(Faker<T> faker)
    {
        _faker = faker;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(T))
        {
            return _faker.Generate();
        }
        
        return new NoSpecimen();
    }
}

#endregion

#region 本地化 SpecimenBuilder

// =============================================================================
// 本地化 SpecimenBuilder
// 支援特定語系的資料產生
// =============================================================================

/// <summary>
/// 台灣地區資料 SpecimenBuilder
/// </summary>
public class TaiwanSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new("zh_TW");

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            // 根據屬性名稱產生台灣本地化資料
            return property.Name.ToLower() switch
            {
                var name when name.Contains("firstname") => _faker.Person.FirstName,
                var name when name.Contains("lastname") => _faker.Person.LastName,
                var name when name.Contains("city") => _faker.Address.City(),
                var name when name.Contains("street") => _faker.Address.StreetAddress(),
                _ => new NoSpecimen()
            };
        }

        return new NoSpecimen();
    }
}

/// <summary>
/// 多語系 SpecimenBuilder
/// 根據指定語系產生資料
/// </summary>
public class LocalizedSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker;
    private readonly string _locale;

    public LocalizedSpecimenBuilder(string locale = "en")
    {
        _locale = locale;
        _faker = new Faker(locale);
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            return property.Name.ToLower() switch
            {
                var name when name.Contains("email") => _faker.Internet.Email(),
                var name when name.Contains("phone") => _faker.Phone.PhoneNumber(),
                var name when name.Contains("firstname") => _faker.Person.FirstName,
                var name when name.Contains("lastname") => _faker.Person.LastName,
                var name when name.Contains("city") => _faker.Address.City(),
                var name when name.Contains("country") => _faker.Address.Country(),
                _ => new NoSpecimen()
            };
        }

        return new NoSpecimen();
    }
}

#endregion

#region SpecimenBuilder 使用測試

// =============================================================================
// SpecimenBuilder 使用測試
// =============================================================================

public class SpecimenBuilderTests
{
    /// <summary>
    /// 測試 EmailSpecimenBuilder
    /// </summary>
    [Fact]
    public void EmailSpecimenBuilder_應產生有效的Email格式()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customizations.Add(new EmailSpecimenBuilder());

        // Act
        var user = fixture.Create<User>();

        // Assert
        user.Email.Should().Contain("@");
        user.Email.Should().MatchRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    /// <summary>
    /// 測試 NameSpecimenBuilder
    /// </summary>
    [Fact]
    public void NameSpecimenBuilder_應產生真實的姓名()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customizations.Add(new NameSpecimenBuilder());

        // Act
        var user = fixture.Create<User>();

        // Assert
        user.FirstName.Should().NotBeNullOrEmpty();
        user.LastName.Should().NotBeNullOrEmpty();
        // Bogus 產生的名字通常是常見的名字
        user.FirstName.Should().NotContain("FirstName"); // 不是 AutoFixture 預設格式
    }

    /// <summary>
    /// 測試多個 SpecimenBuilder 組合
    /// </summary>
    [Fact]
    public void 多個SpecimenBuilder_應能正確組合使用()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customizations.Add(new EmailSpecimenBuilder());
        fixture.Customizations.Add(new PhoneSpecimenBuilder());
        fixture.Customizations.Add(new NameSpecimenBuilder());
        fixture.Customizations.Add(new AddressSpecimenBuilder());

        // Act
        var user = fixture.Create<User>();
        var address = fixture.Create<Address>();

        // Assert
        user.Email.Should().Contain("@");
        user.Phone.Should().MatchRegex(@"[\d\-\(\)\s\+\.x]+");
        user.FirstName.Should().NotBeNullOrEmpty();
        
        address.City.Should().NotBeNullOrEmpty();
        address.Country.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// 測試類型層級 SpecimenBuilder
    /// </summary>
    [Fact]
    public void BogusSpecimenBuilder_應產生完整的物件()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customizations.Add(new BogusSpecimenBuilder());

        // Act
        var product = fixture.Create<Product>();

        // Assert
        product.Id.Should().NotBeEmpty();
        product.Name.Should().NotBeNullOrEmpty();
        product.Description.Should().NotBeNullOrEmpty();
        product.Price.Should().BeGreaterThan(0);
        product.Category.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// 測試自訂類型 SpecimenBuilder
    /// </summary>
    [Fact]
    public void TypedBogusSpecimenBuilder_應使用自訂的Faker()
    {
        // Arrange
        var customUserFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, _ => "John")
            .RuleFor(u => u.LastName, _ => "Doe")
            .RuleFor(u => u.Email, _ => "john.doe@test.com")
            .RuleFor(u => u.Age, _ => 30);

        var fixture = new Fixture();
        fixture.Customizations.Add(new TypedBogusSpecimenBuilder<User>(customUserFaker));

        // Act
        var user = fixture.Create<User>();

        // Assert
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Should().Be("john.doe@test.com");
        user.Age.Should().Be(30);
    }

    /// <summary>
    /// 測試本地化 SpecimenBuilder
    /// </summary>
    [Fact]
    public void LocalizedSpecimenBuilder_應產生指定語系的資料()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customizations.Add(new LocalizedSpecimenBuilder("zh_TW"));

        // Act
        var user = fixture.Create<User>();

        // Assert
        user.FirstName.Should().NotBeNullOrEmpty();
        // 繁體中文名字通常較短
    }
}

#endregion
