using System;
using System.Reflection;
using Xunit;
using AwesomeAssertions;

/// <summary>
/// 反射測試範例
/// 展示如何使用反射技術測試私有方法
/// </summary>
/// 
// ========================================
// 被測試的類別：包含私有方法
// ========================================

namespace MyProject.Core;

public class PaymentProcessor
{
    /// <summary>
    /// 公開方法：處理付款
    /// </summary>
    public PaymentResult ProcessPayment(PaymentRequest request)
    {
        if (!ValidateRequest(request))
            return PaymentResult.InvalidRequest();

        var fee = CalculateFee(request.Amount, request.Method);
        var total = request.Amount + fee;
        
        return PaymentResult.Success(total);
    }

    /// <summary>
    /// 私有方法：驗證請求
    /// </summary>
    private bool ValidateRequest(PaymentRequest request)
    {
        return request is { Amount: > 0 };
    }

    /// <summary>
    /// 私有方法：計算手續費
    /// 這是複雜的業務邏輯，我們想要直接測試它
    /// </summary>
    private decimal CalculateFee(decimal amount, PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.CreditCard => amount * 0.03m,
            PaymentMethod.DebitCard => Math.Max(10, amount * 0.01m),
            PaymentMethod.BankTransfer => Math.Max(10, amount * 0.005m),
            _ => 0
        };
    }

    /// <summary>
    /// 靜態私有方法：檢查是否為工作日
    /// </summary>
    private static bool IsBusinessDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && 
               date.DayOfWeek != DayOfWeek.Sunday;
    }
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    BankTransfer
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public decimal TotalAmount { get; set; }
    public string ErrorMessage { get; set; }

    public static PaymentResult Success(decimal total) => 
        new() { Success = true, TotalAmount = total };
    
    public static PaymentResult InvalidRequest() => 
        new() { Success = false, ErrorMessage = "無效的請求" };
}


// ========================================
// 測試範例
// ========================================

namespace MyProject.Tests;

/// <summary>
/// PaymentProcessor 測試類別
/// </summary>
public class PaymentProcessorReflectionTests
{
    // ========================================
    // 方法一：直接使用反射測試私有實例方法
    // ========================================
    
    [Theory]
    [InlineData(1000, PaymentMethod.CreditCard, 30)]    // 1000 * 0.03 = 30
    [InlineData(1000, PaymentMethod.DebitCard, 10)]     // 1000 * 0.01 = 10（最低10）
    [InlineData(1000, PaymentMethod.BankTransfer, 10)]  // 1000 * 0.005 = 5（最低10）
    [InlineData(100, PaymentMethod.BankTransfer, 10)]   // 100 * 0.005 = 0.5（最低10）
    [InlineData(5000, PaymentMethod.BankTransfer, 25)]  // 5000 * 0.005 = 25
    public void CalculateFee_直接使用反射_應計算正確手續費(
        decimal amount, PaymentMethod method, decimal expected)
    {
        // Arrange
        var processor = new PaymentProcessor();
        var methodInfo = typeof(PaymentProcessor).GetMethod(
            "CalculateFee",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        // Act
        var actual = (decimal)methodInfo.Invoke(processor, new object[] { amount, method });

        // Assert
        actual.Should().Be(expected);
    }

    // ========================================
    // 方法二：測試靜態私有方法
    // ========================================
    
    [Theory]
    [InlineData("2024-03-15", true)]  // 星期五
    [InlineData("2024-03-16", false)] // 星期六
    [InlineData("2024-03-17", false)] // 星期日
    [InlineData("2024-03-18", true)]  // 星期一
    public void IsBusinessDay_直接使用反射_應正確判斷工作日(string dateString, bool expected)
    {
        // Arrange
        var date = DateTime.Parse(dateString);
        var methodInfo = typeof(PaymentProcessor).GetMethod(
            "IsBusinessDay",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act
        var actual = (bool)methodInfo.Invoke(null, new object[] { date });

        // Assert
        actual.Should().Be(expected);
    }
}


// ========================================
// 反射測試輔助類別
// ========================================

namespace MyProject.Tests.Helpers;

/// <summary>
/// 反射測試輔助類別
/// 封裝常用的反射操作，簡化測試程式碼
/// </summary>
public static class ReflectionTestHelper
{
    /// <summary>
    /// 呼叫私有實例方法
    /// </summary>
    /// <param name="instance">物件實例</param>
    /// <param name="methodName">方法名稱</param>
    /// <param name="parameters">方法參數</param>
    /// <returns>方法回傳值</returns>
    public static object InvokePrivateMethod(
        object instance,
        string methodName,
        params object[] parameters)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var methodInfo = instance.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"找不到私有方法: {methodName}");

        return methodInfo.Invoke(instance, parameters);
    }

    /// <summary>
    /// 呼叫私有實例方法（泛型版本）
    /// </summary>
    public static T InvokePrivateMethod<T>(
        object instance,
        string methodName,
        params object[] parameters)
    {
        var result = InvokePrivateMethod(instance, methodName, parameters);
        return (T)result;
    }

    /// <summary>
    /// 呼叫靜態私有方法
    /// </summary>
    /// <param name="type">類別型別</param>
    /// <param name="methodName">方法名稱</param>
    /// <param name="parameters">方法參數</param>
    /// <returns>方法回傳值</returns>
    public static object InvokePrivateStaticMethod(
        Type type,
        string methodName,
        params object[] parameters)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var methodInfo = type.GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"找不到靜態私有方法: {methodName}");

        return methodInfo.Invoke(null, parameters);
    }

    /// <summary>
    /// 呼叫靜態私有方法（泛型版本）
    /// </summary>
    public static T InvokePrivateStaticMethod<T>(
        Type type,
        string methodName,
        params object[] parameters)
    {
        var result = InvokePrivateStaticMethod(type, methodName, parameters);
        return (T)result;
    }

    /// <summary>
    /// 取得私有欄位的值
    /// </summary>
    public static object GetPrivateField(object instance, string fieldName)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var fieldInfo = instance.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (fieldInfo == null)
            throw new InvalidOperationException($"找不到私有欄位: {fieldName}");

        return fieldInfo.GetValue(instance);
    }

    /// <summary>
    /// 設定私有欄位的值
    /// </summary>
    public static void SetPrivateField(object instance, string fieldName, object value)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var fieldInfo = instance.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (fieldInfo == null)
            throw new InvalidOperationException($"找不到私有欄位: {fieldName}");

        fieldInfo.SetValue(instance, value);
    }
}


// ========================================
// 使用輔助類別的測試範例
// ========================================

/// <summary>
/// 使用 ReflectionTestHelper 的測試範例
/// </summary>
public class PaymentProcessorWithHelperTests
{
    [Fact]
    public void CalculateFee_使用輔助方法_應計算正確手續費()
    {
        // Arrange
        var processor = new PaymentProcessor();

        // Act
        var actual = ReflectionTestHelper.InvokePrivateMethod<decimal>(
            processor,
            "CalculateFee",
            1000m,
            PaymentMethod.CreditCard
        );

        // Assert
        actual.Should().Be(30m);
    }

    [Fact]
    public void IsBusinessDay_使用輔助方法_應正確判斷工作日()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15); // 星期五

        // Act
        var actual = ReflectionTestHelper.InvokePrivateStaticMethod<bool>(
            typeof(PaymentProcessor),
            "IsBusinessDay",
            date
        );

        // Assert
        actual.Should().BeTrue();
    }
}


// ========================================
// 反射測試的注意事項與最佳實踐
// ========================================

/*
注意事項：

1. 效能考量
   - 反射比直接呼叫慢約 10-100 倍
   - 不適合在效能測試中使用
   - 大量測試時考慮快取 MethodInfo

2. 型別安全
   - 方法名稱是字串，編譯時不會檢查
   - 重構時容易遺漏更新測試
   - 考慮使用常數儲存方法名稱

3. 可維護性
   - 測試脆弱，方法改名會失敗
   - 增加重構的阻力
   - 定期檢視是否可以改善設計

最佳實踐：

1. 使用輔助方法
   - 封裝反射邏輯
   - 提供泛型版本
   - 統一錯誤處理

2. 清楚的測試命名
   - 在測試名稱中標示使用反射
   - 例如：TestMethod_使用反射_預期結果

3. 使用常數
   private const string CalculateFeeMethod = "CalculateFee";
   
   [Fact]
   public void Test()
   {
       ReflectionTestHelper.InvokePrivateMethod(
           instance, 
           CalculateFeeMethod, 
           parameters
       );
   }

4. 定期檢視
   - 每次 Sprint 檢視反射測試
   - 評估是否可以重構為更好的設計
   - 考慮是否值得繼續維護

何時應該避免反射測試：

❌ 簡單的私有方法（< 10 行）
❌ 容易透過公開方法測試的邏輯
❌ 頻繁變動的實作細節
❌ 純粹的委派呼叫

何時考慮使用反射測試：

✅ 複雜的演算法驗證（> 10 行）
✅ 安全相關的邏輯
✅ 遺留系統重構前的保護網
✅ 短期內無法重構的複雜邏輯

記住：反射測試應該是最後的手段，優先考慮改善設計。
*/
