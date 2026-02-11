# 執行控制與測試品質

> 本文件從 [SKILL.md](../SKILL.md) 提煉，提供 Retry、Timeout、DisplayName 的完整範例與細節。

## Retry 機制：智慧重試策略

```csharp
[Test]
[Retry(3)] // 如果失敗，重試最多 3 次
[Property("Category", "Flaky")]
public async Task NetworkCall_可能不穩定_使用重試機制()
{
    var random = new Random();
    var success = random.Next(1, 4) == 1; // 約 33% 的成功率

    if (!success)
    {
        throw new HttpRequestException("模擬網路錯誤");
    }

    await Assert.That(success).IsTrue();
}
```

**適合使用 Retry 的情況：**

1. 外部服務呼叫：API 請求、資料庫連線可能因網路問題暫時失敗
2. 檔案系統操作：在 CI/CD 環境中，檔案鎖定可能導致暫時性失敗
3. 並行測試競爭：多個測試同時存取共享資源時的競爭條件

**不適合使用 Retry 的情況：**

1. 邏輯錯誤：程式碼本身的錯誤重試多少次都不會成功
2. 預期的例外：測試本身就是要驗證例外情況
3. 效能測試：重試會影響效能測量的準確性

## Timeout 控制：長時間測試管理

```csharp
[Test]
[Timeout(5000)] // 5 秒超時
[Property("Category", "Performance")]
public async Task LongRunningOperation_應在時限內完成()
{
    await Task.Delay(1000); // 1 秒操作，應該在 5 秒限制內
    await Assert.That(true).IsTrue();
}

[Test]
[Timeout(1000)] // 確保不會超過 1 秒
[Property("Category", "Performance")]
[Property("Baseline", "true")]
public async Task SearchFunction_效能基準_應符合SLA要求()
{
    var stopwatch = Stopwatch.StartNew();
    
    var searchResults = await PerformSearch("test query");
    
    stopwatch.Stop();
    
    await Assert.That(searchResults).IsNotNull();
    await Assert.That(searchResults.Count()).IsGreaterThan(0);
    await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(500);
}
```

## DisplayName：自訂測試名稱

```csharp
[Test]
[DisplayName("自訂測試名稱：驗證使用者註冊流程")]
public async Task UserRegistration_CustomDisplayName_測試名稱更易讀()
{
    await Assert.That("user@example.com").Contains("@");
}

// 參數化測試的動態顯示名稱
[Test]
[Arguments("valid@email.com", true)]
[Arguments("invalid-email", false)]
[Arguments("", false)]
[DisplayName("電子郵件驗證：{0} 應為 {1}")]
public async Task EmailValidation_參數化顯示名稱(string email, bool expectedValid)
{
    var isValid = !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
    await Assert.That(isValid).IsEqualTo(expectedValid);
}

// 業務場景驅動的顯示名稱
[Test]
[Arguments(CustomerLevel.一般會員, 1000, 0)]
[Arguments(CustomerLevel.VIP會員, 1000, 50)]
[Arguments(CustomerLevel.白金會員, 1000, 100)]
[DisplayName("會員等級 {0} 購買 ${1} 應獲得 ${2} 折扣")]
public async Task MemberDiscount_根據會員等級_計算正確折扣(
    CustomerLevel level, decimal amount, decimal expectedDiscount)
{
    var calculator = new DiscountCalculator();
    var discount = await calculator.CalculateDiscountAsync(amount, level);
    await Assert.That(discount).IsEqualTo(expectedDiscount);
}
```
