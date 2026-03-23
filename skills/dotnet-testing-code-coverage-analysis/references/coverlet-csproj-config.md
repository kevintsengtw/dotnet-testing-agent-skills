# Coverlet csproj 配置範例

## 在測試專案 csproj 中配置 Coverlet

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- 測試框架套件 -->
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />

    <!-- 覆蓋率收集器 -->
    <PackageReference Include="coverlet.collector" Version="8.0.1">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

## 使用 runsettings 檔案

請參考 `templates/runsettings-template.xml` 檔案，用於更進階的覆蓋率設定：

- 排除特定檔案或命名空間
- 設定覆蓋率閾值
- 自訂報告格式

**使用方式：**

```powershell
dotnet test --settings coverage.runsettings
```
