// =============================================================================
// AutoFixture DataAnnotations 自動整合範例
// 展示 AutoFixture 如何自動識別 System.ComponentModel.DataAnnotations 屬性
// =============================================================================

using System.ComponentModel.DataAnnotations;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace AutoFixtureCustomization.Templates;

// -----------------------------------------------------------------------------
// 1. 使用 DataAnnotations 的模型類別
// -----------------------------------------------------------------------------

/// <summary>
/// 使用 DataAnnotations 驗證屬性的 Person 類別
/// AutoFixture 會自動識別並遵守這些驗證規則
/// </summary>
public class Person
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// StringLength(10) 會讓 AutoFixture 產生固定 10 個字元的字串
    /// </summary>
    [StringLength(10)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Range(10, 80) 會讓 AutoFixture 產生 10-80 範圍內的數值
    /// </summary>
    [Range(10, 80)]
    public int Age { get; set; }
    
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 包含多種驗證規則的 Employee 類別
/// </summary>
public class Employee
{
    public Guid Id { get; set; }
    
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Range(18, 65)]
    public int Age { get; set; }
    
    [Range(typeof(decimal), "25000", "200000")]
    public decimal Salary { get; set; }
    
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;
}

// -----------------------------------------------------------------------------
// 2. DataAnnotations 自動識別測試
// -----------------------------------------------------------------------------

public class DataAnnotationsIntegrationTests
{
    /// <summary>
    /// 驗證 AutoFixture 自動識別 StringLength 和 Range 屬性
    /// </summary>
    [Fact]
    public void AutoFixture_應能識別DataAnnotations()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var person = fixture.Create<Person>();

        // Assert
        person.Name.Length.Should().Be(10);        // StringLength(10) 產生固定 10 字元
        person.Age.Should().BeInRange(10, 80);     // Range(10, 80) 產生 10-80 的值
        person.Id.Should().NotBeEmpty();           // Guid 自動產生
        person.CreateTime.Should().NotBe(default); // DateTime 自動產生
    }

    /// <summary>
    /// 驗證批量產生的物件都符合 DataAnnotations 限制
    /// </summary>
    [Fact]
    public void AutoFixture_批量產生_都符合限制()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var persons = fixture.CreateMany<Person>(10).ToList();

        // Assert
        persons.Should().HaveCount(10);
        persons.Should().AllSatisfy(person =>
        {
            person.Name.Length.Should().Be(10);
            person.Age.Should().BeInRange(10, 80);
            person.Id.Should().NotBeEmpty();
        });
    }

    /// <summary>
    /// 驗證複雜的 DataAnnotations 規則
    /// </summary>
    [Fact]
    public void AutoFixture_應能識別複雜驗證規則()
    {
        // Arrange
        var fixture = new Fixture();

        // Act
        var employees = fixture.CreateMany<Employee>(5).ToList();

        // Assert
        employees.Should().AllSatisfy(employee =>
        {
            employee.Name.Length.Should().BeInRange(2, 50);
            employee.Age.Should().BeInRange(18, 65);
            employee.Salary.Should().BeInRange(25000m, 200000m);
            employee.Department.Length.Should().BeLessOrEqualTo(100);
        });
    }
}

// -----------------------------------------------------------------------------
// 3. .With() 方法：固定值 vs 動態值
// -----------------------------------------------------------------------------

public class Member
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime JoinDate { get; set; }
}

public class WithMethodTests
{
    /// <summary>
    /// 展示 .With() 使用固定值 vs 動態值的差異
    /// 這是一個常見的陷阱！
    /// </summary>
    [Fact]
    public void With方法_固定值vs動態值的差異()
    {
        var fixture = new Fixture();

        // ❌ 固定值：Random.Shared.Next() 只執行一次，所有物件相同年齡
        var fixedAgeMembers = fixture.Build<Member>()
            .With(x => x.Age, Random.Shared.Next(30, 50))  // 這裡的 Random.Shared.Next 只執行一次
            .CreateMany(5)
            .ToList();

        // ✅ 動態值：使用 lambda，每個物件都重新計算
        var dynamicAgeMembers = fixture.Build<Member>()
            .With(x => x.Age, () => Random.Shared.Next(30, 50))  // 這裡的 lambda 每次都執行
            .CreateMany(5)
            .ToList();

        // 驗證固定值：所有物件應該有相同年齡
        fixedAgeMembers.Select(m => m.Age).Distinct().Count().Should().Be(1);

        // 驗證動態值：物件應該有不同年齡（大多數情況）
        // 注意：理論上可能全部相同，但機率很低
        dynamicAgeMembers.Select(m => m.Age).Distinct().Count().Should().BeGreaterThan(1);
    }

    /// <summary>
    /// 結合多個動態值設定
    /// </summary>
    [Fact]
    public void 多重動態值設定()
    {
        var fixture = new Fixture();

        var baseDate = new DateTime(2025, 1, 1);

        var members = fixture.Build<Member>()
            .With(x => x.Age, () => Random.Shared.Next(20, 60))
            .With(x => x.JoinDate, () => baseDate.AddDays(Random.Shared.Next(0, 365)))
            .CreateMany(10)
            .ToList();

        members.Should().AllSatisfy(m =>
        {
            m.Age.Should().BeInRange(20, 59);
            m.JoinDate.Should().BeOnOrAfter(baseDate);
            m.JoinDate.Should().BeBefore(baseDate.AddDays(365));
        });
    }
}

// -----------------------------------------------------------------------------
// 4. Random.Shared vs new Random() 比較
// -----------------------------------------------------------------------------

public class RandomComparisonTests
{
    /// <summary>
    /// 展示 new Random() 可能產生的問題
    /// </summary>
    [Fact]
    public void NewRandom_可能產生相同序列()
    {
        // 在快速連續建立時，new Random() 可能使用相同的種子值
        // 導致產生相同的隨機數序列
        var random1 = new Random();
        var random2 = new Random();
        
        // 這兩個可能產生相同的值（取決於執行速度）
        // var value1 = random1.Next(100);
        // var value2 = random2.Next(100);
        
        // 使用 Random.Shared 可以避免這個問題
        var values = Enumerable.Range(0, 10)
            .Select(_ => Random.Shared.Next(100))
            .Distinct()
            .Count();
            
        // Random.Shared 應該產生更好的分布
        values.Should().BeGreaterThan(1);
    }

    /// <summary>
    /// Random.Shared 是執行緒安全的
    /// </summary>
    [Fact]
    public async Task RandomShared_是執行緒安全的()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => 
                Enumerable.Range(0, 100)
                    .Select(_ => Random.Shared.Next(1000))
                    .ToList()))
            .ToList();

        var results = await Task.WhenAll(tasks);
        
        // 所有任務都應該成功完成，不會有執行緒安全問題
        results.Should().AllSatisfy(list => list.Should().HaveCount(100));
    }
}
