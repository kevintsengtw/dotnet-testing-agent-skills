using Xunit;

namespace MyProject.Tests;

/// <summary>
/// 基本單元測試範本 - 遵循 FIRST 原則與 3A Pattern
/// </summary>
public class BasicTestTemplate
{
    // -------------------------------------------------------------------------
    // [Fact] 單一測試案例範本
    // -------------------------------------------------------------------------

    [Fact]
    public void MethodName_情境描述_預期行為()
    {
        // Arrange - 準備測試資料與相依物件
        // var sut = new SystemUnderTest();
        // const int input = 1;
        // const int expected = 2;

        // Act - 執行被測試的方法
        // var result = sut.Method(input);

        // Assert - 驗證結果是否符合預期
        // Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // 正常路徑測試範本
    // -------------------------------------------------------------------------

    [Fact]
    public void Add_輸入兩個正整數_應回傳正確加總()
    {
        // Arrange
        var calculator = new Calculator();
        const int a = 1;
        const int b = 2;
        const int expected = 3;

        // Act
        var result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // 邊界條件測試範本
    // -------------------------------------------------------------------------

    [Fact]
    public void Add_輸入零_應回傳另一個數值()
    {
        // Arrange
        var calculator = new Calculator();
        const int a = 0;
        const int b = 5;
        const int expected = 5;

        // Act
        var result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // 無效輸入測試範本
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_輸入null_應回傳False()
    {
        // Arrange
        var validator = new Validator();
        string? input = null;

        // Act
        var result = validator.IsValid(input);

        // Assert
        Assert.False(result);
    }

    // -------------------------------------------------------------------------
    // 例外測試範本
    // -------------------------------------------------------------------------

    [Fact]
    public void Divide_除數為零_應拋出DivideByZeroException()
    {
        // Arrange
        var calculator = new Calculator();
        const decimal dividend = 10m;
        const decimal divisor = 0m;

        // Act & Assert
        var exception = Assert.Throws<DivideByZeroException>(
            () => calculator.Divide(dividend, divisor)
        );

        Assert.Equal("除數不能為零", exception.Message);
    }

    // -------------------------------------------------------------------------
    // 範例類別 (僅供範本參考)
    // -------------------------------------------------------------------------

    private class Calculator
    {
        public int Add(int a, int b) => a + b;
        
        public decimal Divide(decimal dividend, decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("除數不能為零");
            return dividend / divisor;
        }
    }

    private class Validator
    {
        public bool IsValid(string? input) => !string.IsNullOrWhiteSpace(input);
    }
}
