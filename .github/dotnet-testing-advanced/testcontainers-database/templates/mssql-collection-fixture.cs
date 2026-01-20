// MSSQL Collection Fixture 範本
// 用於多個測試類別共享同一個 SQL Server 容器
// 大幅提升測試執行效率（減少約 67% 的執行時間）

using Testcontainers.MsSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace YourNamespace.Tests.Fixtures;

// ===== Step 1: 建立 Container Fixture =====

/// <summary>
/// SQL Server 容器的 Collection Fixture
/// 用於在多個測試類別間共享同一個容器實例
/// </summary>
/// <remarks>
/// 效能提升：
/// - 傳統方式：每個測試類別啟動一個容器，若有 3 個類別 = 30 秒
/// - Collection Fixture：所有類別共享同一容器 = 10 秒
/// - 執行時間減少約 67%
/// </remarks>
public class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public SqlServerContainerFixture()
    {
        _container = new MsSqlBuilder()
            // 使用最新的 SQL Server 2022 映像
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            // 設定強密碼（SQL Server 密碼要求：大小寫字母、數字、特殊字元）
            .WithPassword("Test123456!")
            // 測試完成後自動清理容器
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// 靜態連線字串，讓所有測試類別都能存取
    /// </summary>
    public static string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// 初始化容器
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // 等待容器完全啟動（SQL Server 需要較長時間）
        await Task.Delay(2000);

        Console.WriteLine($"SQL Server 容器已啟動，連線字串：{ConnectionString}");
    }

    /// <summary>
    /// 清理容器
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        Console.WriteLine("SQL Server 容器已清理");
    }
}

// ===== Step 2: 定義 Collection Definition =====

/// <summary>
/// 定義測試集合，讓多個測試類別可以共享同一個 SqlServerContainerFixture
/// </summary>
/// <remarks>
/// 使用方式：在測試類別上加上 [Collection(nameof(SqlServerCollectionFixture))]
/// </remarks>
[CollectionDefinition(nameof(SqlServerCollectionFixture))]
public class SqlServerCollectionFixture : ICollectionFixture<SqlServerContainerFixture>
{
    // 此類別只是用來定義 Collection，不需要實作內容
}

// ===== Step 3: 測試基底類別（可選，但推薦）=====

/// <summary>
/// EF Core 測試基底類別，提供共用的設定和清理邏輯
/// </summary>
public abstract class EfCoreTestBase : IDisposable
{
    protected readonly ECommerceDbContext DbContext;
    protected readonly ITestOutputHelper TestOutputHelper;

    protected EfCoreTestBase(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        var connectionString = SqlServerContainerFixture.ConnectionString;

        TestOutputHelper.WriteLine($"使用連線字串：{connectionString}");

        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(connectionString)
            // 啟用敏感資料日誌（僅限開發/測試環境）
            .EnableSensitiveDataLogging()
            // 將 SQL 日誌輸出到測試結果
            .LogTo(testOutputHelper.WriteLine, LogLevel.Information)
            .Options;

        DbContext = new ECommerceDbContext(options);
        DbContext.Database.EnsureCreated();
        
        // 確保資料表存在（使用 SQL 腳本外部化策略）
        EnsureTablesExist();
    }

    /// <summary>
    /// 確保資料表存在，使用外部 SQL 腳本建立
    /// </summary>
    protected virtual void EnsureTablesExist()
    {
        var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
        if (!Directory.Exists(scriptDirectory)) return;

        // 按照依賴順序執行表格建立腳本
        var orderedScripts = new[]
        {
            "Tables/CreateCategoriesTable.sql",
            "Tables/CreateTagsTable.sql",
            "Tables/CreateProductsTable.sql",
            "Tables/CreateOrdersTable.sql",
            "Tables/CreateOrderItemsTable.sql",
            "Tables/CreateProductTagsTable.sql"
        };

        foreach (var scriptPath in orderedScripts)
        {
            var fullPath = Path.Combine(scriptDirectory, scriptPath);
            if (File.Exists(fullPath))
            {
                var script = File.ReadAllText(fullPath);
                DbContext.Database.ExecuteSqlRaw(script);
            }
        }
    }

    /// <summary>
    /// 清理測試資料，按照外鍵約束順序執行 DELETE
    /// </summary>
    public virtual void Dispose()
    {
        // 按照外鍵約束順序清理資料，確保測試隔離
        DbContext.Database.ExecuteSqlRaw("DELETE FROM ProductTags");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM OrderItems");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Orders");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Products");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Categories");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Tags");
        DbContext.Dispose();
    }
}

// ===== Step 4: 實際測試類別範例 =====

/// <summary>
/// EF Core CRUD 操作測試
/// </summary>
[Collection(nameof(SqlServerCollectionFixture))]
public class EfCoreCrudTests : EfCoreTestBase
{
    private readonly IProductRepository _productRepository;

    public EfCoreCrudTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _productRepository = new EfCoreProductRepository(DbContext);
    }

    [Fact]
    public async Task AddAsync_WithValidProduct_ShouldPersistToDatabase()
    {
        // Arrange
        await SeedCategoryAsync();
        var category = await DbContext.Categories.FirstAsync();
        var product = new Product
        {
            Name = "測試商品",
            Price = 1500,
            Stock = 25,
            CategoryId = category.Id,
            SKU = "TEST-001",
            IsActive = true
        };

        // Act
        await _productRepository.AddAsync(product);

        // Assert
        product.Id.Should().BeGreaterThan(0);
        var saved = await DbContext.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("測試商品");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        await SeedCategoryAsync();
        var category = await DbContext.Categories.FirstAsync();
        var product = new Product
        {
            Name = "查詢測試商品",
            Price = 999,
            CategoryId = category.Id,
            SKU = "GET-001",
            IsActive = true
        };
        await _productRepository.AddAsync(product);

        // Act
        var result = await _productRepository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("查詢測試商品");
    }

    private async Task SeedCategoryAsync()
    {
        if (!await DbContext.Categories.AnyAsync())
        {
            DbContext.Categories.Add(new Category
            {
                Name = "電子產品",
                Description = "各種電子設備",
                IsActive = true
            });
            await DbContext.SaveChangesAsync();
        }
    }
}

/// <summary>
/// EF Core 進階功能測試
/// </summary>
[Collection(nameof(SqlServerCollectionFixture))]
public class EfCoreAdvancedTests : EfCoreTestBase
{
    private readonly IProductByEFCoreRepository _advancedRepository;

    public EfCoreAdvancedTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _advancedRepository = new EfCoreProductRepository(DbContext);
    }

    [Fact]
    public async Task GetProductWithCategoryAndTagsAsync_ShouldLoadRelatedData()
    {
        // Arrange
        await CreateProductWithCategoryAndTagsAsync();

        // Act
        var product = await _advancedRepository.GetProductWithCategoryAndTagsAsync(1);

        // Assert
        product.Should().NotBeNull();
        product!.Category.Should().NotBeNull();
        product.ProductTags.Should().NotBeEmpty();
        
        TestOutputHelper.WriteLine($"產品：{product.Name}，分類：{product.Category.Name}");
    }

    [Fact]
    public async Task GetProductsWithNoTrackingAsync_ShouldNotTrackEntities()
    {
        // Arrange
        await CreateMultipleProductsAsync();
        var minPrice = 500m;

        // Act
        var products = await _advancedRepository.GetProductsWithNoTrackingAsync(minPrice);

        // Assert
        products.Should().NotBeEmpty();
        
        // 驗證這些實體不被 ChangeTracker 追蹤
        var trackedEntities = DbContext.ChangeTracker.Entries<Product>().Count();
        trackedEntities.Should().Be(0, "AsNoTracking 查詢不應該追蹤實體");
    }

    private async Task CreateProductWithCategoryAndTagsAsync()
    {
        // 實作測試資料建立邏輯
    }

    private async Task CreateMultipleProductsAsync()
    {
        // 實作測試資料建立邏輯
    }
}

// ===== 替換這些類別為您的實際實作 =====

public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options) { }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}

public class Tag { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class ProductTag { public int Id { get; set; } public int ProductId { get; set; } public int TagId { get; set; } }
public class Order { public int Id { get; set; } }
public class OrderItem { public int Id { get; set; } }

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

public interface IProductByEFCoreRepository
{
    Task<Product?> GetProductWithCategoryAndTagsAsync(int productId);
    Task<IEnumerable<Product>> GetProductsWithNoTrackingAsync(decimal minPrice);
}

public class EfCoreProductRepository : IProductRepository, IProductByEFCoreRepository
{
    private readonly ECommerceDbContext _context;
    public EfCoreProductRepository(ECommerceDbContext context) => _context = context;
    
    public async Task<IEnumerable<Product>> GetAllAsync() => await _context.Products.ToListAsync();
    public async Task<Product?> GetByIdAsync(int id) => await _context.Products.FindAsync(id);
    public async Task AddAsync(Product product) { _context.Products.Add(product); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(Product product) { _context.Products.Update(product); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var entity = await GetByIdAsync(id); if (entity != null) { _context.Products.Remove(entity); await _context.SaveChangesAsync(); } }
    
    public async Task<Product?> GetProductWithCategoryAndTagsAsync(int productId) =>
        await _context.Products.Include(p => p.Category).Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == productId);
    
    public async Task<IEnumerable<Product>> GetProductsWithNoTrackingAsync(decimal minPrice) =>
        await _context.Products.AsNoTracking().Where(p => p.Price >= minPrice).ToListAsync();
}
