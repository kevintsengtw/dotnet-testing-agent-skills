// =============================================================================
// 服務層測試完整範例
// 展示 AutoFixture + NSubstitute 整合在實際服務測試中的應用
// =============================================================================

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using MapsterMapper;
using Mapster;

namespace MyProject.Tests.Services;

#region 測試目標類別

// =============================================================================
// 測試目標類別 - ShipperService
// =============================================================================

/// <summary>
/// 出貨商服務介面
/// </summary>
public interface IShipperService
{
    Task<bool> IsExistsAsync(int shipperId);
    Task<ShipperDto?> GetAsync(int shipperId);
    Task<IEnumerable<ShipperDto>> GetAllAsync();
    Task<IEnumerable<ShipperDto>> GetCollectionAsync(int from, int size);
    Task<IEnumerable<ShipperDto>> SearchAsync(string? companyName, string? phone);
    Task<IResult> CreateAsync(ShipperDto shipper);
}

/// <summary>
/// 出貨商資料存取介面
/// </summary>
public interface IShipperRepository
{
    Task<bool> IsExistsAsync(int shipperId);
    Task<ShipperModel?> GetAsync(int shipperId);
    Task<IEnumerable<ShipperModel>> GetAllAsync();
    Task<IEnumerable<ShipperModel>> GetCollectionAsync(int from, int size);
    Task<IEnumerable<ShipperModel>> SearchAsync(string companyName, string phone);
    Task<int> GetTotalCountAsync();
    Task<IResult> CreateAsync(ShipperModel model);
}

/// <summary>
/// 出貨商服務實作
/// </summary>
public class ShipperService : IShipperService
{
    private readonly IMapper _mapper;
    private readonly IShipperRepository _shipperRepository;

    public ShipperService(IMapper mapper, IShipperRepository shipperRepository)
    {
        _mapper = mapper;
        _shipperRepository = shipperRepository;
    }

    public async Task<bool> IsExistsAsync(int shipperId)
    {
        if (shipperId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(shipperId), 
                shipperId, 
                "ShipperId must be greater than 0");
        }

        return await _shipperRepository.IsExistsAsync(shipperId);
    }

    public async Task<ShipperDto?> GetAsync(int shipperId)
    {
        if (shipperId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(shipperId), 
                shipperId, 
                "ShipperId must be greater than 0");
        }

        var exists = await _shipperRepository.IsExistsAsync(shipperId);
        if (!exists)
        {
            return null;
        }

        var model = await _shipperRepository.GetAsync(shipperId);
        return _mapper.Map<ShipperDto>(model);
    }

    public async Task<IEnumerable<ShipperDto>> GetAllAsync()
    {
        var models = await _shipperRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ShipperDto>>(models);
    }

    public async Task<IEnumerable<ShipperDto>> GetCollectionAsync(int from, int size)
    {
        if (from <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(from), 
                from, 
                "from must be greater than 0");
        }

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size), 
                size, 
                "size must be greater than 0");
        }

        var models = await _shipperRepository.GetCollectionAsync(from, size);
        return _mapper.Map<IEnumerable<ShipperDto>>(models);
    }

    public async Task<IEnumerable<ShipperDto>> SearchAsync(string? companyName, string? phone)
    {
        if (string.IsNullOrWhiteSpace(companyName) && string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("companyName 與 phone 不可都為空白");
        }

        var totalCount = await _shipperRepository.GetTotalCountAsync();
        if (totalCount == 0)
        {
            return [];
        }

        var models = await _shipperRepository.SearchAsync(
            companyName ?? string.Empty, 
            phone ?? string.Empty);
        
        return _mapper.Map<IEnumerable<ShipperDto>>(models);
    }

    public async Task<IResult> CreateAsync(ShipperDto shipper)
    {
        ArgumentNullException.ThrowIfNull(shipper);
        
        var model = _mapper.Map<ShipperModel>(shipper);
        return await _shipperRepository.CreateAsync(model);
    }
}

#endregion

#region 領域模型與 DTO

// =============================================================================
// 領域模型與 DTO
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

public interface IResult
{
    bool IsSuccess { get; }
    string? ErrorMessage { get; }
}

public class Result : IResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

#endregion

#region AutoFixture 設定

// =============================================================================
// AutoFixture 設定
// =============================================================================

/// <summary>
/// Mapster 對應器客製化
/// </summary>
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
            if (_mapper is not null)
            {
                return _mapper;
            }

            var config = new TypeAdapterConfig();
            config.NewConfig<ShipperModel, ShipperDto>();
            config.NewConfig<ShipperDto, ShipperModel>();
            
            _mapper = new Mapper(config);
            return _mapper;
        }
    }
}

/// <summary>
/// 完整專案客製化的 AutoData 屬性
/// </summary>
public class AutoDataWithCustomizationAttribute : AutoDataAttribute
{
    public AutoDataWithCustomizationAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new MapsterMapperCustomization());
    }
}

/// <summary>
/// 完整專案客製化的 InlineAutoData 屬性
/// </summary>
public class InlineAutoDataWithCustomizationAttribute : InlineAutoDataAttribute
{
    public InlineAutoDataWithCustomizationAttribute(params object[] values)
        : base(new AutoDataWithCustomizationAttribute(), values)
    {
    }
}

/// <summary>
/// CollectionSize 屬性
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class CollectionSizeAttribute : CustomizeAttribute
{
    private readonly int _size;

    public CollectionSizeAttribute(int size)
    {
        _size = size;
    }

    public override ICustomization GetCustomization(System.Reflection.ParameterInfo parameter)
    {
        return new RepeatCountCustomization(_size);
    }

    private class RepeatCountCustomization : ICustomization
    {
        private readonly int _count;

        public RepeatCountCustomization(int count)
        {
            _count = count;
        }

        public void Customize(IFixture fixture)
        {
            fixture.RepeatCount = _count;
        }
    }
}

#endregion

#region 基本測試：參數驗證

// =============================================================================
// 基本測試：參數驗證
// =============================================================================

/// <summary>
/// ShipperService 參數驗證測試
/// 這些測試不需要設定相依性的行為
/// </summary>
public class ShipperServiceParameterValidationTests
{
    /// <summary>
    /// 測試 ShipperId 為 0 時應拋出異常
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_輸入的ShipperId為0時_應拋出ArgumentOutOfRangeException(
        ShipperService sut)
    {
        // Arrange
        var shipperId = 0;

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.IsExistsAsync(shipperId));

        // Assert
        exception.ParamName.Should().Be(nameof(shipperId));
        exception.ActualValue.Should().Be(0);
    }

    /// <summary>
    /// 測試 ShipperId 為負數時應拋出異常
    /// </summary>
    [Theory]
    [InlineAutoDataWithCustomization(-1)]
    [InlineAutoDataWithCustomization(-100)]
    [InlineAutoDataWithCustomization(int.MinValue)]
    public async Task IsExistsAsync_輸入的ShipperId為負數時_應拋出ArgumentOutOfRangeException(
        int shipperId,
        ShipperService sut)
    {
        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.IsExistsAsync(shipperId));

        // Assert
        exception.ParamName.Should().Be(nameof(shipperId));
    }

    /// <summary>
    /// 測試分頁參數驗證
    /// </summary>
    [Theory]
    [InlineAutoDataWithCustomization(0, 10, nameof(from))]
    [InlineAutoDataWithCustomization(-1, 10, nameof(from))]
    [InlineAutoDataWithCustomization(1, 0, nameof(size))]
    [InlineAutoDataWithCustomization(1, -1, nameof(size))]
    public async Task GetCollectionAsync_from與size輸入不合規格內容_應拋出ArgumentOutOfRangeException(
        int from,
        int size,
        string parameterName,
        ShipperService sut)
    {
        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.GetCollectionAsync(from, size));

        // Assert
        exception.ParamName.Should().Be(parameterName);
    }

    /// <summary>
    /// 測試搜尋參數驗證：兩個參數都為空
    /// </summary>
    [Theory]
    [InlineAutoDataWithCustomization(null!, null!)]
    [InlineAutoDataWithCustomization("", "")]
    [InlineAutoDataWithCustomization("   ", "   ")]
    public async Task SearchAsync_companyName與phone都為空白_應拋出ArgumentException(
        string? companyName,
        string? phone,
        ShipperService sut)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.SearchAsync(companyName, phone));

        exception.Message.Should().Contain("companyName 與 phone 不可都為空白");
    }

    /// <summary>
    /// 測試建立時傳入 null
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task CreateAsync_輸入null_應拋出ArgumentNullException(
        ShipperService sut)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.CreateAsync(null!));
    }
}

#endregion

#region 進階測試：設定相依行為

// =============================================================================
// 進階測試：設定相依行為
// =============================================================================

/// <summary>
/// ShipperService 業務邏輯測試
/// 這些測試需要設定 Repository 的行為
/// </summary>
public class ShipperServiceBusinessLogicTests
{
    /// <summary>
    /// 測試資料不存在時回傳 false
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_輸入的ShipperId_資料不存在_應回傳false(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut)
    {
        // Arrange
        var shipperId = 99;
        shipperRepository.IsExistsAsync(shipperId).Returns(false);

        // Act
        var actual = await sut.IsExistsAsync(shipperId);

        // Assert
        actual.Should().BeFalse();
    }

    /// <summary>
    /// 測試資料存在時回傳 true
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_輸入的ShipperId_資料存在_應回傳true(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut)
    {
        // Arrange
        var shipperId = 1;
        shipperRepository.IsExistsAsync(shipperId).Returns(true);

        // Act
        var actual = await sut.IsExistsAsync(shipperId);

        // Assert
        actual.Should().BeTrue();
        
        // 驗證 Repository 方法被呼叫
        await shipperRepository.Received(1).IsExistsAsync(shipperId);
    }

    /// <summary>
    /// 測試取得不存在的資料時回傳 null
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAsync_輸入的ShipperId_資料不存在_應回傳null(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut)
    {
        // Arrange
        var shipperId = 99;
        shipperRepository.IsExistsAsync(shipperId).Returns(false);

        // Act
        var actual = await sut.GetAsync(shipperId);

        // Assert
        actual.Should().BeNull();
        
        // 驗證不應該呼叫 GetAsync（因為資料不存在）
        await shipperRepository.DidNotReceive().GetAsync(Arg.Any<int>());
    }

    /// <summary>
    /// 測試取得存在的資料時回傳正確結果
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAsync_輸入的ShipperId_資料有存在_應回傳model(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut,
        ShipperModel model)  // AutoFixture 自動產生
    {
        // Arrange
        var shipperId = model.ShipperId;
        shipperRepository.IsExistsAsync(shipperId).Returns(true);
        shipperRepository.GetAsync(shipperId).Returns(model);

        // Act
        var actual = await sut.GetAsync(shipperId);

        // Assert
        actual.Should().NotBeNull();
        actual!.ShipperId.Should().Be(shipperId);
        actual.CompanyName.Should().Be(model.CompanyName);
        actual.Phone.Should().Be(model.Phone);
    }

    /// <summary>
    /// 測試取得所有資料
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAllAsync_資料表裡有資料_應回傳所有資料(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut,
        List<ShipperModel> models)  // AutoFixture 預設產生 3 筆
    {
        // Arrange
        shipperRepository.GetAllAsync().Returns(models);

        // Act
        var actual = await sut.GetAllAsync();

        // Assert
        actual.Should().HaveCount(models.Count);
    }

    /// <summary>
    /// 測試取得指定數量的資料
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAllAsync_資料表裡有10筆資料_回傳的集合裡有10筆(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut,
        [CollectionSize(10)] IEnumerable<ShipperModel> models)
    {
        // Arrange
        shipperRepository.GetAllAsync().Returns(models);

        // Act
        var actual = await sut.GetAllAsync();

        // Assert
        actual.Should().NotBeEmpty();
        actual.Should().HaveCount(10);
    }

    /// <summary>
    /// 測試分頁取得資料
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task GetCollectionAsync_指定from和size_應回傳正確數量的資料(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut,
        [CollectionSize(5)] IEnumerable<ShipperModel> models)
    {
        // Arrange
        const int from = 1;
        const int size = 5;
        shipperRepository.GetCollectionAsync(from, size).Returns(models);

        // Act
        var actual = await sut.GetCollectionAsync(from, size);

        // Assert
        actual.Should().HaveCount(5);
        
        // 驗證傳入正確的參數
        await shipperRepository.Received(1).GetCollectionAsync(from, size);
    }
}

#endregion

#region 複雜資料設定測試

// =============================================================================
// 複雜資料設定測試
// =============================================================================

/// <summary>
/// 使用 IFixture 進行複雜資料設定的測試
/// </summary>
public class ShipperServiceComplexDataTests
{
    /// <summary>
    /// 測試搜尋功能：只輸入公司名稱
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task SearchAsync_companyName輸入資料_phone無輸入_有符合條件的資料_回傳集合應包含符合條件的資料(
        IFixture fixture,
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut)
    {
        // Arrange
        const string companyName = "TestCompany";
        
        // 使用 fixture 精確控制資料
        var models = fixture.Build<ShipperModel>()
            .With(x => x.CompanyName, companyName)
            .CreateMany(1);

        shipperRepository.GetTotalCountAsync().Returns(1);
        shipperRepository.SearchAsync(companyName, string.Empty).Returns(models);

        // Act
        var actual = await sut.SearchAsync(companyName, string.Empty);

        // Assert
        actual.Should().NotBeEmpty();
        actual.Should().HaveCount(1);
        actual.All(x => x.CompanyName == companyName).Should().BeTrue();
    }

    /// <summary>
    /// 測試搜尋功能：資料表為空
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task SearchAsync_資料表為空_應回傳空集合(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut)
    {
        // Arrange
        shipperRepository.GetTotalCountAsync().Returns(0);

        // Act
        var actual = await sut.SearchAsync("test", null);

        // Assert
        actual.Should().BeEmpty();
        
        // 驗證不應該呼叫 SearchAsync
        await shipperRepository.DidNotReceive()
            .SearchAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    /// <summary>
    /// 測試建立功能
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task CreateAsync_輸入有效的ShipperDto_應回傳成功結果(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut,
        ShipperDto dto)
    {
        // Arrange
        var successResult = Result.Success();
        shipperRepository.CreateAsync(Arg.Any<ShipperModel>()).Returns(successResult);

        // Act
        var actual = await sut.CreateAsync(dto);

        // Assert
        actual.IsSuccess.Should().BeTrue();
        
        // 驗證 Repository 被呼叫，且傳入的 Model 屬性正確
        await shipperRepository.Received(1).CreateAsync(Arg.Is<ShipperModel>(m =>
            m.ShipperId == dto.ShipperId &&
            m.CompanyName == dto.CompanyName &&
            m.Phone == dto.Phone));
    }

    /// <summary>
    /// 測試建立功能：Repository 回傳失敗
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task CreateAsync_Repository回傳失敗_應回傳失敗結果(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut,
        ShipperDto dto)
    {
        // Arrange
        var failureResult = Result.Failure("Database error");
        shipperRepository.CreateAsync(Arg.Any<ShipperModel>()).Returns(failureResult);

        // Act
        var actual = await sut.CreateAsync(dto);

        // Assert
        actual.IsSuccess.Should().BeFalse();
        actual.ErrorMessage.Should().Be("Database error");
    }
}

#endregion

#region 例外處理測試

// =============================================================================
// 例外處理測試
// =============================================================================

/// <summary>
/// Repository 例外處理測試
/// </summary>
public class ShipperServiceExceptionHandlingTests
{
    /// <summary>
    /// 測試 Repository 拋出例外時的處理
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_Repository拋出例外_應傳播例外(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut)
    {
        // Arrange
        var shipperId = 1;
        shipperRepository.IsExistsAsync(shipperId)
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.IsExistsAsync(shipperId));

        exception.Message.Should().Contain("Database connection failed");
    }

    /// <summary>
    /// 測試並發呼叫
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAsync_並發呼叫多次_應正確處理(
        [Frozen] IShipperRepository shipperRepository,
        ShipperService sut,
        ShipperModel model)
    {
        // Arrange
        var shipperId = model.ShipperId;
        shipperRepository.IsExistsAsync(shipperId).Returns(true);
        shipperRepository.GetAsync(shipperId).Returns(model);

        // Act - 並發呼叫 5 次
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => sut.GetAsync(shipperId));

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r =>
        {
            r.Should().NotBeNull();
            r!.ShipperId.Should().Be(shipperId);
        });
    }
}

#endregion

#region 整合測試範例

// =============================================================================
// 整合測試範例：展示完整測試類別結構
// =============================================================================

/// <summary>
/// ShipperService 完整測試類別
/// 展示如何組織一個服務的所有測試
/// </summary>
public class ShipperServiceTests
{
    #region IsExistsAsync 測試

    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_有效ShipperId且資料存在_回傳True(
        [Frozen] IShipperRepository repo,
        ShipperService sut)
    {
        repo.IsExistsAsync(1).Returns(true);
        (await sut.IsExistsAsync(1)).Should().BeTrue();
    }

    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_有效ShipperId且資料不存在_回傳False(
        [Frozen] IShipperRepository repo,
        ShipperService sut)
    {
        repo.IsExistsAsync(99).Returns(false);
        (await sut.IsExistsAsync(99)).Should().BeFalse();
    }

    [Theory]
    [InlineAutoDataWithCustomization(0)]
    [InlineAutoDataWithCustomization(-1)]
    public async Task IsExistsAsync_無效ShipperId_拋出ArgumentOutOfRangeException(
        int invalidId,
        ShipperService sut)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.IsExistsAsync(invalidId));
    }

    #endregion

    #region GetAsync 測試

    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAsync_資料存在_回傳正確DTO(
        [Frozen] IShipperRepository repo,
        ShipperService sut,
        ShipperModel model)
    {
        repo.IsExistsAsync(model.ShipperId).Returns(true);
        repo.GetAsync(model.ShipperId).Returns(model);

        var result = await sut.GetAsync(model.ShipperId);

        result.Should().BeEquivalentTo(model);
    }

    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAsync_資料不存在_回傳Null(
        [Frozen] IShipperRepository repo,
        ShipperService sut)
    {
        repo.IsExistsAsync(Arg.Any<int>()).Returns(false);

        var result = await sut.GetAsync(99);

        result.Should().BeNull();
    }

    #endregion

    #region SearchAsync 測試

    [Theory]
    [AutoDataWithCustomization]
    public async Task SearchAsync_有資料符合條件_回傳符合的資料(
        IFixture fixture,
        [Frozen] IShipperRepository repo,
        ShipperService sut)
    {
        var models = fixture.Build<ShipperModel>()
            .With(x => x.CompanyName, "Target")
            .CreateMany(2);

        repo.GetTotalCountAsync().Returns(2);
        repo.SearchAsync("Target", "").Returns(models);

        var result = await sut.SearchAsync("Target", null);

        result.Should().HaveCount(2);
    }

    [Theory]
    [InlineAutoDataWithCustomization(null!, null!)]
    [InlineAutoDataWithCustomization("", "")]
    public async Task SearchAsync_參數都為空_拋出ArgumentException(
        string? name,
        string? phone,
        ShipperService sut)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.SearchAsync(name, phone));
    }

    #endregion
}

#endregion
