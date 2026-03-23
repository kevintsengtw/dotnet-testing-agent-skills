# Test Data Builder 最佳實踐與進階模式

## 1. 提供合理的預設值

**良好實踐：預設值讓物件處於有效狀態**

```csharp
public class ProductBuilder
{
    private string _name = "Default Product";
    private decimal _price = 100m;
    private int _stock = 10;
    private bool _isAvailable = true;

    // 預設值確保建立的物件是有效的
    public Product Build() => new()
    {
        Name = _name,
        Price = _price,
        Stock = _stock,
        IsAvailable = _isAvailable
    };
}
```

## 2. 使用語意化的命名

**良好實踐：方法名稱表達測試意圖**

```csharp
public static class UserScenarios
{
    public static UserBuilder ANewUser() => UserBuilder.AUser()
        .CreatedOn(DateTime.UtcNow);

    public static UserBuilder AnExpiredUser() => UserBuilder.AUser()
        .CreatedOn(DateTime.UtcNow.AddYears(-5))
        .IsInactive();

    public static UserBuilder APremiumUser() => UserBuilder.AUser()
        .WithRoles("Premium", "User")
        .WithSettings(new UserSettings { FeatureFlags = new[] { "AdvancedSearch" } });
}
```

## 3. Builder 之間的組合

**良好實踐：Builder 可以組合使用**

```csharp
public class OrderBuilder
{
    private User _customer = UserBuilder.AUser().Build();
    private List<Product> _products = new();
    private decimal _totalAmount = 0m;

    public OrderBuilder ForCustomer(User customer)
    {
        _customer = customer;
        return this;
    }

    public OrderBuilder WithProduct(Product product)
    {
        _products.Add(product);
        _totalAmount += product.Price;
        return this;
    }

    public OrderBuilder WithProducts(params Product[] products)
    {
        _products.AddRange(products);
        _totalAmount = _products.Sum(p => p.Price);
        return this;
    }

    public Order Build() => new()
    {
        Customer = _customer,
        Products = _products,
        TotalAmount = _totalAmount,
        OrderDate = DateTime.UtcNow
    };
}

// 使用組合的 Builder
var order = new OrderBuilder()
    .ForCustomer(UserBuilder.APremiumUser().Build())
    .WithProducts(
        ProductBuilder.AProduct().WithPrice(100m).Build(),
        ProductBuilder.AProduct().WithPrice(200m).Build()
    )
    .Build();
```

## 4. 避免過度複雜化

**不良實踐：Builder 過於複雜**

```csharp
// 避免在 Builder 中加入複雜的業務邏輯
public UserBuilder WithComplexValidation()
{
    // ❌ 不要在 Builder 中進行複雜驗證
    if (_email.Contains("@"))
    {
        var parts = _email.Split('@');
        if (parts[1].Length > 10)
        {
            _email = parts[0] + "@short.com";
        }
    }
    return this;
}
```

**良好實踐：保持 Builder 簡單**

```csharp
// Builder 只負責建立物件，不包含業務邏輯
public UserBuilder WithShortDomainEmail()
{
    _email = "user@short.com";
    return this;
}
```

## 5. 統一管理測試資料

**良好實踐：建立共享的測試資料類別**

```csharp
public static class TestData
{
    public static class Users
    {
        public static User John => UserBuilder.AUser()
            .WithName("John Doe")
            .WithEmail("john@example.com")
            .Build();

        public static User AdminUser => UserBuilder.AnAdminUser()
            .WithName("Admin User")
            .WithEmail("admin@company.com")
            .Build();
    }

    public static class Products
    {
        public static Product Laptop => ProductBuilder.AProduct()
            .WithName("Laptop")
            .WithPrice(1000m)
            .Build();
    }
}

// 在測試中使用
[Fact]
public void ProcessOrder_有效訂單_應成功處理()
{
    var order = new OrderBuilder()
        .ForCustomer(TestData.Users.John)
        .WithProduct(TestData.Products.Laptop)
        .Build();
    // ...
}
```
