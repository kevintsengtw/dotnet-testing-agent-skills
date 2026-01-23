using System;
using Xunit;
using AwesomeAssertions;
using NSubstitute;

/// <summary>
/// 策略模式重構範例
/// 展示如何透過策略模式改善可測試性，避免測試私有方法
/// </summary>
/// 
// ========================================
// 重構前：難以測試的設計
// ========================================

namespace MyProject.BeforeRefactoring;

/// <summary>
/// 重構前：包含複雜私有方法的定價服務
/// </summary>
public class PricingService
{
    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        
        // 複雜的私有方法，難以獨立測試
        var discount = CalculateDiscount(customer, product);
        var tax = CalculateTax(product, customer.Location);
        
        return basePrice - discount + tax;
    }

    /// <summary>
    /// 私有方法：計算折扣（20 行複雜邏輯）
    /// </summary>
    private decimal CalculateDiscount(Customer customer, Product product)
    {
        decimal discount = 0;

        // VIP 折扣
        if (customer.IsVIP)
            discount += product.BasePrice * 0.1m;

        // 大量購買折扣
        if (customer.PurchaseHistory > 10000)
            discount += product.BasePrice * 0.05m;

        // 季節性折扣
        if (DateTime.Now.Month == 12)
            discount += product.BasePrice * 0.05m;

        // 商品類別折扣
        if (product.Category == "Electronics")
            discount += product.BasePrice * 0.03m;

        return Math.Min(discount, product.BasePrice * 0.3m); // 最高 30% 折扣
    }

    /// <summary>
    /// 私有方法：計算稅金（15 行複雜邏輯）
    /// </summary>
    private decimal CalculateTax(Product product, Location location)
    {
        var taxRate = 0.05m; // 基本稅率

        // 根據地區調整
        if (location.Country == "TW")
        {
            if (location.City == "Taipei")
                taxRate = 0.05m;
            else
                taxRate = 0.03m;
        }

        // 商品類型調整
        if (product.Category == "Food")
            taxRate = 0m; // 食品免稅

        return product.BasePrice * taxRate;
    }
}


// ========================================
// 重構後：使用策略模式
// ========================================

namespace MyProject.AfterRefactoring;

// ========================================
// 1. 定義策略介面
// ========================================

/// <summary>
/// 折扣計算策略介面
/// </summary>
public interface IDiscountStrategy
{
    decimal Calculate(Customer customer, Product product);
}

/// <summary>
/// 稅金計算策略介面
/// </summary>
public interface ITaxStrategy
{
    decimal Calculate(Product product, Location location);
}


// ========================================
// 2. 實作具體策略
// ========================================

/// <summary>
/// 標準折扣策略
/// </summary>
public class StandardDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Customer customer, Product product)
    {
        decimal discount = 0;

        // VIP 折扣
        if (customer.IsVIP)
            discount += product.BasePrice * 0.1m;

        // 大量購買折扣
        if (customer.PurchaseHistory > 10000)
            discount += product.BasePrice * 0.05m;

        return Math.Min(discount, product.BasePrice * 0.3m);
    }
}

/// <summary>
/// 季節性折扣策略
/// </summary>
public class SeasonalDiscountStrategy : IDiscountStrategy
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SeasonalDiscountStrategy(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public decimal Calculate(Customer customer, Product product)
    {
        var baseStrategy = new StandardDiscountStrategy();
        var baseDiscount = baseStrategy.Calculate(customer, product);

        // 12 月額外折扣
        if (_dateTimeProvider.Now.Month == 12)
            baseDiscount += product.BasePrice * 0.05m;

        return Math.Min(baseDiscount, product.BasePrice * 0.3m);
    }
}

/// <summary>
/// 台灣稅金策略
/// </summary>
public class TaiwanTaxStrategy : ITaxStrategy
{
    public decimal Calculate(Product product, Location location)
    {
        // 食品免稅
        if (product.Category == "Food")
            return 0;

        // 根據城市決定稅率
        var taxRate = location.City == "Taipei" ? 0.05m : 0.03m;
        
        return product.BasePrice * taxRate;
    }
}


// ========================================
// 3. 改進的定價服務
// ========================================

/// <summary>
/// 重構後的定價服務
/// 使用策略模式，依賴注入，易於測試
/// </summary>
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


// ========================================
// 測試範例
// ========================================

namespace MyProject.Tests;

/// <summary>
/// 策略獨立測試：折扣策略
/// </summary>
public class StandardDiscountStrategyTests
{
    [Fact]
    public void Calculate_VIP客戶_應給予10%折扣()
    {
        // Arrange
        var strategy = new StandardDiscountStrategy();
        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(100m); // 1000 * 0.1 = 100
    }

    [Fact]
    public void Calculate_大量購買客戶_應給予5%折扣()
    {
        // Arrange
        var strategy = new StandardDiscountStrategy();
        var customer = new Customer { IsVIP = false, PurchaseHistory = 15000 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(50m); // 1000 * 0.05 = 50
    }

    [Fact]
    public void Calculate_VIP且大量購買_應給予累計折扣但不超過30%()
    {
        // Arrange
        var strategy = new StandardDiscountStrategy();
        var customer = new Customer { IsVIP = true, PurchaseHistory = 15000 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(150m); // (10% + 5%) = 15%
    }
}

/// <summary>
/// 季節性折扣策略測試
/// </summary>
public class SeasonalDiscountStrategyTests
{
    [Fact]
    public void Calculate_12月_應額外給予5%折扣()
    {
        // Arrange
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.Now.Returns(new DateTime(2024, 12, 15));
        
        var strategy = new SeasonalDiscountStrategy(dateTimeProvider);
        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(150m); // VIP 10% + 季節 5% = 15%
    }

    [Fact]
    public void Calculate_非12月_不應額外折扣()
    {
        // Arrange
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.Now.Returns(new DateTime(2024, 6, 15));
        
        var strategy = new SeasonalDiscountStrategy(dateTimeProvider);
        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(100m); // 只有 VIP 10%
    }
}

/// <summary>
/// 稅金策略測試
/// </summary>
public class TaiwanTaxStrategyTests
{
    [Fact]
    public void Calculate_台北地區_應計算5%稅金()
    {
        // Arrange
        var strategy = new TaiwanTaxStrategy();
        var product = new Product { BasePrice = 1000m, Category = "Electronics" };
        var location = new Location { Country = "TW", City = "Taipei" };

        // Act
        var tax = strategy.Calculate(product, location);

        // Assert
        tax.Should().Be(50m); // 1000 * 0.05 = 50
    }

    [Fact]
    public void Calculate_非台北地區_應計算3%稅金()
    {
        // Arrange
        var strategy = new TaiwanTaxStrategy();
        var product = new Product { BasePrice = 1000m, Category = "Electronics" };
        var location = new Location { Country = "TW", City = "Kaohsiung" };

        // Act
        var tax = strategy.Calculate(product, location);

        // Assert
        tax.Should().Be(30m); // 1000 * 0.03 = 30
    }

    [Fact]
    public void Calculate_食品類別_應免稅()
    {
        // Arrange
        var strategy = new TaiwanTaxStrategy();
        var product = new Product { BasePrice = 1000m, Category = "Food" };
        var location = new Location { Country = "TW", City = "Taipei" };

        // Act
        var tax = strategy.Calculate(product, location);

        // Assert
        tax.Should().Be(0m);
    }
}

/// <summary>
/// 定價服務整合測試
/// </summary>
public class PricingServiceTests
{
    [Fact]
    public void CalculatePrice_整合測試_應正確計算最終價格()
    {
        // Arrange
        var discountStrategy = new StandardDiscountStrategy();
        var taxStrategy = new TaiwanTaxStrategy();
        var service = new PricingService(discountStrategy, taxStrategy);

        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m, Category = "Electronics" };
        var location = new Location { Country = "TW", City = "Taipei" };
        customer.Location = location;

        // Act
        var finalPrice = service.CalculatePrice(product, customer);

        // Assert
        // 1000 (基本價格) - 100 (VIP 10% 折扣) + 50 (5% 稅金) = 950
        finalPrice.Should().Be(950m);
    }

    [Fact]
    public void CalculatePrice_使用Mock_可以獨立測試邏輯()
    {
        // Arrange
        var discountStrategy = Substitute.For<IDiscountStrategy>();
        var taxStrategy = Substitute.For<ITaxStrategy>();
        
        discountStrategy.Calculate(Arg.Any<Customer>(), Arg.Any<Product>())
            .Returns(100m);
        taxStrategy.Calculate(Arg.Any<Product>(), Arg.Any<Location>())
            .Returns(50m);

        var service = new PricingService(discountStrategy, taxStrategy);
        var customer = new Customer();
        var product = new Product { BasePrice = 1000m };

        // Act
        var finalPrice = service.CalculatePrice(product, customer);

        // Assert
        finalPrice.Should().Be(950m); // 1000 - 100 + 50
        
        // 驗證策略被正確呼叫
        discountStrategy.Received(1).Calculate(customer, product);
        taxStrategy.Received(1).Calculate(product, customer.Location);
    }
}


// ========================================
// 支援類別定義
// ========================================

public class Product
{
    public decimal BasePrice { get; set; }
    public string Category { get; set; }
}

public class Customer
{
    public bool IsVIP { get; set; }
    public decimal PurchaseHistory { get; set; }
    public Location Location { get; set; }
}

public class Location
{
    public string Country { get; set; }
    public string City { get; set; }
}

public interface IDateTimeProvider
{
    DateTime Now { get; }
}


// ========================================
// 重構前後的比較
// ========================================

/*
重構前的問題：

1. 測試困難
   ❌ 私有方法無法直接測試
   ❌ 需要使用反射，增加維護成本
   ❌ 測試脆弱，重構時容易失敗

2. 設計問題
   ❌ 單一類別承擔多個職責
   ❌ 違反開放封閉原則（新增折扣類型需修改類別）
   ❌ 難以擴展新的策略

3. 可讀性問題
   ❌ 複雜邏輯藏在私有方法中
   ❌ 難以理解整體設計意圖

重構後的優點：

1. 測試友善
   ✅ 每個策略可以獨立測試
   ✅ 不需要使用反射
   ✅ 測試穩定，重構時不易失敗

2. 設計優良
   ✅ 符合單一職責原則
   ✅ 符合開放封閉原則
   ✅ 符合依賴反轉原則
   ✅ 易於擴展新策略

3. 可讀性高
   ✅ 意圖清楚（折扣策略、稅金策略）
   ✅ 易於理解和維護
   ✅ 職責分明

4. 靈活性高
   ✅ 可以動態切換策略
   ✅ 可以組合不同的策略
   ✅ 易於測試不同的組合

重構步驟：

1. 識別複雜的私有方法
2. 定義策略介面
3. 將私有方法邏輯提取為策略實作
4. 修改原類別使用依賴注入
5. 撰寫獨立的策略測試
6. 撰寫整合測試驗證組合

何時應該考慮策略模式重構：

✅ 私有方法超過 10 行
✅ 包含重要的業務規則
✅ 有多種變體或演算法
✅ 需要經常擴展新的實作
✅ 測試覆蓋率不足

記住：好的設計自然就有好的可測試性。
與其糾結如何測試私有方法，不如改善設計讓測試變得簡單。
*/
