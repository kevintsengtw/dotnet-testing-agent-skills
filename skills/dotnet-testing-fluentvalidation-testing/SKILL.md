---
name: dotnet-testing-fluentvalidation-testing
description: |
  測試 FluentValidation 驗證器的專門技能。當需要為 Validator 類別建立測試、驗證業務規則、測試錯誤訊息時使用。涵蓋 FluentValidation.TestHelper 完整使用、ShouldHaveValidationErrorFor、非同步驗證、跨欄位邏輯等。
  Keywords: validator, 驗證器, fluentvalidation, validation testing, UserValidator, CreateOrderValidator, TestHelper, ShouldHaveValidationErrorFor, ShouldNotHaveValidationErrorFor, TestValidate, TestValidateAsync, 測試驗證器, 驗證業務規則
license: MIT
metadata:
  author: Kevin Tseng
  version: "1.0.0"
  tags: ".NET, testing, FluentValidation, validator, validation"
  related_skills: "awesome-assertions-guide, nsubstitute-mocking, unit-test-fundamentals"
---

# FluentValidation 驗證器測試指南

## 適用情境

此技能專注於使用 FluentValidation.TestHelper 測試資料驗證邏輯，涵蓋基本驗證、複雜業務規則、非同步驗證和測試最佳實踐。

## 為什麼要測試驗證器？

驗證器是應用程式的第一道防線，測試驗證器能：

1. **確保資料完整性** - 防止無效資料進入系統
2. **業務規則文件化** - 測試即活文件，清楚展示業務規則
3. **安全性保障** - 防止惡意或不當資料輸入
4. **重構安全網** - 業務規則變更時提供保障
5. **跨欄位邏輯驗證** - 確保複雜邏輯正確運作

## 前置需求

### 套件安裝

```xml
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.TestHelper" Version="11.11.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="Microsoft.Extensions.Time.Testing" Version="9.0.0" />
<PackageReference Include="NSubstitute" Version="5.3.0" />
<PackageReference Include="AwesomeAssertions" Version="9.1.0" />
```

### 基本 using 指令

```csharp
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;
using AwesomeAssertions;
```

## 核心測試模式

本節涵蓋 7 種核心測試模式，每種模式包含驗證器定義與完整測試範例。

> 📖 完整程式碼範例請參考 [references/core-test-patterns.md](references/core-test-patterns.md)

- **模式 1：基本欄位驗證** — 使用 `TestValidate` + `ShouldHaveValidationErrorFor` / `ShouldNotHaveValidationErrorFor` 測試單一欄位規則
- **模式 2：參數化測試** — 使用 `[Theory]` + `[InlineData]` 測試多種無效/有效輸入組合
- **模式 3：跨欄位驗證** — 密碼確認、自訂 `Must()` 規則等多欄位關聯驗證
- **模式 4：時間相依驗證** — 注入 `TimeProvider`，搭配 `FakeTimeProvider` 控制時間進行測試
- **模式 5：條件式驗證** — 使用 `.When()` 的可選欄位驗證，測試條件觸發與跳過情境
- **模式 6：非同步驗證** — `MustAsync` + `TestValidateAsync`，搭配 NSubstitute Mock 外部服務
- **模式 7：集合驗證** — 驗證集合非空與元素有效性

### 快速範例：基本欄位驗證

```csharp
public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Fact]
    public void Validate_空白使用者名稱_應該驗證失敗()
    {
        var result = _validator.TestValidate(
            new UserRegistrationRequest { Username = "" });

        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱不可為 null 或空白");
    }
}
```

## FluentValidation.TestHelper 核心 API

### 測試方法

| 方法                       | 用途           | 範例                                          |
| -------------------------- | -------------- | --------------------------------------------- |
| `TestValidate(model)`      | 執行同步驗證   | `_validator.TestValidate(request)`            |
| `TestValidateAsync(model)` | 執行非同步驗證 | `await _validator.TestValidateAsync(request)` |

### 斷言方法

| 方法                                               | 用途                     | 範例                                                   |
| -------------------------------------------------- | ------------------------ | ------------------------------------------------------ |
| `ShouldHaveValidationErrorFor(x => x.Property)`    | 斷言該屬性應該有錯誤     | `result.ShouldHaveValidationErrorFor(x => x.Username)` |
| `ShouldNotHaveValidationErrorFor(x => x.Property)` | 斷言該屬性不應該有錯誤   | `result.ShouldNotHaveValidationErrorFor(x => x.Email)` |
| `ShouldNotHaveAnyValidationErrors()`               | 斷言整個物件沒有任何錯誤 | `result.ShouldNotHaveAnyValidationErrors()`            |

### 錯誤訊息驗證

| 方法                       | 用途             | 範例                                      |
| -------------------------- | ---------------- | ----------------------------------------- |
| `WithErrorMessage(string)` | 驗證錯誤訊息內容 | `.WithErrorMessage("使用者名稱不可為空")` |
| `WithErrorCode(string)`    | 驗證錯誤代碼     | `.WithErrorCode("NOT_EMPTY")`             |

## 測試最佳實踐

### ✅ 推薦做法

1. **使用參數化測試** - 用 Theory 測試多種輸入組合
2. **測試邊界值** - 特別注意邊界條件
3. **控制時間** - 使用 FakeTimeProvider 處理時間相依
4. **Mock 外部依賴** - 使用 NSubstitute 隔離外部服務
5. **建立輔助方法** - 統一管理測試資料
6. **清楚的測試命名** - 使用 `方法_情境_預期結果` 格式
7. **測試錯誤訊息** - 確保使用者看到正確的錯誤訊息

### ❌ 避免做法

1. **避免使用 DateTime.Now** - 會導致測試不穩定
2. **避免測試過度耦合** - 每個測試只驗證一個規則
3. **避免硬編碼測試資料** - 使用輔助方法建立
4. **避免忽略邊界條件** - 邊界值是最容易出錯的地方
5. **避免跳過錯誤訊息驗證** - 錯誤訊息是使用者體驗的一部分

## 常見測試場景

### 場景 1：Email 格式驗證

```csharp
[Theory]
[InlineData("", "電子郵件不可為 null 或空白")]
[InlineData("invalid", "電子郵件格式不正確")]
[InlineData("@example.com", "電子郵件格式不正確")]
public void Validate_無效Email_應該驗證失敗(string email, string expectedError)
{
    var request = new UserRegistrationRequest { Email = email };
    var result = _validator.TestValidate(request);
    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(expectedError);
}
```

### 場景 2：年齡範圍驗證

```csharp
[Theory]
[InlineData(17, "年齡必須大於或等於 18 歲")]
[InlineData(121, "年齡必須小於或等於 120 歲")]
public void Validate_無效年齡_應該驗證失敗(int age, string expectedError)
{
    var request = new UserRegistrationRequest { Age = age };
    var result = _validator.TestValidate(request);
    result.ShouldHaveValidationErrorFor(x => x.Age).WithErrorMessage(expectedError);
}
```

### 場景 3：必填欄位驗證

```csharp
[Fact]
public void Validate_未同意條款_應該驗證失敗()
{
    var request = new UserRegistrationRequest { AgreeToTerms = false };
    var result = _validator.TestValidate(request);
    result.ShouldHaveValidationErrorFor(x => x.AgreeToTerms)
          .WithErrorMessage("必須同意使用條款");
}
```

## 測試輔助工具

### 測試資料建構器

```csharp
public static class TestDataBuilder
{
    public static UserRegistrationRequest CreateValidRequest()
    {
        return new UserRegistrationRequest
        {
            Username = "testuser123",
            Email = "test@example.com",
            Password = "TestPass123",
            ConfirmPassword = "TestPass123",
            BirthDate = new DateTime(1990, 1, 1),
            Age = 34,
            PhoneNumber = "0912345678",
            Roles = new List<string> { "User" },
            AgreeToTerms = true
        };
    }

    public static UserRegistrationRequest WithUsername(this UserRegistrationRequest request, string username)
    {
        request.Username = username;
        return request;
    }

    public static UserRegistrationRequest WithEmail(this UserRegistrationRequest request, string email)
    {
        request.Email = email;
        return request;
    }
}

// 使用範例
var request = TestDataBuilder.CreateValidRequest()
                            .WithUsername("newuser")
                            .WithEmail("new@example.com");
```

## 與其他技能整合

此技能可與以下技能組合使用：

- **unit-test-fundamentals**: 單元測試基礎與 3A 模式
- **test-naming-conventions**: 測試命名規範
- **nsubstitute-mocking**: Mock 外部服務依賴
- **test-data-builder-pattern**: 建構複雜測試資料
- **datetime-testing-timeprovider**: 時間相依測試

## 疑難排解

### Q1: 如何測試需要資料庫查詢的驗證？

**A:** 使用 Mock 隔離資料庫依賴：

```csharp
_mockUserService.IsUsernameAvailableAsync("username")
                .Returns(Task.FromResult(false));
```

### Q2: 如何處理時間相關的驗證？

**A:** 使用 FakeTimeProvider 控制時間：

```csharp
_fakeTimeProvider.SetUtcNow(new DateTime(2024, 1, 1));
```

### Q3: 如何測試複雜的跨欄位驗證？

**A:** 分別測試每個條件，確保完整覆蓋：

```csharp
// 測試生日已過的情況
// 測試生日未到的情況
// 測試邊界日期
```

### Q4: 應該測試到什麼程度？

**A:** 重點測試：

- 每個驗證規則至少一個測試
- 邊界值和特殊情況
- 錯誤訊息正確性
- 跨欄位邏輯的所有組合

## 範本檔案參考

本技能提供以下範本檔案：

- `templates/validator-test-template.cs`: 完整的驗證器測試範例
- `templates/async-validator-examples.cs`: 非同步驗證範例

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 18 - 驗證測試：FluentValidation Test Extensions**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10376147
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day18

### 官方文件

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [FluentValidation.TestHelper](https://docs.fluentvalidation.net/en/latest/testing.html)
- [FluentValidation GitHub](https://github.com/FluentValidation/FluentValidation)

### 相關技能

- `unit-test-fundamentals` - 單元測試基礎
- `nsubstitute-mocking` - 測試替身與模擬
