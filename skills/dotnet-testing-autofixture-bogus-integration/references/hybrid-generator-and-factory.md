# 混合產生器、測試資料工廠與基底類別

## ITestDataGenerator 介面

```csharp
public interface ITestDataGenerator
{
    T Generate<T>();
    IEnumerable<T> Generate<T>(int count);
    T Generate<T>(Action<T> configure);
}
```

## HybridTestDataGenerator 實作

```csharp
public class HybridTestDataGenerator : ITestDataGenerator
{
    private readonly IFixture _fixture;

    public HybridTestDataGenerator(int? seed = null)
    {
        _fixture = new Fixture()
            .WithBogus()
            .WithOmitOnRecursion();

        if (seed.HasValue)
        {
            Bogus.Randomizer.Seed = new Random(seed.Value);
        }
    }

    public T Generate<T>() => _fixture.Create<T>();

    public IEnumerable<T> Generate<T>(int count)
        => Enumerable.Range(0, count).Select(_ => Generate<T>());

    public T Generate<T>(Action<T> configure)
    {
        var item = Generate<T>();
        configure(item);
        return item;
    }
}
```

## IntegratedTestDataFactory

```csharp
public class IntegratedTestDataFactory
{
    private readonly IFixture _fixture;
    private readonly Dictionary<Type, object> _cache = new();

    public IntegratedTestDataFactory(int? seed = null)
    {
        _fixture = new Fixture()
            .WithBogus()
            .WithOmitOnRecursion()
            .WithRepeatCount(3);

        if (seed.HasValue)
        {
            _fixture.WithSeed(seed.Value);
        }
    }

    public T CreateFresh<T>() => _fixture.Create<T>();

    public List<T> CreateMany<T>(int count = 3)
        => _fixture.CreateMany<T>(count).ToList();

    public T GetCached<T>() where T : class
    {
        var type = typeof(T);
        if (_cache.TryGetValue(type, out var cached))
            return (T)cached;

        var instance = CreateFresh<T>();
        _cache[type] = instance;
        return instance;
    }

    public void ClearCache() => _cache.Clear();

    /// <summary>
    /// 建立完整測試場景
    /// </summary>
    public TestScenario CreateTestScenario()
    {
        var company = CreateFresh<Company>();
        var users = CreateMany<User>(5);
        var orders = CreateMany<Order>(10);

        // 建立關聯
        foreach (var user in users)
        {
            user.Company = company;
        }

        company.Employees = users;

        return new TestScenario
        {
            Company = company,
            Users = users,
            Orders = orders
        };
    }
}
```

## TestBase 基底類別

```csharp
public abstract class TestBase
{
    protected readonly IFixture Fixture;
    protected readonly HybridTestDataGenerator Generator;
    protected readonly IntegratedTestDataFactory Factory;

    protected TestBase(int? seed = null)
    {
        Fixture = new Fixture()
            .WithBogus()
            .WithOmitOnRecursion()
            .WithRepeatCount(3);

        if (seed.HasValue)
        {
            Fixture.WithSeed(seed.Value);
        }

        Generator = new HybridTestDataGenerator(seed);
        Factory = new IntegratedTestDataFactory(seed);
    }

    protected T Create<T>() => Fixture.Create<T>();

    protected List<T> CreateMany<T>(int count = 3)
        => Fixture.CreateMany<T>(count).ToList();

    protected T Create<T>(Action<T> configure)
    {
        var instance = Create<T>();
        configure(instance);
        return instance;
    }
}
```

## 使用範例

### 基本整合使用

```csharp
[Fact]
public void AutoFixture_整合_Bogus_應能產生真實感資料()
{
    // Arrange
    var fixture = new Fixture().WithBogus();

    // Act
    var user = fixture.Create<User>();

    // Assert
    user.Email.Should().Contain("@");
    user.FirstName.Should().NotBeNullOrEmpty();
    user.Phone.Should().MatchRegex(@"[\d\-\(\)\s]+");
}
```

### 使用工廠建立測試場景

```csharp
[Fact]
public void 工廠_應能建立完整的測試場景()
{
    // Arrange
    var factory = new IntegratedTestDataFactory(seed: 42);

    // Act
    var scenario = factory.CreateTestScenario();

    // Assert
    scenario.Company.Should().NotBeNull();
    scenario.Users.Should().HaveCount(5);
    scenario.Orders.Should().HaveCount(10);

    scenario.Users.Should().AllSatisfy(user =>
    {
        user.Company.Should().Be(scenario.Company);
        user.Email.Should().Contain("@");
    });
}
```
