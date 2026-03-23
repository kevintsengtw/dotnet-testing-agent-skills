# TUnit 專案建立 - 完整範例

## 方式一：手動建立（理解底層架構）

```bash
# 建立專案目錄
mkdir TUnitDemo
cd TUnitDemo

# 建立解決方案
dotnet new sln -n MyApp

# 建立主專案
dotnet new classlib -n MyApp.Core -o src/MyApp.Core

# 建立測試專案（使用 console 模板）
dotnet new console -n MyApp.Tests -o tests/MyApp.Tests

# 加入解決方案
dotnet sln add src/MyApp.Core/MyApp.Core.csproj
dotnet sln add tests/MyApp.Tests/MyApp.Tests.csproj

# 加入專案參考
dotnet add tests/MyApp.Tests/MyApp.Tests.csproj reference src/MyApp.Core/MyApp.Core.csproj
```

## 方式二：使用 TUnit Template（推薦）

```bash
# 安裝 TUnit 專案模板
dotnet new install TUnit.Templates

# 使用 TUnit template 建立測試專案
dotnet new tunit -n MyApp.Tests -o tests/MyApp.Tests
```

## 測試專案 csproj 設定

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- TUnit 核心套件 -->
    <PackageReference Include="TUnit" Version="1.19.57" />
    <!-- 程式碼覆蓋率支援（已內建於 TUnit 1.x meta-package，此處鎖定版本）-->
    <PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" Version="18.5.2" />
    <!-- TRX 報告支援（已內建於 TUnit 1.x meta-package，此處鎖定版本）-->
    <PackageReference Include="Microsoft.Testing.Extensions.TrxReport" Version="2.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\MyApp.Core\MyApp.Core.csproj" />
  </ItemGroup>

</Project>
```

## GlobalUsings 設定

```csharp
// GlobalUsings.cs
global using TUnit.Core;
global using TUnit.Assertions;
global using MyApp.Core;
```

## 非同步測試方法（必要）

TUnit 的**所有測試方法都必須是非同步的**，這是框架的技術要求：

```csharp
// ❌ 錯誤：無法編譯
[Test]
public void WrongTest()
{
    Assert.That(1 + 1).IsEqualTo(2);
}

// ✅ 正確：使用 async Task
[Test]
public async Task CorrectTest()
{
    await Assert.That(1 + 1).IsEqualTo(2);
}
```
