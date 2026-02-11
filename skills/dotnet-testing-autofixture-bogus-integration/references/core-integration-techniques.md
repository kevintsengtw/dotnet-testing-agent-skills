# 核心整合技術

> 本文件從 [SKILL.md](../SKILL.md) 的「核心整合技術」章節提煉而來，包含完整的 SpecimenBuilder 實作範例與擴充方法。

---

### 1. 屬性層級 SpecimenBuilder

透過 `ISpecimenBuilder` 介面，根據屬性名稱決定是否使用 Bogus：

```csharp
public class EmailSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && 
            property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase))
        {
            return _faker.Internet.Email();
        }
        
        return new NoSpecimen();
    }
}
```

**常用 SpecimenBuilder**：

| Builder                    | 匹配條件                    | Bogus 方法                           |
| -------------------------- | --------------------------- | ------------------------------------ |
| EmailSpecimenBuilder       | 包含 "Email"                | `Internet.Email()`                   |
| PhoneSpecimenBuilder       | 包含 "Phone"                | `Phone.PhoneNumber()`                |
| NameSpecimenBuilder        | FirstName/LastName/FullName | `Person.FirstName/LastName/FullName` |
| AddressSpecimenBuilder     | Street/City/Postal/Country  | `Address.*`                          |
| WebsiteSpecimenBuilder     | 包含 "Website"              | `Internet.Url()`                     |
| CompanyNameSpecimenBuilder | Company 類型的 Name         | `Company.CompanyName()`              |

### 2. 類型層級 SpecimenBuilder

為整個類型建立完整的 Bogus 產生器：

```csharp
public class BogusSpecimenBuilder : ISpecimenBuilder
{
    private readonly Dictionary<Type, object> _fakers;

    public BogusSpecimenBuilder()
    {
        _fakers = new Dictionary<Type, object>();
        RegisterFakers();
    }

    private void RegisterFakers()
    {
        _fakers[typeof(User)] = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName))
            .Ignore(u => u.Company); // 避免循環參考

        _fakers[typeof(Address)] = new Faker<Address>()
            .RuleFor(a => a.Street, f => f.Address.StreetAddress())
            .RuleFor(a => a.City, f => f.Address.City());
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && _fakers.TryGetValue(type, out var faker))
        {
            return GenerateWithFaker(faker);
        }
        return new NoSpecimen();
    }
}
```

### 3. 擴充方法整合

```csharp
public static class FixtureExtensions
{
    /// <summary>
    /// 為 AutoFixture 加入 Bogus 整合功能
    /// </summary>
    public static IFixture WithBogus(this IFixture fixture)
    {
        // 先處理循環參考
        fixture.WithOmitOnRecursion();

        // 加入屬性層級整合
        fixture.Customizations.Add(new EmailSpecimenBuilder());
        fixture.Customizations.Add(new PhoneSpecimenBuilder());
        fixture.Customizations.Add(new NameSpecimenBuilder());
        fixture.Customizations.Add(new AddressSpecimenBuilder());

        // 加入類型層級整合
        fixture.Customizations.Add(new BogusSpecimenBuilder());

        return fixture;
    }

    /// <summary>
    /// 處理循環參考
    /// </summary>
    public static IFixture WithOmitOnRecursion(this IFixture fixture)
    {
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    /// <summary>
    /// 設定隨機種子
    /// </summary>
    public static IFixture WithSeed(this IFixture fixture, int seed)
    {
        Bogus.Randomizer.Seed = new Random(seed);
        return fixture;
    }
}
```
