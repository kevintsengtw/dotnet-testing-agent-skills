using System.Data;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// 資料庫管理員 - 使用 Aspire 提供的連線字串
/// 負責資料庫初始化、結構建立和測試資料清理
/// </summary>
public class DatabaseManager
{
    private readonly Func<Task<string>> _getConnectionStringAsync;
    private Respawner? _respawner;

    public DatabaseManager(Func<Task<string>> getConnectionStringAsync)
    {
        _getConnectionStringAsync = getConnectionStringAsync;
    }

    /// <summary>
    /// 初始化資料庫結構
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        var connectionString = await _getConnectionStringAsync();

        // 首先確保資料庫存在
        await EnsureDatabaseExistsAsync(connectionString);

        // 連線到資料庫並確保資料表存在
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // 確保資料表存在
        await EnsureTablesExistAsync(connection);

        // 初始化 Respawner - 關鍵：指定 PostgreSQL 適配器
        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
                SchemasToInclude = new[] { "public" },
                DbAdapter = DbAdapter.Postgres  // 重要：明確指定 PostgreSQL 適配器
            });
        }
    }

    /// <summary>
    /// 清理測試資料
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        if (_respawner == null) return;

        var connectionString = await _getConnectionStringAsync();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// 取得連線字串
    /// </summary>
    public async Task<string> GetConnectionStringAsync()
    {
        return await _getConnectionStringAsync();
    }

    /// <summary>
    /// 確保資料庫存在 - Aspire 會啟動容器但不會自動建立資料庫
    /// </summary>
    private async Task EnsureDatabaseExistsAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        // 連線到 postgres 預設資料庫檢查並創建目標資料庫
        builder.Database = "postgres";
        var masterConnectionString = builder.ToString();

        await using var connection = new NpgsqlConnection(masterConnectionString);
        await WaitForDatabaseConnectionAsync(connection);

        // 檢查資料庫是否已存在
        var checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
        await using var checkCommand = new NpgsqlCommand(checkDbQuery, connection);
        var dbExists = await checkCommand.ExecuteScalarAsync();

        if (dbExists == null)
        {
            // 創建資料庫
            var createDbQuery = $"CREATE DATABASE \"{databaseName}\"";
            await using var createCommand = new NpgsqlCommand(createDbQuery, connection);
            await createCommand.ExecuteNonQueryAsync();
            Console.WriteLine($"已建立資料庫: {databaseName}");
        }
    }

    /// <summary>
    /// 等待資料庫連線就緒的重試機制
    /// </summary>
    private async Task WaitForDatabaseConnectionAsync(NpgsqlConnection connection)
    {
        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await connection.OpenAsync();
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"資料庫連線嘗試 {i + 1} 失敗: {ex.Message}");
                await Task.Delay(delayMs);

                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        await connection.OpenAsync();
    }

    /// <summary>
    /// 確保必要的資料表存在
    /// 可從外部 SQL 檔案載入或直接定義
    /// </summary>
    private async Task EnsureTablesExistAsync(NpgsqlConnection connection)
    {
        var createProductTableSql = """
            CREATE TABLE IF NOT EXISTS products (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(100) NOT NULL,
                price DECIMAL(10,2) NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            """;

        await using var command = new NpgsqlCommand(createProductTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
