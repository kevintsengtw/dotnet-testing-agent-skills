# 資料驅動測試進階技巧

> 本文件從 [SKILL.md](../SKILL.md) 提煉，提供 TUnit 資料驅動測試的完整範例與細節。

## 資料來源方式比較

| 資料來源方式         | 適用場景           | 優勢       | 注意事項         |
| :------------------- | :----------------- | :--------- | :--------------- |
| **Arguments**        | 簡單固定資料       | 語法簡潔   | 資料量不宜過大   |
| **MethodDataSource** | 動態資料、複雜物件 | 最大靈活性 | 需要額外方法定義 |
| **ClassDataSource**  | 共享資料、依賴注入 | 可重用性高 | 類別生命週期管理 |
| **Matrix Tests**     | 組合測試           | 覆蓋率高   | 容易產生過多測試 |

## MethodDataSource：方法作為資料來源

最靈活的資料提供方式，適合動態產生或從外部來源載入資料：

```csharp
[Test]
[MethodDataSource(nameof(GetOrderTestData))]
public async Task CreateOrder_各種情況_應正確處理(
    string customerId, 
    CustomerLevel level, 
    List<OrderItem> items, 
    decimal expectedTotal)
{
    // Arrange
    var orderService = new OrderService(_repository, _discountCalculator, _shippingCalculator, _logger);

    // Act
    var order = await orderService.CreateOrderAsync(customerId, level, items);

    // Assert
    await Assert.That(order).IsNotNull();
    await Assert.That(order.CustomerId).IsEqualTo(customerId);
    await Assert.That(order.TotalAmount).IsEqualTo(expectedTotal);
}

public static IEnumerable<object[]> GetOrderTestData()
{
    // 一般會員訂單
    yield return new object[]
    {
        "CUST001",
        CustomerLevel.一般會員,
        new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "商品A", UnitPrice = 100m, Quantity = 2 }
        },
        200m
    };

    // VIP會員訂單
    yield return new object[]
    {
        "CUST002", 
        CustomerLevel.VIP會員,
        new List<OrderItem>
        {
            new() { ProductId = "PROD002", ProductName = "商品B", UnitPrice = 500m, Quantity = 1 }
        },
        500m
    };
}
```

**從檔案載入測試資料：**

```csharp
[Test]
[MethodDataSource(nameof(GetDiscountTestDataFromFile))]
public async Task CalculateDiscount_從檔案讀取_應套用正確折扣(
    string scenario, 
    decimal originalAmount, 
    CustomerLevel level, 
    string discountCode, 
    decimal expectedDiscount)
{
    var calculator = new DiscountCalculator(new MockDiscountRepository(), new MockLogger<DiscountCalculator>());
    var order = new Order
    {
        CustomerLevel = level,
        Items = [new OrderItem { UnitPrice = originalAmount, Quantity = 1 }]
    };

    var discount = await calculator.CalculateDiscountAsync(order, discountCode);

    await Assert.That(discount).IsEqualTo(expectedDiscount);
}

public static IEnumerable<object[]> GetDiscountTestDataFromFile()
{
    var filePath = Path.Combine("TestData", "discount-scenarios.json");
    var jsonData = File.ReadAllText(filePath);
    var scenarios = JsonSerializer.Deserialize<List<DiscountScenario>>(jsonData);
    if (scenarios == null) yield break;
    
    foreach (var s in scenarios)
    {
        yield return new object[] { s.Scenario, s.Amount, (CustomerLevel)s.Level, s.Code, s.Expected };
    }
}
```

## ClassDataSource：類別作為資料提供者

當測試資料需要共享給多個測試類別時使用：

```csharp
[Test]
[ClassDataSource<OrderValidationTestData>]
public async Task ValidateOrder_各種驗證情況_應回傳正確結果(OrderValidationScenario scenario)
{
    var validator = new OrderValidator(_discountRepository, _logger);
    var result = await validator.ValidateAsync(scenario.Order);

    await Assert.That(result.IsValid).IsEqualTo(scenario.ExpectedValid);
    if (!scenario.ExpectedValid)
    {
        await Assert.That(result.ErrorMessage).Contains(scenario.ExpectedErrorKeyword);
    }
}

public class OrderValidationTestData : IEnumerable<OrderValidationScenario>
{
    public IEnumerator<OrderValidationScenario> GetEnumerator()
    {
        yield return new OrderValidationScenario
        {
            Name = "有效的一般訂單",
            Order = CreateValidOrder(),
            ExpectedValid = true,
            ExpectedErrorKeyword = null
        };

        yield return new OrderValidationScenario
        {
            Name = "客戶ID為空",
            Order = CreateOrderWithEmptyCustomerId(),
            ExpectedValid = false,
            ExpectedErrorKeyword = "客戶ID"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static Order CreateValidOrder() => new()
    {
        CustomerId = "CUST001",
        CustomerLevel = CustomerLevel.一般會員,
        Items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "測試商品", UnitPrice = 100m, Quantity = 1 }
        }
    };

    private static Order CreateOrderWithEmptyCustomerId() => new()
    {
        CustomerId = "",
        CustomerLevel = CustomerLevel.一般會員,
        Items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "測試商品", UnitPrice = 100m, Quantity = 1 }
        }
    };
}
```

**AutoFixture 整合：**

```csharp
public class AutoFixtureOrderTestData : IEnumerable<Order>
{
    private readonly Fixture _fixture;

    public AutoFixtureOrderTestData()
    {
        _fixture = new Fixture();
        
        _fixture.Customize<Order>(composer => composer
            .With(o => o.CustomerId, () => $"CUST{_fixture.Create<int>() % 1000:D3}")
            .With(o => o.CustomerLevel, () => _fixture.Create<CustomerLevel>())
            .With(o => o.Items, () => _fixture.CreateMany<OrderItem>(Random.Shared.Next(1, 5)).ToList()));

        _fixture.Customize<OrderItem>(composer => composer
            .With(oi => oi.ProductId, () => $"PROD{_fixture.Create<int>() % 1000:D3}")
            .With(oi => oi.ProductName, () => $"測試商品{_fixture.Create<int>() % 100}")
            .With(oi => oi.UnitPrice, () => Math.Round(_fixture.Create<decimal>() % 1000 + 1, 2))
            .With(oi => oi.Quantity, () => _fixture.Create<int>() % 10 + 1));
    }

    public IEnumerator<Order> GetEnumerator()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return _fixture.Create<Order>();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

## Matrix Tests：組合測試

自動產生所有參數組合的測試案例：

```csharp
[Test]
[MatrixDataSource]
public async Task CalculateShipping_客戶等級與金額組合_應遵循運費規則(
    [Matrix(0, 1, 2, 3)] CustomerLevel customerLevel, // 0=一般會員, 1=VIP會員, 2=白金會員, 3=鑽石會員
    [Matrix(100, 500, 1000, 2000)] decimal orderAmount)
{
    // Arrange
    var calculator = new ShippingCalculator();
    var order = new Order
    {
        CustomerLevel = customerLevel,
        Items = [new OrderItem { UnitPrice = orderAmount, Quantity = 1 }]
    };

    // Act
    var shippingFee = calculator.CalculateShippingFee(order);
    var isFreeShipping = calculator.IsEligibleForFreeShipping(order);

    // Assert
    if (isFreeShipping)
    {
        await Assert.That(shippingFee).IsEqualTo(0m);
    }
    else
    {
        await Assert.That(shippingFee).IsGreaterThan(0m);
    }

    // 驗證特定規則
    switch (customerLevel)
    {
        case CustomerLevel.鑽石會員:
            await Assert.That(shippingFee).IsEqualTo(0m); // 鑽石會員永遠免運
            break;
        case CustomerLevel.VIP會員 or CustomerLevel.白金會員:
            if (orderAmount < 1000m)
                await Assert.That(shippingFee).IsEqualTo(40m); // VIP+ 運費半價
            break;
        case CustomerLevel.一般會員:
            if (orderAmount < 1000m)
                await Assert.That(shippingFee).IsEqualTo(80m); // 一般會員標準運費
            break;
    }
}
```

**⚠️ Matrix Tests 注意事項：**

- 使用 `[MatrixDataSource]` 屬性標記測試方法
- 由於 C# 屬性限制，enum 必須用數值表示
- 限制參數組合數量，避免超過 50-100 個案例
- 這會產生 4 × 4 = 16 個測試案例
