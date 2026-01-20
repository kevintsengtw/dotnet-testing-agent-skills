// =============================================================================
// Bogus 基本使用範例
// 展示 Faker<T> 的基本語法、RuleFor 規則設定、資料產生方式
// =============================================================================

using Bogus;
using FluentAssertions;
using Xunit;

namespace BogusBasics.Templates;

#region 測試模型類別

// =============================================================================
// 測試模型類別
// =============================================================================

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsAvailable { get; set; }
}

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public bool IsPremium { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

#endregion

#region 基本 Faker<T> 使用

// =============================================================================
// 基本 Faker<T> 使用
// =============================================================================

public class BasicFakerUsageExamples
{
    /// <summary>
    /// 最基本的 Faker 使用方式
    /// </summary>
    [Fact]
    public void BasicFaker_產生單筆資料()
    {
        // Arrange - 建立 Faker<T> 並定義規則
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)              // 遞增索引
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())   // 產品名稱
            .RuleFor(p => p.Category, f => f.Commerce.Department()) // 類別
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000)) // 價格
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())   // 描述
            .RuleFor(p => p.CreatedDate, f => f.Date.Past())        // 過去日期
            .RuleFor(p => p.IsAvailable, f => f.Random.Bool());     // 布林值

        // Act - 產生單筆資料
        var product = productFaker.Generate();

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().NotBeNullOrEmpty();
        product.Price.Should().BeInRange(10, 1000);
        product.CreatedDate.Should().BeBefore(DateTime.Now);
    }

    /// <summary>
    /// 產生多筆資料
    /// </summary>
    [Fact]
    public void BasicFaker_產生多筆資料()
    {
        // Arrange
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000));

        // Act - 產生 10 筆資料
        var products = productFaker.Generate(10);

        // Assert
        products.Should().HaveCount(10);
        products.Select(p => p.Id).Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// 使用 Generate 與 GenerateLazy 的差異
    /// </summary>
    [Fact]
    public void GenerateLazy_延遲產生資料()
    {
        // Arrange
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName());

        // Act - GenerateLazy 回傳 IEnumerable，延遲執行
        IEnumerable<Product> lazyProducts = productFaker.GenerateLazy(100);
        
        // 只取前 5 筆，不會產生全部 100 筆
        var firstFive = lazyProducts.Take(5).ToList();

        // Assert
        firstFive.Should().HaveCount(5);
    }
}

#endregion

#region RuleFor 規則設定

// =============================================================================
// RuleFor 規則設定
// =============================================================================

public class RuleForExamples
{
    /// <summary>
    /// 基本 RuleFor 使用各種資料類型
    /// </summary>
    [Fact]
    public void RuleFor_各種資料類型()
    {
        var customerFaker = new Faker<Customer>()
            // GUID
            .RuleFor(c => c.Id, f => f.Random.Guid())
            // 字串 - 使用 Person DataSet
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 字串 - 使用 Internet DataSet
            .RuleFor(c => c.Email, f => f.Internet.Email())
            // 字串 - 使用 Phone DataSet
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            // 字串 - 使用 Address DataSet
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            // 日期
            .RuleFor(c => c.BirthDate, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
            // 布林
            .RuleFor(c => c.IsPremium, f => f.Random.Bool());

        var customer = customerFaker.Generate();

        customer.Id.Should().NotBe(Guid.Empty);
        customer.Email.Should().Contain("@");
    }

    /// <summary>
    /// RuleFor 參考其他屬性
    /// </summary>
    [Fact]
    public void RuleFor_參考其他屬性()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 根據 Name 產生 Email（使用 (f, c) 參數）
            .RuleFor(c => c.Email, (f, c) => 
            {
                var nameParts = c.Name.Split(' ');
                var firstName = nameParts.FirstOrDefault() ?? "user";
                var lastName = nameParts.LastOrDefault() ?? "name";
                return f.Internet.Email(firstName, lastName, "company.com");
            });

        var customer = customerFaker.Generate();

        customer.Email.Should().EndWith("@company.com");
    }

    /// <summary>
    /// 使用 IndexFaker 產生遞增 ID
    /// </summary>
    [Fact]
    public void IndexFaker_遞增ID()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName());

        var products = productFaker.Generate(5);

        products[0].Id.Should().Be(0);
        products[1].Id.Should().Be(1);
        products[2].Id.Should().Be(2);
        products[3].Id.Should().Be(3);
        products[4].Id.Should().Be(4);
    }

    /// <summary>
    /// 使用 RuleFor 設定巢狀物件
    /// </summary>
    [Fact]
    public void RuleFor_巢狀物件()
    {
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker)
            .RuleFor(o => o.CustomerId, f => f.Random.Guid())
            .RuleFor(o => o.CustomerName, f => f.Person.FullName)
            .RuleFor(o => o.OrderDate, f => f.Date.Past())
            .RuleFor(o => o.Status, f => f.PickRandom("Pending", "Processing", "Shipped", "Delivered"))
            // 產生 1-5 個訂單明細
            .RuleFor(o => o.Items, f =>
            {
                var itemFaker = new Faker<OrderItem>()
                    .RuleFor(i => i.Id, f => f.IndexFaker)
                    .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
                    .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
                    .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(10, 500));
                
                return itemFaker.Generate(f.Random.Int(1, 5));
            })
            // 計算總金額
            .RuleFor(o => o.TotalAmount, (f, o) => o.Items.Sum(i => i.Subtotal));

        var order = orderFaker.Generate();

        order.Items.Should().HaveCountGreaterOrEqualTo(1);
        order.TotalAmount.Should().Be(order.Items.Sum(i => i.Subtotal));
    }
}

#endregion

#region 可重現性控制（Seed）

// =============================================================================
// 可重現性控制（Seed）
// =============================================================================

public class SeedExamples
{
    /// <summary>
    /// 使用 Seed 確保可重現的測試資料
    /// </summary>
    [Fact]
    public void Seed_確保資料可重現()
    {
        // Arrange - 設定相同的 seed
        Randomizer.Seed = new Random(12345);
        
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 100));

        var products1 = productFaker.Generate(3);

        // 重置 seed
        Randomizer.Seed = new Random(12345);
        var products2 = productFaker.Generate(3);

        // Assert - 相同的 seed 產生相同的資料
        for (int i = 0; i < 3; i++)
        {
            products1[i].Name.Should().Be(products2[i].Name);
            products1[i].Price.Should().Be(products2[i].Price);
        }

        // 清理 - 重置為隨機
        Randomizer.Seed = new Random();
    }

    /// <summary>
    /// 使用 UseSeed 方法設定單一 Faker 的 seed
    /// </summary>
    [Fact]
    public void UseSeed_單一Faker設定Seed()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .UseSeed(42);  // 設定這個 Faker 的 seed

        var products1 = productFaker.Generate(3);
        
        // 重新建立 Faker 使用相同 seed
        var productFaker2 = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .UseSeed(42);

        var products2 = productFaker2.Generate(3);

        // 相同結果
        for (int i = 0; i < 3; i++)
        {
            products1[i].Name.Should().Be(products2[i].Name);
        }
    }
}

#endregion

#region 條件式產生

// =============================================================================
// 條件式產生
// =============================================================================

public class ConditionalGenerationExamples
{
    /// <summary>
    /// 使用 PickRandom 選擇固定選項
    /// </summary>
    [Fact]
    public void PickRandom_固定選項選擇()
    {
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Status, f => f.PickRandom("Pending", "Processing", "Shipped", "Delivered"));

        var orders = orderFaker.Generate(100);

        orders.All(o => new[] { "Pending", "Processing", "Shipped", "Delivered" }.Contains(o.Status))
            .Should().BeTrue();
    }

    /// <summary>
    /// 使用 PickRandomWeighted 權重選擇
    /// </summary>
    [Fact]
    public void PickRandomWeighted_權重選擇()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 70% User, 25% Admin, 5% SuperAdmin
            .RuleFor(c => c.Address, f => f.PickRandomWeighted(
                new[] { "User", "Admin", "SuperAdmin" },
                new[] { 0.7f, 0.25f, 0.05f }));

        var customers = customerFaker.Generate(1000);

        // 統計分布應該接近設定的權重
        var userCount = customers.Count(c => c.Address == "User");
        userCount.Should().BeGreaterThan(600); // 約 70%
    }

    /// <summary>
    /// 使用 OrNull 產生可能為 null 的值
    /// </summary>
    [Fact]
    public void OrNull_產生可能為Null的值()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 50% 機率為 null
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber().OrNull(f, 0.5f));

        var customers = customerFaker.Generate(100);

        var nullCount = customers.Count(c => c.Phone == null);
        nullCount.Should().BeGreaterThan(30).And.BeLessThan(70); // 約 50%
    }

    /// <summary>
    /// 使用 Bool 設定機率
    /// </summary>
    [Fact]
    public void Bool_機率控制()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 20% 機率為 Premium
            .RuleFor(c => c.IsPremium, f => f.Random.Bool(0.2f));

        var customers = customerFaker.Generate(1000);

        var premiumCount = customers.Count(c => c.IsPremium);
        premiumCount.Should().BeGreaterThan(150).And.BeLessThan(250); // 約 20%
    }
}

#endregion

#region 多語言支援

// =============================================================================
// 多語言支援
// =============================================================================

public class LocalizationExamples
{
    /// <summary>
    /// 使用繁體中文語系
    /// </summary>
    [Fact]
    public void Localization_繁體中文()
    {
        // 使用 zh_TW 語系
        var customerFaker = new Faker<Customer>("zh_TW")
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Person.FullName)
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber());

        var customer = customerFaker.Generate();

        customer.Name.Should().NotBeNullOrEmpty();
        customer.Address.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// 使用日文語系
    /// </summary>
    [Fact]
    public void Localization_日文()
    {
        var customerFaker = new Faker<Customer>("ja")
            .RuleFor(c => c.Name, f => f.Person.FullName)
            .RuleFor(c => c.Address, f => f.Address.FullAddress());

        var customer = customerFaker.Generate();

        customer.Name.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// 動態語系選擇
    /// </summary>
    [Fact]
    public void Localization_動態語系()
    {
        var locales = new[] { "en_US", "zh_TW", "ja", "ko", "fr" };
        
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Address, f => f.PickRandom(locales)) // 暫存語系
            .RuleFor(c => c.Name, (f, c) =>
            {
                var localFaker = new Faker(c.Address); // 使用暫存的語系
                return localFaker.Person.FullName;
            });

        var customers = customerFaker.Generate(5);

        customers.Should().AllSatisfy(c => c.Name.Should().NotBeNullOrEmpty());
    }
}

#endregion
