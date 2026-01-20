// =============================================================================
// AutoFixture 與 xUnit 整合
// 展示 Fixture 共享、Theory 測試整合、實務應用場景
// =============================================================================

using AutoFixture;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Mail;

namespace TestProject.AutoFixtureBasics;

/// <summary>
/// 展示 AutoFixture 與 xUnit 的整合方式
/// </summary>
public class XunitIntegrationTests
{
    #region Fixture 共享模式

    /// <summary>
    /// 類別層級的 Fixture 共享
    /// </summary>
    public class ProductServiceTests
    {
        private readonly Fixture _fixture;

        public ProductServiceTests()
        {
            _fixture = new Fixture();

            // 在建構式中進行共同的客製化設定
            _fixture.Customize<ProductCreateRequest>(c => c
                .With(x => x.Price, () => Math.Round((decimal)Random.Shared.NextDouble() * 10000, 2))
                .With(x => x.Name, () => $"Product-{_fixture.Create<string>()[..8]}")
            );
        }

        [Fact]
        public void CreateProduct_使用共享Fixture_應成功建立()
        {
            // Arrange
            var productData = _fixture.Create<ProductCreateRequest>();

            // Assert
            productData.Price.Should().BeLessThan(10000);
            productData.Name.Should().StartWith("Product-");
        }

        [Fact]
        public void CreateProducts_共享Fixture客製化_應一致()
        {
            // Arrange
            var products = _fixture.CreateMany<ProductCreateRequest>(5);

            // Assert
            products.Should().AllSatisfy(p =>
            {
                p.Name.Should().StartWith("Product-");
                p.Price.Should().BeLessThan(10000);
            });
        }
    }

    #endregion

    #region Theory 測試整合

    [Theory]
    [InlineData(CustomerType.Regular)]
    [InlineData(CustomerType.Premium)]
    [InlineData(CustomerType.VIP)]
    public void CalculateDiscount_不同客戶類型_應套用正確折扣(CustomerType customerType)
    {
        // Arrange
        var fixture = new Fixture();

        var customer = fixture.Build<Customer>()
            .With(x => x.Type, customerType)
            .Create();

        var calculator = new DiscountCalculator();

        // Act
        var discount = calculator.Calculate(customer);

        // Assert
        switch (customerType)
        {
            case CustomerType.Regular:
                discount.Should().Be(0m);
                break;
            case CustomerType.Premium:
                discount.Should().Be(0.10m);
                break;
            case CustomerType.VIP:
                discount.Should().Be(0.20m);
                break;
        }
    }

    [Theory]
    [InlineData(0, CustomerLevel.Bronze)]
    [InlineData(5000, CustomerLevel.Bronze)]
    [InlineData(15000, CustomerLevel.Silver)]
    [InlineData(60000, CustomerLevel.Gold)]
    [InlineData(120000, CustomerLevel.Diamond)]
    public void GetLevel_不同消費金額_應回傳正確等級(decimal totalSpent, CustomerLevel expected)
    {
        // Arrange
        var fixture = new Fixture();
        var customer = fixture.Build<Customer>()
            .With(x => x.TotalSpent, totalSpent)
            .Create();

        // Act
        var level = customer.GetLevel();

        // Assert
        level.Should().Be(expected);
    }

    #endregion

    #region MemberData 結合 AutoFixture

    public static IEnumerable<object[]> GetPricingTestData()
    {
        var fixture = new Fixture();

        var quantities = new[] { 1, 3, 5, 10, 20 };

        foreach (var quantity in quantities)
        {
            var product = fixture.Create<Product>();
            var expectedTotal = product.Price * quantity;

            yield return new object[] { product, quantity, expectedTotal };
        }
    }

    [Theory]
    [MemberData(nameof(GetPricingTestData))]
    public void CalculateTotal_各種產品和數量_應計算正確總額(
        Product product, int quantity, decimal expectedTotal)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act
        var total = calculator.Calculate(product, quantity);

        // Assert
        total.Should().Be(expectedTotal);
    }

    #endregion

    #region 匿名測試原則

    [Fact]
    public void 匿名測試_專注於行為而非資料()
    {
        // Arrange - 不關心具體資料值
        var fixture = new Fixture();
        var customer = fixture.Create<Customer>();
        var repository = new CustomerRepository();

        // Act
        var result = repository.Add(customer);

        // Assert - 測試關心的是行為
        result.Should().BeTrue();
    }

    [Fact]
    public void 明確設定關鍵值_確保測試穩定()
    {
        // Arrange - 只設定測試關心的值
        var fixture = new Fixture();
        var customer = fixture.Build<Customer>()
            .With(x => x.Age, 25)  // 明確設定年齡
            .Create();

        var validator = new CustomerValidator();

        // Act
        var isAdult = validator.IsAdult(customer);

        // Assert - 穩定的結果
        isAdult.Should().BeTrue();
    }

    #endregion

    #region DTO 驗證測試

    [Fact]
    public void ValidateRequest_有效資料_應通過驗證()
    {
        // Arrange
        var fixture = new Fixture();

        var request = fixture.Build<CreateCustomerRequest>()
            .With(x => x.Name, fixture.Create<string>()[..50])
            .With(x => x.Email, fixture.Create<MailAddress>().Address)
            .With(x => x.Age, Random.Shared.Next(18, 78))
            .Create();

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(request, context, results, true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void ValidateRequest_姓名過長_應驗證失敗()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Build<CreateCustomerRequest>()
            .With(x => x.Name, new string('A', 101))  // 超過 100 字元
            .With(x => x.Email, fixture.Create<MailAddress>().Address)
            .With(x => x.Age, 25)
            .Create();

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(request, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(request.Name)));
    }

    #endregion

    #region 大量資料測試

    [Fact]
    public void ProcessBatch_大量資料_應正確處理()
    {
        // Arrange
        var fixture = new Fixture();
        var records = fixture.CreateMany<DataRecord>(1000).ToList();
        var processor = new DataProcessor();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = processor.ProcessBatch(records);
        stopwatch.Stop();

        // Assert
        result.ProcessedCount.Should().Be(1000);
        result.ErrorCount.Should().Be(0);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    [Fact]
    public void 序列化反序列化_任意物件_應保持一致()
    {
        // Arrange
        var fixture = new Fixture();
        var original = fixture.Create<Customer>();

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(original);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<Customer>(json);

        // Assert
        deserialized.Should().BeEquivalentTo(original);
    }

    #endregion

    #region 測試用的 Model 與 Service 類別

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public CustomerType Type { get; set; }
        public decimal TotalSpent { get; set; }

        public CustomerLevel GetLevel()
        {
            return TotalSpent switch
            {
                >= 100000 => CustomerLevel.Diamond,
                >= 50000 => CustomerLevel.Gold,
                >= 10000 => CustomerLevel.Silver,
                _ => CustomerLevel.Bronze
            };
        }
    }

    public enum CustomerType { Regular, Premium, VIP }
    public enum CustomerLevel { Bronze, Silver, Gold, Diamond }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class ProductCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class CreateCustomerRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Range(18, 120)]
        public int Age { get; set; }
    }

    public class DataRecord
    {
        public int Id { get; set; }
        public string Data { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ProcessingResult
    {
        public int ProcessedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    // 簡化的 Service 類別
    public class DiscountCalculator
    {
        public decimal Calculate(Customer customer)
        {
            return customer.Type switch
            {
                CustomerType.VIP => 0.20m,
                CustomerType.Premium => 0.10m,
                _ => 0m
            };
        }
    }

    public class PriceCalculator
    {
        public decimal Calculate(Product product, int quantity)
        {
            return product.Price * quantity;
        }
    }

    public class CustomerRepository
    {
        public bool Add(Customer customer)
        {
            return customer != null && !string.IsNullOrEmpty(customer.Name);
        }
    }

    public class CustomerValidator
    {
        public bool IsAdult(Customer customer)
        {
            return customer.Age >= 18;
        }
    }

    public class DataProcessor
    {
        public ProcessingResult ProcessBatch(IEnumerable<DataRecord> records)
        {
            var processed = 0;
            var errors = new List<string>();

            foreach (var record in records)
            {
                try
                {
                    // 模擬處理
                    processed++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Record {record.Id}: {ex.Message}");
                }
            }

            return new ProcessingResult
            {
                ProcessedCount = processed,
                ErrorCount = errors.Count,
                Errors = errors
            };
        }
    }

    #endregion
}
