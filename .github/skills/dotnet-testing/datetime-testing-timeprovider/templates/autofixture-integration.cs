// =============================================================================
// AutoFixture 與 TimeProvider 整合範例
// 展示如何結合 AutoFixture 進行自動化時間測試
// =============================================================================

using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace TimeProviderExamples.Tests;

#region FakeTimeProvider Customization

/// <summary>
/// FakeTimeProvider 的 AutoFixture Customization
/// 讓 AutoFixture 知道如何建立 FakeTimeProvider
/// </summary>
public class FakeTimeProviderCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // 註冊 FakeTimeProvider 的建立方式
        fixture.Register(() => new FakeTimeProvider());
    }
}

#endregion

#region AutoDataWithCustomization 屬性

/// <summary>
/// 整合 NSubstitute 和 FakeTimeProvider 的自訂 AutoData 屬性
/// </summary>
public class AutoDataWithCustomizationAttribute : AutoDataAttribute
{
    public AutoDataWithCustomizationAttribute() : base(CreateFixture)
    {
    }
    
    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new FakeTimeProviderCustomization());
    }
}

/// <summary>
/// 結合 InlineData 和 AutoFixture 的屬性
/// 用於參數化測試與自動產生物件的組合
/// </summary>
public class InlineAutoDataWithCustomizationAttribute : InlineAutoDataAttribute
{
    public InlineAutoDataWithCustomizationAttribute(params object[] values)
        : base(new AutoDataWithCustomizationAttribute(), values)
    {
    }
}

#endregion

#region 傳統寫法 vs AutoFixture 寫法對比

/// <summary>
/// 傳統測試寫法 - 手動建立所有物件
/// </summary>
public class OrderServiceTraditionalTests
{
    [Fact]
    public void CanPlaceOrder_在營業時間內_傳統寫法()
    {
        // Arrange - 需要手動建立所有物件
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0));
        
        var orderService = new OrderService(fakeTimeProvider);
        
        // Act
        var result = orderService.CanPlaceOrder();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void GetTimeBasedDiscount_週五_傳統寫法()
    {
        // Arrange - 每次都要重複這些設定
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0)); // 週五
        
        var orderService = new OrderService(fakeTimeProvider);
        
        // Act
        var discount = orderService.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be("週五快樂：九折優惠");
    }
}

/// <summary>
/// AutoFixture 測試寫法 - 自動化物件建立
/// </summary>
public class OrderServiceAutoFixtureTests
{
    /// <summary>
    /// 使用 AutoFixture 簡化測試
    /// [Frozen(Matching.DirectBaseType)] 是關鍵！
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public void GetTimeBasedDiscount_週一_應回傳無優惠(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut) // sut = System Under Test，由 AutoFixture 自動建立
    {
        // Arrange - 只需要設定測試相關的時間
        var mondayTime = new DateTime(2024, 3, 11, 14, 0, 0); // 2024/3/11 是週一
        fakeTimeProvider.SetLocalNow(mondayTime);
        
        // Act
        var discount = sut.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be("無優惠");
    }
    
    [Theory]
    [AutoDataWithCustomization]
    public void GetTimeBasedDiscount_週五_應回傳九折優惠(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange - 設定為週五
        var fridayTime = new DateTime(2024, 3, 15, 14, 0, 0); // 2024/3/15 是週五
        fakeTimeProvider.SetLocalNow(fridayTime);
        
        // Act
        var discount = sut.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be("週五快樂：九折優惠");
    }
    
    [Theory]
    [AutoDataWithCustomization]
    public void GetTimeBasedDiscount_聖誕節_應回傳八折優惠(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 25, 10, 0, 0));
        
        // Act
        var discount = sut.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be("聖誕特惠：八折優惠");
    }
    
    [Theory]
    [AutoDataWithCustomization]
    public void CanPlaceOrder_營業時間邊界測試_上午9點(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange - 上午9點整（營業時間開始）
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 9, 0, 0));
        
        // Act & Assert
        sut.CanPlaceOrder().Should().BeTrue();
    }
    
    [Theory]
    [AutoDataWithCustomization]
    public void CanPlaceOrder_營業時間邊界測試_下午5點(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange - 下午5點整（營業時間結束）
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 17, 0, 0));
        
        // Act & Assert
        sut.CanPlaceOrder().Should().BeFalse();
    }
}

#endregion

#region Matching.DirectBaseType 詳細說明

/*
 * ============================================================
 * Matching.DirectBaseType 的重要性
 * ============================================================
 * 
 * 問題：
 * OrderService 的建構式需要 TimeProvider（抽象類別）
 * 但我們想在測試中使用 FakeTimeProvider（衍生類別）
 * 
 * 如果不使用 Matching.DirectBaseType：
 * 
 *     [Theory]
 *     [AutoDataWithCustomization]
 *     public void Test([Frozen] FakeTimeProvider provider, OrderService sut)
 *     {
 *         // ❌ 失敗！AutoFixture 只知道 FakeTimeProvider
 *         // 當建立 OrderService 時，它會另外建立一個新的 TimeProvider
 *         // 導致 provider 和 sut 使用不同的時間來源
 *     }
 * 
 * 使用 Matching.DirectBaseType 的解決方案：
 * 
 *     [Theory]
 *     [AutoDataWithCustomization]
 *     public void Test([Frozen(Matching.DirectBaseType)] FakeTimeProvider provider, OrderService sut)
 *     {
 *         // ✅ 成功！AutoFixture 會將 FakeTimeProvider 也註冊為 TimeProvider
 *         // 當建立 OrderService 時，會使用同一個 FakeTimeProvider 實例
 *     }
 * 
 * 運作流程：
 * 1. AutoFixture 看到需要建立 OrderService
 * 2. 發現 OrderService 的建構式需要 TimeProvider 參數
 * 3. 檢查是否有被 [Frozen] 標記的實例可以滿足這個需求
 * 4. 找到 [Frozen(Matching.DirectBaseType)] FakeTimeProvider
 * 5. 確認 TimeProvider 是 FakeTimeProvider 的直接基底類型
 * 6. 將 FakeTimeProvider 實例注入到 OrderService 的建構式中
 */

#endregion

#region 快取測試與 AutoFixture 整合

/// <summary>
/// 結合 AutoFixture 的快取測試
/// AutoFixture 自動產生測試資料（key, value）
/// </summary>
public class TimedCacheAutoFixtureTests
{
    [Theory]
    [AutoDataWithCustomization]
    public void TimedCache_使用AutoFixture測試過期機制_應正確處理(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        string key,    // AutoFixture 自動產生
        string value)  // AutoFixture 自動產生
    {
        // Arrange
        var startTime = new DateTime(2024, 3, 15, 10, 0, 0);
        fakeTimeProvider.SetLocalNow(startTime);
        
        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(30));
        
        // Act & Assert - 設定和立即取得
        cache.Set(key, value);
        cache.Get(key).Should().Be(value);
        
        // Act & Assert - 快轉時間後應過期
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(31));
        cache.Get(key).Should().BeNull();
    }
    
    [Theory]
    [AutoDataWithCustomization]
    public void TimedCache_多個項目測試(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        string key1, string value1,
        string key2, string value2,
        string key3, string value3)
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 10, 0, 0));
        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(10));
        
        // Act - 設定多個項目
        cache.Set(key1, value1);
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
        cache.Set(key2, value2);
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
        cache.Set(key3, value3);
        
        // Assert - 此時 key1 已經過了 6 分鐘，key2 過了 3 分鐘，key3 剛設定
        // 再過 5 分鐘（共 11 分鐘）
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));
        
        cache.Get(key1).Should().BeNull();   // 11 分鐘 > 10 分鐘，已過期
        cache.Get(key2).Should().Be(value2); // 8 分鐘 < 10 分鐘，仍有效
        cache.Get(key3).Should().Be(value3); // 5 分鐘 < 10 分鐘，仍有效
    }
}

#endregion

#region InlineAutoData 結合參數化測試

/// <summary>
/// 使用 InlineAutoDataWithCustomization 結合參數化測試
/// 部分參數由 InlineData 提供，其餘由 AutoFixture 產生
/// </summary>
public class OrderServiceInlineAutoDataTests
{
    [Theory]
    [InlineAutoDataWithCustomization(8, false)]   // 上午 8 點 - 營業時間前
    [InlineAutoDataWithCustomization(9, true)]    // 上午 9 點 - 剛開始營業
    [InlineAutoDataWithCustomization(12, true)]   // 中午 12 點 - 營業時間內
    [InlineAutoDataWithCustomization(16, true)]   // 下午 4 點 - 營業時間內
    [InlineAutoDataWithCustomization(17, false)]  // 下午 5 點 - 剛結束營業
    [InlineAutoDataWithCustomization(18, false)]  // 下午 6 點 - 營業時間後
    public void CanPlaceOrder_不同時間點_AutoFixture版本(
        int hour,      // 由 InlineData 提供
        bool expected, // 由 InlineData 提供
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut) // 由 AutoFixture 自動建立
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, hour, 0, 0));
        
        // Act
        var result = sut.CanPlaceOrder();
        
        // Assert
        result.Should().Be(expected);
    }
}

#endregion

#region 排程服務的 AutoFixture 測試

/// <summary>
/// 排程服務使用 AutoFixture 測試
/// </summary>
public class ScheduleServiceAutoFixtureTests
{
    [Theory]
    [AutoDataWithCustomization]
    public void ShouldExecuteJob_已到執行時間_應回傳True(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ScheduleService sut)
    {
        // Arrange
        var now = new DateTime(2024, 3, 15, 14, 30, 0);
        fakeTimeProvider.SetLocalNow(now);
        
        var schedule = new JobSchedule
        {
            NextExecutionTime = now.AddMinutes(-30), // 30 分鐘前應該執行
            CronExpression = "0 0 * * *"
        };
        
        // Act
        var result = sut.ShouldExecuteJob(schedule);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Theory]
    [AutoDataWithCustomization]
    public void ShouldExecuteJob_尚未到執行時間_應回傳False(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ScheduleService sut)
    {
        // Arrange
        var now = new DateTime(2024, 3, 15, 14, 30, 0);
        fakeTimeProvider.SetLocalNow(now);
        
        var schedule = new JobSchedule
        {
            NextExecutionTime = now.AddMinutes(30), // 30 分鐘後才執行
            CronExpression = "0 0 * * *"
        };
        
        // Act
        var result = sut.ShouldExecuteJob(schedule);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Theory]
    [InlineAutoDataWithCustomization("2024-03-15 14:30:00", "2024-03-15 14:00:00", true)]
    [InlineAutoDataWithCustomization("2024-03-15 13:30:00", "2024-03-15 14:00:00", false)]
    [InlineAutoDataWithCustomization("2024-03-15 14:00:00", "2024-03-15 14:00:00", true)]
    public void ShouldExecuteJob_參數化測試(
        string currentTimeStr,
        string scheduledTimeStr,
        bool expected,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ScheduleService sut)
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(DateTime.Parse(currentTimeStr));
        
        var schedule = new JobSchedule
        {
            NextExecutionTime = DateTime.Parse(scheduledTimeStr)
        };
        
        // Act
        var result = sut.ShouldExecuteJob(schedule);
        
        // Assert
        result.Should().Be(expected);
    }
}

#endregion

#region AutoFixture 的優勢總結

/*
 * ============================================================
 * AutoFixture 與 TimeProvider 整合的優勢
 * ============================================================
 * 
 * 1. 減少樣板程式碼
 *    - 不需要手動 new FakeTimeProvider()
 *    - 不需要手動 new OrderService(fakeTimeProvider)
 *    - AutoFixture 自動處理依賴注入
 * 
 * 2. 提高測試覆蓋率
 *    - 可以輕鬆產生多種測試案例
 *    - AutoFixture 自動產生測試資料（如 key, value）
 * 
 * 3. 保持測試獨立性
 *    - 每個測試都有獨立的 FakeTimeProvider 實例
 *    - 不會互相干擾
 * 
 * 4. 增強可讀性
 *    - 測試重點更聚焦在業務邏輯驗證
 *    - 而非物件建立細節
 * 
 * 5. 提升維護性
 *    - 當建構式參數變更時，AutoFixture 會自動適應
 *    - 減少測試程式碼的修改範圍
 * 
 * ============================================================
 * 何時使用 AutoFixture？
 * ============================================================
 * 
 * 建議使用的情況：
 * - 測試類別有多個相似的測試方法
 * - 被測試的類別有複雜的建構式參數
 * - 需要大量不同的測試資料組合
 * - 希望減少測試程式碼的重複性
 * 
 * 可以考慮傳統寫法的情況：
 * - 測試案例很簡單，只有少數幾個
 * - 需要對物件建立過程有完全的控制
 * - 團隊對 AutoFixture 不熟悉，學習成本考量
 */

#endregion
