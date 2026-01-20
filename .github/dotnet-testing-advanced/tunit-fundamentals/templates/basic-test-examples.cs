namespace MyApp.Tests;

/// <summary>
/// TUnit 基本測試範例
/// 展示 [Test] 屬性、非同步斷言、參數化測試
/// </summary>
public class CalculatorTests
{
    private readonly Calculator _calculator;

    public CalculatorTests()
    {
        _calculator = new Calculator();
    }

    #region 基本測試

    /// <summary>
    /// 基本的 [Test] 測試方法
    /// 注意：TUnit 測試方法必須是 async Task
    /// </summary>
    [Test]
    public async Task Add_輸入1和2_應回傳3()
    {
        // Arrange
        int a = 1;
        int b = 2;
        int expected = 3;

        // Act
        var result = _calculator.Add(a, b);

        // Assert - 使用流暢式斷言，必須加上 await
        await Assert.That(result).IsEqualTo(expected);
    }

    /// <summary>
    /// 例外測試範例
    /// </summary>
    [Test]
    public async Task Divide_輸入0作為除數_應拋出DivideByZeroException()
    {
        // Arrange
        int dividend = 10;
        int divisor = 0;

        // Act & Assert
        await Assert.That(() => _calculator.Divide(dividend, divisor))
            .Throws<DivideByZeroException>();
    }

    /// <summary>
    /// 例外訊息驗證
    /// </summary>
    [Test]
    public async Task Divide_輸入0作為除數_應包含正確的錯誤訊息()
    {
        // Arrange
        int dividend = 10;
        int divisor = 0;

        // Act & Assert
        await Assert.That(() => _calculator.Divide(dividend, divisor))
            .Throws<DivideByZeroException>()
            .WithMessage("除數不能為零");
    }

    #endregion

    #region 參數化測試

    /// <summary>
    /// 參數化測試範例
    /// TUnit 使用 [Arguments] 取代 xUnit 的 [InlineData]
    /// </summary>
    [Test]
    [Arguments(1, 2, 3)]
    [Arguments(-1, 1, 0)]
    [Arguments(0, 0, 0)]
    [Arguments(100, -50, 50)]
    [Arguments(int.MaxValue, 0, int.MaxValue)]
    public async Task Add_多組輸入_應回傳正確結果(int a, int b, int expected)
    {
        // Act
        var result = _calculator.Add(a, b);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    /// <summary>
    /// 布林值參數化測試
    /// </summary>
    [Test]
    [Arguments(1, true)]
    [Arguments(-1, false)]
    [Arguments(0, false)]
    [Arguments(100, true)]
    [Arguments(-999, false)]
    public async Task IsPositive_各種數值_應回傳正確結果(int number, bool expected)
    {
        // Act
        var result = _calculator.IsPositive(number);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    /// <summary>
    /// 浮點數比較（允許誤差範圍）
    /// </summary>
    [Test]
    [Arguments(3.14159, 3.14, 0.01)]
    [Arguments(1.0001, 1.0, 0.001)]
    [Arguments(99.999, 100.0, 0.01)]
    public async Task 浮點數比較_應允許誤差範圍(double actual, double expected, double tolerance)
    {
        await Assert.That(actual)
            .IsEqualTo(expected)
            .Within(tolerance);
    }

    #endregion
}

/// <summary>
/// 待測試的 Calculator 類別
/// </summary>
public class Calculator
{
    public int Add(int a, int b) => a + b;

    public double Divide(int dividend, int divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("除數不能為零");
        }
        return (double)dividend / divisor;
    }

    public bool IsPositive(int number) => number > 0;
}
