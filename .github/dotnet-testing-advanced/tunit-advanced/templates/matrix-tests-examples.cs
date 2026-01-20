// TUnit Matrix Tests 組合測試範例

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

namespace TUnit.Advanced.Matrix.Examples;

#region Domain Models

public enum CustomerLevel
{
    一般會員 = 0,
    VIP會員 = 1,
    白金會員 = 2,
    鑽石會員 = 3
}

public class Order
{
    public CustomerLevel CustomerLevel { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal SubTotal => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class OrderItem
{
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

#endregion

#region Matrix Tests Examples

/// <summary>
/// Matrix Tests 基本使用範例
/// 自動產生所有參數組合的測試案例
/// </summary>
public class MatrixTestsBasicExamples
{
    /// <summary>
    /// 基本 Matrix 測試
    /// 這會產生 4 × 4 = 16 個測試案例
    /// 
    /// 重要注意事項：
    /// - 使用 [MatrixDataSource] 屬性標記測試方法
    /// - 由於 C# 屬性限制，enum 必須用數值表示
    /// - TUnit 會自動將數值轉換為對應的 enum 值
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task CalculateShipping_客戶等級與金額組合_應遵循運費規則(
        [Matrix(0, 1, 2, 3)] CustomerLevel customerLevel, // 0=一般會員, 1=VIP會員, 2=白金會員, 3=鑽石會員
        [Matrix(100, 500, 1000, 2000)] decimal orderAmount)
    {
        // Arrange
        var order = new Order
        {
            CustomerLevel = customerLevel,
            Items = [new OrderItem { UnitPrice = orderAmount, Quantity = 1 }]
        };

        // Act
        var shippingFee = CalculateShippingFee(order);
        var isFreeShipping = IsEligibleForFreeShipping(order);

        // Assert - 驗證運費邏輯的一致性
        if (isFreeShipping)
        {
            await Assert.That(shippingFee).IsEqualTo(0m);
        }
        else
        {
            await Assert.That(shippingFee).IsGreaterThan(0m);
        }

        // 驗證特定規則
        switch (customerLevel)
        {
            case CustomerLevel.鑽石會員:
                await Assert.That(shippingFee).IsEqualTo(0m); // 鑽石會員永遠免運
                break;

            case CustomerLevel.VIP會員 or CustomerLevel.白金會員:
                if (orderAmount < 1000m)
                {
                    await Assert.That(shippingFee).IsEqualTo(40m); // VIP+ 運費半價
                }
                break;

            case CustomerLevel.一般會員:
                if (orderAmount < 1000m)
                {
                    await Assert.That(shippingFee).IsEqualTo(80m); // 一般會員標準運費
                }
                break;
        }
    }

    /// <summary>
    /// 折扣邏輯的 Matrix 測試
    /// 專注於核心組合 - 總共 2 × 4 = 8 個測試
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task TestDiscountLogic_會員與金額組合_應正確計算(
        [Matrix(true, false)] bool isMember,
        [Matrix(0, 1, 100, 1000)] int amount)
    {
        // Arrange
        var discount = CalculateMemberDiscount(isMember, amount);

        // Assert
        if (!isMember)
        {
            await Assert.That(discount).IsEqualTo(0m);
        }
        else if (amount >= 1000)
        {
            await Assert.That(discount).IsGreaterThan(0m);
        }
    }

    /// <summary>
    /// 布林值組合的 Matrix 測試
    /// 產生 2 × 2 = 4 個測試
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task ValidateInput_布林組合_應正確驗證(
        [Matrix(true, false)] bool hasEmail,
        [Matrix(true, false)] bool hasPhone)
    {
        // Arrange
        var isValid = hasEmail || hasPhone; // 至少需要一種聯絡方式

        // Assert
        if (hasEmail || hasPhone)
        {
            await Assert.That(isValid).IsTrue();
        }
        else
        {
            await Assert.That(isValid).IsFalse();
        }
    }

    /// <summary>
    /// 字串與數值組合的 Matrix 測試
    /// 產生 3 × 4 = 12 個測試
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task ProcessPayment_付款方式與金額組合_應正確處理(
        [Matrix("CreditCard", "DebitCard", "BankTransfer")] string paymentMethod,
        [Matrix(100, 500, 1000, 5000)] decimal amount)
    {
        // Arrange
        var fee = CalculatePaymentFee(paymentMethod, amount);

        // Assert
        await Assert.That(fee).IsGreaterThanOrEqualTo(0);

        // 特定規則驗證
        if (paymentMethod == "BankTransfer" && amount >= 1000)
        {
            await Assert.That(fee).IsEqualTo(0); // 銀行轉帳滿額免手續費
        }
    }

    #region Helper Methods

    private static decimal CalculateShippingFee(Order order)
    {
        // 鑽石會員永遠免運
        if (order.CustomerLevel == CustomerLevel.鑽石會員)
            return 0m;

        // 滿額免運
        if (order.SubTotal >= 1000m)
            return 0m;

        // VIP 和白金會員運費半價
        if (order.CustomerLevel == CustomerLevel.VIP會員 || 
            order.CustomerLevel == CustomerLevel.白金會員)
            return 40m;

        // 一般會員標準運費
        return 80m;
    }

    private static bool IsEligibleForFreeShipping(Order order)
    {
        return order.CustomerLevel == CustomerLevel.鑽石會員 || order.SubTotal >= 1000m;
    }

    private static decimal CalculateMemberDiscount(bool isMember, int amount)
    {
        if (!isMember) return 0m;
        if (amount >= 1000) return amount * 0.1m;
        if (amount >= 100) return amount * 0.05m;
        return 0m;
    }

    private static decimal CalculatePaymentFee(string paymentMethod, decimal amount)
    {
        return paymentMethod switch
        {
            "CreditCard" => amount * 0.03m,
            "DebitCard" => amount * 0.01m,
            "BankTransfer" when amount >= 1000 => 0m,
            "BankTransfer" => 30m,
            _ => 0m
        };
    }

    #endregion
}

#endregion

#region Matrix Tests Best Practices

/// <summary>
/// Matrix Tests 最佳實踐範例
/// 展示如何有效使用 Matrix Tests 避免常見陷阱
/// </summary>
public class MatrixTestsBestPractices
{
    /// <summary>
    /// 好的做法：專注於核心組合
    /// 總共 2 × 4 = 8 個測試，每個都有明確意義
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task GoodPractice_專注核心組合_避免過多測試(
        [Matrix(true, false)] bool isMember,
        [Matrix(0, 1, 100, 1000)] int amount) // 關鍵金額門檻
    {
        var discount = isMember && amount >= 100 ? amount * 0.05m : 0;
        await Assert.That(discount).IsGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Matrix Tests 適合的場景範例：
    /// 業務規則的交叉驗證
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task BusinessRuleValidation_訂單限制驗證(
        [Matrix(0, 1, 2, 3)] CustomerLevel level, // 客戶等級
        [Matrix(1, 5, 10, 50)] int itemCount)     // 商品數量
    {
        // 不同等級有不同的商品數量限制
        var maxItems = level switch
        {
            CustomerLevel.鑽石會員 => 100,
            CustomerLevel.白金會員 => 50,
            CustomerLevel.VIP會員 => 30,
            _ => 20
        };

        var isValid = itemCount <= maxItems;

        await Assert.That(isValid).IsTrue();
    }

    /// <summary>
    /// Matrix Tests 適合的場景範例：
    /// API 參數的有效性檢查
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task ApiParameterValidation_頁面參數驗證(
        [Matrix(1, 10, 50, 100)] int pageSize,
        [Matrix(1, 2, 10, 100)] int pageNumber)
    {
        // 驗證分頁參數的有效性
        var isValidPageSize = pageSize >= 1 && pageSize <= 100;
        var isValidPageNumber = pageNumber >= 1;

        await Assert.That(isValidPageSize).IsTrue();
        await Assert.That(isValidPageNumber).IsTrue();
    }
}

#endregion

#region Matrix Tests Warnings

/// <summary>
/// Matrix Tests 注意事項與警告
/// 展示應該避免的反模式
/// </summary>
public class MatrixTestsWarnings
{
    /*
     * ⚠️ 避免的做法：過多組合會造成指數級增長
     * 
     * 以下範例會產生 5 × 4 × 3 × 6 = 360 個測試案例！
     * 這會讓測試執行時間過長
     * 
     * [Test]
     * [MatrixDataSource]
     * public async Task TooManyParameters_過多組合_應避免(
     *     [Matrix(1, 2, 3, 4, 5)] int quantity,
     *     [Matrix(0, 1, 2, 3)] CustomerLevel level,
     *     [Matrix(true, false, null)] bool? expedited,
     *     [Matrix("Standard", "Express", "Overnight", "International", "Pickup", "Digital")] string method)
     * {
     *     // 360 個測試案例會讓測試執行時間過長！
     * }
     */

    /*
     * ⚠️ C# enum 常數限制
     * 
     * 在 C# 中，enum 值不能直接在 attribute 中使用作為常數
     * 
     * ❌ 這樣寫會編譯錯誤：
     * [Test]
     * [MatrixDataSource]
     * public async Task TestMethod(
     *     [Matrix(CustomerLevel.一般會員, CustomerLevel.VIP會員)] CustomerLevel level)
     * {
     * }
     * 
     * ✅ 正確做法：使用數值代表 enum
     * [Test]
     * [MatrixDataSource]
     * public async Task TestMethod(
     *     [Matrix(0, 1, 2, 3)] CustomerLevel level) // 0=一般會員, 1=VIP會員, 2=白金會員, 3=鑽石會員
     * {
     * }
     */

    /// <summary>
    /// 實務建議：
    /// 1. 限制參數組合數量，避免超過 50-100 個案例
    /// 2. 考慮使用 [Arguments] 來指定重要的組合
    /// 3. 使用 Theory 測試來補充邊界情況
    /// 4. Matrix Tests 不適合執行時間較長的整合測試
    /// </summary>
    [Test]
    public async Task MatrixTestsGuidelines_最佳實踐提示()
    {
        // 這只是一個文件說明測試
        await Assert.That(true).IsTrue();
    }
}

#endregion
