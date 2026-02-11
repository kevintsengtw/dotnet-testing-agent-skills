# 外部測試資料整合

> 本文件從 [SKILL.md](../SKILL.md) 提取，提供 CSV/JSON 外部資料與 AutoData 整合的完整指南。

## 測試專案檔案設定

在 `.csproj` 中設定外部資料檔案：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- CSV 檔案 -->
    <Content Include="TestData\*.csv">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    
    <!-- JSON 檔案 -->
    <Content Include="TestData\*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CsvHelper" Version="33.0.1" />
  </ItemGroup>
</Project>
```

## CSV 檔案整合

**TestData/products.csv**

```csv
ProductId,Name,Category,Price,IsAvailable
1,"iPhone 15","3C產品",35900,true
2,"MacBook Pro","3C產品",89900,true
3,"AirPods Pro","3C產品",7490,false
4,"Nike Air Max","運動用品",4200,true
```

**CSV 讀取與整合**

```csharp
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

public class ExternalDataIntegrationTests
{
    public static IEnumerable<object[]> GetProductsFromCsv()
    {
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "products.csv");
        
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

    [Theory]
    [MemberAutoData(nameof(GetProductsFromCsv))]
    public void CSV整合測試_產品驗證(
        int productId,
        string productName,
        string category,
        decimal price,
        bool isAvailable,
        Customer customer,
        Order order)
    {
        // Assert - CSV 資料
        productId.Should().BePositive();
        productName.Should().NotBeNullOrEmpty();
        category.Should().BeOneOf("3C產品", "運動用品");
        price.Should().BePositive();

        // Assert - AutoFixture 產生的資料
        customer.Should().NotBeNull();
        order.Should().NotBeNull();
    }
}

public class ProductCsvRecord
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}
```

## JSON 檔案整合

**TestData/customers.json**

```json
[
  {
    "customerId": 1,
    "name": "張三",
    "email": "zhang@example.com",
    "type": "VIP",
    "creditLimit": 100000
  },
  {
    "customerId": 2,
    "name": "李四",
    "email": "li@example.com",
    "type": "Premium",
    "creditLimit": 50000
  }
]
```

**JSON 讀取與整合**

```csharp
using System.Text.Json;

public static IEnumerable<object[]> GetCustomersFromJson()
{
    var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "customers.json");
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

[Theory]
[MemberAutoData(nameof(GetCustomersFromJson))]
public void JSON整合測試_客戶驗證(
    int customerId,
    string name,
    string email,
    string customerType,
    decimal creditLimit,
    Order order)
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
```
