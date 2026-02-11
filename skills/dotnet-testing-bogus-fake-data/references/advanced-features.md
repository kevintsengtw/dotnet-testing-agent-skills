# 進階功能

> 本文件從 [SKILL.md](../SKILL.md) 的「進階功能」章節提煉而來，提供完整的程式碼範例與詳細說明。

---

## 可重現性控制（Seed）

透過設定 seed，確保每次產生相同的資料序列：

```csharp
// 設定全域 seed
Randomizer.Seed = new Random(12345);

var productFaker = new Faker<Product>()
    .RuleFor(p => p.Name, f => f.Commerce.ProductName());

// 每次執行都會產生相同的產品名稱序列
var products1 = productFaker.Generate(5);

// 重置 seed 後重新產生
Randomizer.Seed = new Random(12345);
var products2 = productFaker.Generate(5); // 相同的資料

// 重置為隨機
Randomizer.Seed = new Random();
```

## 條件式產生與機率控制

```csharp
var userFaker = new Faker<User>()
    .RuleFor(u => u.Name, f => f.Person.FullName)
    // 80% 機率有 Premium 會員
    .RuleFor(u => u.IsPremium, f => f.Random.Bool(0.8f))
    // OrNull：50% 機率為 null
    .RuleFor(u => u.MiddleName, f => f.Name.FirstName().OrNull(f, 0.5f))
    // 隨機選擇陣列元素
    .RuleFor(u => u.Department, f => f.PickRandom("IT", "HR", "Finance", "Marketing"))
    // 權重式隨機選擇
    .RuleFor(u => u.Role, f => f.PickRandomWeighted(
        new[] { "User", "Admin", "SuperAdmin" },
        new[] { 0.7f, 0.25f, 0.05f }));
```

## 關聯資料與巢狀物件

```csharp
// 產生具有關聯性的訂單資料
var orderFaker = new Faker<Order>()
    .RuleFor(o => o.Id, f => f.IndexFaker)
    .RuleFor(o => o.CustomerName, f => f.Person.FullName)
    .RuleFor(o => o.OrderDate, f => f.Date.Past())
    // 產生 1-5 個訂單明細
    .RuleFor(o => o.Items, f => 
    {
        var itemFaker = new Faker<OrderItem>()
            .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
            .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(i => i.UnitPrice, f => decimal.Parse(f.Commerce.Price(10, 100)));
        
        return itemFaker.Generate(f.Random.Int(1, 5));
    })
    // 計算總金額（參考其他屬性）
    .RuleFor(o => o.TotalAmount, (f, o) => 
        o.Items.Sum(item => item.Quantity * item.UnitPrice));
```

## 複雜業務邏輯約束

```csharp
// 具有複雜業務邏輯的員工資料產生
var employeeFaker = new Faker<Employee>()
    .RuleFor(e => e.Id, f => f.Random.Guid())
    .RuleFor(e => e.FirstName, f => f.Person.FirstName)
    .RuleFor(e => e.LastName, f => f.Person.LastName)
    // 根據姓名產生 Email
    .RuleFor(e => e.Email, (f, e) => 
        f.Internet.Email(e.FirstName, e.LastName, "company.com"))
    // 年齡範圍限制
    .RuleFor(e => e.Age, f => f.Random.Int(22, 65))
    // 根據年齡決定職級
    .RuleFor(e => e.Level, (f, e) => e.Age switch
    {
        < 25 => "Junior",
        < 35 => "Senior",
        < 45 => "Lead",
        _ => "Principal"
    })
    // 根據職級決定薪資範圍
    .RuleFor(e => e.Salary, (f, e) => e.Level switch
    {
        "Junior" => f.Random.Decimal(35000, 50000),
        "Senior" => f.Random.Decimal(50000, 80000),
        "Lead" => f.Random.Decimal(80000, 120000),
        "Principal" => f.Random.Decimal(120000, 200000),
        _ => f.Random.Decimal(35000, 50000)
    });
```

## 自訂 DataSet 擴充

```csharp
// 建立自訂的台灣資料產生器
public static class TaiwanDataSetExtensions
{
    private static readonly string[] TaiwanCities = 
    {
        "台北市", "新北市", "桃園市", "台中市", "台南市", "高雄市",
        "基隆市", "新竹市", "嘉義市", "宜蘭縣", "新竹縣", "苗栗縣"
    };
    
    private static readonly string[] TaiwanCompanies = 
    {
        "台積電", "鴻海", "聯發科", "中華電信", "台塑", "統一"
    };
    
    public static string TaiwanCity(this Faker faker)
        => faker.PickRandom(TaiwanCities);
    
    public static string TaiwanCompany(this Faker faker)
        => faker.PickRandom(TaiwanCompanies);
    
    public static string TaiwanMobilePhone(this Faker faker)
    {
        var prefix = "09";
        var middle = faker.Random.Int(0, 9);
        var suffix = faker.Random.String2(7, "0123456789");
        return $"{prefix}{middle}{suffix}";
    }
    
    public static string TaiwanIdCard(this Faker faker)
    {
        var firstChar = faker.PickRandom("ABCDEFGHJKLMNPQRSTUVXYWZIO");
        var genderDigit = faker.Random.Int(1, 2);
        var digits = faker.Random.String2(8, "0123456789");
        return $"{firstChar}{genderDigit}{digits}";
    }
}

// 使用自訂擴充
var taiwanPersonFaker = new Faker<TaiwanPerson>()
    .RuleFor(p => p.City, f => f.TaiwanCity())
    .RuleFor(p => p.Company, f => f.TaiwanCompany())
    .RuleFor(p => p.Mobile, f => f.TaiwanMobilePhone())
    .RuleFor(p => p.IdCard, f => f.TaiwanIdCard());
```
