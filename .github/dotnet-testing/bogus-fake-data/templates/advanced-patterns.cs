// =============================================================================
// Bogus 進階模式與自訂擴充範例
// 展示複雜業務邏輯、自訂 DataSet、效能最佳化、測試整合
// =============================================================================

using Bogus;
using FluentAssertions;
using Xunit;

namespace BogusAdvanced.Templates;

#region 測試模型類別

// =============================================================================
// 測試模型類別
// =============================================================================

public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Level { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public string Department { get; set; } = string.Empty;
    public bool IsManager { get; set; }
}

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> Technologies { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class TaiwanPerson
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IdCard { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;
}

public class GlobalUser
{
    public Guid Id { get; set; }
    public string Locale { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class TestBoundaryData
{
    public string? NullableString { get; set; }
    public string ShortString { get; set; } = string.Empty;
    public string LongString { get; set; } = string.Empty;
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public int ZeroValue { get; set; }
    public int NegativeValue { get; set; }
    public int PositiveValue { get; set; }
    public string SpecialChars { get; set; } = string.Empty;
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
}

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

#endregion

#region 複雜業務邏輯約束

// =============================================================================
// 複雜業務邏輯約束
// =============================================================================

public class ComplexBusinessLogicExamples
{
    /// <summary>
    /// 具有複雜業務邏輯的員工資料產生
    /// </summary>
    [Fact]
    public void Employee_複雜業務邏輯約束()
    {
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
            })
            // 入職日期約束（年齡 - 22 年內）
            .RuleFor(e => e.HireDate, (f, e) =>
            {
                var maxYearsAgo = Math.Max(1, e.Age - 22);
                return f.Date.Past(maxYearsAgo);
            })
            // 產生技能清單
            .RuleFor(e => e.Skills, f =>
            {
                var allSkills = new[] 
                { 
                    "C#", ".NET", "JavaScript", "TypeScript", "React", "Angular", "Vue",
                    "SQL Server", "PostgreSQL", "MongoDB", "Redis",
                    "Azure", "AWS", "Docker", "Kubernetes", "Git"
                };
                return f.PickRandom(allSkills, f.Random.Int(2, 6)).ToList();
            })
            // 部門選擇
            .RuleFor(e => e.Department, f => 
                f.PickRandom("Engineering", "Product", "Design", "QA", "DevOps"))
            // Lead 以上有 30% 機率是 Manager
            .RuleFor(e => e.IsManager, (f, e) => 
                (e.Level == "Lead" || e.Level == "Principal") && f.Random.Bool(0.3f))
            // 產生專案經驗
            .RuleFor(e => e.Projects, (f, e) =>
            {
                var projectFaker = new Faker<Project>()
                    .RuleFor(p => p.Id, f => f.Random.Guid())
                    .RuleFor(p => p.Name, f => f.Company.CatchPhrase())
                    .RuleFor(p => p.Description, f => f.Lorem.Sentence())
                    .RuleFor(p => p.StartDate, f => f.Date.Between(e.HireDate, DateTime.Now.AddMonths(-1)))
                    .RuleFor(p => p.EndDate, (f, p) => 
                        f.Random.Bool(0.8f) ? f.Date.Between(p.StartDate, DateTime.Now) : null)
                    .RuleFor(p => p.Status, (f, p) => 
                        p.EndDate.HasValue ? "Completed" : f.PickRandom("In Progress", "On Hold"))
                    .RuleFor(p => p.Technologies, f => 
                        f.PickRandom(e.Skills, f.Random.Int(1, Math.Min(3, e.Skills.Count))).ToList());

                var yearsOfExperience = (DateTime.Now - e.HireDate).Days / 365;
                var projectCount = Math.Max(1, yearsOfExperience / 2);
                return projectFaker.Generate(f.Random.Int(1, projectCount));
            });

        // 產生員工
        var employee = employeeFaker.Generate();

        // 驗證業務邏輯約束
        employee.Age.Should().BeInRange(22, 65);
        employee.Email.Should().EndWith("@company.com");
        employee.HireDate.Should().BeBefore(DateTime.Now);
        employee.Skills.Should().HaveCountGreaterOrEqualTo(2);
        
        // 驗證職級與薪資對應
        if (employee.Level == "Junior")
        {
            employee.Salary.Should().BeInRange(35000, 50000);
        }
    }

    /// <summary>
    /// 產生具有階層關係的組織資料
    /// </summary>
    [Fact]
    public void Organization_階層關係()
    {
        var departments = new[] { "Engineering", "Product", "Design", "QA", "DevOps" };
        
        var managerFaker = new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.Age, f => f.Random.Int(35, 55))
            .RuleFor(e => e.Level, _ => "Lead")
            .RuleFor(e => e.IsManager, _ => true)
            .RuleFor(e => e.Salary, f => f.Random.Decimal(100000, 150000));

        var employeeFaker = new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.Age, f => f.Random.Int(22, 40))
            .RuleFor(e => e.Level, f => f.PickRandom("Junior", "Senior"))
            .RuleFor(e => e.IsManager, _ => false)
            .RuleFor(e => e.Salary, (f, e) => e.Level == "Junior" 
                ? f.Random.Decimal(35000, 50000) 
                : f.Random.Decimal(50000, 80000));

        // 每個部門一個經理，3-8 個員工
        var organization = departments.Select(dept =>
        {
            var manager = managerFaker.Generate();
            manager.Department = dept;

            var teamSize = new Faker().Random.Int(3, 8);
            var team = employeeFaker.Generate(teamSize);
            team.ForEach(e => e.Department = dept);

            return new { Department = dept, Manager = manager, Team = team };
        }).ToList();

        organization.Should().HaveCount(5);
        organization.All(o => o.Manager.IsManager).Should().BeTrue();
        organization.All(o => o.Team.All(e => !e.IsManager)).Should().BeTrue();
    }
}

#endregion

#region 自訂 DataSet 擴充

// =============================================================================
// 自訂 DataSet 擴充
// =============================================================================

/// <summary>
/// 台灣資料產生器擴充方法
/// </summary>
public static class TaiwanDataSetExtensions
{
    private static readonly string[] TaiwanCities = 
    {
        "台北市", "新北市", "桃園市", "台中市", "台南市", "高雄市",
        "基隆市", "新竹市", "嘉義市", "宜蘭縣", "新竹縣", "苗栗縣",
        "彰化縣", "南投縣", "雲林縣", "嘉義縣", "屏東縣", "台東縣",
        "花蓮縣", "澎湖縣", "金門縣", "連江縣"
    };

    private static readonly string[] TaiwanDistricts = 
    {
        "中正區", "大同區", "中山區", "松山區", "大安區", "萬華區",
        "信義區", "士林區", "北投區", "內湖區", "南港區", "文山區"
    };

    private static readonly string[] TaiwanUniversities = 
    {
        "台灣大學", "清華大學", "交通大學", "成功大學", "中山大學",
        "政治大學", "中央大學", "中正大學", "中興大學", "師範大學",
        "台北科技大學", "台灣科技大學", "高雄科技大學"
    };

    private static readonly string[] TaiwanCompanies = 
    {
        "台積電", "鴻海", "聯發科", "中華電信", "台塑", "統一",
        "富邦", "中信", "國泰", "遠傳", "華碩", "宏碁", "廣達"
    };

    /// <summary>
    /// 產生台灣城市名稱
    /// </summary>
    public static string TaiwanCity(this Faker faker)
        => faker.PickRandom(TaiwanCities);

    /// <summary>
    /// 產生台灣區域名稱
    /// </summary>
    public static string TaiwanDistrict(this Faker faker)
        => faker.PickRandom(TaiwanDistricts);

    /// <summary>
    /// 產生台灣大學名稱
    /// </summary>
    public static string TaiwanUniversity(this Faker faker)
        => faker.PickRandom(TaiwanUniversities);

    /// <summary>
    /// 產生台灣公司名稱
    /// </summary>
    public static string TaiwanCompany(this Faker faker)
        => faker.PickRandom(TaiwanCompanies);

    /// <summary>
    /// 產生台灣身分證字號（格式正確但非真實有效）
    /// </summary>
    public static string TaiwanIdCard(this Faker faker)
    {
        // 第一碼：英文字母（對應縣市）
        var firstChar = faker.PickRandom("ABCDEFGHJKLMNPQRSTUVXYWZIO".ToCharArray());
        // 第二碼：1=男性、2=女性
        var genderDigit = faker.Random.Int(1, 2);
        // 後八碼：隨機數字
        var digits = faker.Random.String2(8, "0123456789");
        return $"{firstChar}{genderDigit}{digits}";
    }

    /// <summary>
    /// 產生台灣手機號碼
    /// </summary>
    public static string TaiwanMobilePhone(this Faker faker)
    {
        // 格式：09XX-XXX-XXX
        var thirdDigit = faker.Random.Int(0, 9);
        var fourthDigit = faker.Random.Int(0, 9);
        var middle = faker.Random.String2(3, "0123456789");
        var suffix = faker.Random.String2(3, "0123456789");
        return $"09{thirdDigit}{fourthDigit}-{middle}-{suffix}";
    }

    /// <summary>
    /// 產生台灣市內電話
    /// </summary>
    public static string TaiwanLandlinePhone(this Faker faker)
    {
        // 格式：(02) XXXX-XXXX 或 (04) XXX-XXXX
        var areaCodes = new[] { "02", "03", "04", "05", "06", "07", "08" };
        var areaCode = faker.PickRandom(areaCodes);
        
        var part1 = areaCode == "02" 
            ? faker.Random.String2(4, "0123456789")
            : faker.Random.String2(3, "0123456789");
        var part2 = faker.Random.String2(4, "0123456789");
        
        return $"({areaCode}) {part1}-{part2}";
    }

    /// <summary>
    /// 產生台灣完整地址
    /// </summary>
    public static string TaiwanFullAddress(this Faker faker)
    {
        var city = faker.TaiwanCity();
        var district = faker.TaiwanDistrict();
        var road = faker.PickRandom("中正路", "中山路", "民生路", "忠孝路", "復興路", "建國路");
        var number = faker.Random.Int(1, 500);
        var floor = faker.Random.Int(1, 20);
        
        return $"{city}{district}{road}{number}號{floor}樓";
    }
}

public class TaiwanDataSetTests
{
    /// <summary>
    /// 使用台灣自訂 DataSet
    /// </summary>
    [Fact]
    public void TaiwanDataSet_完整使用()
    {
        var taiwanPersonFaker = new Faker<TaiwanPerson>("zh_TW")
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Person.FullName)
            .RuleFor(p => p.IdCard, f => f.TaiwanIdCard())
            .RuleFor(p => p.City, f => f.TaiwanCity())
            .RuleFor(p => p.District, f => f.TaiwanDistrict())
            .RuleFor(p => p.Address, f => f.TaiwanFullAddress())
            .RuleFor(p => p.Mobile, f => f.TaiwanMobilePhone())
            .RuleFor(p => p.Company, f => f.TaiwanCompany())
            .RuleFor(p => p.University, f => f.TaiwanUniversity());

        var person = taiwanPersonFaker.Generate();

        person.IdCard.Should().HaveLength(10);
        person.Mobile.Should().StartWith("09");
        person.Address.Should().NotBeNullOrEmpty();
    }
}

#endregion

#region 多語言進階應用

// =============================================================================
// 多語言進階應用
// =============================================================================

public class MultiLanguageAdvancedExamples
{
    /// <summary>
    /// 動態語系選擇產生國際化使用者資料
    /// </summary>
    [Fact]
    public void GlobalUser_多語言動態產生()
    {
        var locales = new[] { "en_US", "zh_TW", "ja", "ko", "fr", "de" };

        var globalUserFaker = new Faker<GlobalUser>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            // 先隨機選擇語系
            .RuleFor(u => u.Locale, f => f.PickRandom(locales))
            // 根據語系產生對應的資料
            .RuleFor(u => u.Name, (f, u) =>
            {
                var localFaker = new Faker(u.Locale);
                return localFaker.Person.FullName;
            })
            .RuleFor(u => u.Address, (f, u) =>
            {
                var localFaker = new Faker(u.Locale);
                return localFaker.Address.FullAddress();
            })
            .RuleFor(u => u.Phone, (f, u) =>
            {
                var localFaker = new Faker(u.Locale);
                return localFaker.Phone.PhoneNumber();
            });

        var users = globalUserFaker.Generate(10);

        users.Should().HaveCount(10);
        users.All(u => !string.IsNullOrEmpty(u.Name)).Should().BeTrue();
    }
}

#endregion

#region 邊界值測試資料產生

// =============================================================================
// 邊界值測試資料產生
// =============================================================================

public class BoundaryTestExamples
{
    /// <summary>
    /// 產生各種邊界值測試資料
    /// </summary>
    [Fact]
    public void BoundaryTest_邊界值產生()
    {
        var boundaryFaker = new Faker<TestBoundaryData>()
            // 字串邊界
            .RuleFor(t => t.NullableString, f => f.PickRandom<string?>(null, "", " ", "valid"))
            .RuleFor(t => t.ShortString, f => f.Random.String2(1, 10))
            .RuleFor(t => t.LongString, f => f.Random.String2(255, 1000))
            // 數值邊界
            .RuleFor(t => t.MinValue, _ => int.MinValue)
            .RuleFor(t => t.MaxValue, _ => int.MaxValue)
            .RuleFor(t => t.ZeroValue, _ => 0)
            .RuleFor(t => t.NegativeValue, f => f.Random.Int(int.MinValue, -1))
            .RuleFor(t => t.PositiveValue, f => f.Random.Int(1, int.MaxValue))
            // 特殊字元
            .RuleFor(t => t.SpecialChars, f => f.PickRandom(
                "!@#$%^&*()", 
                "<script>alert('xss')</script>",
                "中文字符",
                "日本語テスト",
                "한국어 테스트",
                "emoji: 😀🎉🔥"))
            // 日期邊界
            .RuleFor(t => t.MinDate, _ => DateTime.MinValue)
            .RuleFor(t => t.MaxDate, _ => DateTime.MaxValue);

        var boundaryData = boundaryFaker.Generate();

        boundaryData.MinValue.Should().Be(int.MinValue);
        boundaryData.MaxValue.Should().Be(int.MaxValue);
    }
}

#endregion

#region 效能最佳化

// =============================================================================
// 效能最佳化
// =============================================================================

/// <summary>
/// 最佳化的資料產生器
/// </summary>
public static class OptimizedDataGenerator
{
    // 靜態 Faker 實例，避免重複建立
    private static readonly Faker _faker = new();
    
    // 預編譯的 Faker<T>
    private static readonly Faker<User> _userFaker = CreateUserFaker();
    private static readonly Faker<Product> _productFaker = CreateProductFaker();

    private static Faker<User> CreateUserFaker()
    {
        return new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80));
    }

    private static Faker<Product> CreateProductFaker()
    {
        return new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
            .RuleFor(p => p.Category, f => f.Commerce.Department());
    }

    /// <summary>
    /// 產生使用者（使用預編譯的 Faker）
    /// </summary>
    public static List<User> GenerateUsers(int count) 
        => _userFaker.Generate(count);

    /// <summary>
    /// 產生產品（使用預編譯的 Faker）
    /// </summary>
    public static List<Product> GenerateProducts(int count) 
        => _productFaker.Generate(count);

    /// <summary>
    /// 批次產生使用者（使用 yield return 減少記憶體）
    /// </summary>
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
}

/// <summary>
/// 使用 Lazy 初始化的 Faker
/// </summary>
public static class LazyFakerProvider
{
    // 延遲初始化，直到第一次使用才建立
    private static readonly Lazy<Faker<Employee>> _employeeFaker = 
        new(() => CreateEmployeeFaker(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static Faker<Employee> CreateEmployeeFaker()
    {
        return new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.Email, f => f.Internet.Email())
            .RuleFor(e => e.Age, f => f.Random.Int(22, 65))
            .RuleFor(e => e.Level, f => f.PickRandom("Junior", "Senior", "Lead", "Principal"))
            .RuleFor(e => e.Salary, f => f.Random.Decimal(35000, 200000));
    }

    public static Employee GenerateEmployee() 
        => _employeeFaker.Value.Generate();

    public static List<Employee> GenerateEmployees(int count) 
        => _employeeFaker.Value.Generate(count);
}

public class PerformanceOptimizationTests
{
    /// <summary>
    /// 使用最佳化的產生器
    /// </summary>
    [Fact]
    public void OptimizedGenerator_大量資料產生()
    {
        // Act - 使用預編譯的 Faker
        var users = OptimizedDataGenerator.GenerateUsers(1000);
        var products = OptimizedDataGenerator.GenerateProducts(500);

        // Assert
        users.Should().HaveCount(1000);
        products.Should().HaveCount(500);
    }

    /// <summary>
    /// 使用批次產生
    /// </summary>
    [Fact]
    public void BatchGeneration_減少記憶體使用()
    {
        // Act - 批次產生，使用 yield return
        var users = OptimizedDataGenerator.GenerateUsersBatch(10000, 500);
        
        // 只取前 100 筆，不會產生全部 10000 筆
        var sample = users.Take(100).ToList();

        // Assert
        sample.Should().HaveCount(100);
    }

    /// <summary>
    /// 使用 Lazy 初始化
    /// </summary>
    [Fact]
    public void LazyInitialization_延遲載入()
    {
        // Act - 使用 Lazy 初始化的 Faker
        var employees = LazyFakerProvider.GenerateEmployees(50);

        // Assert
        employees.Should().HaveCount(50);
        employees.All(e => e.Age >= 22 && e.Age <= 65).Should().BeTrue();
    }
}

#endregion

#region 測試整合範例

// =============================================================================
// 測試整合範例
// =============================================================================

// 模擬的服務介面
public interface IEmailService
{
    string GenerateWelcomeEmail(User user);
}

// 模擬的服務實作
public class EmailService : IEmailService
{
    public string GenerateWelcomeEmail(User user)
    {
        return $"Dear {user.Name},\n\nWelcome to our service!\n\nYour registered email: {user.Email}";
    }
}

public class EmailServiceTests
{
    /// <summary>
    /// 使用 Bogus 產生真實感測試資料進行服務測試
    /// </summary>
    [Fact]
    public void GenerateWelcomeEmail_使用Bogus產生測試資料()
    {
        // Arrange
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email());

        var user = userFaker.Generate();
        var emailService = new EmailService();

        // Act
        var emailContent = emailService.GenerateWelcomeEmail(user);

        // Assert
        emailContent.Should().Contain(user.Name);
        emailContent.Should().Contain(user.Email);
        emailContent.Should().Contain("Welcome");
    }

    /// <summary>
    /// 使用 Seed 確保測試可重現
    /// </summary>
    [Fact]
    public void GenerateWelcomeEmail_使用Seed確保可重現()
    {
        // Arrange - 設定 seed
        Randomizer.Seed = new Random(42);

        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email());

        var user = userFaker.Generate();
        var expectedName = user.Name;  // 記錄產生的名稱

        // 重置 seed
        Randomizer.Seed = new Random(42);
        var user2 = userFaker.Generate();

        // Assert - 相同 seed 產生相同資料
        user2.Name.Should().Be(expectedName);

        // 清理
        Randomizer.Seed = new Random();
    }
}

/// <summary>
/// 資料庫種子範例
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// 產生資料庫種子資料
    /// </summary>
    public static List<User> GenerateSeedUsers(int count = 100)
    {
        // 設定 seed 確保每次產生相同資料
        Randomizer.Seed = new Random(42);

        var userFaker = new Faker<User>("zh_TW")
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 70));

        var users = userFaker.Generate(count);

        // 重置 seed
        Randomizer.Seed = new Random();

        return users;
    }

    /// <summary>
    /// 產生產品種子資料
    /// </summary>
    public static List<Product> GenerateSeedProducts(int count = 50)
    {
        Randomizer.Seed = new Random(42);

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(100, 10000))
            .RuleFor(p => p.Category, f => f.Commerce.Department());

        var products = productFaker.Generate(count);

        Randomizer.Seed = new Random();

        return products;
    }
}

#endregion
