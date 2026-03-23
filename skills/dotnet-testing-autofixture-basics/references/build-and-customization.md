# Build<T>() 模式與自訂化

## Build<T>() 模式：精確控制

當需要對特定屬性進行控制時，使用 `Build<T>()` 模式：

```csharp
[Fact]
public void Build模式_指定特定屬性()
{
    var fixture = new Fixture();

    var customer = fixture.Build<Customer>()
        .With(x => x.Name, "測試客戶")           // 指定固定值
        .With(x => x.Age, 25)                    // 指定固定值
        .Without(x => x.InternalId)              // 排除屬性
        .Create();

    customer.Name.Should().Be("測試客戶");
    customer.Age.Should().Be(25);
    customer.InternalId.Should().Be(default);
}
```

## OmitAutoProperties() 控制自動設定

```csharp
[Fact]
public void OmitAutoProperties_僅設定必要屬性()
{
    var fixture = new Fixture();

    var customer = fixture.Build<Customer>()
        .OmitAutoProperties()           // 不自動設定任何屬性
        .With(x => x.Id, 123)          // 只設定關心的屬性
        .With(x => x.Name, "測試客戶")
        .Create();

    customer.Id.Should().Be(123);
    customer.Name.Should().Be("測試客戶");
    customer.Email.Should().BeNullOrEmpty();  // 保持預設值
    customer.Age.Should().Be(0);              // 保持預設值
}
```

## 循環參考處理

### 預設行為：ThrowingRecursionBehavior

```csharp
// 預設會拋出例外
[Fact]
public void 循環參考_預設行為_拋出例外()
{
    var fixture = new Fixture();

    // Category 有 Parent 屬性指向自己，造成循環參考
    Action act = () => fixture.Create<Category>();

    act.Should().Throw<ObjectCreationException>();
}
```

### OmitOnRecursionBehavior：忽略循環參考

```csharp
[Fact]
public void 循環參考_使用OmitOnRecursion_成功建立()
{
    var fixture = new Fixture();

    // 移除預設的拋出例外行為
    fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
        .ForEach(b => fixture.Behaviors.Remove(b));

    // 加入忽略循環參考行為
    fixture.Behaviors.Add(new OmitOnRecursionBehavior());

    var category = fixture.Create<Category>();

    category.Should().NotBeNull();
    category.Name.Should().NotBeNullOrEmpty();
}
```

### 共用基底類別

建議建立基底類別來統一處理循環參考：

```csharp
public abstract class AutoFixtureTestBase
{
    protected Fixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}

public class CustomerServiceTests : AutoFixtureTestBase
{
    [Fact]
    public void ProcessOrder_正常訂單_應處理成功()
    {
        var fixture = CreateFixture();
        var customer = fixture.Create<Customer>();

        // 測試邏輯...
    }
}
```
