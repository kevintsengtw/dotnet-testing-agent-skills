// =============================================================================
// 測試命名規範範例
// 標準格式：方法名稱_情境描述_預期結果（三段式命名法）
// =============================================================================

using Xunit;

namespace TestNamingConventions.Examples;

// =============================================================================
// 測試類別命名：{被測類別}Tests
// =============================================================================

/// <summary>
/// 計算器功能測試 - 展示基本運算的三段式命名
/// </summary>
public class CalculatorTests
{
    // ✅ 正常路徑測試
    [Fact]
    public void Add_輸入1和2_應回傳3()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(1, 2);

        // Assert
        Assert.Equal(3, result);
    }

    // ✅ 邊界條件測試
    [Fact]
    public void Add_輸入0和0_應回傳0()
    {
        var calculator = new Calculator();
        var result = calculator.Add(0, 0);
        Assert.Equal(0, result);
    }

    // ✅ 負數測試
    [Fact]
    public void Add_輸入負數和正數_應回傳正確結果()
    {
        var calculator = new Calculator();
        var result = calculator.Add(-1, 3);
        Assert.Equal(2, result);
    }
}

/// <summary>
/// Email 驗證測試 - 展示驗證邏輯的命名方式
/// </summary>
public class EmailValidatorTests
{
    // ✅ 有效輸入測試
    [Fact]
    public void IsValidEmail_輸入有效Email_應回傳True()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail("user@example.com");
        Assert.True(result);
    }

    // ✅ 無效輸入 - null
    [Fact]
    public void IsValidEmail_輸入null值_應回傳False()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail(null!);
        Assert.False(result);
    }

    // ✅ 無效輸入 - 空字串
    [Fact]
    public void IsValidEmail_輸入空字串_應回傳False()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail("");
        Assert.False(result);
    }

    // ✅ 格式錯誤
    [Fact]
    public void IsValidEmail_輸入無效Email格式_應回傳False()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail("not-an-email");
        Assert.False(result);
    }
}

/// <summary>
/// 訂單處理測試 - 展示業務邏輯與例外的命名方式
/// </summary>
public class OrderServiceTests
{
    // ✅ 正常處理流程
    [Fact]
    public void ProcessOrder_輸入有效訂單_應回傳處理後訂單()
    {
        // Arrange & Act & Assert
    }

    // ✅ 例外情境
    [Fact]
    public void ProcessOrder_輸入null_應拋出ArgumentNullException()
    {
        // Arrange & Act & Assert
    }

    // ✅ 計算邏輯
    [Fact]
    public void Calculate_輸入100元和10Percent折扣_應回傳90元()
    {
        // Arrange & Act & Assert
    }

    // ✅ 狀態變化
    [Fact]
    public void Cancel_已完成訂單_應拋出InvalidOperationException()
    {
        // Arrange & Act & Assert
    }
}

// =============================================================================
// ❌ 不良命名範例（請勿模仿）
// =============================================================================

// ❌ public void TestAdd() { }              // 缺乏情境與預期結果
// ❌ public void Test1() { }                // 毫無意義的名稱
// ❌ public void EmailTest() { }            // 缺乏三段式結構
// ❌ public void OrderTest() { }            // 無法看出測試目的

// =============================================================================
// 常用情境詞彙參考
// =============================================================================
// 正常路徑：有效、正確、正常、成功
// 邊界條件：最小值、最大值、空集合、零
// 錯誤情境：null、空字串、無效格式、超出範圍
// 狀態變化：從X到Y、初始狀態、已完成、已取消
// 預期結果：應回傳、應拋出、應包含、應為空、應相等
