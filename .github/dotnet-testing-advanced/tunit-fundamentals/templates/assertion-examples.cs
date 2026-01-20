namespace MyApp.Tests;

/// <summary>
/// TUnit 斷言系統完整範例
/// 展示各種斷言類型的使用方式
/// </summary>
public class AssertionExamplesTests
{
    #region 基本相等性斷言

    [Test]
    public async Task 基本相等性斷言範例()
    {
        // 數值相等檢查
        var expected = 42;
        var actual = 40 + 2;
        await Assert.That(actual).IsEqualTo(expected);
        await Assert.That(actual).IsNotEqualTo(43);

        // 物件相等檢查
        var obj1 = new object();
        var obj2 = obj1;
        await Assert.That(obj2).IsEqualTo(obj1);
    }

    [Test]
    public async Task Null值檢查範例()
    {
        string? nullValue = null;
        string notNullValue = "test";

        await Assert.That(nullValue).IsNull();
        await Assert.That(notNullValue).IsNotNull();
    }

    #endregion

    #region 布林值斷言

    [Test]
    public async Task 布林值斷言範例()
    {
        // 基本布林檢查
        var condition1 = 1 + 1 == 2;
        var condition2 = 1 + 1 == 3;
        await Assert.That(condition1).IsTrue();
        await Assert.That(condition2).IsFalse();

        // 條件式布林檢查
        var number = 10;
        await Assert.That(number > 5).IsTrue();
        await Assert.That(number < 5).IsFalse();
    }

    #endregion

    #region 數值比較斷言

    [Test]
    public async Task 數值比較斷言範例()
    {
        var actualValue = 5 + 5;  // 10
        var compareValue = 3 + 2; // 5
        var equalValue = 4 + 6;   // 10

        // 基本數值比較
        await Assert.That(actualValue).IsGreaterThan(compareValue);
        await Assert.That(actualValue).IsGreaterThanOrEqualTo(equalValue);
        await Assert.That(compareValue).IsLessThan(actualValue);
        await Assert.That(compareValue).IsLessThanOrEqualTo(compareValue);

        // 數值範圍檢查
        await Assert.That(actualValue).IsBetween(5, 15);
    }

    [Test]
    [Arguments(3.14159, 3.14, 0.01)]
    [Arguments(1.0001, 1.0, 0.001)]
    [Arguments(99.999, 100.0, 0.01)]
    public async Task 浮點數精確度控制(double actual, double expected, double tolerance)
    {
        // 浮點數比較允許誤差範圍
        await Assert.That(actual)
            .IsEqualTo(expected)
            .Within(tolerance);
    }

    #endregion

    #region 字串斷言

    [Test]
    public async Task 字串斷言範例()
    {
        var email = "user@example.com";
        var emptyString = "";

        // 包含檢查
        await Assert.That(email).Contains("@");
        await Assert.That(email).Contains("example");
        await Assert.That(email).DoesNotContain(" ");

        // 開始/結束檢查
        await Assert.That(email).StartsWith("user");
        await Assert.That(email).EndsWith(".com");

        // 空字串檢查
        await Assert.That(emptyString).IsEmpty();
        await Assert.That(email).IsNotEmpty();

        // 字串長度檢查
        await Assert.That(email.Length).IsGreaterThan(5);
        await Assert.That(email.Length).IsEqualTo(16);
    }

    #endregion

    #region 集合斷言

    [Test]
    public async Task 集合斷言範例()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5 };
        var emptyList = new List<string>();

        // 集合計數檢查
        await Assert.That(numbers).HasCount(5);
        await Assert.That(emptyList).IsEmpty();
        await Assert.That(numbers).IsNotEmpty();

        // 元素包含檢查
        await Assert.That(numbers).Contains(3);
        await Assert.That(numbers).DoesNotContain(10);

        // 集合位置檢查
        await Assert.That(numbers.First()).IsEqualTo(1);
        await Assert.That(numbers.Last()).IsEqualTo(5);
        await Assert.That(numbers[2]).IsEqualTo(3);

        // 集合全部檢查
        await Assert.That(numbers.All(x => x > 0)).IsTrue();
        await Assert.That(numbers.Any(x => x > 3)).IsTrue();
    }

    #endregion

    #region 例外斷言

    [Test]
    public async Task 例外斷言範例()
    {
        var calculator = new Calculator();

        // 檢查特定例外類型
        await Assert.That(() => calculator.Divide(10, 0))
            .Throws<DivideByZeroException>();

        // 檢查例外訊息
        await Assert.That(() => calculator.Divide(10, 0))
            .Throws<DivideByZeroException>()
            .WithMessage("除數不能為零");

        // 檢查不拋出例外
        await Assert.That(() => calculator.Add(1, 2))
            .DoesNotThrow();
    }

    #endregion

    #region And 條件組合

    [Test]
    public async Task And條件組合範例()
    {
        var number = 10;

        // 組合多個條件 - 全部都必須成立
        await Assert.That(number)
            .IsGreaterThan(5)
            .And.IsLessThan(15)
            .And.IsEqualTo(10);

        var email = "test@example.com";
        await Assert.That(email)
            .Contains("@")
            .And.EndsWith(".com")
            .And.StartsWith("test");
    }

    #endregion

    #region Or 條件組合

    [Test]
    public async Task Or條件組合範例()
    {
        var number = 15;

        // 任一條件成立即可通過
        await Assert.That(number)
            .IsEqualTo(10)
            .Or.IsEqualTo(15)
            .Or.IsEqualTo(20);

        var text = "Hello World";
        await Assert.That(text)
            .StartsWith("Hi")
            .Or.StartsWith("Hello")
            .Or.StartsWith("Hey");
    }

    [Test]
    public async Task Or條件實務範例()
    {
        var email = "admin@company.com";

        // 檢查是否為管理員或測試帳號
        await Assert.That(email)
            .StartsWith("admin@")
            .Or.StartsWith("test@")
            .Or.Contains("@localhost");

        var httpStatusCode = 200;

        // 檢查是否為成功的 HTTP 狀態碼
        await Assert.That(httpStatusCode)
            .IsEqualTo(200)  // OK
            .Or.IsEqualTo(201)  // Created
            .Or.IsEqualTo(204); // No Content
    }

    #endregion
}
