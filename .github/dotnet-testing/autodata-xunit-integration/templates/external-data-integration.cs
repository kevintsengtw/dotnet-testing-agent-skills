// =============================================================================
// CSV/JSON 外部資料整合範例
// 展示如何整合外部檔案作為測試資料來源
// =============================================================================

using System.Globalization;
using System.Text.Json;
using AutoFixture.Xunit2;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Xunit;

namespace AutoDataXunitIntegration.Templates;

// -----------------------------------------------------------------------------
// 1. 測試模型類別
// -----------------------------------------------------------------------------

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
}

public class Order
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// -----------------------------------------------------------------------------
// 2. CSV 資料格式類別
// -----------------------------------------------------------------------------

/// <summary>
/// CSV 檔案對應的資料類別
/// 欄位名稱需與 CSV 標題一致
/// </summary>
public class ProductCsvRecord
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

// -----------------------------------------------------------------------------
// 3. JSON 資料格式類別
// -----------------------------------------------------------------------------

/// <summary>
/// JSON 檔案對應的資料類別
/// </summary>
public class CustomerJsonRecord
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
}

// -----------------------------------------------------------------------------
// 4. CSV 整合測試
// -----------------------------------------------------------------------------

public class CsvIntegrationTests
{
    /// <summary>
    /// 從 CSV 檔案讀取產品資料
    /// 
    /// 注意：需要在專案中建立 TestData/products.csv 檔案
    /// 並在 .csproj 中設定：
    /// <Content Include="TestData\*.csv">
    ///   <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    /// </Content>
    /// </summary>
    public static IEnumerable<object[]> GetProductsFromCsv()
    {
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "products.csv");

        // 如果檔案不存在，提供預設測試資料
        if (!File.Exists(csvPath))
        {
            yield return new object[] { 1, "iPhone 15", "3C產品", 35900m, true };
            yield return new object[] { 2, "MacBook Pro", "3C產品", 89900m, true };
            yield return new object[] { 3, "Nike Air Max", "運動用品", 4200m, true };
            yield break;
        }

        using var reader = new StreamReader(csvPath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<ProductCsvRecord>().ToList();

        foreach (var record in records)
        {
            yield return new object[]
            {
                record.ProductId,
                record.Name,
                record.Category,
                record.Price,
                record.IsAvailable
            };
        }
    }

    /// <summary>
    /// CSV 整合測試：產品驗證
    /// </summary>
    [Theory]
    [MemberAutoData(nameof(GetProductsFromCsv))]
    public void CSV整合測試_產品驗證(
        int productId,
        string productName,
        string category,
        decimal price,
        bool isAvailable,
        Customer customer,  // 由 AutoFixture 產生
        Order order)        // 由 AutoFixture 產生
    {
        // Assert - CSV 資料
        productId.Should().BePositive();
        productName.Should().NotBeNullOrEmpty();
        category.Should().NotBeNullOrEmpty();
        price.Should().BePositive();

        // Assert - AutoFixture 產生的資料
        customer.Should().NotBeNull();
        order.Should().NotBeNull();
    }

    /// <summary>
    /// 篩選特定類別的 CSV 資料
    /// </summary>
    public static IEnumerable<object[]> GetElectronicsFromCsv()
    {
        return GetProductsFromCsv()
            .Where(data => data[2].ToString() == "3C產品");
    }

    [Theory]
    [MemberAutoData(nameof(GetElectronicsFromCsv))]
    public void CSV整合測試_電子產品篩選(
        int productId,
        string productName,
        string category,
        decimal price,
        bool isAvailable,
        Order order)
    {
        // Assert
        category.Should().Be("3C產品");
        productId.Should().BePositive();
        price.Should().BeGreaterThan(1000m); // 電子產品通常價格較高
    }
}

// -----------------------------------------------------------------------------
// 5. JSON 整合測試
// -----------------------------------------------------------------------------

public class JsonIntegrationTests
{
    /// <summary>
    /// 從 JSON 檔案讀取客戶資料
    /// 
    /// 注意：需要在專案中建立 TestData/customers.json 檔案
    /// 並在 .csproj 中設定：
    /// <Content Include="TestData\*.json">
    ///   <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    /// </Content>
    /// </summary>
    public static IEnumerable<object[]> GetCustomersFromJson()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "customers.json");

        // 如果檔案不存在，提供預設測試資料
        if (!File.Exists(jsonPath))
        {
            yield return new object[] { 1, "張三", "zhang@example.com", "VIP", 100000m };
            yield return new object[] { 2, "李四", "li@example.com", "Premium", 50000m };
            yield return new object[] { 3, "王五", "wang@example.com", "Regular", 20000m };
            yield break;
        }

        var jsonContent = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var customers = JsonSerializer.Deserialize<List<CustomerJsonRecord>>(jsonContent, options)!;

        foreach (var customer in customers)
        {
            yield return new object[]
            {
                customer.CustomerId,
                customer.Name,
                customer.Email,
                customer.Type,
                customer.CreditLimit
            };
        }
    }

    /// <summary>
    /// JSON 整合測試：客戶驗證
    /// </summary>
    [Theory]
    [MemberAutoData(nameof(GetCustomersFromJson))]
    public void JSON整合測試_客戶驗證(
        int customerId,
        string name,
        string email,
        string customerType,
        decimal creditLimit,
        Order order)  // 由 AutoFixture 產生
    {
        // Assert - JSON 資料
        customerId.Should().BePositive();
        name.Should().NotBeNullOrEmpty();
        email.Should().Contain("@");
        customerType.Should().BeOneOf("VIP", "Premium", "Regular");
        creditLimit.Should().BePositive();

        // Assert - AutoFixture 產生的資料
        order.Should().NotBeNull();
    }

    /// <summary>
    /// 篩選 VIP 客戶
    /// </summary>
    public static IEnumerable<object[]> GetVipCustomersFromJson()
    {
        return GetCustomersFromJson()
            .Where(data => data[3].ToString() == "VIP");
    }

    [Theory]
    [MemberAutoData(nameof(GetVipCustomersFromJson))]
    public void JSON整合測試_VIP客戶篩選(
        int customerId,
        string name,
        string email,
        string customerType,
        decimal creditLimit,
        Order order)
    {
        // Assert
        customerType.Should().Be("VIP");
        creditLimit.Should().BeGreaterOrEqualTo(100000m);
    }
}

// -----------------------------------------------------------------------------
// 6. 混合資料來源測試
// -----------------------------------------------------------------------------

public class MixedDataSourceTests
{
    /// <summary>
    /// 整合 CSV 產品與 JSON 客戶資料
    /// </summary>
    public static IEnumerable<object[]> GetOrderScenarios()
    {
        // 簡化版：直接定義測試場景
        yield return new object[] { "VIP", 100000m, "iPhone 15", 35900m };
        yield return new object[] { "Premium", 50000m, "MacBook Pro", 89900m };
        yield return new object[] { "Regular", 20000m, "AirPods Pro", 7490m };
    }

    [Theory]
    [MemberAutoData(nameof(GetOrderScenarios))]
    public void 混合資料來源_訂單場景測試(
        string customerType,
        decimal creditLimit,
        string productName,
        decimal productPrice,
        Order order)  // 由 AutoFixture 產生
    {
        // Arrange
        var customer = new Customer
        {
            Type = customerType,
            CreditLimit = creditLimit
        };

        var product = new Product
        {
            Name = productName,
            Price = productPrice
        };

        order.Amount = productPrice;

        // Act
        var canOrder = customer.CreditLimit >= order.Amount;

        // Assert
        if (customerType == "Regular" && productName == "MacBook Pro")
        {
            canOrder.Should().BeFalse("Regular 客戶信用額度不足以購買 MacBook Pro");
        }
        else
        {
            canOrder.Should().BeTrue();
        }
    }
}

// -----------------------------------------------------------------------------
// 7. 測試資料輔助類別
// -----------------------------------------------------------------------------

/// <summary>
/// 測試資料路徑輔助類別
/// </summary>
public static class TestDataHelper
{
    public static string GetTestDataPath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
    }

    public static bool TestDataFileExists(string fileName)
    {
        return File.Exists(GetTestDataPath(fileName));
    }

    /// <summary>
    /// 安全讀取 CSV 檔案，如果不存在則回傳空集合
    /// </summary>
    public static IEnumerable<T> ReadCsvSafely<T>(string fileName)
    {
        var path = GetTestDataPath(fileName);

        if (!File.Exists(path))
        {
            return Enumerable.Empty<T>();
        }

        using var reader = new StreamReader(path);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// 安全讀取 JSON 檔案，如果不存在則回傳空集合
    /// </summary>
    public static IEnumerable<T> ReadJsonSafely<T>(string fileName)
    {
        var path = GetTestDataPath(fileName);

        if (!File.Exists(path))
        {
            return Enumerable.Empty<T>();
        }

        var jsonContent = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<T>>(jsonContent, options) ?? new List<T>();
    }
}
