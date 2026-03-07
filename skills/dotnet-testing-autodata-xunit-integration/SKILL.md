---
name: dotnet-testing-autodata-xunit-integration
description: |
  AutoFixture 與 xUnit 整合完整指南。當需要使用 AutoData 或 InlineAutoData 簡化 xUnit 參數化測試資料準備時使用。涵蓋自訂 Customization 與測試資料屬性，提升測試可讀性與維護性。
  Make sure to use this skill whenever the user mentions AutoData, InlineAutoData, MemberAutoData, AutoFixture xUnit integration, or parameterized test data attributes, even if they don't explicitly ask for AutoData guidance.
  Keywords: AutoData, InlineAutoData, AutoFixture xUnit, [AutoData], [InlineAutoData], AutoDataAttribute, ICustomization, DataAttribute, 參數化測試, Theory AutoData, MemberAutoData, 測試資料屬性, fixture.Customize
---

# AutoData 屬性家族：xUnit 與 AutoFixture 的整合應用

## 概述

AutoData 屬性家族是 `AutoFixture.Xunit2` 套件提供的功能，將 AutoFixture 的資料產生能力與 xUnit 的參數化測試整合，讓測試參數自動注入，大幅減少測試準備程式碼。

### 核心特色

1. **AutoData**：自動產生所有測試參數
2. **InlineAutoData**：混合固定值與自動產生
3. **MemberAutoData**：結合外部資料來源
4. **CompositeAutoData**：多重資料來源整合
5. **CollectionSizeAttribute**：控制集合產生數量

## 安裝套件

```xml
<PackageReference Include="AutoFixture" Version="4.18.1" />
<PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
```

```bash
dotnet add package AutoFixture.Xunit2
```

## AutoData：完全自動產生參數

`AutoData` 是最基礎的屬性，自動為測試方法的所有參數產生測試資料。

### 基本使用

```csharp
using AutoFixture.Xunit2;

public class Person
{
    public Guid Id { get; set; }

    [StringLength(10)]
    public string Name { get; set; } = string.Empty;

    [Range(18, 80)]
    public int Age { get; set; }

    public string Email { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}

[Theory]
[AutoData]
public void AutoData_應能自動產生所有參數(Person person, string message, int count)
{
    // Arrange & Act - 參數已由 AutoData 自動產生

    // Assert
    person.Should().NotBeNull();
    person.Id.Should().NotBe(Guid.Empty);
    person.Name.Should().HaveLength(10);     // 遵循 StringLength 限制
    person.Age.Should().BeInRange(18, 80);   // 遵循 Range 限制
    message.Should().NotBeNullOrEmpty();
    count.Should().NotBe(0);
}
```

### 透過 DataAnnotation 約束參數

```csharp
[Theory]
[AutoData]
public void AutoData_透過DataAnnotation約束參數(
    [StringLength(5, MinimumLength = 3)] string shortName,
    [Range(1, 100)] int percentage,
    Person person)
{
    // Assert
    shortName.Length.Should().BeInRange(3, 5);
    percentage.Should().BeInRange(1, 100);
    person.Should().NotBeNull();
}
```

## InlineAutoData：混合固定值與自動產生

`InlineAutoData` 結合了 `InlineData` 的固定值特性與 `AutoData` 的自動產生功能。

### 基本語法

```csharp
[Theory]
[InlineAutoData("VIP客戶", 1000)]
[InlineAutoData("一般客戶", 500)]
[InlineAutoData("新客戶", 100)]
public void InlineAutoData_混合固定值與自動產生(
    string customerType,    // 對應第 1 個固定值
    decimal creditLimit,    // 對應第 2 個固定值
    Person person)          // 由 AutoFixture 產生
{
    // Arrange
    var customer = new Customer
    {
        Person = person,
        Type = customerType,
        CreditLimit = creditLimit
    };

    // Assert
    customer.Type.Should().Be(customerType);
    customer.CreditLimit.Should().BeOneOf(1000, 500, 100);
    customer.Person.Should().NotBeNull();
}
```

### 參數順序一致性

固定值的順序必須與方法參數順序一致：

```csharp
[Theory]
[InlineAutoData("產品A", 100)]  // 依序對應 name, price
[InlineAutoData("產品B", 200)]
public void InlineAutoData_參數順序一致性(
    string name,        // 對應第 1 個固定值
    decimal price,      // 對應第 2 個固定值
    string description, // 由 AutoFixture 產生
    Product product)    // 由 AutoFixture 產生
{
    // Assert
    name.Should().BeOneOf("產品A", "產品B");
    price.Should().BeOneOf(100, 200);
    description.Should().NotBeNullOrEmpty();
    product.Should().NotBeNull();
}
```

### 重要限制：只能使用編譯時常數

```csharp
// 正確：使用常數
[InlineAutoData("VIP", 100000)]
[InlineAutoData("Premium", 50000)]

// 錯誤：不能使用變數
private const decimal VipCreditLimit = 100000m;
[InlineAutoData("VIP", VipCreditLimit)]  // 編譯錯誤

// 錯誤：不能使用運算式
[InlineAutoData("VIP", 100 * 1000)]  // 編譯錯誤
```

需要使用複雜資料時，應使用 `MemberAutoData`。

## MemberAutoData：結合外部資料來源

`MemberAutoData` 允許從類別的方法、屬性或欄位中獲取測試資料。

### 使用靜態方法

```csharp
public class MemberAutoDataTests
{
    public static IEnumerable<object[]> GetProductCategories()
    {
        yield return new object[] { "3C產品", "TECH" };
        yield return new object[] { "服飾配件", "FASHION" };
        yield return new object[] { "居家生活", "HOME" };
        yield return new object[] { "運動健身", "SPORTS" };
    }

    [Theory]
    [MemberAutoData(nameof(GetProductCategories))]
    public void MemberAutoData_使用靜態方法資料(
        string categoryName,   // 來自 GetProductCategories
        string categoryCode,   // 來自 GetProductCategories
        Product product)       // 由 AutoFixture 產生
    {
        // Arrange
        var categorizedProduct = new CategorizedProduct
        {
            Product = product,
            CategoryName = categoryName,
            CategoryCode = categoryCode
        };

        // Assert
        categorizedProduct.CategoryName.Should().Be(categoryName);
        categorizedProduct.CategoryCode.Should().Be(categoryCode);
        categorizedProduct.Product.Should().NotBeNull();
    }
}
```

### 使用靜態屬性

```csharp
public static IEnumerable<object[]> StatusTransitions => new[]
{
    new object[] { OrderStatus.Created, OrderStatus.Confirmed },
    new object[] { OrderStatus.Confirmed, OrderStatus.Shipped },
    new object[] { OrderStatus.Shipped, OrderStatus.Delivered },
    new object[] { OrderStatus.Delivered, OrderStatus.Completed }
};

[Theory]
[MemberAutoData(nameof(StatusTransitions))]
public void MemberAutoData_使用靜態屬性_訂單狀態轉換(
    OrderStatus fromStatus,
    OrderStatus toStatus,
    Order order)
{
    // Arrange
    order.Status = fromStatus;

    // Act
    order.TransitionTo(toStatus);

    // Assert
    order.Status.Should().Be(toStatus);
}
```

## 自訂 AutoData 屬性

建立專屬的 AutoData 配置：

### DomainAutoDataAttribute

```csharp
public class DomainAutoDataAttribute : AutoDataAttribute
{
    public DomainAutoDataAttribute() : base(() => CreateFixture())
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        // 設定 Person 的自訂規則
        fixture.Customize<Person>(composer => composer
            .With(p => p.Age, () => Random.Shared.Next(18, 65))
            .With(p => p.Email, () => $"user{Random.Shared.Next(1000)}@example.com")
            .With(p => p.Name, () => $"測試用戶{Random.Shared.Next(100)}"));

        // 設定 Product 的自訂規則
        fixture.Customize<Product>(composer => composer
            .With(p => p.Price, () => Random.Shared.Next(100, 10000))
            .With(p => p.IsAvailable, true)
            .With(p => p.Name, () => $"產品{Random.Shared.Next(1000)}"));

        return fixture;
    }
}
```

### BusinessAutoDataAttribute

```csharp
public class BusinessAutoDataAttribute : AutoDataAttribute
{
    public BusinessAutoDataAttribute() : base(() => CreateFixture())
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        // 設定 Order 的業務規則
        fixture.Customize<Order>(composer => composer
            .With(o => o.Status, OrderStatus.Created)
            .With(o => o.Amount, () => Random.Shared.Next(1000, 50000))
            .With(o => o.OrderNumber, () => $"ORD{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000):D4}"));

        return fixture;
    }
}
```

### 使用自訂 AutoData

```csharp
[Theory]
[DomainAutoData]
public void 使用DomainAutoData(Person person, Product product)
{
    person.Age.Should().BeInRange(18, 64);
    person.Email.Should().EndWith("@example.com");
    product.IsAvailable.Should().BeTrue();
}

[Theory]
[BusinessAutoData]
public void 使用BusinessAutoData(Order order)
{
    order.Status.Should().Be(OrderStatus.Created);
    order.Amount.Should().BeInRange(1000, 49999);
    order.OrderNumber.Should().StartWith("ORD");
}
```

## CompositeAutoData：多重資料來源整合

透過繼承 `AutoDataAttribute` 並以反射合併多個 Fixture 的 Customizations，組合多個自訂 AutoData 配置：

```csharp
// 使用方式 - 自動套用 DomainAutoData + BusinessAutoData 的所有設定
[Theory]
[CompositeAutoData(typeof(DomainAutoDataAttribute), typeof(BusinessAutoDataAttribute))]
public void CompositeAutoData_整合多重資料來源(
    Person person, Product product, Order order)
{
    person.Age.Should().BeInRange(18, 64);       // DomainAutoData 的設定
    product.IsAvailable.Should().BeTrue();        // DomainAutoData 的設定
    order.Status.Should().Be(OrderStatus.Created); // BusinessAutoData 的設定
}
```

## CollectionSizeAttribute：控制集合大小

AutoData 預設的集合大小是 3，可透過自訂 `CollectionSizeAttribute` 控制產生的集合元素數量。完整實作與使用方式請參考 [CollectionSizeAttribute 完整指南](references/collection-size-attribute.md)。

## 外部測試資料整合

整合 CSV、JSON 等外部資料來源與 MemberAutoData，同時保留 AutoFixture 自動產生其餘參數的能力。完整指南請參考 [外部測試資料整合](references/external-data-integration.md)。

## 資料來源設計模式

階層式資料組織與可重用資料集的設計模式，建立 `BaseTestData` 基底類別統一管理測試資料路徑。完整範例請參考 [資料來源設計模式](references/data-source-patterns.md)。

## 與 Awesome Assertions 協作

```csharp
[Theory]
[InlineAutoData("VIP", 100000)]
[InlineAutoData("Premium", 50000)]
[InlineAutoData("Regular", 20000)]
public void AutoData與AwesomeAssertions協作_客戶等級驗證(
    string customerLevel,
    decimal expectedCreditLimit,
    [Range(1000, 15000)] decimal orderAmount,
    Customer customer,
    Order order)
{
    // Arrange
    customer.Type = customerLevel;
    customer.CreditLimit = expectedCreditLimit;
    order.Amount = orderAmount;

    // Act
    var canPlaceOrder = customer.CanPlaceOrder(order.Amount);
    var discountRate = CalculateDiscount(customer.Type, order.Amount);

    // Assert - 使用 Awesome Assertions 語法
    customer.Type.Should().Be(customerLevel);
    customer.CreditLimit.Should().Be(expectedCreditLimit);
    customer.CreditLimit.Should().BePositive();

    order.Amount.Should().BeInRange(1000m, 15000m);
    canPlaceOrder.Should().BeTrue();
    discountRate.Should().BeInRange(0m, 0.3m);
}

private static decimal CalculateDiscount(string customerType, decimal orderAmount)
{
    var baseDiscount = customerType switch
    {
        "VIP" => 0.15m,
        "Premium" => 0.10m,
        "Regular" => 0.05m,
        _ => 0m
    };

    var largeOrderBonus = orderAmount > 30000m ? 0.05m : 0m;
    return Math.Min(baseDiscount + largeOrderBonus, 0.3m);
}
```

## 最佳實踐

### 應該做

1. **善用 DataAnnotation**
   - 在參數上使用 `[StringLength]`、`[Range]` 約束資料範圍
   - 確保產生的資料符合業務規則

2. **建立可重用的自訂 AutoData**
   - 為不同領域建立專屬的 AutoData 屬性
   - 集中管理測試資料的產生規則

3. **使用 MemberAutoData 處理複雜資料**
   - 當 InlineAutoData 無法滿足需求時使用
   - 支援變數、運算式和外部資料來源

4. **組織測試資料來源**
   - 建立階層式的資料來源結構
   - 將相關資料集中管理

### 應該避免

1. **在 InlineAutoData 使用非常數值**
   - InlineAutoData 只接受編譯時常數
   - 需要動態值時改用 MemberAutoData

2. **過度複雜的 CompositeAutoData**
   - 避免組合過多的 AutoData 來源
   - 保持配置的可理解性

3. **忽略參數順序**
   - InlineAutoData 的固定值必須與參數順序一致
   - 錯誤的順序會導致型別不符

## 程式碼範本

請參考 [templates](./templates) 資料夾中的範例檔案：

- [autodata-attributes.cs](./templates/autodata-attributes.cs) - AutoData 屬性家族使用範例
- [external-data-integration.cs](./templates/external-data-integration.cs) - CSV/JSON 外部資料整合
- [advanced-patterns.cs](./templates/advanced-patterns.cs) - 進階模式與 CollectionSizeAttribute

## 與其他技能的關係

- **autofixture-basics**：本技能的前置知識
- **autofixture-customization**：自訂化策略可用於 AutoData 屬性
- **autofixture-nsubstitute-integration**：下一步學習目標
- **awesome-assertions-guide**：搭配使用提升測試可讀性

## 輸出格式

- 產生使用 `[Theory]` 搭配 `[AutoData]` 或 `[InlineAutoData]` 的測試方法
- 產生自訂 `AutoDataAttribute` 衍生類別檔案（`*AutoDataAttribute.cs`）
- 測試參數透過屬性注入，減少 Arrange 區段程式碼
- 固定值參數置於方法簽章前方，自動產生參數置於後方
- 搭配 DataAnnotation 約束參數範圍

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 12 - 結合 AutoData：xUnit 與 AutoFixture 的整合應用**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10375296
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day12

### 官方文件

- [AutoFixture Cheat Sheet - AutoData Theories](https://github.com/AutoFixture/AutoFixture/wiki/Cheat-Sheet#autodata-theories)
- [AutoDataAttribute API Reference](https://autofixture.io/api/AutoFixture.Xunit.AutoDataAttribute.html)
- [InlineAutoDataAttribute API Reference](https://autofixture.io/api/AutoFixture.Xunit.InlineAutoDataAttribute.html)
- [CsvHelper 官方文件](https://joshclose.github.io/CsvHelper/)
