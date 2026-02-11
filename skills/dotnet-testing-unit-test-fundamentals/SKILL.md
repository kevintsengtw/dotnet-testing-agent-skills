---
name: dotnet-testing-unit-test-fundamentals
description: |
  .NET 單元測試基礎與 FIRST 原則的專門技能。當需要建立單元測試、了解測試基礎、學習 3A Pattern、掌握測試最佳實踐時使用。涵蓋 FIRST 原則、AAA Pattern、Fact/Theory、測試金字塔等。
  Keywords: unit test, 單元測試, unit testing, test fundamentals, 測試基礎, FIRST principle, FIRST 原則, 3A pattern, AAA pattern, Arrange Act Assert, Fact, Theory, InlineData, 如何寫測試, testing best practices, 建立單元測試
license: MIT
metadata:
  author: Kevin Tseng
  version: "1.0.0"
  tags: ".NET, testing, unit test, FIRST, AAA pattern, xUnit"
  related_skills: "xunit-project-setup, test-naming-conventions, awesome-assertions-guide"
---

# .NET 單元測試基礎指南

## 適用情境

當被要求執行以下任務時，請使用此技能：

- 為 .NET 類別或方法建立單元測試
- 檢視或改進現有測試的品質
- 設計符合 FIRST 原則的測試案例
- 解釋測試命名規範與最佳實踐
- 使用 xUnit 撰寫測試

## FIRST 原則

每個單元測試都**必須**符合以下原則：

### F - Fast (快速)

測試執行時間應在毫秒級，不依賴外部資源。

```csharp
[Fact] // Fast: 不依賴外部資源，執行快速
public void Add_輸入1和2_應回傳3()
{
    // 純記憶體運算，無 I/O 或網路延遲
    var calculator = new Calculator();
    var result = calculator.Add(1, 2);
    Assert.Equal(3, result);
}
```

### I - Independent (獨立)

測試之間不應有相依性，每個測試都建立新的實例。

```csharp
[Fact] // Independent: 每個測試都建立新的實例
public void Increment_從0開始_應回傳1()
{
    var counter = new Counter(); // 每個測試都建立新的實例，不受其他測試影響
    counter.Increment();
    Assert.Equal(1, counter.Value);
}
```

### R - Repeatable (可重複)

在任何環境都能得到相同結果，不依賴外部狀態。

```csharp
[Fact] // Repeatable: 每次執行都得到相同結果
public void Increment_多次執行_應產生一致結果()
{
    var counter = new Counter();
    counter.Increment();
    counter.Increment();
    counter.Increment();
    
    // 每次執行這個測試都會得到相同結果
    Assert.Equal(3, counter.Value);
}
```

### S - Self-Validating (自我驗證)

測試結果應為明確的通過或失敗，使用清晰的斷言。

```csharp
[Fact] // Self-Validating: 明確的驗證
public void IsValidEmail_輸入有效Email_應回傳True()
{
    var emailHelper = new EmailHelper();
    var result = emailHelper.IsValidEmail("test@example.com");
    
    Assert.True(result); // 明確的通過或失敗
}
```

### T - Timely (及時)

測試應在產品程式碼之前或同時撰寫，確保程式碼的可測試性。

## 3A Pattern 結構

每個測試方法**必須**遵循 Arrange-Act-Assert 模式：

```csharp
[Fact]
public void Add_輸入負數和正數_應回傳正確結果()
{
    // Arrange - 準備測試資料與相依物件
    var calculator = new Calculator();
    const int a = -5;
    const int b = 3;
    const int expected = -2;

    // Act - 執行被測試的方法
    var result = calculator.Add(a, b);

    // Assert - 驗證結果是否符合預期
    Assert.Equal(expected, result);
}
```

### 各區塊職責

| 區塊        | 職責                           | 注意事項                            |
| ----------- | ------------------------------ | ----------------------------------- |
| **Arrange** | 準備測試所需的物件、資料、Mock | 使用 `const` 宣告常數值，提高可讀性 |
| **Act**     | 執行被測試的方法               | 通常只有一行，呼叫被測方法          |
| **Assert**  | 驗證結果                       | 每個測試只驗證一個行為              |

## 測試命名規範

使用以下格式命名測試方法：

```text
[被測試方法名稱]_[測試情境]_[預期行為]
```

### 命名範例

| 方法名稱                                       | 說明         |
| ---------------------------------------------- | ------------ |
| `Add_輸入1和2_應回傳3`                         | 測試正常輸入 |
| `Add_輸入負數和正數_應回傳正確結果`            | 測試邊界條件 |
| `Divide_輸入10和0_應拋出DivideByZeroException` | 測試例外情況 |
| `IsValidEmail_輸入null值_應回傳False`          | 測試無效輸入 |
| `GetDomain_輸入有效Email_應回傳網域名稱`       | 測試回傳值   |

> 💡 **提示**：使用中文命名可以讓測試報告更易讀，特別是在團隊溝通時。

## xUnit 測試屬性

### [Fact] - 單一測試案例

用於測試單一情境：

```csharp
[Fact]
public void Add_輸入0和0_應回傳0()
{
    var calculator = new Calculator();
    var result = calculator.Add(0, 0);
    Assert.Equal(0, result);
}
```

### [Theory] + [InlineData] - 參數化測試

用於測試多個輸入組合：

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
[InlineData(100, -50, 50)]
public void Add_輸入各種數值組合_應回傳正確結果(int a, int b, int expected)
{
    var calculator = new Calculator();
    var result = calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### 測試多個無效輸入

```csharp
[Theory]
[InlineData("invalid-email")]
[InlineData("@example.com")]
[InlineData("test@")]
[InlineData("test.example.com")]
public void IsValidEmail_輸入無效Email格式_應回傳False(string invalidEmail)
{
    var emailHelper = new EmailHelper();
    var result = emailHelper.IsValidEmail(invalidEmail);
    Assert.False(result);
}
```

## 例外測試

測試預期會拋出例外的情況：

```csharp
[Fact]
public void Divide_輸入10和0_應拋出DivideByZeroException()
{
    // Arrange
    var calculator = new Calculator();
    const decimal dividend = 10m;
    const decimal divisor = 0m;

    // Act & Assert
    var exception = Assert.Throws<DivideByZeroException>(
        () => calculator.Divide(dividend, divisor)
    );

    // 驗證例外訊息
    Assert.Equal("除數不能為零", exception.Message);
}
```

## 測試專案結構

建議的專案結構：

```text
Solution/
├── src/
│   └── MyProject/
│       ├── Calculator.cs
│       └── MyProject.csproj
└── tests/
    └── MyProject.Tests/
        ├── CalculatorTests.cs
        └── MyProject.Tests.csproj
```

## 測試專案範本 (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="coverlet.collector" Version="6.0.4">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
        <PackageReference Include="xunit" Version="2.9.3" />
        <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
    </ItemGroup>

    <ItemGroup>
        <Using Include="Xunit" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\MyProject\MyProject.csproj" />
    </ItemGroup>

</Project>
```

## 常用斷言方法

| 斷言方法                            | 用途             |
| ----------------------------------- | ---------------- |
| `Assert.Equal(expected, actual)`    | 驗證相等         |
| `Assert.NotEqual(expected, actual)` | 驗證不相等       |
| `Assert.True(condition)`            | 驗證條件為真     |
| `Assert.False(condition)`           | 驗證條件為假     |
| `Assert.Null(object)`               | 驗證為 null      |
| `Assert.NotNull(object)`            | 驗證不為 null    |
| `Assert.Throws<T>(action)`          | 驗證拋出特定例外 |
| `Assert.Empty(collection)`          | 驗證集合為空     |
| `Assert.Contains(item, collection)` | 驗證集合包含項目 |

## 生成測試的檢查清單

為方法生成測試時，請確保涵蓋：

- [ ] **正常路徑** - 標準輸入產生預期輸出
- [ ] **邊界條件** - 最小值、最大值、零、空字串
- [ ] **無效輸入** - null、負數、格式錯誤
- [ ] **例外情況** - 預期會拋出例外的情境

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 01 - 老派工程師的測試啟蒙**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10373888
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day01
