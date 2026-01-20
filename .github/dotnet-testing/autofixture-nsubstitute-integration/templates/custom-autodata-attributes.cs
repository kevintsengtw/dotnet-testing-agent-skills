// =============================================================================
// 自訂 AutoData 屬性範本
// AutoFixture + NSubstitute 整合的 AutoData 屬性實作範例
// =============================================================================

#region 基本使用範例

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Mapster;
using MapsterMapper;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace MyProject.Tests.AutoFixtureConfigurations;

// =============================================================================
// 基礎 AutoData 屬性（僅含 AutoNSubstitute）
// =============================================================================

/// <summary>
/// 基礎的自動模擬 AutoData 屬性
/// 自動為所有介面和抽象類別建立 NSubstitute 替身
/// </summary>
/// <example>
/// <code>
/// [Theory]
/// [AutoNSubstituteData]
/// public void Test([Frozen] IRepository repo, MyService sut)
/// {
///     repo.GetAsync(1).Returns(someData);
///     // ...
/// }
/// </code>
/// </example>
public class AutoNSubstituteDataAttribute : AutoDataAttribute
{
    public AutoNSubstituteDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        return new Fixture().Customize(new AutoNSubstituteCustomization());
    }
}

/// <summary>
/// 基礎的 InlineAutoData 版本
/// </summary>
/// <example>
/// <code>
/// [Theory]
/// [InlineAutoNSubstituteData(0)]
/// [InlineAutoNSubstituteData(-1)]
/// public void Test_InvalidId(int invalidId, MyService sut)
/// {
///     // invalidId 是固定值，sut 自動產生
/// }
/// </code>
/// </example>
public class InlineAutoNSubstituteDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoNSubstituteDataAttribute(params object[] values)
        : base(new AutoNSubstituteDataAttribute(), values)
    {
    }
}

#endregion

#region 進階 AutoData 屬性（含多個 Customization）

// =============================================================================
// 整合多個 Customization 的 AutoData 屬性
// =============================================================================

/// <summary>
/// 包含完整專案客製化設定的 AutoData 屬性
/// 整合 AutoNSubstitute + Mapster + 領域模型設定
/// </summary>
/// <remarks>
/// 適用於需要以下功能的測試：
/// - 自動為介面建立 NSubstitute 替身
/// - 使用真實的 Mapster 對應器
/// - 套用領域模型的客製化規則
/// </remarks>
/// <example>
/// <code>
/// [Theory]
/// [AutoDataWithCustomization]
/// public async Task GetAsync_資料存在_應回傳正確資料(
///     [Frozen] IRepository repository,
///     MyService sut,
///     MyModel model)
/// {
///     repository.GetAsync(Arg.Any&lt;int&gt;()).Returns(model);
///     var result = await sut.GetAsync(model.Id);
///     result.Should().BeEquivalentTo(model);
/// }
/// </code>
/// </example>
public class AutoDataWithCustomizationAttribute : AutoDataAttribute
{
    public AutoDataWithCustomizationAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            // 自動為介面建立 NSubstitute 替身
            .Customize(new AutoNSubstituteCustomization())
            // 使用真實的 Mapster 對應器
            .Customize(new MapsterMapperCustomization())
            // 套用領域模型的客製化規則
            .Customize(new DomainModelCustomization());

        return fixture;
    }
}

/// <summary>
/// 包含完整專案客製化設定的 InlineAutoData 屬性
/// </summary>
/// <remarks>
/// 用於需要混合固定測試值與自動產生物件的場景：
/// - 邊界值測試
/// - 異常參數測試
/// - 多組固定條件測試
/// </remarks>
/// <example>
/// <code>
/// [Theory]
/// [InlineAutoDataWithCustomization(0, 10, nameof(from))]
/// [InlineAutoDataWithCustomization(-1, 10, nameof(from))]
/// [InlineAutoDataWithCustomization(1, 0, nameof(size))]
/// public async Task GetCollection_無效參數_應拋出異常(
///     int from,
///     int size,
///     string parameterName,
///     MyService sut)
/// {
///     var ex = await Assert.ThrowsAsync&lt;ArgumentOutOfRangeException&gt;(
///         () => sut.GetCollectionAsync(from, size));
///     ex.Message.Should().Contain(parameterName);
/// }
/// </code>
/// </example>
public class InlineAutoDataWithCustomizationAttribute : InlineAutoDataAttribute
{
    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="values">固定值（將填入測試方法的前幾個參數）</param>
    public InlineAutoDataWithCustomizationAttribute(params object[] values)
        : base(new AutoDataWithCustomizationAttribute(), values)
    {
    }
}

#endregion

#region Mapster Customization

// =============================================================================
// Mapster 對應器客製化
// =============================================================================

/// <summary>
/// Mapster 對應器客製化
/// 提供真實的 IMapper 實例，而非 Mock
/// </summary>
/// <remarks>
/// 為什麼不用 Mock：
/// 1. IMapper 是工具型相依性，不是業務邏輯
/// 2. 測試需要驗證對應邏輯是否正確
/// 3. 為每個對應方法設定 Returns 反而增加複雜度
/// </remarks>
public class MapsterMapperCustomization : ICustomization
{
    private IMapper? _mapper;

    public void Customize(IFixture fixture)
    {
        fixture.Register(() => this.Mapper);
    }

    private IMapper Mapper
    {
        get
        {
            if (this._mapper is not null)
            {
                return this._mapper;
            }

            var typeAdapterConfig = new TypeAdapterConfig();
            
            // 掃描包含對應設定的組件
            // 替換為您專案中的 MapRegister 類別
            typeAdapterConfig.Scan(typeof(ServiceMapRegister).Assembly);
            
            this._mapper = new Mapper(typeAdapterConfig);
            return this._mapper;
        }
    }
}

/// <summary>
/// 範例：Mapster 對應設定
/// </summary>
public class ServiceMapRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 設定 ShipperModel -> ShipperDto 的對應規則
        config.NewConfig<ShipperModel, ShipperDto>();
        
        // 設定 ShipperDto -> ShipperModel 的對應規則
        config.NewConfig<ShipperDto, ShipperModel>();
    }
}

#endregion

#region AutoMapper Customization

// =============================================================================
// AutoMapper 對應器客製化（替代方案）
// =============================================================================

/// <summary>
/// AutoMapper 對應器客製化
/// 適用於使用 AutoMapper 的專案
/// </summary>
public class AutoMapperCustomization : ICustomization
{
    private IMapper? _mapper;

    public void Customize(IFixture fixture)
    {
        fixture.Register<IMapper>(() => this.Mapper);
    }

    private IMapper Mapper
    {
        get
        {
            if (this._mapper is not null)
            {
                return this._mapper;
            }

            var configuration = new MapperConfiguration(cfg =>
            {
                // 掃描包含 Profile 的組件
                cfg.AddMaps(typeof(MappingProfile).Assembly);
            });

            this._mapper = configuration.CreateMapper();
            return this._mapper;
        }
    }
}

/// <summary>
/// 範例：AutoMapper Profile
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ShipperModel, ShipperDto>().ReverseMap();
    }
}

#endregion

#region Domain Model Customization

// =============================================================================
// 領域模型客製化
// =============================================================================

/// <summary>
/// 領域模型客製化
/// 設定特定類型的建立規則
/// </summary>
public class DomainModelCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // 設定 ShipperModel 的建立規則
        fixture.Customize<ShipperModel>(composer => composer
            .With(x => x.ShipperId, () => fixture.Create<int>() % 1000 + 1)
            .With(x => x.CompanyName, () => $"Company_{fixture.Create<string>()[..8]}")
            .With(x => x.Phone, () => $"02-{fixture.Create<int>() % 90000000 + 10000000}"));

        // 設定 OrderModel 的建立規則
        fixture.Customize<OrderModel>(composer => composer
            .With(x => x.OrderId, () => fixture.Create<int>() % 10000 + 1)
            .With(x => x.OrderDate, () => DateTime.Today.AddDays(-fixture.Create<int>() % 365))
            .With(x => x.TotalAmount, () => Math.Round(fixture.Create<decimal>() % 10000, 2)));
    }
}

#endregion

#region 特殊用途的 AutoData 屬性

// =============================================================================
// 特殊用途的 AutoData 屬性
// =============================================================================

/// <summary>
/// 包含 Logger 替身的 AutoData 屬性
/// 自動為 ILogger&lt;T&gt; 建立可驗證的替身
/// </summary>
public class AutoDataWithLoggerAttribute : AutoDataAttribute
{
    public AutoDataWithLoggerAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new LoggerCustomization());

        return fixture;
    }
}

/// <summary>
/// Logger 客製化
/// 提供可驗證的 ILogger 實例
/// </summary>
public class LoggerCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // 註冊 ILoggerFactory
        fixture.Register<ILoggerFactory>(() => NSubstitute.Substitute.For<ILoggerFactory>());
    }
}

/// <summary>
/// 無遞迴的 AutoData 屬性
/// 避免產生包含自我參考的複雜物件圖
/// </summary>
public class AutoDataNoRecursionAttribute : AutoDataAttribute
{
    public AutoDataNoRecursionAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization());
        
        // 設定遞迴行為為拋出異常（而非無限遞迴）
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}

/// <summary>
/// 限制遞迴深度的 AutoData 屬性
/// 適用於有複雜巢狀結構的物件
/// </summary>
public class AutoDataWithRecursionDepthAttribute : AutoDataAttribute
{
    public AutoDataWithRecursionDepthAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization());
        
        // 移除預設的遞迴行為
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        
        // 設定遞迴深度為 1（只產生一層巢狀）
        fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 1));

        return fixture;
    }
}

#endregion

#region 組合多個 AutoData 屬性

// =============================================================================
// 使用 CompositeCustomization 組合多個設定
// =============================================================================

/// <summary>
/// 組合多個 Customization 的輔助類別
/// </summary>
public class ProjectCustomization : CompositeCustomization
{
    public ProjectCustomization()
        : base(
            new AutoNSubstituteCustomization(),
            new MapsterMapperCustomization(),
            new DomainModelCustomization())
    {
    }
}

/// <summary>
/// 使用 CompositeCustomization 的 AutoData 屬性
/// </summary>
public class ProjectAutoDataAttribute : AutoDataAttribute
{
    public ProjectAutoDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        return new Fixture().Customize(new ProjectCustomization());
    }
}

/// <summary>
/// 使用 CompositeCustomization 的 InlineAutoData 屬性
/// </summary>
public class ProjectInlineAutoDataAttribute : InlineAutoDataAttribute
{
    public ProjectInlineAutoDataAttribute(params object[] values)
        : base(new ProjectAutoDataAttribute(), values)
    {
    }
}

#endregion

#region 範例領域模型

// =============================================================================
// 範例領域模型（供上方範例使用）
// =============================================================================

public class ShipperModel
{
    public int ShipperId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class ShipperDto
{
    public int ShipperId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class OrderModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

#endregion
