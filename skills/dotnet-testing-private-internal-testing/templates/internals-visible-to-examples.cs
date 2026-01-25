using System.Runtime.CompilerServices;

/// <summary>
/// InternalsVisibleTo 設定範例
/// 展示四種設定 InternalsVisibleTo 的方法
/// </summary>
/// 
// ========================================
// 方法一：直接在程式碼中宣告屬性
// ========================================
// 適用於：簡單專案、單一測試專案

// 在主專案的任何 .cs 檔案中加入（通常是 AssemblyInfo.cs）
[assembly: InternalsVisibleTo("MyProject.Tests")]
[assembly: InternalsVisibleTo("MyProject.IntegrationTests")]

// 如果使用簽署組件，需要包含公鑰
[assembly: InternalsVisibleTo("MyProject.Tests, PublicKey=0024000004800000...")]


// ========================================
// 方法二：在 .csproj 中使用 AssemblyAttribute
// ========================================
// 適用於：需要使用 MSBuild 變數的專案

/*
<!-- MyProject.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
      <_Parameter1>$(AssemblyName).Tests</_Parameter1>
    </AssemblyAttribute>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
      <_Parameter1>$(AssemblyName).IntegrationTests</_Parameter1>
    </AssemblyAttribute>
  </ItemGroup>
</Project>
*/


// ========================================
// 方法三：使用 Meziantou.MSBuild.InternalsVisibleTo（推薦）
// ========================================
// 適用於：複雜專案、需要支援 NSubstitute/Moq 動態代理的專案

/*
<!-- MyProject.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <!-- 1. 安裝 NuGet 套件 -->
  <ItemGroup>
    <PackageReference Include="Meziantou.MSBuild.InternalsVisibleTo" Version="1.0.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <!-- 2. 宣告可見的測試專案 -->
  <ItemGroup>
    <InternalsVisibleTo Include="$(AssemblyName).Tests" />
    <InternalsVisibleTo Include="$(AssemblyName).IntegrationTests" />
    
    <!-- 3. 自動支援 NSubstitute/Moq 動態代理（自動包含正確的公鑰） -->
    <InternalsVisibleTo Include="DynamicProxyGenAssembly2" 
                        Key="0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7" />
  </ItemGroup>
</Project>

安裝 NuGet 套件：
dotnet add package Meziantou.MSBuild.InternalsVisibleTo

參考資源：
- https://www.meziantou.net/declaring-internalsvisibleto-in-the-csproj.htm
- https://github.com/meziantou/Meziantou.MSBuild.InternalsVisibleTo
*/


// ========================================
// 測試 Internal 類別的範例
// ========================================

namespace MyProject.Core;

/// <summary>
/// Internal 類別：價格計算器
/// </summary>
internal class PriceCalculator
{
    /// <summary>
    /// 計算商品等級（Internal 方法）
    /// </summary>
    internal string CalculatePriceLevel(decimal price)
    {
        return price switch
        {
            >= 10000 => "豪華級",
            >= 5000 => "高級",
            >= 1000 => "中級",
            > 0 => "經濟級",
            _ => "無效價格"
        };
    }

    /// <summary>
    /// 計算折扣後價格（Internal 方法）
    /// </summary>
    internal decimal CalculateDiscountedPrice(decimal originalPrice, decimal discountRate)
    {
        if (discountRate is < 0 or > 1)
            throw new ArgumentException("折扣率必須在 0 到 1 之間", nameof(discountRate));

        return originalPrice * (1 - discountRate);
    }
}


// ========================================
// 測試專案中的測試範例
// ========================================

/*
using Xunit;
using AwesomeAssertions;

namespace MyProject.Tests;

/// <summary>
/// PriceCalculator 測試類別
/// 因為設定了 InternalsVisibleTo，可以存取 internal 成員
/// </summary>
public class PriceCalculatorTests
{
    [Theory]
    [InlineData(15000, "豪華級")]
    [InlineData(8000, "高級")]
    [InlineData(3000, "中級")]
    [InlineData(500, "經濟級")]
    [InlineData(0, "無效價格")]
    public void CalculatePriceLevel_不同價格_應回傳正確等級(decimal price, string expected)
    {
        // Arrange
        var calculator = new PriceCalculator(); // 可以存取 internal 類別

        // Act
        var actual = calculator.CalculatePriceLevel(price); // 可以呼叫 internal 方法

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1000, 0.1, 900)]
    [InlineData(2000, 0.2, 1600)]
    [InlineData(500, 0.05, 475)]
    public void CalculateDiscountedPrice_正常折扣_應計算正確價格(
        decimal originalPrice, decimal discountRate, decimal expected)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act
        var actual = calculator.CalculateDiscountedPrice(originalPrice, discountRate);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void CalculateDiscountedPrice_無效折扣率_應拋出例外(decimal invalidDiscountRate)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act & Assert
        var action = () => calculator.CalculateDiscountedPrice(1000, invalidDiscountRate);
        action.Should().Throw<ArgumentException>()
            .WithMessage("折扣率必須在 0 到 1 之間*");
    }
}
*/


// ========================================
// 方法比較與選擇建議
// ========================================

/*
方法比較：

| 方法                                   | 優點                         | 缺點                         | 適用情境               |
|----------------------------------------|------------------------------|------------------------------|------------------------|
| 1. 直接宣告屬性                         | 簡單直接                     | 硬編碼專案名稱               | 簡單專案               |
| 2. .csproj AssemblyAttribute           | 可使用 MSBuild 變數          | 語法較複雜                   | 需要動態組件名稱       |
| 3. Meziantou.MSBuild.InternalsVisibleTo| 自動處理公鑰、可讀性高       | 需要額外套件                 | 複雜專案（推薦）       |

選擇建議：
- 簡單專案：方法一
- 需要 MSBuild 變數：方法二
- 複雜專案或使用 Mock 框架：方法三（推薦）

注意事項：
1. 只對真正需要測試的 internal 成員開放可見性
2. 避免濫用 InternalsVisibleTo，優先考慮重構為更好的設計
3. 記錄為何需要開放 internal 可見性（設計文件或註解）
4. 定期檢視是否仍需要這些可見性設定
*/
