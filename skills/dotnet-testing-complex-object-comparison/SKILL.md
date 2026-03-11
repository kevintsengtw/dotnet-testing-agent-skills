---
name: dotnet-testing-complex-object-comparison
description: |
  處理複雜物件比對與深層驗證的專門技能。當需要比對深層物件、排除特定屬性、處理循環參照、驗證 DTO/Entity 時使用。涵蓋 BeEquivalentTo、Excluding、Including、自訂比對規則等。
  Make sure to use this skill whenever the user mentions deep object comparison, BeEquivalentTo, DTO comparison, Excluding properties, or complex object validation in tests, even if they don't explicitly ask for comparison guidance.
  Keywords: object comparison, 物件比對, deep comparison, 深層比對, BeEquivalentTo, DTO 比對, Entity 驗證, 排除屬性, 循環參照, Excluding, Including, ExcludingNestedObjects, RespectingRuntimeTypes, WithStrictOrdering, 忽略時間戳記, exclude timestamp
---

# 複雜物件比對指南（Complex Object Comparison）

## 核心使用場景

### 1. 深層物件結構比對 (Object Graph Comparison)

當需要比對包含多層巢狀屬性的複雜物件時：

```csharp
[Fact]
public void ComplexObject_深層結構比對_應完全相符()
{
    var expected = new Order
    {
        Id = 1,
        Customer = new Customer
        {
            Name = "John Doe",
            Address = new Address
            {
                Street = "123 Main St",
                City = "Seattle",
                ZipCode = "98101"
            }
        },
        Items = new[]
        {
            new OrderItem { ProductName = "Laptop", Quantity = 1, Price = 999.99m },
            new OrderItem { ProductName = "Mouse", Quantity = 2, Price = 29.99m }
        }
    };

    var actual = orderService.GetOrder(1);

    // 深層物件比對
    actual.Should().BeEquivalentTo(expected);
}
```

### 2. 循環參照處理 (Circular Reference Handling)

處理物件之間存在循環參照的情況：

```csharp
[Fact]
public void TreeStructure_循環參照_應正確處理()
{
    // 建立具有父子雙向參照的樹狀結構
    var parent = new TreeNode { Value = "Root" };
    var child1 = new TreeNode { Value = "Child1", Parent = parent };
    var child2 = new TreeNode { Value = "Child2", Parent = parent };
    parent.Children = new[] { child1, child2 };

    var actualTree = treeService.GetTree("Root");

    // 處理循環參照
    actualTree.Should().BeEquivalentTo(parent, options =>
        options.IgnoringCyclicReferences()
               .WithMaxRecursionDepth(10)
    );
}
```

### 3-6. 進階比對模式

AwesomeAssertions 還提供多種進階比對模式：動態欄位排除（排除時間戳記、自動生成欄位）、巢狀物件欄位排除、大量資料效能最佳化比對（選擇性屬性比對、抽樣驗證策略）、以及嚴格/寬鬆排序控制。

> 完整程式碼範例請參閱 [references/detailed-comparison-patterns.md](references/detailed-comparison-patterns.md)

## 比對選項速查表

| 選項方法                     | 用途           | 適用場景                   |
| ---------------------------- | -------------- | -------------------------- |
| `Excluding(x => x.Property)` | 排除特定屬性   | 排除時間戳記、自動生成欄位 |
| `Including(x => x.Property)` | 只包含特定屬性 | 關鍵屬性驗證               |
| `IgnoringCyclicReferences()` | 忽略循環參照   | 樹狀結構、雙向關聯         |
| `WithMaxRecursionDepth(n)`   | 限制遞迴深度   | 深層巢狀結構               |
| `WithStrictOrdering()`       | 嚴格順序比對   | 陣列/集合順序重要時        |
| `WithoutStrictOrdering()`    | 寬鬆順序比對   | 陣列/集合順序不重要時      |
| `WithTracing()`              | 啟用追蹤       | 除錯複雜比對失敗           |

## 常見比對模式與解決方案

### 模式 1：Entity Framework 實體比對

```csharp
[Fact]
public void EFEntity_資料庫實體_應排除導航屬性()
{
    var expected = new Product { Id = 1, Name = "Laptop", Price = 999 };
    var actual = dbContext.Products.Find(1);

    actual.Should().BeEquivalentTo(expected, options =>
        options.ExcludingMissingMembers()  // 排除 EF 追蹤屬性
               .Excluding(p => p.CreatedAt)
               .Excluding(p => p.UpdatedAt)
    );
}
```

### 模式 2：API Response 比對

```csharp
[Fact]
public void ApiResponse_JSON反序列化_應忽略額外欄位()
{
    var expected = new UserDto 
    { 
        Id = 1, 
        Username = "john_doe" 
    };

    var response = await httpClient.GetAsync("/api/users/1");
    var actual = await response.Content.ReadFromJsonAsync<UserDto>();

    actual.Should().BeEquivalentTo(expected, options =>
        options.ExcludingMissingMembers()  // 忽略 API 額外欄位
    );
}
```

### 模式 3：測試資料建構器比對

```csharp
[Fact]
public void Builder_測試資料_應匹配預期結構()
{
    var expected = new OrderBuilder()
        .WithId(1)
        .WithCustomer("John Doe")
        .WithItems(3)
        .Build();

    var actual = orderService.CreateOrder(orderRequest);

    actual.Should().BeEquivalentTo(expected, options =>
        options.Excluding(o => o.OrderNumber)  // 系統生成
               .Excluding(o => o.CreatedAt)
    );
}
```

## 錯誤訊息最佳化

### 提供有意義的錯誤訊息

```csharp
[Fact]
public void Comparison_錯誤訊息_應清楚說明差異()
{
    var expected = new User { Name = "John", Age = 30 };
    var actual = userService.GetUser(1);

    // 使用 because 參數提供上下文
    actual.Should().BeEquivalentTo(expected, options =>
        options.Excluding(u => u.Id)
               .Because("ID 是系統自動生成的，不應納入比對")
    );
}
```

### 使用 AssertionScope 進行批次驗證

```csharp
[Fact]
public void MultipleComparisons_批次驗證_應一次顯示所有失敗()
{
    var users = userService.GetAllUsers();

    using (new AssertionScope())
    {
        foreach (var user in users)
        {
            user.Id.Should().BeGreaterThan(0);
            user.Name.Should().NotBeNullOrEmpty();
            user.Email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        }
    }
    // 所有失敗會一起報告，而非遇到第一個失敗就停止
}
```

## 與其他技能整合

此技能可與以下技能組合使用：

- **awesome-assertions-guide**: 基礎斷言語法與常用 API
- **autofixture-data-generation**: 自動生成測試資料
- **test-data-builder-pattern**: 建構複雜測試物件
- **unit-test-fundamentals**: 單元測試基礎與 3A 模式

## 最佳實踐建議

### 推薦做法

1. **優先使用屬性排除而非包含**：除非只需驗證少數屬性，否則使用 `Excluding` 更清楚
2. **建立可重用的排除擴充方法**：避免在每個測試重複排除邏輯
3. **為大量資料比對設定合理策略**：平衡效能與驗證完整性
4. **使用 AssertionScope 進行批次驗證**：一次看到所有失敗原因
5. **提供有意義的 because 說明**：幫助未來維護者理解測試意圖

### 避免做法

1. **避免過度依賴完整物件比對**：考慮只驗證關鍵屬性
2. **避免忽略循環參照問題**：使用 `IgnoringCyclicReferences()` 明確處理
3. **避免在每個測試重複排除邏輯**：提取為擴充方法
4. **避免對大量資料做完整深度比對**：使用抽樣或關鍵屬性驗證

## 疑難排解

### Q1: BeEquivalentTo 效能很慢怎麼辦？

**A:** 使用以下策略優化：

- 使用 `Including` 只比對關鍵屬性
- 對大量資料採用抽樣驗證
- 使用 `WithMaxRecursionDepth` 限制遞迴深度
- 考慮使用 `AssertKeyPropertiesOnly` 快速比對關鍵欄位

### Q2: 如何處理 StackOverflowException？

**A:** 通常由循環參照引起：

```csharp
options.IgnoringCyclicReferences()
       .WithMaxRecursionDepth(10)
```

### Q3: 如何排除所有時間相關欄位？

**A:** 使用路徑模式匹配：

```csharp
options.Excluding(ctx => ctx.Path.EndsWith("At"))
       .Excluding(ctx => ctx.Path.EndsWith("Time"))
       .Excluding(ctx => ctx.Path.Contains("Timestamp"))
```

### Q4: 比對失敗但看不出差異？

**A:** 啟用詳細追蹤：

```csharp
options.WithTracing()  // 產生詳細的比對追蹤資訊
```

## 範本檔案參考

本技能提供以下範本檔案：

- `templates/comparison-patterns.cs`: 常見比對模式範例
- `templates/exclusion-strategies.cs`: 欄位排除策略與擴充方法

## 輸出格式

- 產生使用 BeEquivalentTo 的深層物件比對斷言
- 包含 Excluding/Including 屬性過濾設定
- **務必提及循環參照處理**：即使使用者未明確問到，也應說明 `IgnoringCyclicReferences()` 和 `WithMaxRecursionDepth(n)` 的用法，因為深層巢狀物件經常會遇到循環參照問題
- 包含 DTO/Entity 比對的完整測試程式碼

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 05 - AwesomeAssertions 進階技巧與複雜情境應用**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10374425
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day05

### 官方文件

- [AwesomeAssertions GitHub](https://github.com/AwesomeAssertions/AwesomeAssertions)
- [AwesomeAssertions Documentation](https://awesomeassertions.org/)

### 相關技能

- `awesome-assertions-guide` - AwesomeAssertions 基礎與進階用法
- `unit-test-fundamentals` - 單元測試基礎
