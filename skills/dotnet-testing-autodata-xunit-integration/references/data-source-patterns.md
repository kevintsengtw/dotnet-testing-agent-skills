# 資料來源設計模式

> 本文件從 [SKILL.md](../SKILL.md) 提取，提供階層式資料組織與可重用資料集的設計模式。

## 階層式資料組織

```csharp
namespace AutoData.Tests.DataSources;

/// <summary>
/// 測試資料來源基底類別
/// </summary>
public abstract class BaseTestData
{
    protected static string GetTestDataPath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
    }
}

/// <summary>
/// 產品測試資料來源
/// </summary>
public class ProductTestDataSource : BaseTestData
{
    public static IEnumerable<object[]> BasicProducts()
    {
        yield return new object[] { "iPhone", 35900m, true };
        yield return new object[] { "MacBook", 89900m, true };
        yield return new object[] { "AirPods", 7490m, false };
    }

    public static IEnumerable<object[]> ElectronicsProducts()
    {
        // 從 CSV 檔案讀取
        var csvPath = GetTestDataPath("electronics.csv");
        // ... 讀取邏輯
    }
}

/// <summary>
/// 客戶測試資料來源
/// </summary>
public class CustomerTestDataSource : BaseTestData
{
    public static IEnumerable<object[]> VipCustomers()
    {
        yield return new object[] { "張三", "VIP", 100000m };
        yield return new object[] { "李四", "VIP", 150000m };
    }
}
```

## 可重用資料集

```csharp
/// <summary>
/// 可重用的測試資料集
/// </summary>
public static class ReusableTestDataSets
{
    public static class ProductCategories
    {
        public static IEnumerable<object[]> All()
        {
            yield return new object[] { "3C產品", "TECH" };
            yield return new object[] { "服飾配件", "FASHION" };
            yield return new object[] { "居家生活", "HOME" };
        }

        public static IEnumerable<object[]> Electronics()
        {
            yield return new object[] { "手機", "MOBILE" };
            yield return new object[] { "筆電", "LAPTOP" };
        }
    }

    public static class CustomerTypes
    {
        public static IEnumerable<object[]> All()
        {
            yield return new object[] { "VIP", 100000m, 0.15m };
            yield return new object[] { "Premium", 50000m, 0.10m };
            yield return new object[] { "Regular", 20000m, 0.05m };
        }
    }
}
```
