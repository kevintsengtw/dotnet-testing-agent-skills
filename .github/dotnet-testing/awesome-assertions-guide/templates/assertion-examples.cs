using AwesomeAssertions;
using Xunit;

namespace YourProject.Tests.Examples;

/// <summary>
/// AwesomeAssertions 常用斷言範例集合
/// 涵蓋物件、字串、數值、集合、例外、非同步等各種情境
/// </summary>
public class AssertionExamples
{
    #region 物件斷言範例

    [Fact]
    public void ObjectAssertions_基本驗證()
    {
        var user = new User 
        { 
            Id = 1, 
            Name = "John Doe", 
            Email = "john@example.com" 
        };

        // 空值檢查
        user.Should().NotBeNull();

        // 類型檢查
        user.Should().BeOfType<User>();
        user.Should().BeAssignableTo<IUser>();

        // 相等性檢查
        var anotherUser = new User 
        { 
            Id = 1, 
            Name = "John Doe", 
            Email = "john@example.com" 
        };
        user.Should().BeEquivalentTo(anotherUser);
    }

    [Fact]
    public void ObjectAssertions_屬性驗證()
    {
        var product = new Product 
        { 
            Id = 1, 
            Name = "Laptop", 
            Price = 999.99m, 
            Stock = 10 
        };

        // 單一屬性驗證
        product.Id.Should().BeGreaterThan(0);
        product.Name.Should().NotBeNullOrEmpty();
        product.Price.Should().BePositive();

        // 多屬性匿名物件比對
        product.Should().BeEquivalentTo(new
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m
        });
    }

    #endregion

    #region 字串斷言範例

    [Fact]
    public void StringAssertions_內容驗證()
    {
        var message = "Hello World";

        // 基本檢查
        message.Should().NotBeNullOrEmpty();
        message.Should().NotBeNullOrWhiteSpace();

        // 內容檢查
        message.Should().Contain("Hello");
        message.Should().StartWith("Hello");
        message.Should().EndWith("World");
        message.Should().ContainEquivalentOf("WORLD"); // 忽略大小寫

        // 長度檢查
        message.Should().HaveLength(11);
        message.Should().HaveLengthGreaterThan(5);
    }

    [Fact]
    public void StringAssertions_模式匹配()
    {
        var email = "user@example.com";

        // 正規表示式匹配
        email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");

        // 包含關鍵字
        email.Should().Contain("@").And.Contain(".");

        // Email 格式驗證範例
        email.Should().NotBeNullOrEmpty()
             .And.Contain("@")
             .And.MatchRegex(@"@[\w-]+\.");
    }

    #endregion

    #region 數值斷言範例

    [Fact]
    public void NumericAssertions_範圍驗證()
    {
        var age = 25;

        // 比較運算
        age.Should().BeGreaterThan(18);
        age.Should().BeLessThan(65);
        age.Should().BeInRange(18, 65);

        // 特定值檢查
        age.Should().BeOneOf(25, 30, 35);
        age.Should().BePositive();
    }

    [Fact]
    public void NumericAssertions_浮點數處理()
    {
        var pi = 3.14159;

        // 精度比較（重要！避免浮點數精度問題）
        pi.Should().BeApproximately(3.14, 0.01);

        // 特殊值檢查
        double.NaN.Should().Be(double.NaN);
        double.PositiveInfinity.Should().BePositiveInfinity();
        double.NegativeInfinity.Should().BeNegativeInfinity();
    }

    #endregion

    #region 集合斷言範例

    [Fact]
    public void CollectionAssertions_基本驗證()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // 數量檢查
        numbers.Should().NotBeEmpty();
        numbers.Should().HaveCount(5);
        numbers.Should().HaveCountGreaterThan(3);

        // 內容檢查
        numbers.Should().Contain(3);
        numbers.Should().ContainSingle(x => x == 3);
        numbers.Should().NotContain(0);

        // 完整比對
        numbers.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void CollectionAssertions_複雜物件()
    {
        var users = new[]
        {
            new User { Id = 1, Name = "John", Age = 30 },
            new User { Id = 2, Name = "Jane", Age = 25 },
            new User { Id = 3, Name = "Bob", Age = 35 }
        };

        // 條件過濾
        users.Should().Contain(u => u.Name == "John");
        users.Should().OnlyContain(u => u.Age >= 18);

        // 全部滿足條件
        users.Should().AllSatisfy(u =>
        {
            u.Id.Should().BeGreaterThan(0);
            u.Name.Should().NotBeNullOrEmpty();
            u.Age.Should().BePositive();
        });

        // 順序檢查
        var ages = users.Select(u => u.Age).ToArray();
        ages.Should().BeInAscendingOrder();
    }

    #endregion

    #region 例外斷言範例

    [Fact]
    public void ExceptionAssertions_基本驗證()
    {
        var calculator = new Calculator();

        // 預期拋出例外
        Action act = () => calculator.Divide(10, 0);

        act.Should().Throw<DivideByZeroException>()
           .WithMessage("*cannot divide by zero*");
    }

    [Fact]
    public void ExceptionAssertions_參數驗證()
    {
        var userService = new UserService();

        // 參數例外驗證
        Action act = () => userService.GetUser(-1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*User ID must be positive*")
           .And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void ExceptionAssertions_不應拋出()
    {
        var calculator = new Calculator();

        // 不應拋出任何例外
        Action act = () => calculator.Add(1, 2);
        act.Should().NotThrow();
    }

    #endregion

    #region 非同步斷言範例

    [Fact]
    public async Task AsyncAssertions_任務完成()
    {
        var service = new AsyncService();

        // 等待任務完成並驗證
        var result = await service.GetDataAsync();

        result.Should().NotBeNull();
        result.Should().BeOfType<DataResult>();
    }

    [Fact]
    public async Task AsyncAssertions_執行時間()
    {
        var service = new AsyncService();

        // 驗證執行時間
        Func<Task> act = () => service.GetDataAsync();

        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AsyncAssertions_例外處理()
    {
        var service = new AsyncService();

        // 非同步例外驗證
        Func<Task> act = async () => await service.GetInvalidDataAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*data not found*");
    }

    #endregion

    #region 複雜物件比對範例

    [Fact]
    public void ComplexObjectComparison_深度比較()
    {
        var order = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            Items = new[]
            {
                new OrderItem { ProductId = 1, Quantity = 2, Price = 10.5m },
                new OrderItem { ProductId = 2, Quantity = 1, Price = 25.0m }
            },
            TotalAmount = 46.0m
        };

        var expected = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            Items = new[]
            {
                new OrderItem { ProductId = 1, Quantity = 2, Price = 10.5m },
                new OrderItem { ProductId = 2, Quantity = 1, Price = 25.0m }
            },
            TotalAmount = 46.0m
        };

        // 深度物件比較
        order.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ComplexObjectComparison_排除欄位()
    {
        var user = new User
        {
            Id = 1,
            Name = "John",
            Email = "john@example.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var expected = new User
        {
            Id = 1,
            Name = "John",
            Email = "john@example.com",
            CreatedAt = DateTime.Now.AddDays(-1),  // 不同的時間
            UpdatedAt = DateTime.Now.AddHours(-2)   // 不同的時間
        };

        // 排除時間戳記欄位
        user.Should().BeEquivalentTo(expected, options => options
            .Excluding(u => u.CreatedAt)
            .Excluding(u => u.UpdatedAt)
        );
    }

    [Fact]
    public void ComplexObjectComparison_部分屬性()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m,
            Stock = 10,
            CreatedAt = DateTime.Now
        };

        // 只比對特定屬性
        product.Should().BeEquivalentTo(new
        {
            Name = "Laptop",
            Price = 999.99m
        });
    }

    #endregion

    #region AssertionScope 範例

    [Fact]
    public void AssertionScope_收集多個失敗()
    {
        var user = new User
        {
            Id = 0,  // 錯誤：應該 > 0
            Name = "",  // 錯誤：不應為空
            Email = "invalid-email"  // 錯誤：格式不正確
        };

        // 使用 AssertionScope 收集所有失敗的斷言
        using (new AssertionScope())
        {
            user.Id.Should().BeGreaterThan(0, "User ID must be positive");
            user.Name.Should().NotBeNullOrEmpty("User name is required");
            user.Email.Should().MatchRegex(@"@.*\.", "Email format is invalid");
        }
        // 所有失敗會一次顯示，而不是在第一個失敗時就停止
    }

    #endregion
}

#region 測試用的模型類別

public interface IUser
{
    int Id { get; set; }
    string Name { get; set; }
}

public class User : IUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderItem[] Items { get; set; } = Array.Empty<OrderItem>();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class DataResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion

#region 測試用的服務類別

public class Calculator
{
    public int Add(int a, int b) => a + b;
    
    public int Divide(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException("Cannot divide by zero");
        return a / b;
    }
}

public class UserService
{
    public User GetUser(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be positive", nameof(userId));
        
        return new User { Id = userId, Name = "Test User" };
    }
}

public class AsyncService
{
    public async Task<DataResult> GetDataAsync()
    {
        await Task.Delay(100);
        return new DataResult { IsSuccess = true, Message = "Success" };
    }
    
    public async Task<DataResult> GetInvalidDataAsync()
    {
        await Task.Delay(100);
        throw new InvalidOperationException("Data not found");
    }
}

#endregion
