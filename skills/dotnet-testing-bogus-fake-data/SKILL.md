---
name: dotnet-testing-bogus-fake-data
description: |
  使用 Bogus 產生擬真假資料的專門技能。當需要產生真實感的姓名、地址、電話、Email、公司資訊等測試資料時使用。涵蓋 Faker 類別、多語言支援、自訂規則、大量資料產生等。
  Make sure to use this skill whenever the user mentions Bogus, Faker, fake data, realistic test data, generating names/addresses/emails, or seed data, even if they don't explicitly ask for fake data guidance.
  Keywords: bogus, faker, fake data, 假資料, 擬真資料, realistic data, fake name, fake address, fake email, Faker<T>, RuleFor, Generate, faker.Name, faker.Address, faker.Internet, 產生假資料, generate fake data, seed data
---

# Bogus 假資料產生器

## 套件安裝與設定

### 安裝 Bogus

```bash
dotnet add package Bogus
```

### NuGet 套件資訊

| 套件名稱 | 用途             | NuGet 連結                                         |
| -------- | ---------------- | -------------------------------------------------- |
| `Bogus`  | 假資料產生函式庫 | [nuget.org](https://www.nuget.org/packages/Bogus/) |

**GitHub 儲存庫**：[bchavez/Bogus](https://github.com/bchavez/Bogus)

---

## 核心概念

### 基本語法結構

Bogus 的核心是 `Faker<T>` 類別，使用 `RuleFor` 方法定義屬性的產生規則：

```csharp
using Bogus;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

// 建立產品資料的 Faker
var productFaker = new Faker<Product>()
    .RuleFor(p => p.Id, f => f.IndexFaker)
    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
    .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
    .RuleFor(p => p.Description, f => f.Lorem.Sentence())
    .RuleFor(p => p.CreatedDate, f => f.Date.Past());

// 產生單筆資料
var product = productFaker.Generate();

// 產生多筆資料
var products = productFaker.Generate(10);
```

### 內建 DataSet 概覽

Bogus 提供豐富的內建 DataSet，每個都專注於特定領域的資料產生：

#### 個人資訊 (Person DataSet)

```csharp
var faker = new Faker();

var fullName = faker.Person.FullName;        // 完整姓名
var firstName = faker.Person.FirstName;      // 名字
var lastName = faker.Person.LastName;        // 姓氏
var email = faker.Person.Email;              // 電子郵件
var gender = faker.Person.Gender;            // 性別
var dateOfBirth = faker.Person.DateOfBirth;  // 生日
```

#### 地址資訊 (Address DataSet)

```csharp
var fullAddress = faker.Address.FullAddress();    // 完整地址
var streetAddress = faker.Address.StreetAddress(); // 街道地址
var city = faker.Address.City();                   // 城市
var state = faker.Address.State();                 // 州/省
var zipCode = faker.Address.ZipCode();             // 郵遞區號
var country = faker.Address.Country();             // 國家
var latitude = faker.Address.Latitude();           // 緯度
var longitude = faker.Address.Longitude();         // 經度
```

#### 商業資訊 (Company & Commerce DataSet)

```csharp
var companyName = faker.Company.CompanyName();     // 公司名稱
var catchPhrase = faker.Company.CatchPhrase();     // 標語
var department = faker.Commerce.Department();      // 部門
var productName = faker.Commerce.ProductName();    // 產品名稱
var price = faker.Commerce.Price(1, 1000, 2);      // 價格（字串格式）
var ean13 = faker.Commerce.Ean13();                // EAN-13 條碼
```

#### 網路資訊 (Internet DataSet)

```csharp
var url = faker.Internet.Url();               // URL
var domainName = faker.Internet.DomainName(); // 網域名稱
var ipAddress = faker.Internet.Ip();          // IPv4 地址
var ipv6 = faker.Internet.Ipv6();             // IPv6 地址
var userName = faker.Internet.UserName();     // 使用者名稱
var password = faker.Internet.Password();     // 密碼
var email = faker.Internet.Email();           // 電子郵件
```

#### 金融資訊 (Finance DataSet)

```csharp
var creditCardNumber = faker.Finance.CreditCardNumber();  // 信用卡號
var creditCardCvv = faker.Finance.CreditCardCvv();        // CVV
var account = faker.Finance.Account();                     // 帳戶號碼
var amount = faker.Finance.Amount(100, 10000, 2);          // 金額
var currency = faker.Finance.Currency();                   // 貨幣
var iban = faker.Finance.Iban();                          // IBAN
var bic = faker.Finance.Bic();                            // BIC/SWIFT
```

#### 時間資訊 (Date DataSet)

```csharp
var pastDate = faker.Date.Past();                        // 過去日期
var futureDate = faker.Date.Future();                    // 未來日期
var recentDate = faker.Date.Recent();                    // 最近日期
var soonDate = faker.Date.Soon();                        // 即將到來的日期
var between = faker.Date.Between(start, end);            // 範圍內日期
var weekday = faker.Date.Weekday();                      // 星期幾
```

#### 隨機資料 (Random DataSet)

```csharp
var randomInt = faker.Random.Int(1, 100);                  // 整數
var randomDecimal = faker.Random.Decimal(0, 1000);         // 小數
var randomBool = faker.Random.Bool();                      // 布林
var randomGuid = faker.Random.Guid();                      // GUID
var randomEnum = faker.Random.Enum<DayOfWeek>();           // 隨機列舉
var randomElement = faker.Random.ArrayElement(array);      // 陣列隨機元素
var shuffled = faker.Random.Shuffle(collection);           // 洗牌
```

#### 文字內容 (Lorem DataSet)

```csharp
var word = faker.Lorem.Word();             // 單字
var words = faker.Lorem.Words(5);          // 多個單字
var sentence = faker.Lorem.Sentence();     // 句子
var paragraph = faker.Lorem.Paragraph();   // 段落
var text = faker.Lorem.Text();             // 文字區塊
```

---

## 多語言支援

Bogus 的一大特色是支援多種語言和文化，讓產生的資料更符合當地習慣：

```csharp
// 繁體中文
var chineseFaker = new Faker<Person>("zh_TW")
    .RuleFor(p => p.Name, f => f.Person.FullName)
    .RuleFor(p => p.Address, f => f.Address.FullAddress());

// 日文
var japaneseFaker = new Faker<Person>("ja")
    .RuleFor(p => p.Name, f => f.Person.FullName)
    .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber());

// 法文
var frenchFaker = new Faker<Person>("fr")
    .RuleFor(p => p.Name, f => f.Person.FullName)
    .RuleFor(p => p.Company, f => f.Company.CompanyName());
```

### 支援的語言代碼

| 語言         | 代碼    | 語言     | 代碼    |
| ------------ | ------- | -------- | ------- |
| 英文（美國） | `en_US` | 簡體中文 | `zh_CN` |
| 繁體中文     | `zh_TW` | 日文     | `ja`    |
| 韓文         | `ko`    | 法文     | `fr`    |
| 德文         | `de`    | 西班牙文 | `es`    |
| 俄文         | `ru`    | 葡萄牙文 | `pt_BR` |

---

## 進階功能

涵蓋 Seed 可重現性控制、條件式產生與機率控制（`OrNull`、`PickRandomWeighted`）、關聯資料與巢狀物件、複雜業務邏輯約束，以及自訂 DataSet 擴充（如台灣在地資料產生器）。

> 完整內容請參閱 [references/advanced-features.md](references/advanced-features.md)

---

## Bogus vs AutoFixture 比較

### 設計理念差異

| 項目         | AutoFixture               | Bogus                           |
| ------------ | ------------------------- | ------------------------------- |
| **核心理念** | 匿名測試 (Anonymous Test) | 真實模擬 (Realistic Simulation) |
| **資料品質** | 隨機填充，專注測試邏輯    | 有意義資料，模擬真實情境        |
| **學習成本** | 自動推斷，零配置          | 明確定義，需要學習 DataSet      |
| **可讀性**   | 抽象化，減少資料噪音      | 具體化，資料有意義              |

### 適用場景分析

| 場景           | 建議工具    | 原因                           |
| -------------- | ----------- | ------------------------------ |
| **單元測試**   | AutoFixture | 專注於邏輯驗證，不關心資料內容 |
| **整合測試**   | Bogus       | 需要真實感的資料進行端到端測試 |
| **UI 原型**    | Bogus       | 展示用的擬真資料               |
| **效能測試**   | Bogus       | 大量真實格式的資料             |
| **資料庫種子** | Bogus       | 初始化開發/測試環境            |
| **複雜相依性** | AutoFixture | 自動處理循環參考和巢狀物件     |

### 程式碼比較

```csharp
// AutoFixture：簡單直接，自動推斷
var fixture = new Fixture();
var user = fixture.Create<User>(); // 一行搞定，但資料無意義

// Bogus：需要設定，但資料有意義
var userFaker = new Faker<User>()
    .RuleFor(u => u.Name, f => f.Person.FullName)    // 真實的姓名格式
    .RuleFor(u => u.Email, f => f.Internet.Email()); // 真實的郵件格式
var user = userFaker.Generate();
```

---

## 效能最佳化

### 重用 Faker 實例

```csharp
public class OptimizedDataGenerator
{
    // 預編譯 Faker 以提升效能（靜態欄位，只初始化一次）
    private static readonly Faker<User> _userFaker = new Faker<User>()
        .RuleFor(u => u.Id, f => f.Random.Guid())
        .RuleFor(u => u.Name, f => f.Person.FullName)
        .RuleFor(u => u.Email, f => f.Internet.Email());
    
    public static List<User> GenerateUsers(int count) 
        => _userFaker.Generate(count);
}
```

### 批次產生

```csharp
// 批次產生以減少記憶體分配
public static IEnumerable<User> GenerateUsersBatch(int totalCount, int batchSize = 1000)
{
    var generated = 0;
    while (generated < totalCount)
    {
        var currentBatchSize = Math.Min(batchSize, totalCount - generated);
        var batch = _userFaker.Generate(currentBatchSize);
        
        foreach (var user in batch)
        {
            yield return user;
        }
        
        generated += currentBatchSize;
    }
}
```

### Lazy 初始化

```csharp
// 使用 Lazy 延遲初始化複雜的 Faker
private static readonly Lazy<Faker<ComplexEntity>> _complexFaker = 
    new(() => new Faker<ComplexEntity>()
        .RuleFor(e => e.Id, f => f.Random.Guid())
        .RuleFor(e => e.Data, f => GenerateComplexData(f)));

public static ComplexEntity Generate() => _complexFaker.Value.Generate();
```

---

## 測試實作範例

### 郵件服務測試

```csharp
[Fact]
public void EmailService_SendWelcomeEmail_ShouldFormatCorrectly()
{
    // Arrange - 需要真實的使用者資料來測試郵件格式
    var userFaker = new Faker<User>()
        .RuleFor(u => u.Name, f => f.Person.FullName)
        .RuleFor(u => u.Email, f => f.Internet.Email());
    
    var user = userFaker.Generate();
    var emailService = new EmailService();
    
    // Act
    var emailContent = emailService.GenerateWelcomeEmail(user);
    
    // Assert
    emailContent.Should().Contain(user.Name);
    emailContent.Should().Contain(user.Email);
}
```

### 資料庫種子

```csharp
public static class DatabaseSeeder
{
    public static void SeedDatabase(AppDbContext context)
    {
        // 設定 seed 確保可重現
        Randomizer.Seed = new Random(42);
        
        var customerFaker = new Faker<Customer>("zh_TW")
            .RuleFor(c => c.Name, f => f.Person.FullName)
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Address, f => f.Address.FullAddress());
        
        var customers = customerFaker.Generate(100);
        context.Customers.AddRange(customers);
        context.SaveChanges();
    }
}
```

---

## 最佳實踐

### 命名與組織

1. **Faker 命名慣例**：使用 `{EntityName}Faker` 格式命名
2. **集中管理**：將 Faker 定義集中在 `TestDataGenerators` 或 `Fakers` 資料夾
3. **重用靜態實例**：避免重複建立 Faker 實例

### 程式碼組織

```text
MyProject.Tests/
├── Fakers/
│   ├── CustomerFaker.cs
│   ├── OrderFaker.cs
│   └── TaiwanDataSetExtensions.cs
├── Services/
│   └── CustomerServiceTests.cs
└── ...
```

### 常見陷阱

1. **避免過度配置**：只設定測試需要的屬性
2. **注意隨機性**：使用 seed 確保測試可重現
3. **效能考量**：大量資料時使用批次產生

---

## 相關技能

| 技能名稱                        | 關聯說明                                     |
| ------------------------------- | -------------------------------------------- |
| `autofixture-basics`            | AutoFixture 基礎使用，適合單元測試的匿名資料 |
| `autofixture-bogus-integration` | AutoFixture 與 Bogus 混合使用策略            |
| `test-data-builder-pattern`     | 手動 Builder Pattern，適合簡單場景           |

---

## 輸出格式

- 產生 Faker<T> 設定類別（含 RuleFor 規則）
- 提供多語言假資料產生範例
- 包含 .csproj 套件參考（Bogus）
- 產生大量資料產生與 Seed 控制程式碼

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 14 - 使用 Bogus 產生假資料**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10375501
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day14

### 官方文件

- [Bogus NuGet Package](https://www.nuget.org/packages/Bogus/)
- [Bogus GitHub Repository](https://github.com/bchavez/Bogus)
- [Bogus 官方文件](https://github.com/bchavez/Bogus#readme)

### 延伸閱讀

- [Bogus 與 AutoFixture 在測試中的應用 | mrkt的程式學習筆記](https://www.dotblogs.com.tw/mrkt/2024/09/29/191300)


