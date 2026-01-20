using Xunit;

namespace MyProject.Tests;

/// <summary>
/// 參數化測試範本 - 使用 [Theory] 與 [InlineData] 測試多個案例
/// </summary>
public class ParameterizedTestTemplate
{
    // -------------------------------------------------------------------------
    // [Theory] + [InlineData] 基本範本
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    [InlineData(100, -50, 50)]
    public void Add_輸入各種數值組合_應回傳正確結果(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // 測試多個無效輸入
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_輸入空白或null_應回傳False(string? input)
    {
        // Arrange
        var validator = new Validator();

        // Act
        var result = validator.IsValid(input);

        // Assert
        Assert.False(result);
    }

    // -------------------------------------------------------------------------
    // 測試多個有效輸入
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("admin@company.co.uk")]
    [InlineData("test123@test-domain.com")]
    public void IsValidEmail_輸入有效Email格式_應回傳True(string validEmail)
    {
        // Arrange
        var emailHelper = new EmailHelper();

        // Act
        var result = emailHelper.IsValidEmail(validEmail);

        // Assert
        Assert.True(result);
    }

    // -------------------------------------------------------------------------
    // 測試輸入輸出對應關係
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("test@gmail.com", "gmail.com")]
    [InlineData("admin@company.co.uk", "company.co.uk")]
    [InlineData("user@sub.domain.org", "sub.domain.org")]
    public void GetDomain_輸入各種有效Email_應回傳對應網域(string email, string expectedDomain)
    {
        // Arrange
        var emailHelper = new EmailHelper();

        // Act
        var result = emailHelper.GetDomain(email);

        // Assert
        Assert.Equal(expectedDomain, result);
    }

    // -------------------------------------------------------------------------
    // 測試除法運算
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(10, 2, 5)]
    [InlineData(15, 3, 5)]
    [InlineData(7, 2, 3.5)]
    [InlineData(-10, 2, -5)]
    public void Divide_輸入各種有效數值_應回傳正確結果(
        decimal dividend, 
        decimal divisor, 
        decimal expected)
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Divide(dividend, divisor);

        // Assert
        Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // 測試設定任意值
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(10)]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(999)]
    public void SetValue_輸入任意值_應設定正確數值(int value)
    {
        // Arrange
        var counter = new Counter();

        // Act
        counter.SetValue(value);

        // Assert
        Assert.Equal(value, counter.Value);
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

    private class EmailHelper
    {
        public bool IsValidEmail(string email) => 
            !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');
        
        public string? GetDomain(string email)
        {
            if (!IsValidEmail(email)) return null;
            return email.Split('@')[1];
        }
    }

    private class Counter
    {
        public int Value { get; private set; }
        public void SetValue(int value) => Value = value;
    }
}
