using Npgsql;
using Respawn;

namespace YourProject.Tests.Integration.Fixtures;

/// <summary>
/// 資料庫管理器 - 負責資料庫初始化與清理
/// </summary>
public class DatabaseManager
{
    private readonly string _connectionString;
    private Respawner? _respawner;
    private bool _isInitialized;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 初始化資料庫結構
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // 確保資料表存在
        await EnsureTablesExistAsync(connection);

        // 初始化 Respawner
        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" },
                TablesToIgnore = new Respawn.Graph.Table[]
                {
                    // 可以忽略不需要清理的資料表，例如：
                    // "schema_migrations"
                }
            });
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 清理資料庫資料（保留結構）
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        if (_respawner == null)
        {
            throw new InvalidOperationException("Respawner 尚未初始化，請先呼叫 InitializeDatabaseAsync");
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// 確保資料表存在，使用外部 SQL 指令碼建立
    /// </summary>
    private async Task EnsureTablesExistAsync(NpgsqlConnection connection)
    {
        var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
        if (!Directory.Exists(scriptDirectory))
        {
            throw new DirectoryNotFoundException($"SQL 指令碼目錄不存在: {scriptDirectory}");
        }

        // 按照依賴順序執行表格建立腳本
        var orderedScripts = new[]
        {
            "Tables/CreateProductsTable.sql"
            // 依需求新增更多資料表
        };

        foreach (var scriptPath in orderedScripts)
        {
            var fullPath = Path.Combine(scriptDirectory, scriptPath);
            if (File.Exists(fullPath))
            {
                var script = await File.ReadAllTextAsync(fullPath);
                await using var command = new NpgsqlCommand(script, connection);
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                throw new FileNotFoundException($"SQL 指令碼檔案不存在: {fullPath}");
            }
        }
    }

    /// <summary>
    /// 執行自訂 SQL 指令碼
    /// </summary>
    /// <param name="sql">SQL 指令碼</param>
    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 執行查詢並傳回結果
    /// </summary>
    public async Task<T?> QuerySingleAsync<T>(string sql, Func<NpgsqlDataReader, T> mapper)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return mapper(reader);
        }

        return default;
    }

    /// <summary>
    /// 新增測試產品資料
    /// </summary>
    public async Task<Guid> SeedProductAsync(string name, decimal price)
    {
        var id = Guid.NewGuid();
        var sql = $@"
            INSERT INTO products (id, name, price, created_at, updated_at)
            VALUES ('{id}', '{name}', {price}, NOW(), NOW())";

        await ExecuteAsync(sql);
        return id;
    }

    /// <summary>
    /// 批次新增測試產品資料
    /// </summary>
    public async Task SeedProductsAsync(int count)
    {
        var tasks = Enumerable.Range(1, count)
            .Select(i => SeedProductAsync($"產品 {i:D2}", i * 10.0m));
        await Task.WhenAll(tasks);
    }
}
