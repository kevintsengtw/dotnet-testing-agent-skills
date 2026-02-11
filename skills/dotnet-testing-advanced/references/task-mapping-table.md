# 常見任務映射表

### 任務 1：測試 ASP.NET Core Web API（基礎）

**情境**：為簡單的 ProductsController 建立整合測試

**推薦技能**：
- `dotnet-testing-advanced-aspnet-integration-testing`

**適用條件**：
- 簡單的 CRUD API
- 不需要真實資料庫
- 測試基本的 HTTP 端點

**實施步驟**：
1. 建立 CustomWebApplicationFactory
2. 設定記憶體資料庫
3. 撰寫 GET、POST 測試
4. 使用 FluentAssertions.Web 驗證回應

**提示詞範例**：
```
請使用 dotnet-testing-advanced-aspnet-integration-testing skill
為我的 ProductsController 建立整合測試。Controller 有 GetAll 和 GetById 兩個端點。
```

**預期程式碼結構**：
```csharp
public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.Should().Be200Ok();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeEmpty();
    }
}
```

---

### 任務 2：測試 ASP.NET Core Web API（完整流程）

**情境**：為 ShippersController 建立完整的 CRUD 測試

**推薦技能**：
- `dotnet-testing-advanced-webapi-integration-testing`

**適用條件**：
- 完整的 CRUD API
- 需要測試錯誤處理
- 需要測試資料準備與清理

**實施步驟**：
1. 建立測試基底類別（BaseIntegrationTest）
2. 實作 IAsyncLifetime 進行資料準備/清理
3. 測試所有 CRUD 端點
4. 驗證錯誤處理（404、400、409 等）

**提示詞範例**：
```
請使用 dotnet-testing-advanced-webapi-integration-testing skill
為我的 ShippersController 建立完整的 CRUD 測試。需要測試：
- GET /api/shippers（取得所有）
- GET /api/shippers/{id}（取得單一）
- POST /api/shippers（新增）
- PUT /api/shippers/{id}（更新）
- DELETE /api/shippers/{id}（刪除）
並且驗證錯誤情境（如找不到資源）。
```

**預期測試涵蓋**：
- ✅ GET 成功回傳資料
- ✅ GET 不存在的 ID 回傳 404
- ✅ POST 新增成功
- ✅ POST 無效資料回傳 400
- ✅ PUT 更新成功
- ✅ PUT 不存在的 ID 回傳 404
- ✅ DELETE 刪除成功
- ✅ DELETE 不存在的 ID 回傳 404

---

### 任務 3：測試需要真實資料庫的程式碼（SQL）

**情境**：測試 OrderRepository（使用 SQL Server）

**推薦技能**：
- `dotnet-testing-advanced-testcontainers-database`

**適用條件**：
- 使用 EF Core 或 Dapper
- 需要測試真實資料庫行為
- 需要測試資料庫特定功能

**實施步驟**：
1. 設定 Testcontainers.MsSql
2. 執行資料庫遷移
3. 測試 Repository 方法
4. 每個測試後清理資料

**提示詞範例**：
```
請使用 dotnet-testing-advanced-testcontainers-database skill
為我的 OrderRepository 建立測試。Repository 使用 EF Core 連接 SQL Server。
需要測試 GetById、Create、Update、Delete 方法。
```

**預期程式碼結構**：
```csharp
public class OrderRepositoryTests : IAsyncLifetime
{
    private MsSqlContainer _container;
    private OrderDbContext _context;
    private OrderRepository _sut;

    public async Task InitializeAsync()
    {
        // 啟動 SQL Server 容器
        _container = new MsSqlBuilder().Build();
        await _container.StartAsync();

        // 建立 DbContext
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        _context = new OrderDbContext(options);
        await _context.Database.MigrateAsync();

        _sut = new OrderRepository(_context);
    }

    [Fact]
    public async Task GetById_ExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = new Order { /* ... */ };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetById(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

---

### 任務 4：測試 NoSQL 資料庫（MongoDB、Redis）

**情境**：測試 CacheService（使用 Redis）

**推薦技能**：
- `dotnet-testing-advanced-testcontainers-nosql`

**適用條件**：
- 使用 MongoDB、Redis、Elasticsearch
- 需要測試 NoSQL 特定功能

**實施步驟**：
1. 設定 Testcontainers.Redis
2. 測試快取邏輯
3. 驗證過期時間
4. 測試快取失效

**提示詞範例**：
```
請使用 dotnet-testing-advanced-testcontainers-nosql skill
為我的 CacheService 建立測試。Service 使用 Redis 做快取。
需要測試 Set、Get、Remove 以及過期時間。
```

**預期測試涵蓋**：
- ✅ Set 成功儲存資料
- ✅ Get 成功取得資料
- ✅ Get 不存在的鍵回傳 null
- ✅ 過期時間正確運作
- ✅ Remove 成功移除資料

---

### 任務 5：測試微服務架構（.NET Aspire）

**情境**：測試 .NET Aspire 微服務專案

**推薦技能**：
- `dotnet-testing-advanced-aspire-testing`

**適用條件**：
- 使用 .NET Aspire
- 多服務協作
- 分散式應用

**實施步驟**：
1. 建立 DistributedApplication 測試
2. 設定服務依賴
3. 測試服務間通訊
4. 驗證完整流程

**提示詞範例**：
```
請使用 dotnet-testing-advanced-aspire-testing skill
為我的 .NET Aspire 專案建立測試。專案包含 API Service 和 Worker Service。
需要測試兩個服務的協作。
```

---

### 任務 6：升級 xUnit 到 3.x

**情境**：現有專案使用 xUnit 2.9.x，想升級到 3.x

**推薦技能**：
- `dotnet-testing-advanced-xunit-upgrade-guide`

**實施步驟**：
1. 了解重大變更
2. 更新套件版本
3. 處理相容性問題
4. 驗證測試執行

**提示詞範例**：
```
請使用 dotnet-testing-advanced-xunit-upgrade-guide skill
協助我升級專案中的 xUnit 到 3.x 版本。目前使用 2.9.2。
```

---

### 任務 7：評估是否遷移到 TUnit

**情境**：考慮從 xUnit 遷移到 TUnit

**推薦技能**：
1. `dotnet-testing-advanced-tunit-fundamentals`（了解基礎）
2. `dotnet-testing-advanced-tunit-advanced`（評估進階功能）

**實施步驟**：
1. 了解 TUnit 與 xUnit 差異
2. 評估遷移成本
3. 試驗性遷移一個測試檔案
4. 決定是否全面遷移

**提示詞範例**：
```
請使用 dotnet-testing-advanced-tunit-fundamentals skill
評估是否應將專案從 xUnit 遷移到 TUnit。專案目前有 500+ 測試。
```
