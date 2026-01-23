// =============================================================================
// AutoFixture 與 Bogus 整合 - 整合工廠與測試情境
// 提供完整的測試資料管理解決方案
// =============================================================================

using AutoFixture;
using AutoFixture.Kernel;
using Bogus;
using FluentAssertions;
using System.Collections.Concurrent;
using System.Reflection;
using Xunit;

namespace AutoFixtureBogusIntegration.Templates;

#region IntegratedTestDataFactory

// =============================================================================
// 整合測試資料工廠
// 提供快取、批次產生與場景建立功能
// =============================================================================

/// <summary>
/// 整合測試資料工廠
/// 結合 AutoFixture 與 Bogus，提供完整的測試資料管理
/// </summary>
public class IntegratedTestDataFactory : IDisposable
{
    private readonly Fixture _fixture;
    private readonly ConcurrentDictionary<Type, object> _cache;
    private readonly Dictionary<string, object> _namedCache;
    private readonly object _cacheLock = new();

    /// <summary>
    /// 取得底層的 Fixture 實例
    /// </summary>
    public Fixture Fixture => _fixture;

    public IntegratedTestDataFactory()
    {
        _fixture = new Fixture();
        _cache = new ConcurrentDictionary<Type, object>();
        _namedCache = new Dictionary<string, object>();

        ConfigureFixture();
    }

    /// <summary>
    /// 設定 Fixture 的預設行為
    /// </summary>
    private void ConfigureFixture()
    {
        // 處理循環參考
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // 加入 Bogus SpecimenBuilder
        _fixture.Customizations.Add(new EmailSpecimenBuilder());
        _fixture.Customizations.Add(new PhoneSpecimenBuilder());
        _fixture.Customizations.Add(new NameSpecimenBuilder());
        _fixture.Customizations.Add(new AddressSpecimenBuilder());
        _fixture.Customizations.Add(new WebsiteSpecimenBuilder());
        _fixture.Customizations.Add(new CompanyNameSpecimenBuilder());
        _fixture.Customizations.Add(new IndustrySpecimenBuilder());
        _fixture.Customizations.Add(new ProductSpecimenBuilder());
    }

    #region 基本產生方法

    /// <summary>
    /// 產生新的物件
    /// </summary>
    public T CreateFresh<T>()
    {
        return _fixture.Create<T>();
    }

    /// <summary>
    /// 產生多個新物件
    /// </summary>
    public IEnumerable<T> CreateMany<T>(int count = 3)
    {
        return _fixture.CreateMany<T>(count);
    }

    /// <summary>
    /// 產生物件並進行自訂設定
    /// </summary>
    public T CreateFresh<T>(Action<T> configure)
    {
        var instance = _fixture.Create<T>();
        configure(instance);
        return instance;
    }

    #endregion

    #region 快取方法

    /// <summary>
    /// 取得或建立快取的物件
    /// 相同類型只會產生一次
    /// </summary>
    public T GetCached<T>() where T : class
    {
        return (T)_cache.GetOrAdd(typeof(T), _ => _fixture.Create<T>());
    }

    /// <summary>
    /// 取得或建立具名快取的物件
    /// 可為相同類型建立多個不同的快取實例
    /// </summary>
    public T GetCached<T>(string name) where T : class
    {
        var key = $"{typeof(T).FullName}:{name}";
        
        lock (_cacheLock)
        {
            if (!_namedCache.TryGetValue(key, out var cached))
            {
                cached = _fixture.Create<T>();
                _namedCache[key] = cached;
            }
            return (T)cached;
        }
    }

    /// <summary>
    /// 取得或建立具名快取的物件（帶設定）
    /// </summary>
    public T GetCached<T>(string name, Action<T> configure) where T : class
    {
        var key = $"{typeof(T).FullName}:{name}";
        
        lock (_cacheLock)
        {
            if (!_namedCache.TryGetValue(key, out var cached))
            {
                cached = _fixture.Create<T>();
                configure((T)cached);
                _namedCache[key] = cached;
            }
            return (T)cached;
        }
    }

    /// <summary>
    /// 清除所有快取
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        lock (_cacheLock)
        {
            _namedCache.Clear();
        }
    }

    /// <summary>
    /// 清除特定類型的快取
    /// </summary>
    public void ClearCache<T>() where T : class
    {
        _cache.TryRemove(typeof(T), out _);
        
        lock (_cacheLock)
        {
            var keysToRemove = _namedCache.Keys
                .Where(k => k.StartsWith($"{typeof(T).FullName}:"))
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _namedCache.Remove(key);
            }
        }
    }

    #endregion

    #region 測試情境方法

    /// <summary>
    /// 建立測試情境
    /// </summary>
    public TestScenario CreateTestScenario()
    {
        return new TestScenario(this);
    }

    /// <summary>
    /// 建立預設的完整測試情境
    /// 包含使用者、公司、產品和訂單
    /// </summary>
    public CompleteTestScenario CreateCompleteScenario()
    {
        var user = CreateFresh<User>(u =>
        {
            u.HomeAddress = CreateFresh<Address>();
        });

        var company = CreateFresh<Company>(c =>
        {
            c.Address = CreateFresh<Address>();
        });

        user.Company = company;

        var products = CreateMany<Product>(5).ToList();

        var order = CreateFresh<Order>(o =>
        {
            o.Customer = user;
            o.Items = products.Take(3).Select(p => new OrderItem
            {
                Id = Guid.NewGuid(),
                Product = p,
                Quantity = new Random().Next(1, 5),
                UnitPrice = p.Price
            }).ToList();
            o.TotalAmount = o.Items.Sum(i => i.TotalPrice);
            o.Status = OrderStatus.Pending;
        });

        return new CompleteTestScenario
        {
            User = user,
            Company = company,
            Products = products,
            Order = order
        };
    }

    #endregion

    #region 自訂設定方法

    /// <summary>
    /// 註冊特定類型的 Faker
    /// </summary>
    public IntegratedTestDataFactory WithFaker<T>(Faker<T> faker) where T : class
    {
        _fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return this;
    }

    /// <summary>
    /// 設定種子值
    /// </summary>
    public IntegratedTestDataFactory WithSeed(int seed)
    {
        Randomizer.Seed = new Random(seed);
        return this;
    }

    /// <summary>
    /// 設定 CreateMany 的預設數量
    /// </summary>
    public IntegratedTestDataFactory WithRepeatCount(int count)
    {
        _fixture.RepeatCount = count;
        return this;
    }

    #endregion

    public void Dispose()
    {
        ClearCache();
    }
}

#endregion

#region TestScenario

// =============================================================================
// 測試情境類別
// 提供流暢的測試資料建構 API
// =============================================================================

/// <summary>
/// 測試情境建構器
/// 提供流暢的 API 建立複雜的測試資料
/// </summary>
public class TestScenario
{
    private readonly IntegratedTestDataFactory _factory;
    private readonly Dictionary<string, object> _entities;

    public TestScenario(IntegratedTestDataFactory factory)
    {
        _factory = factory;
        _entities = new Dictionary<string, object>();
    }

    /// <summary>
    /// 新增使用者到情境
    /// </summary>
    public TestScenario WithUser(string name = "DefaultUser", Action<User>? configure = null)
    {
        var user = _factory.CreateFresh<User>(u =>
        {
            u.HomeAddress = _factory.CreateFresh<Address>();
            configure?.Invoke(u);
        });
        
        _entities[name] = user;
        return this;
    }

    /// <summary>
    /// 新增公司到情境
    /// </summary>
    public TestScenario WithCompany(string name = "DefaultCompany", Action<Company>? configure = null)
    {
        var company = _factory.CreateFresh<Company>(c =>
        {
            c.Address = _factory.CreateFresh<Address>();
            configure?.Invoke(c);
        });
        
        _entities[name] = company;
        return this;
    }

    /// <summary>
    /// 新增產品到情境
    /// </summary>
    public TestScenario WithProduct(string name = "DefaultProduct", Action<Product>? configure = null)
    {
        var product = _factory.CreateFresh<Product>(configure ?? (_ => { }));
        _entities[name] = product;
        return this;
    }

    /// <summary>
    /// 新增多個產品到情境
    /// </summary>
    public TestScenario WithProducts(int count, string prefix = "Product")
    {
        var products = _factory.CreateMany<Product>(count).ToList();
        for (int i = 0; i < products.Count; i++)
        {
            _entities[$"{prefix}{i + 1}"] = products[i];
        }
        _entities[$"{prefix}s"] = products;
        return this;
    }

    /// <summary>
    /// 新增訂單到情境
    /// </summary>
    public TestScenario WithOrder(string name = "DefaultOrder", 
        string? customerName = null, 
        Action<Order>? configure = null)
    {
        var order = _factory.CreateFresh<Order>(o =>
        {
            if (customerName != null && _entities.TryGetValue(customerName, out var customer))
            {
                o.Customer = customer as User;
            }
            configure?.Invoke(o);
        });
        
        _entities[name] = order;
        return this;
    }

    /// <summary>
    /// 建立關聯：使用者與公司
    /// </summary>
    public TestScenario LinkUserToCompany(string userName, string companyName)
    {
        if (_entities.TryGetValue(userName, out var userObj) && 
            _entities.TryGetValue(companyName, out var companyObj))
        {
            var user = userObj as User;
            var company = companyObj as Company;
            
            if (user != null && company != null)
            {
                user.Company = company;
                if (!company.Employees.Contains(user))
                {
                    company.Employees.Add(user);
                }
            }
        }
        return this;
    }

    /// <summary>
    /// 取得情境中的實體
    /// </summary>
    public T Get<T>(string name) where T : class
    {
        return _entities.TryGetValue(name, out var entity) ? (entity as T)! : null!;
    }

    /// <summary>
    /// 嘗試取得情境中的實體
    /// </summary>
    public bool TryGet<T>(string name, out T? entity) where T : class
    {
        if (_entities.TryGetValue(name, out var obj) && obj is T typedEntity)
        {
            entity = typedEntity;
            return true;
        }
        entity = null;
        return false;
    }

    /// <summary>
    /// 取得所有指定類型的實體
    /// </summary>
    public IEnumerable<T> GetAll<T>() where T : class
    {
        return _entities.Values.OfType<T>();
    }
}

/// <summary>
/// 完整測試情境
/// 包含預先建立的相關實體
/// </summary>
public class CompleteTestScenario
{
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public List<Product> Products { get; set; } = new();
    public Order Order { get; set; } = null!;
}

#endregion

#region IntegratedTestBase

// =============================================================================
// 整合測試基底類別
// 提供工廠與常用方法
// =============================================================================

/// <summary>
/// 整合測試基底類別
/// 提供完整的測試資料產生功能
/// </summary>
public abstract class IntegratedTestBase : IDisposable
{
    protected readonly IntegratedTestDataFactory Factory;
    protected readonly Fixture Fixture;

    protected IntegratedTestBase()
    {
        Factory = new IntegratedTestDataFactory();
        Fixture = Factory.Fixture;
    }

    #region 便捷方法

    /// <summary>
    /// 產生新物件
    /// </summary>
    protected T Create<T>() => Factory.CreateFresh<T>();

    /// <summary>
    /// 產生多個物件
    /// </summary>
    protected IEnumerable<T> CreateMany<T>(int count = 3) => Factory.CreateMany<T>(count);

    /// <summary>
    /// 產生物件並設定
    /// </summary>
    protected T Create<T>(Action<T> configure) => Factory.CreateFresh(configure);

    /// <summary>
    /// 取得快取物件
    /// </summary>
    protected T GetCached<T>() where T : class => Factory.GetCached<T>();

    /// <summary>
    /// 取得具名快取物件
    /// </summary>
    protected T GetCached<T>(string name) where T : class => Factory.GetCached<T>(name);

    /// <summary>
    /// 建立測試情境
    /// </summary>
    protected TestScenario CreateScenario() => Factory.CreateTestScenario();

    /// <summary>
    /// 建立完整測試情境
    /// </summary>
    protected CompleteTestScenario CreateCompleteScenario() => Factory.CreateCompleteScenario();

    #endregion

    public virtual void Dispose()
    {
        Factory.Dispose();
    }
}

/// <summary>
/// 具有種子控制的整合測試基底類別
/// </summary>
public abstract class SeededIntegratedTestBase : IntegratedTestBase
{
    protected SeededIntegratedTestBase(int seed = 12345)
    {
        Randomizer.Seed = new Random(seed);
    }
}

#endregion

#region 服務測試範例

// =============================================================================
// 服務測試範例
// 展示如何在實際服務測試中使用整合工廠
// =============================================================================

/// <summary>
/// 使用者服務（模擬）
/// </summary>
public interface IUserService
{
    Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<User>> GetUsersByCompanyAsync(Guid companyId);
    Task<bool> UpdateUserAsync(User user);
}

/// <summary>
/// 模擬的使用者服務實作
/// </summary>
public class MockUserService : IUserService
{
    private readonly List<User> _users = new();

    public Task<User> CreateUserAsync(User user)
    {
        if (user.Id == Guid.Empty)
            user.Id = Guid.NewGuid();
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(Guid id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<IEnumerable<User>> GetUsersByCompanyAsync(Guid companyId)
    {
        return Task.FromResult<IEnumerable<User>>(
            _users.Where(u => u.Company?.Id == companyId).ToList());
    }

    public Task<bool> UpdateUserAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing == null) return Task.FromResult(false);
        
        var index = _users.IndexOf(existing);
        _users[index] = user;
        return Task.FromResult(true);
    }
}

/// <summary>
/// 使用者服務測試
/// 展示整合測試基底類別的使用
/// </summary>
public class UserServiceTests : IntegratedTestBase
{
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userService = new MockUserService();
    }

    [Fact]
    public async Task CreateUserAsync_應成功建立使用者()
    {
        // Arrange
        var user = Create<User>();

        // Act
        var result = await _userService.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Email.Should().Contain("@");
    }

    [Fact]
    public async Task GetUserByIdAsync_應返回正確的使用者()
    {
        // Arrange
        var user = Create<User>();
        await _userService.CreateUserAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUsersByCompanyAsync_應返回同公司的使用者()
    {
        // Arrange
        var scenario = CreateScenario()
            .WithCompany("TestCompany")
            .WithUser("User1")
            .WithUser("User2")
            .WithUser("User3")
            .LinkUserToCompany("User1", "TestCompany")
            .LinkUserToCompany("User2", "TestCompany");

        var company = scenario.Get<Company>("TestCompany");
        var user1 = scenario.Get<User>("User1");
        var user2 = scenario.Get<User>("User2");
        var user3 = scenario.Get<User>("User3"); // 不屬於 TestCompany

        await _userService.CreateUserAsync(user1);
        await _userService.CreateUserAsync(user2);
        await _userService.CreateUserAsync(user3);

        // Act
        var result = (await _userService.GetUsersByCompanyAsync(company.Id)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == user1.Id);
        result.Should().Contain(u => u.Id == user2.Id);
        result.Should().NotContain(u => u.Id == user3.Id);
    }

    [Fact]
    public async Task UpdateUserAsync_應成功更新使用者資料()
    {
        // Arrange
        var user = Create<User>();
        await _userService.CreateUserAsync(user);
        
        user.FirstName = "UpdatedName";
        user.Age = 99;

        // Act
        var result = await _userService.UpdateUserAsync(user);
        var updated = await _userService.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        updated!.FirstName.Should().Be("UpdatedName");
        updated.Age.Should().Be(99);
    }
}

#endregion

#region 工廠測試

// =============================================================================
// IntegratedTestDataFactory 測試
// =============================================================================

public class IntegratedTestDataFactoryTests : IDisposable
{
    private readonly IntegratedTestDataFactory _factory;

    public IntegratedTestDataFactoryTests()
    {
        _factory = new IntegratedTestDataFactory();
    }

    [Fact]
    public void CreateFresh_應每次產生新物件()
    {
        // Act
        var user1 = _factory.CreateFresh<User>();
        var user2 = _factory.CreateFresh<User>();

        // Assert
        user1.Id.Should().NotBe(user2.Id);
        user1.Email.Should().NotBe(user2.Email);
    }

    [Fact]
    public void CreateMany_應產生指定數量的物件()
    {
        // Act
        var users = _factory.CreateMany<User>(5).ToList();

        // Assert
        users.Should().HaveCount(5);
        users.Select(u => u.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetCached_應返回相同物件()
    {
        // Act
        var user1 = _factory.GetCached<User>();
        var user2 = _factory.GetCached<User>();

        // Assert
        user1.Should().BeSameAs(user2);
    }

    [Fact]
    public void GetCached_具名_不同名稱應返回不同物件()
    {
        // Act
        var user1 = _factory.GetCached<User>("Admin");
        var user2 = _factory.GetCached<User>("Customer");

        // Assert
        user1.Should().NotBeSameAs(user2);
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void GetCached_具名_相同名稱應返回相同物件()
    {
        // Act
        var user1 = _factory.GetCached<User>("Admin");
        var user2 = _factory.GetCached<User>("Admin");

        // Assert
        user1.Should().BeSameAs(user2);
    }

    [Fact]
    public void ClearCache_應清除所有快取()
    {
        // Arrange
        var original = _factory.GetCached<User>();

        // Act
        _factory.ClearCache();
        var newUser = _factory.GetCached<User>();

        // Assert
        original.Should().NotBeSameAs(newUser);
    }

    [Fact]
    public void CreateCompleteScenario_應建立完整的測試情境()
    {
        // Act
        var scenario = _factory.CreateCompleteScenario();

        // Assert
        scenario.User.Should().NotBeNull();
        scenario.User.HomeAddress.Should().NotBeNull();
        scenario.User.Company.Should().BeSameAs(scenario.Company);
        scenario.Company.Should().NotBeNull();
        scenario.Company.Address.Should().NotBeNull();
        scenario.Products.Should().HaveCount(5);
        scenario.Order.Should().NotBeNull();
        scenario.Order.Customer.Should().BeSameAs(scenario.User);
        scenario.Order.Items.Should().HaveCount(3);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}

public class TestScenarioTests : IDisposable
{
    private readonly IntegratedTestDataFactory _factory;

    public TestScenarioTests()
    {
        _factory = new IntegratedTestDataFactory();
    }

    [Fact]
    public void WithUser_應新增使用者到情境()
    {
        // Arrange
        var scenario = _factory.CreateTestScenario();

        // Act
        scenario.WithUser("TestUser");

        // Assert
        var user = scenario.Get<User>("TestUser");
        user.Should().NotBeNull();
        user.HomeAddress.Should().NotBeNull();
    }

    [Fact]
    public void LinkUserToCompany_應建立關聯()
    {
        // Arrange
        var scenario = _factory.CreateTestScenario()
            .WithUser("Employee")
            .WithCompany("TechCorp");

        // Act
        scenario.LinkUserToCompany("Employee", "TechCorp");

        // Assert
        var user = scenario.Get<User>("Employee");
        var company = scenario.Get<Company>("TechCorp");
        
        user.Company.Should().BeSameAs(company);
        company.Employees.Should().Contain(user);
    }

    [Fact]
    public void WithProducts_應新增多個產品()
    {
        // Arrange
        var scenario = _factory.CreateTestScenario();

        // Act
        scenario.WithProducts(3, "Item");

        // Assert
        var item1 = scenario.Get<Product>("Item1");
        var item2 = scenario.Get<Product>("Item2");
        var item3 = scenario.Get<Product>("Item3");
        
        item1.Should().NotBeNull();
        item2.Should().NotBeNull();
        item3.Should().NotBeNull();

        var allProducts = scenario.GetAll<Product>().ToList();
        allProducts.Should().HaveCount(3);
    }

    [Fact]
    public void GetAll_應返回所有指定類型的實體()
    {
        // Arrange
        var scenario = _factory.CreateTestScenario()
            .WithUser("User1")
            .WithUser("User2")
            .WithCompany("Company1");

        // Act
        var users = scenario.GetAll<User>().ToList();
        var companies = scenario.GetAll<Company>().ToList();

        // Assert
        users.Should().HaveCount(2);
        companies.Should().HaveCount(1);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}

#endregion
