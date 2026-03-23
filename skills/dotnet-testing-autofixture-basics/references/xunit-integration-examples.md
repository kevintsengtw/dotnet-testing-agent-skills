# xUnit 整合與匿名測試範例

## 使用 Fixture 共享客製化

```csharp
public class ProductServiceTests
{
    private readonly Fixture _fixture;

    public ProductServiceTests()
    {
        _fixture = new Fixture();

        // 共同的客製化設定
        _fixture.Customize<ProductCreateRequest>(c => c
            .With(x => x.Price, () => _fixture.Create<decimal>() % 10000)
            .With(x => x.Name, () => $"Product-{_fixture.Create<string>()[..8]}")
        );
    }

    [Fact]
    public void CreateProduct_使用共享Fixture_應成功建立()
    {
        var productData = _fixture.Create<ProductCreateRequest>();
        var service = new ProductService();

        var result = service.CreateProduct(productData);

        result.Should().NotBeNull();
        productData.Price.Should().BeLessThan(10000);
    }
}
```

## 結合 Theory 測試

```csharp
[Theory]
[InlineData(CustomerType.Regular)]
[InlineData(CustomerType.Premium)]
[InlineData(CustomerType.VIP)]
public void CalculateDiscount_不同客戶類型_應套用正確折扣(CustomerType customerType)
{
    var fixture = new Fixture();

    var customer = fixture.Build<Customer>()
        .With(x => x.Type, customerType)
        .Create();

    var order = fixture.Create<Order>();
    var calculator = new DiscountCalculator();

    var discount = calculator.Calculate(customer, order);

    switch (customerType)
    {
        case CustomerType.Regular:
            discount.Should().Be(0);
            break;
        case CustomerType.Premium:
            discount.Should().BeInRange(0.05m, 0.10m);
            break;
        case CustomerType.VIP:
            discount.Should().BeInRange(0.15m, 0.25m);
            break;
    }
}
```

## 匿名測試原則

測試應該關注「行為」而不是「資料」。在大多數情況下，我們並不在乎具體的資料值是什麼：

```csharp
// ✅ 好的做法：專注於測試邏輯
[Fact]
public void AddCustomer_任何有效客戶_應成功新增()
{
    var fixture = new Fixture();
    var customer = fixture.Create<Customer>();
    var repository = new CustomerRepository();

    var result = repository.Add(customer);

    result.Should().BeTrue();
}

// ❌ 避免：依賴隨機值的具體內容
[Fact]
public void BadTest_依賴隨機值()
{
    var fixture = new Fixture();
    var customer = fixture.Create<Customer>();

    // 錯誤：假設隨機產生的年齡會大於 18
    customer.Age.Should().BeGreaterThan(18); // 可能失敗
}

// ✅ 正確：明確設定關鍵值
[Fact]
public void GoodTest_明確設定關鍵值()
{
    var fixture = new Fixture();
    var customer = fixture.Build<Customer>()
        .With(x => x.Age, 25)  // 明確設定
        .Create();

    var validator = new CustomerValidator();
    var isValid = validator.IsAdult(customer);

    isValid.Should().BeTrue();  // 穩定的結果
}
```

## Test Data Builder vs AutoFixture 比較

### 傳統 Test Data Builder

```csharp
// 需要手動建立 Builder 類別 (40+ 行)
public class OrderBuilder
{
    private int _id = 1;
    private Customer _customer = new Customer { Name = "Default" };
    private List<OrderItem> _items = new();

    public OrderBuilder WithCustomer(Customer customer)
    {
        _customer = customer;
        return this;
    }

    public OrderBuilder WithItems(params OrderItem[] items)
    {
        _items = items.ToList();
        return this;
    }

    public Order Build() => new Order
    {
        Id = _id,
        Customer = _customer,
        Items = _items
    };
}
```

### AutoFixture 方式

```csharp
// 零設定成本，專注於測試邏輯 (5 行)
var fixture = new Fixture();
var order = fixture.Build<Order>()
    .With(x => x.Status, OrderStatus.Completed)
    .Create();
```

### 混合策略

```csharp
public static class TestDataFactory
{
    private static readonly Fixture _fixture = new();

    // 用 AutoFixture 建立基礎資料，再用 Builder 加工
    public static OrderBuilder AnOrder()
    {
        var baseOrder = _fixture.Create<Order>();
        return new OrderBuilder(baseOrder);
    }

    // 大量隨機資料產生
    public static IEnumerable<User> CreateRandomUsers(int count)
    {
        return _fixture.CreateMany<User>(count);
    }
}
```
