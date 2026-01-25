# xUnit 測試專案結構範例

## 標準專案結構

以下是一個完整的 .NET 解決方案與 xUnit 測試專案的標準結構：

```text
MyProject/
│
├── MyProject.sln                         # 解決方案檔
│
├── src/                                  # 主程式碼目錄
│   │
│   └── MyProject.Core/                   # 核心業務邏輯專案
│       ├── MyProject.Core.csproj
│       │
│       ├── Models/                       # 資料模型
│       │   ├── User.cs
│       │   ├── Order.cs
│       │   └── Product.cs
│       │
│       ├── Services/                     # 業務邏輯服務
│       │   ├── IOrderService.cs
│       │   ├── OrderService.cs
│       │   ├── IUserService.cs
│       │   └── UserService.cs
│       │
│       ├── Repositories/                 # 資料存取層
│       │   ├── IOrderRepository.cs
│       │   ├── OrderRepository.cs
│       │   ├── IUserRepository.cs
│       │   └── UserRepository.cs
│       │
│       └── Utilities/                    # 工具類別
│           ├── Calculator.cs
│           ├── StringHelper.cs
│           └── DateTimeHelper.cs
│
├── tests/                                # 測試程式碼目錄
│   │
│   └── MyProject.Core.Tests/             # 核心業務邏輯測試專案
│       ├── MyProject.Core.Tests.csproj
│       │
│       ├── Models/                       # 對應 Models 的測試
│       │   ├── UserTests.cs
│       │   ├── OrderTests.cs
│       │   └── ProductTests.cs
│       │
│       ├── Services/                     # 對應 Services 的測試
│       │   ├── OrderServiceTests.cs
│       │   └── UserServiceTests.cs
│       │
│       ├── Repositories/                 # 對應 Repositories 的測試
│       │   ├── OrderRepositoryTests.cs
│       │   └── UserRepositoryTests.cs
│       │
│       ├── Utilities/                    # 對應 Utilities 的測試
│       │   ├── CalculatorTests.cs
│       │   ├── StringHelperTests.cs
│       │   └── DateTimeHelperTests.cs
│       │
│       └── Fixtures/                     # 測試共用資源（Fixtures）
│           ├── DatabaseFixture.cs
│           └── TestDataFixture.cs
│
├── .gitignore
└── README.md
```

## 多專案解決方案結構

當解決方案包含多個專案時，建議的完整結構：

```text
MyCompany.MyProduct/
│
├── MyCompany.MyProduct.sln
│
├── src/
│   │
│   ├── MyCompany.MyProduct.Core/         # 核心業務邏輯
│   │   └── MyCompany.MyProduct.Core.csproj
│   │
│   ├── MyCompany.MyProduct.Web/          # Web API 專案
│   │   └── MyCompany.MyProduct.Web.csproj
│   │
│   ├── MyCompany.MyProduct.Infrastructure/ # 基礎設施層
│   │   └── MyCompany.MyProduct.Infrastructure.csproj
│   │
│   └── MyCompany.MyProduct.Shared/       # 共用程式碼
│       └── MyCompany.MyProduct.Shared.csproj
│
├── tests/
│   │
│   ├── MyCompany.MyProduct.Core.Tests/   # 核心業務邏輯單元測試
│   │   └── MyCompany.MyProduct.Core.Tests.csproj
│   │
│   ├── MyCompany.MyProduct.Web.Tests/    # Web API 單元測試
│   │   └── MyCompany.MyProduct.Web.Tests.csproj
│   │
│   ├── MyCompany.MyProduct.Infrastructure.Tests/ # 基礎設施單元測試
│   │   └── MyCompany.MyProduct.Infrastructure.Tests.csproj
│   │
│   └── MyCompany.MyProduct.Integration.Tests/    # 整合測試
│       └── MyCompany.MyProduct.Integration.Tests.csproj
│
├── docs/                                 # 文件目錄
│   ├── architecture.md
│   └── api-documentation.md
│
├── .editorconfig                         # 編輯器設定
├── .gitignore
├── Directory.Build.props                 # 共用 MSBuild 屬性
├── Directory.Build.targets               # 共用 MSBuild 目標
└── README.md
```

## 測試類別檔案結構

### 單一測試類別檔案範例

```csharp
// CalculatorTests.cs
using Xunit;
using MyProject.Core.Utilities;

namespace MyProject.Core.Tests.Utilities;

/// <summary>
/// Calculator 類別的單元測試
/// </summary>
public class CalculatorTests : IDisposable
{
    private readonly Calculator _calculator;

    // 建構函式：每個測試執行前初始化
    public CalculatorTests()
    {
        _calculator = new Calculator();
    }

    // Fact：單一測試案例
    [Fact]
    public void Add_輸入兩個正整數_應回傳正確的加總()
    {
        // Arrange
        int a = 5;
        int b = 3;

        // Act
        var result = _calculator.Add(a, b);

        // Assert
        Assert.Equal(8, result);
    }

    // Theory：參數化測試案例
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    public void Add_多組輸入_應回傳正確結果(int a, int b, int expected)
    {
        // Act
        var result = _calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    // Dispose：每個測試執行後清理資源
    public void Dispose()
    {
        // 清理邏輯（如果需要）
    }
}
```

## 命名慣例

### 測試專案命名

| 主專案                 | 測試專案                           | 說明           |
| ---------------------- | ---------------------------------- | -------------- |
| `MyProject.Core`       | `MyProject.Core.Tests`             | 單元測試       |
| `MyProject.Web`        | `MyProject.Web.Tests`              | Web 層單元測試 |
| `MyProject.Web`        | `MyProject.Web.Integration.Tests`  | Web 層整合測試 |
| `MyProject`            | `MyProject.Acceptance.Tests`       | 驗收測試       |
| `MyProject`            | `MyProject.Performance.Tests`      | 效能測試       |

### 測試類別命名

| 被測試的類別     | 測試類別                 | 檔案名稱                     |
| ---------------- | ------------------------ | ---------------------------- |
| `Calculator`     | `CalculatorTests`        | `CalculatorTests.cs`         |
| `OrderService`   | `OrderServiceTests`      | `OrderServiceTests.cs`       |
| `UserRepository` | `UserRepositoryTests`    | `UserRepositoryTests.cs`     |
| `StringHelper`   | `StringHelperTests`      | `StringHelperTests.cs`       |

## 目錄對應原則

測試專案的資料夾結構應該鏡像主專案：

```text
主專案：
src/MyProject.Core/Services/OrderService.cs

測試專案：
tests/MyProject.Core.Tests/Services/OrderServiceTests.cs
```

這樣的結構讓開發者可以快速找到對應的測試檔案。

## 建立命令參考

### 使用 .NET CLI 建立此結構

```powershell
# 1. 建立解決方案
dotnet new sln -n MyProject

# 2. 建立主專案
dotnet new classlib -n MyProject.Core -o src/MyProject.Core

# 3. 建立測試專案
dotnet new xunit -n MyProject.Core.Tests -o tests/MyProject.Core.Tests

# 4. 加入到解決方案
dotnet sln add src/MyProject.Core/MyProject.Core.csproj
dotnet sln add tests/MyProject.Core.Tests/MyProject.Core.Tests.csproj

# 5. 建立專案參考
dotnet add tests/MyProject.Core.Tests/MyProject.Core.Tests.csproj reference src/MyProject.Core/MyProject.Core.csproj

# 6. 在主專案中建立子目錄
mkdir src/MyProject.Core/Models
mkdir src/MyProject.Core/Services
mkdir src/MyProject.Core/Repositories
mkdir src/MyProject.Core/Utilities

# 7. 在測試專案中建立對應子目錄
mkdir tests/MyProject.Core.Tests/Models
mkdir tests/MyProject.Core.Tests/Services
mkdir tests/MyProject.Core.Tests/Repositories
mkdir tests/MyProject.Core.Tests/Utilities
mkdir tests/MyProject.Core.Tests/Fixtures
```

## 最佳實踐建議

### ✅ 推薦做法

1. **測試專案與主專案分離**：使用 `src/` 和 `tests/` 目錄
2. **命名一致性**：測試專案名稱為 `{主專案}.Tests`
3. **目錄鏡像**：測試專案的資料夾結構對應主專案
4. **一個類別一個測試檔**：`Calculator.cs` → `CalculatorTests.cs`
5. **使用 Fixtures 資料夾**：存放共用的測試資源

### ❌ 避免做法

1. **不要混合測試與生產程式碼**：不要在主專案中放測試
2. **不要巢狀過深**：資料夾層級不要超過 3-4 層
3. **不要在測試專案中包含業務邏輯**：測試專案只包含測試
4. **主專案不要參考測試專案**：只能是測試參考主專案
5. **不要將測試專案打包**：確保 `IsPackable=false`

## 範例：完整的小型專案

```text
Calculator/
├── Calculator.sln
│
├── src/
│   └── Calculator.Core/
│       ├── Calculator.Core.csproj
│       ├── Calculator.cs
│       └── MathHelper.cs
│
└── tests/
    └── Calculator.Core.Tests/
        ├── Calculator.Core.Tests.csproj
        ├── CalculatorTests.cs
        └── MathHelperTests.cs
```

**建立指令：**

```powershell
dotnet new sln -n Calculator
dotnet new classlib -n Calculator.Core -o src/Calculator.Core
dotnet new xunit -n Calculator.Core.Tests -o tests/Calculator.Core.Tests
dotnet sln add src/Calculator.Core/Calculator.Core.csproj
dotnet sln add tests/Calculator.Core.Tests/Calculator.Core.Tests.csproj
dotnet add tests/Calculator.Core.Tests reference src/Calculator.Core
```

這個結構簡潔、清晰，適合小型到中型專案使用。
