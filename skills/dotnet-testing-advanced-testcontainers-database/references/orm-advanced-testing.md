# ORM 進階功能測試

> 本文件從 SKILL.md 提取，包含 EF Core 與 Dapper 的進階測試範例。

## EF Core 進階功能測試

### Include/ThenInclude 多層關聯查詢

```csharp
[Fact]
public async Task GetProductWithCategoryAndTagsAsync_載入完整關聯資料_應正確載入()
{
    // Arrange
    await CreateProductWithCategoryAndTagsAsync();

    // Act
    var product = await _repository.GetProductWithCategoryAndTagsAsync(1);

    // Assert
    product.Should().NotBeNull();
    product!.Category.Should().NotBeNull();
    product.ProductTags.Should().NotBeEmpty();
}
```

### AsSplitQuery 避免笛卡兒積

```csharp
[Fact]
public async Task GetProductsByCategoryWithSplitQueryAsync_使用分割查詢_應避免笛卡兒積()
{
    // Arrange
    await CreateMultipleProductsWithTagsAsync();

    // Act
    var products = await _repository.GetProductsByCategoryWithSplitQueryAsync(1);

    // Assert
    products.Should().NotBeEmpty();
    products.All(p => p.ProductTags.Any()).Should().BeTrue();
}
```

> **笛卡兒積問題**：當一個查詢 JOIN 多個一對多關聯時，會為每個可能的組合產生一列資料。`AsSplitQuery()` 將查詢分解成多個獨立 SQL 查詢，在記憶體中組合結果，避免此問題。

### N+1 查詢問題驗證

```csharp
[Fact]
public async Task N1QueryProblemVerification_對比Repository方法_應展示效率差異()
{
    // Arrange
    await CreateCategoriesWithProductsAsync();

    // Act 1: 測試有問題的方法
    var categoriesWithProblem = await _repository.GetCategoriesWithN1ProblemAsync();

    // Act 2: 測試最佳化方法
    var categoriesOptimized = await _repository.GetCategoriesWithProductsOptimizedAsync();

    // Assert
    categoriesOptimized.All(c => c.Products.Any()).Should().BeTrue();
}
```

### AsNoTracking 唯讀查詢最佳化

```csharp
[Fact]
public async Task GetProductsWithNoTrackingAsync_唯讀查詢_不應追蹤實體()
{
    // Arrange
    await CreateMultipleProductsAsync();

    // Act
    var products = await _repository.GetProductsWithNoTrackingAsync(500m);

    // Assert
    products.Should().NotBeEmpty();
    var trackedEntities = _dbContext.ChangeTracker.Entries<Product>().Count();
    trackedEntities.Should().Be(0, "AsNoTracking 查詢不應追蹤實體");
}
```

## Dapper 進階功能測試

### 基本 CRUD 測試

```csharp
[Collection(nameof(SqlServerCollectionFixture))]
public class DapperCrudTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IProductRepository _productRepository;

    public DapperCrudTests()
    {
        var connectionString = SqlServerContainerFixture.ConnectionString;
        _connection = new SqlConnection(connectionString);
        _connection.Open();

        _productRepository = new DapperProductRepository(connectionString);
        EnsureTablesExist();
    }

    public void Dispose()
    {
        _connection.Execute("DELETE FROM Products");
        _connection.Execute("DELETE FROM Categories");
        _connection.Close();
        _connection.Dispose();
    }
}
```

### QueryMultiple 一對多關聯處理

```csharp
public async Task<Product?> GetProductWithTagsAsync(int productId)
{
    const string sql = @"
        SELECT * FROM Products WHERE Id = @ProductId;
        SELECT t.* FROM Tags t
        INNER JOIN ProductTags pt ON t.Id = pt.TagId
        WHERE pt.ProductId = @ProductId;";

    using var multi = await _connection.QueryMultipleAsync(sql, new { ProductId = productId });
    var product = await multi.ReadSingleOrDefaultAsync<Product>();
    if (product != null)
    {
        product.Tags = (await multi.ReadAsync<Tag>()).ToList();
    }
    return product;
}
```

### DynamicParameters 動態查詢

```csharp
public async Task<IEnumerable<Product>> SearchProductsAsync(
    int? categoryId = null,
    decimal? minPrice = null,
    bool? isActive = null)
{
    var sql = new StringBuilder("SELECT * FROM Products WHERE 1=1");
    var parameters = new DynamicParameters();

    if (categoryId.HasValue)
    {
        sql.Append(" AND CategoryId = @CategoryId");
        parameters.Add("CategoryId", categoryId.Value);
    }

    if (minPrice.HasValue)
    {
        sql.Append(" AND Price >= @MinPrice");
        parameters.Add("MinPrice", minPrice.Value);
    }

    if (isActive.HasValue)
    {
        sql.Append(" AND IsActive = @IsActive");
        parameters.Add("IsActive", isActive.Value);
    }

    return await _connection.QueryAsync<Product>(sql.ToString(), parameters);
}
```
