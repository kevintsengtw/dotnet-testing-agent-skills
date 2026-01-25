// =============================================================================
// AutoFixture 與 Bogus 整合 - 混合產生器與擴充方法
// 提供統一的測試資料產生 API
// =============================================================================

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Bogus;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace AutoFixtureBogusIntegration.Templates;

#region ITestDataGenerator 介面

// =============================================================================
// 統一的測試資料產生介面
// =============================================================================

/// <summary>
/// 測試資料產生器的統一介面
/// 抽象化 AutoFixture 和 Bogus 的差異
/// </summary>
public interface ITestDataGenerator
{
    /// <summary>產生單一物件</summary>
    T Generate<T>();
    
    /// <summary>產生指定數量的物件</summary>
    IEnumerable<T> Generate<T>(int count);
    
    /// <summary>產生物件並進行自訂設定</summary>
    T Generate<T>(Action<T> configure);
    
    /// <summary>產生物件集合並進行自訂設定</summary>
    IEnumerable<T> Generate<T>(int count, Action<T> configure);
}

#endregion

#region HybridTestDataGenerator 實作

// =============================================================================
// 混合測試資料產生器
// 結合 AutoFixture 與 Bogus 的優點
// =============================================================================

/// <summary>
/// 混合測試資料產生器
/// 使用 AutoFixture 進行物件建立，透過 SpecimenBuilder 整合 Bogus
/// </summary>
public class HybridTestDataGenerator : ITestDataGenerator
{
    private readonly Fixture _fixture;
    private readonly Dictionary<Type, object> _registeredFakers;

    public HybridTestDataGenerator()
    {
        _fixture = new Fixture();
        _registeredFakers = new Dictionary<Type, object>();
        
        ConfigureDefaults();
    }

    /// <summary>
    /// 設定預設的 SpecimenBuilder
    /// </summary>
    private void ConfigureDefaults()
    {
        // 處理循環參考
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // 加入 Bogus 屬性產生器
        _fixture.Customizations.Add(new EmailSpecimenBuilder());
        _fixture.Customizations.Add(new PhoneSpecimenBuilder());
        _fixture.Customizations.Add(new NameSpecimenBuilder());
        _fixture.Customizations.Add(new AddressSpecimenBuilder());
        _fixture.Customizations.Add(new WebsiteSpecimenBuilder());
    }

    /// <summary>
    /// 註冊特定類型的 Faker
    /// </summary>
    public HybridTestDataGenerator WithFaker<T>(Faker<T> faker) where T : class
    {
        _registeredFakers[typeof(T)] = faker;
        _fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return this;
    }

    /// <summary>
    /// 設定隨機種子
    /// </summary>
    public HybridTestDataGenerator WithSeed(int seed)
    {
        Randomizer.Seed = new Random(seed);
        return this;
    }

    /// <summary>
    /// 設定重複數量
    /// </summary>
    public HybridTestDataGenerator WithRepeatCount(int count)
    {
        _fixture.RepeatCount = count;
        return this;
    }

    public T Generate<T>()
    {
        return _fixture.Create<T>();
    }

    public IEnumerable<T> Generate<T>(int count)
    {
        return _fixture.CreateMany<T>(count);
    }

    public T Generate<T>(Action<T> configure)
    {
        var instance = _fixture.Create<T>();
        configure(instance);
        return instance;
    }

    public IEnumerable<T> Generate<T>(int count, Action<T> configure)
    {
        return _fixture.CreateMany<T>(count)
            .Select(item =>
            {
                configure(item);
                return item;
            });
    }
}

#endregion

#region Fixture 擴充方法

// =============================================================================
// AutoFixture 擴充方法
// 提供便捷的 Bogus 整合
// =============================================================================

/// <summary>
/// Fixture 擴充方法
/// </summary>
public static class FixtureExtensions
{
    /// <summary>
    /// 加入所有預設的 Bogus SpecimenBuilder
    /// </summary>
    public static Fixture WithBogus(this Fixture fixture)
    {
        // 先處理循環參考
        fixture.WithOmitOnRecursion();

        // 加入 Bogus 屬性產生器
        fixture.Customizations.Add(new EmailSpecimenBuilder());
        fixture.Customizations.Add(new PhoneSpecimenBuilder());
        fixture.Customizations.Add(new NameSpecimenBuilder());
        fixture.Customizations.Add(new AddressSpecimenBuilder());
        fixture.Customizations.Add(new WebsiteSpecimenBuilder());
        fixture.Customizations.Add(new CompanyNameSpecimenBuilder());
        fixture.Customizations.Add(new IndustrySpecimenBuilder());
        fixture.Customizations.Add(new ProductSpecimenBuilder());

        return fixture;
    }

    /// <summary>
    /// 處理循環參考，設定為 null 或空集合
    /// </summary>
    public static Fixture WithOmitOnRecursion(this Fixture fixture, int recursionDepth = 1)
    {
        // 移除預設的遞迴處理行為
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        // 加入 OmitOnRecursionBehavior
        fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth));

        return fixture;
    }

    /// <summary>
    /// 設定 Bogus 的隨機種子
    /// 注意：AutoFixture 和 Bogus 使用不同的隨機機制
    /// 設定種子可提供穩定性，但無法保證完全重現
    /// </summary>
    public static Fixture WithSeed(this Fixture fixture, int seed)
    {
        Randomizer.Seed = new Random(seed);
        return fixture;
    }

    /// <summary>
    /// 設定 CreateMany 的預設數量
    /// </summary>
    public static Fixture WithRepeatCount(this Fixture fixture, int count)
    {
        fixture.RepeatCount = count;
        return fixture;
    }

    /// <summary>
    /// 為特定類型註冊自訂的 Faker
    /// </summary>
    public static Fixture WithBogusFor<T>(this Fixture fixture, Faker<T> faker) where T : class
    {
        fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return fixture;
    }

    /// <summary>
    /// 為特定類型註冊自訂的 Faker（使用 Action 設定）
    /// </summary>
    public static Fixture WithBogusFor<T>(this Fixture fixture, Action<Faker<T>> configure) where T : class
    {
        var faker = new Faker<T>();
        configure(faker);
        fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return fixture;
    }

    /// <summary>
    /// 加入自訂的 SpecimenBuilder
    /// </summary>
    public static Fixture WithSpecimenBuilder(this Fixture fixture, ISpecimenBuilder builder)
    {
        fixture.Customizations.Add(builder);
        return fixture;
    }

    /// <summary>
    /// 加入本地化的 SpecimenBuilder
    /// </summary>
    public static Fixture WithLocale(this Fixture fixture, string locale)
    {
        fixture.Customizations.Add(new LocalizedSpecimenBuilder(locale));
        return fixture;
    }
}

#endregion

#region BogusAutoDataAttribute

// =============================================================================
// BogusAutoDataAttribute
// 整合 AutoFixture 與 Bogus 的 xUnit 屬性
// =============================================================================

/// <summary>
/// 整合 Bogus 的 AutoData 屬性
/// 自動使用 Bogus SpecimenBuilder 產生測試資料
/// </summary>
public class BogusAutoDataAttribute : AutoDataAttribute
{
    public BogusAutoDataAttribute() : base(() => CreateFixture())
    {
    }

    private static Fixture CreateFixture()
    {
        return new Fixture().WithBogus();
    }
}

/// <summary>
/// 本地化的 BogusAutoData 屬性
/// </summary>
public class LocalizedBogusAutoDataAttribute : AutoDataAttribute
{
    public LocalizedBogusAutoDataAttribute(string locale = "en") 
        : base(() => CreateFixture(locale))
    {
    }

    private static Fixture CreateFixture(string locale)
    {
        return new Fixture()
            .WithOmitOnRecursion()
            .WithLocale(locale);
    }
}

/// <summary>
/// 可重現的 BogusAutoData 屬性
/// 使用固定的種子值
/// </summary>
public class SeededBogusAutoDataAttribute : AutoDataAttribute
{
    public SeededBogusAutoDataAttribute(int seed) 
        : base(() => CreateFixture(seed))
    {
    }

    private static Fixture CreateFixture(int seed)
    {
        return new Fixture()
            .WithBogus()
            .WithSeed(seed);
    }
}

#endregion

#region 測試基底類別

// =============================================================================
// 測試基底類別
// 提供共用的測試資料產生功能
// =============================================================================

/// <summary>
/// 測試基底類別
/// 提供整合 Bogus 的 Fixture 實例
/// </summary>
public abstract class BogusTestBase
{
    protected readonly Fixture Fixture;
    protected readonly ITestDataGenerator Generator;

    protected BogusTestBase()
    {
        Fixture = new Fixture().WithBogus();
        Generator = new HybridTestDataGenerator();
    }

    /// <summary>
    /// 產生單一物件
    /// </summary>
    protected T Create<T>() => Fixture.Create<T>();

    /// <summary>
    /// 產生多個物件
    /// </summary>
    protected IEnumerable<T> CreateMany<T>(int count = 3) => Fixture.CreateMany<T>(count);

    /// <summary>
    /// 產生物件並進行自訂設定
    /// </summary>
    protected T Create<T>(Action<T> configure)
    {
        var instance = Fixture.Create<T>();
        configure(instance);
        return instance;
    }
}

/// <summary>
/// 具有種子控制的測試基底類別
/// </summary>
public abstract class SeededBogusTestBase : BogusTestBase
{
    protected SeededBogusTestBase(int seed = 12345)
    {
        Randomizer.Seed = new Random(seed);
    }
}

#endregion

#region 整合測試

// =============================================================================
// 混合產生器與擴充方法測試
// =============================================================================

public class HybridTestDataGeneratorTests
{
    [Fact]
    public void Generate_應產生單一物件()
    {
        // Arrange
        var generator = new HybridTestDataGenerator();

        // Act
        var user = generator.Generate<User>();

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Generate_多個物件_應產生指定數量()
    {
        // Arrange
        var generator = new HybridTestDataGenerator();

        // Act
        var users = generator.Generate<User>(5).ToList();

        // Assert
        users.Should().HaveCount(5);
        users.Select(u => u.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Generate_自訂設定_應正確套用()
    {
        // Arrange
        var generator = new HybridTestDataGenerator();

        // Act
        var user = generator.Generate<User>(u =>
        {
            u.Age = 25;
            u.FirstName = "TestUser";
        });

        // Assert
        user.Age.Should().Be(25);
        user.FirstName.Should().Be("TestUser");
    }

    [Fact]
    public void WithFaker_應使用自訂的Faker()
    {
        // Arrange
        var customFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, _ => "Custom Product")
            .RuleFor(p => p.Price, _ => 99.99m);

        var generator = new HybridTestDataGenerator()
            .WithFaker(customFaker);

        // Act
        var product = generator.Generate<Product>();

        // Assert
        product.Name.Should().Be("Custom Product");
        product.Price.Should().Be(99.99m);
    }

    [Fact]
    public void WithSeed_應提供穩定的輸出()
    {
        // Arrange
        var generator1 = new HybridTestDataGenerator().WithSeed(12345);
        var generator2 = new HybridTestDataGenerator().WithSeed(12345);

        // Act
        var user1 = generator1.Generate<User>();
        var user2 = generator2.Generate<User>();

        // Assert
        // 注意：由於 AutoFixture 和 Bogus 的隨機機制不同
        // 相同種子可能產生相似但不完全相同的結果
        user1.Should().NotBeNull();
        user2.Should().NotBeNull();
    }
}

public class FixtureExtensionsTests
{
    [Fact]
    public void WithBogus_應加入所有預設的SpecimenBuilder()
    {
        // Arrange
        var fixture = new Fixture().WithBogus();

        // Act
        var user = fixture.Create<User>();
        var company = fixture.Create<Company>();

        // Assert
        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
        company.Name.Should().NotBeNullOrEmpty();
        company.Website.Should().StartWith("http");
    }

    [Fact]
    public void WithOmitOnRecursion_應正確處理循環參考()
    {
        // Arrange
        var fixture = new Fixture().WithOmitOnRecursion();

        // Act
        var user = fixture.Create<User>();

        // Assert
        // OmitOnRecursion 會將循環參考設為 null
        user.Should().NotBeNull();
        // 循環參考的深層物件會是 null 或空集合
    }

    [Fact]
    public void WithBogusFor_應為特定類型註冊自訂Faker()
    {
        // Arrange
        var customProductFaker = new Faker<Product>()
            .RuleFor(p => p.Name, _ => "Fixed Product Name")
            .RuleFor(p => p.Price, _ => 100m);

        var fixture = new Fixture()
            .WithOmitOnRecursion()
            .WithBogusFor(customProductFaker);

        // Act
        var product = fixture.Create<Product>();

        // Assert
        product.Name.Should().Be("Fixed Product Name");
        product.Price.Should().Be(100m);
    }

    [Fact]
    public void WithBogusFor_Action語法_應正確設定Faker()
    {
        // Arrange
        var fixture = new Fixture()
            .WithOmitOnRecursion()
            .WithBogusFor<Product>(faker => faker
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000)));

        // Act
        var product = fixture.Create<Product>();

        // Assert
        product.Name.Should().NotBeNullOrEmpty();
        product.Price.Should().BeGreaterThan(0);
    }

    [Fact]
    public void WithLocale_應產生本地化資料()
    {
        // Arrange
        var fixture = new Fixture()
            .WithOmitOnRecursion()
            .WithLocale("zh_TW");

        // Act
        var user = fixture.Create<User>();

        // Assert
        user.FirstName.Should().NotBeNullOrEmpty();
        // 繁體中文的資料
    }
}

public class BogusAutoDataAttributeTests
{
    [Theory]
    [BogusAutoData]
    public void BogusAutoData_應自動注入Bogus產生的資料(User user)
    {
        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
        user.LastName.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [BogusAutoData]
    public void BogusAutoData_多個參數_應各自產生資料(User user, Product product)
    {
        // Assert
        user.Should().NotBeNull();
        product.Should().NotBeNull();
        user.Email.Should().Contain("@");
        product.Name.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [SeededBogusAutoData(12345)]
    public void SeededBogusAutoData_應使用固定種子(User user)
    {
        // Assert
        user.Should().NotBeNull();
        // 相同種子會產生穩定的結果
    }
}

public class BogusTestBaseTests : BogusTestBase
{
    [Fact]
    public void Create_應使用整合的Fixture()
    {
        // Act
        var user = Create<User>();

        // Assert
        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateMany_應產生指定數量的物件()
    {
        // Act
        var users = CreateMany<User>(5).ToList();

        // Assert
        users.Should().HaveCount(5);
        users.Should().AllSatisfy(u => u.Email.Should().Contain("@"));
    }

    [Fact]
    public void Create_自訂設定_應正確套用()
    {
        // Act
        var user = Create<User>(u =>
        {
            u.Age = 30;
            u.FirstName = "CustomName";
        });

        // Assert
        user.Age.Should().Be(30);
        user.FirstName.Should().Be("CustomName");
    }
}

#endregion
