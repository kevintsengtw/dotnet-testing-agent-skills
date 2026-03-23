# xUnit 升級步驟 SOP

## 步驟 1：建立升級分支

```bash
git checkout -b feature/upgrade-xunit-v3
```

## 步驟 2：更新專案檔案

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- xUnit v3 套件 -->
    <PackageReference Include="xunit.v3" Version="3.2.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />

    <!-- 常用輔助套件 -->
    <PackageReference Include="AwesomeAssertions" Version="9.4.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
  </ItemGroup>
</Project>
```

## 步驟 3：修正 async void 測試

使用 IDE 搜尋：

```regex
async\s+void.*\[(Fact|Theory)\]
```

將所有 `async void` 改為 `async Task`。

## 步驟 4：更新 using 陳述式

```csharp
// 移除 (不再需要)
// using Xunit.Abstractions;

// 保留
using Xunit;
```

## 步驟 5：編譯與測試

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test --verbosity normal
```

---

## .NET 10 SDK：MTP 模式詳細指南

### 兩種 `dotnet test` 模式

| 模式 | SDK 版本 | 說明 |
| --- | --- | --- |
| **VSTest 模式** | .NET 9 以前（預設） | 透過 VSTest 執行，MTP 需設定 `TestingPlatformDotnetTestSupport` 並用額外 `--` 傳遞參數 |
| **MTP 模式** | .NET 10+（需啟用） | 原生支援 MTP，不需要額外橋接屬性，直接傳遞 MTP 命令列參數 |

### 啟用 MTP 模式

在專案根目錄的 `global.json` 加入 `test` 區段：

```json
{
    "sdk": {
        "version": "10.0.100"
    },
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

> **重要**：MTP 模式需要 Microsoft.Testing.Platform **1.7+** 版本。xUnit 3.2+ 已內含相容版本。

### MTP 模式下的 CLI 用法

```bash
# 直接傳入 MTP 參數，不再需要額外的 --
dotnet test --report-trx

# 明確指定方案 / 專案（.NET 10 新語法）
dotnet test --solution MySolution.sln
dotnet test --project MyTests.csproj

# 指定測試模組（支援 globbing）
dotnet test --test-modules **/bin/**/MyTests.dll
```

### 從 VSTest 模式遷移至 MTP 模式

若原先在 .NET 9 SDK 使用 VSTest 模式搭配 MTP，升級至 .NET 10 SDK 時建議遷移：

1. 在 `global.json` 加入 `"test": { "runner": "Microsoft.Testing.Platform" }`
2. **移除** `<TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>` MSBuild 屬性
3. **移除** `<TestingPlatformCaptureOutput>` 與 `<TestingPlatformShowTestsFailure>` 屬性
4. CLI 中移除額外的 `--`：`dotnet test -- --report-trx` → `dotnet test --report-trx`
5. 方案/專案路徑改用明確參數：`dotnet test My.sln` → `dotnet test --solution My.sln`

### xUnit v3 + .NET 10 SDK 搭配建議

| 項目 | 建議 |
| --- | --- |
| `EnableMicrosoftTestingPlatform` | 維持 `true`（xUnit 3.x 預設值） |
| `xunit.runner.visualstudio` | **仍建議保留** — IDE 測試探索仍依賴此套件 |
| `Microsoft.NET.Test.Sdk` | **仍建議保留** — IDE 整合與 `dotnet test` VSTest 模式仍需要 |
| `global.json` test runner | 設定 `Microsoft.Testing.Platform` 以啟用 MTP 模式 |
| `Directory.Build.props` | 建議在此統一設定 MTP 相關屬性，避免各專案不一致 |

> **注意**：在同一方案中混用 VSTest 專案與 MTP 專案是不被支援的。升級時應統一所有測試專案的執行平台。
