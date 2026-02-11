# 測試生命週期管理

> 本文件從 [SKILL.md](../SKILL.md) 提煉，提供 TUnit 測試生命週期的完整範例與細節。

## 生命週期方法概述

| 生命週期方法      | 執行時機                 | 適用場景                         |
| :---------------- | :----------------------- | :------------------------------- |
| `[Before(Class)]` | 類別中第一個測試開始前   | 昂貴的資源初始化（如資料庫連線） |
| `建構式`          | 每個測試開始前           | 測試實例的基本設定               |
| `[Before(Test)]`  | 每個測試方法執行前       | 測試特定的前置作業               |
| `測試方法`        | 實際測試執行             | 測試邏輯本身                     |
| `[After(Test)]`   | 每個測試方法執行後       | 測試特定的清理作業               |
| `Dispose`         | 測試實例銷毀時           | 釋放測試實例的資源               |
| `[After(Class)]`  | 類別中最後一個測試完成後 | 清理共享資源                     |

## Before/After 屬性家族

```csharp
// Before 屬性
[Before(Test)]           // 實例方法 - 每個測試前執行
[Before(Class)]          // 靜態方法 - 類別第一個測試前執行一次
[Before(Assembly)]       // 靜態方法 - 組件第一個測試前執行一次
[Before(TestSession)]    // 靜態方法 - 測試會話開始前執行一次

// After 屬性
[After(Test)]           // 實例方法 - 每個測試後執行
[After(Class)]          // 靜態方法 - 類別最後一個測試後執行一次
[After(Assembly)]       // 靜態方法 - 組件最後一個測試後執行一次
[After(TestSession)]    // 靜態方法 - 測試會話結束後執行一次

// 全域鉤子
[BeforeEvery(Test)]     // 靜態方法 - 每個測試前都執行（全域）
[AfterEvery(Test)]      // 靜態方法 - 每個測試後都執行（全域）
```

## 實際範例

```csharp
public class LifecycleTests
{
    private readonly StringBuilder _logBuilder;
    private static readonly List<string> ClassLog = [];

    public LifecycleTests()
    {
        Console.WriteLine("1. 建構式執行 - 測試實例建立");
        _logBuilder = new StringBuilder();
    }

    [Before(Class)]
    public static async Task BeforeClass()
    {
        Console.WriteLine("2. BeforeClass 執行 - 類別層級初始化");
        ClassLog.Add("BeforeClass 執行");
        await Task.Delay(10);
    }

    [Before(Test)]
    public async Task BeforeTest()
    {
        Console.WriteLine("3. BeforeTest 執行 - 測試前置設定");
        _logBuilder.AppendLine("BeforeTest 執行");
        await Task.Delay(5);
    }

    [Test]
    public async Task TestMethod_應按正確順序執行生命週期方法()
    {
        Console.WriteLine("4. TestMethod 執行");
        await Assert.That(ClassLog).Contains("BeforeClass 執行");
    }

    [After(Test)]
    public async Task AfterTest()
    {
        Console.WriteLine("5. AfterTest 執行 - 測試後清理");
        await Task.Delay(5);
    }

    [After(Class)]
    public static async Task AfterClass()
    {
        Console.WriteLine("6. AfterClass 執行 - 類別層級清理");
        await Task.Delay(10);
    }
}
```

**重要觀察：**

1. **建構式優先級**：永遠在所有 TUnit 生命週期屬性之前執行
2. **BeforeClass 只執行一次**：在所有測試開始前執行一次
3. **測試執行是並行的**：多個測試方法可能同時執行
4. **AfterClass 只執行一次**：在所有測試完成後執行一次
