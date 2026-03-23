# 重構模式與部分模擬 - 完整範例

## 核心原則：設計優先思維

### 解決方案：重構而非測試

```csharp
// ❌ 有問題的設計
public class OrderProcessor
{
    public OrderResult ProcessOrder(Order order)
    {
        // 使用多個複雜的私有方法
        var discount = CalculateDiscount(order); // 20 行邏輯
        var tax = CalculateTax(order, discount);  // 15 行邏輯
        // ...
    }

    private decimal CalculateDiscount(Order order) { /* 複雜邏輯 */ }
    private decimal CalculateTax(Order order, decimal discount) { /* 複雜邏輯 */ }
}

// ✅ 改進的設計：責任分離
public class OrderProcessor
{
    private readonly IDiscountCalculator _discountCalculator;
    private readonly ITaxCalculator _taxCalculator;

    public OrderProcessor(
        IDiscountCalculator discountCalculator,
        ITaxCalculator taxCalculator)
    {
        _discountCalculator = discountCalculator;
        _taxCalculator = taxCalculator;
    }

    public OrderResult ProcessOrder(Order order)
    {
        var discount = _discountCalculator.Calculate(order);
        var tax = _taxCalculator.Calculate(order, discount);
        // ...
    }
}

// 現在可以獨立測試每個計算器
public class DiscountCalculator : IDiscountCalculator
{
    public decimal Calculate(Order order)
    {
        // 複雜邏輯現在是公開方法，容易測試
    }
}
```

## 策略模式改善可測試性

### 重構前：難以測試的設計

```csharp
public class PricingService
{
    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        var discount = CalculateDiscount(customer, product); // 私有方法
        var tax = CalculateTax(product, customer.Location);   // 私有方法
        return basePrice - discount + tax;
    }

    private decimal CalculateDiscount(Customer customer, Product product)
    {
        // 20 行複雜的折扣計算邏輯
    }

    private decimal CalculateTax(Product product, Location location)
    {
        // 15 行複雜的稅率計算
    }
}
```

### 重構後：使用策略模式

```csharp
// 策略介面
public interface IDiscountStrategy
{
    decimal Calculate(Customer customer, Product product);
}

public interface ITaxStrategy
{
    decimal Calculate(Product product, Location location);
}

// 具體策略實作
public class StandardDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Customer customer, Product product)
    {
        // 折扣邏輯現在是公開方法，容易測試
        if (customer.IsVIP)
            return product.BasePrice * 0.1m;

        return 0;
    }
}

public class TaiwanTaxStrategy : ITaxStrategy
{
    public decimal Calculate(Product product, Location location)
    {
        // 稅率邏輯現在是公開方法，容易測試
        return product.BasePrice * 0.05m;
    }
}

// 改進的服務
public class PricingService
{
    private readonly IDiscountStrategy _discountStrategy;
    private readonly ITaxStrategy _taxStrategy;

    public PricingService(
        IDiscountStrategy discountStrategy,
        ITaxStrategy taxStrategy)
    {
        _discountStrategy = discountStrategy;
        _taxStrategy = taxStrategy;
    }

    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        var discount = _discountStrategy.Calculate(customer, product);
        var tax = _taxStrategy.Calculate(product, customer.Location);
        return basePrice - discount + tax;
    }
}
```

**優點：**

- 每個策略可以獨立測試
- 符合開放封閉原則
- 易於擴展新的策略
- 減少對反射的依賴

## 部分模擬（Partial Mock）

有時需要模擬類別的部分行為：

```csharp
// 需要部分模擬的類別
public class DataProcessor
{
    public ProcessResult Process(string input)
    {
        var validated = ValidateInput(input);
        if (!validated)
            return ProcessResult.InvalidInput();

        var data = TransformData(input);
        var saved = SaveData(data); // 想模擬這個方法避免實際資料庫操作

        return saved
            ? ProcessResult.Success()
            : ProcessResult.Failed();
    }

    protected virtual bool SaveData(string data)
    {
        // 實際的資料庫操作
        return true;
    }

    private bool ValidateInput(string input) => !string.IsNullOrEmpty(input);
    private string TransformData(string input) => input.ToUpper();
}

// 測試用的子類別
public class TestableDataProcessor : DataProcessor
{
    protected override bool SaveData(string data)
    {
        // 模擬實作，避免實際資料庫操作
        return true;
    }
}

// 測試
[Fact]
public void Process_使用部分模擬_應成功處理()
{
    // Arrange
    var processor = new TestableDataProcessor();

    // Act
    var result = processor.Process("test");

    // Assert
    result.Success.Should().BeTrue();
}
```
