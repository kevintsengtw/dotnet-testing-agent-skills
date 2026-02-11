# 私有方法測試技術

> 本文件從 SKILL.md 提取，提供私有方法測試的完整技術細節與程式碼範例。

## 決策樹：是否應該測試私有方法

```text
開始
  ↓
是否可以重構為獨立類別？
  ├─ 是 → 重構並測試新類別 ✅
  └─ 否 ↓
      私有方法是否超過 10 行？
        ├─ 否 → 透過公開方法測試 ✅
        └─ 是 ↓
            是否包含複雜演算法/安全邏輯？
              ├─ 否 → 重新考慮設計 ⚠️
              └─ 是 → 考慮使用反射測試 ⚠️
```

## 何時考慮測試私有方法

**必要條件（需同時滿足）：**

1. **複雜度高**：超過 10 行的複雜邏輯
2. **業務關鍵**：包含重要業務規則或演算法
3. **難以間接測試**：無法透過公開方法完整驗證
4. **重構成本高**：短期內無法重構為獨立類別

**典型情境：**

- 複雜的數學運算或演算法
- 加密、解密等安全相關邏輯
- 效能關鍵的內部實作
- 遺留系統重構前的保護網

## 使用反射測試私有方法

當確定需要測試私有方法時，可使用反射技術：

### 測試私有實例方法

```csharp
[Theory]
[InlineData(1000, PaymentMethod.CreditCard, 30)]
[InlineData(1000, PaymentMethod.DebitCard, 10)]
public void TestPrivateInstanceMethod_使用反射(
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
```

### 測試靜態私有方法

```csharp
[Theory]
[InlineData("2024-03-15", true)]  // 星期五
[InlineData("2024-03-16", false)] // 星期六
public void TestPrivateStaticMethod_使用反射(string dateString, bool expected)
{
    // Arrange
    var date = DateTime.Parse(dateString);
    var methodInfo = typeof(DateHelper).GetMethod(
        "IsBusinessDay",
        BindingFlags.NonPublic | BindingFlags.Static
    );

    // Act
    var actual = (bool)methodInfo.Invoke(null, new object[] { date });

    // Assert
    actual.Should().Be(expected);
}
```

### 反射測試輔助類別

建立輔助方法簡化反射操作：

```csharp
public static class ReflectionTestHelper
{
    /// <summary>
    /// 呼叫私有實例方法
    /// </summary>
    public static object InvokePrivateMethod(
        object instance, 
        string methodName, 
        params object[] parameters)
    {
        var methodInfo = instance.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"找不到私有方法: {methodName}");

        return methodInfo.Invoke(instance, parameters);
    }

    /// <summary>
    /// 呼叫靜態私有方法
    /// </summary>
    public static object InvokePrivateStaticMethod(
        Type type,
        string methodName,
        params object[] parameters)
    {
        var methodInfo = type.GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"找不到靜態私有方法: {methodName}");

        return methodInfo.Invoke(null, parameters);
    }
}

// 使用範例
[Fact]
public void TestWithHelper_更簡潔的反射測試()
{
    // Arrange
    var processor = new PaymentProcessor();

    // Act
    var actual = (decimal)ReflectionTestHelper.InvokePrivateMethod(
        processor, 
        "CalculateFee", 
        1000m, 
        PaymentMethod.CreditCard
    );

    // Assert
    actual.Should().Be(30m);
}
```

## 反射測試的注意事項

**風險：**

- ⚠️ 測試脆弱：方法名稱改變會導致測試失敗
- ⚠️ 重構阻力：增加重構的難度
- ⚠️ 維護成本：需要額外維護反射代碼
- ⚠️ 效能影響：反射比直接呼叫慢

**最佳實踐：**

- 使用輔助方法封裝反射邏輯
- 在測試名稱中明確標示使用反射
- 定期檢視是否可以重構為更好的設計
- 考慮使用常數儲存方法名稱
