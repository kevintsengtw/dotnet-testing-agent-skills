# Redis 容器化測試

> 本文件從 [SKILL.md](../SKILL.md) 擷取，包含 Redis 容器化測試的完整程式碼範例與詳細說明。

---

### Redis Container Fixture

```csharp
using StackExchange.Redis;
using Testcontainers.Redis;

namespace YourProject.Integration.Tests.Fixtures;

/// <summary>
/// Redis 容器 Fixture - 管理 Redis 容器生命週期
/// </summary>
public class RedisContainerFixture : IAsyncLifetime
{
    private RedisContainer? _container;

    public IConnectionMultiplexer Connection { get; private set; } = null!;
    public IDatabase Database { get; private set; } = null!;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // 使用 Redis 7.2 版本
        _container = new RedisBuilder()
                     .WithImage("redis:7.2")
                     .WithPortBinding(6379, true)
                     .Build();

        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();
        Connection = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
        Database = Connection.GetDatabase();
    }

    public async Task DisposeAsync()
    {
        if (Connection != null)
        {
            await Connection.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// 清空資料庫 - 使用 KeyDelete 而非 FLUSHDB（避免權限問題）
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var server = Connection.GetServer(Connection.GetEndPoints().First());
        var keys = server.Keys(Database.Database);
        if (keys.Any())
        {
            await Database.KeyDeleteAsync(keys.ToArray());
        }
    }
}

[CollectionDefinition("Redis Collection")]
public class RedisCollectionFixture : ICollectionFixture<RedisContainerFixture>
{
}
```

### Redis 快取模型

```csharp
using System.Text.Json.Serialization;

namespace YourProject.Core.Models.Redis;

/// <summary>
/// 通用快取包裝器 - 提供豐富的快取元資料
/// </summary>
public class CacheItem<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("access_count")]
    public int AccessCount { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonIgnore]
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    [JsonIgnore]
    public double TtlSeconds => ExpiresAt.HasValue
        ? Math.Max(0, ExpiresAt.Value.Subtract(DateTime.UtcNow).TotalSeconds)
        : -1;

    public static CacheItem<T> Create(string key, T data, TimeSpan? ttl = null, params string[] tags)
    {
        return new CacheItem<T>
        {
            Key = key,
            Data = data,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null,
            Tags = tags.ToList()
        };
    }
}

/// <summary>
/// 使用者 Session - Hash 結構範例
/// </summary>
public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// 最近瀏覽紀錄 - List 結構範例
/// </summary>
public class RecentView
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// 排行榜項目 - Sorted Set 結構範例
/// </summary>
public class LeaderboardEntry
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public double Score { get; set; }
}
```

### Redis 五種資料結構測試

```csharp
using StackExchange.Redis;
using AwesomeAssertions;

namespace YourProject.Integration.Tests.Redis;

[Collection("Redis Collection")]
public class RedisCacheServiceTests
{
    private readonly RedisCacheService _redisCacheService;
    private readonly RedisContainerFixture _fixture;

    public RedisCacheServiceTests(RedisContainerFixture fixture)
    {
        _fixture = fixture;
        _redisCacheService = new RedisCacheService(
            fixture.Connection,
            Options.Create(new RedisSettings()),
            NullLogger<RedisCacheService>.Instance,
            TimeProvider.System);
    }

    #region String 測試

    [Fact]
    public async Task SetStringAsync_輸入字串值_應成功設定快取()
    {
        // Arrange
        var key = $"test_string_{Guid.NewGuid():N}";
        var value = "test_string_value";

        // Act
        var result = await _redisCacheService.SetStringAsync(key, value);

        // Assert
        result.Should().BeTrue();
        var retrieved = await _redisCacheService.GetStringAsync<string>(key);
        retrieved.Should().Be(value);
    }

    [Fact]
    public async Task SetObjectCacheAsync_輸入物件_應成功序列化並快取()
    {
        // Arrange
        var key = $"object_test_{Guid.NewGuid():N}";
        var user = new UserDocument
        {
            Username = "objecttest",
            Email = "object@test.com",
            Profile = new UserProfile { FirstName = "Object", LastName = "Test" }
        };

        // Act
        var result = await _redisCacheService.SetStringAsync(key, user, TimeSpan.FromMinutes(30));

        // Assert
        result.Should().BeTrue();
        var retrieved = await _redisCacheService.GetStringAsync<UserDocument>(key);
        retrieved.Should().NotBeNull();
        retrieved!.Username.Should().Be("objecttest");
    }

    [Fact]
    public async Task SetMultipleStringAsync_輸入多個鍵值對_應成功批次設定()
    {
        // Arrange
        var prefix = Guid.NewGuid().ToString("N")[..8];
        var keyValues = new Dictionary<string, string>
        {
            { $"multi1_{prefix}", "value1" },
            { $"multi2_{prefix}", "value2" },
            { $"multi3_{prefix}", "value3" }
        };

        // Act
        var result = await _redisCacheService.SetMultipleStringAsync(keyValues);

        // Assert
        result.Should().BeTrue();
        foreach (var kvp in keyValues)
        {
            var value = await _redisCacheService.GetStringAsync<string>(kvp.Key);
            value.Should().Be(kvp.Value);
        }
    }

    #endregion

    #region Hash 測試

    [Fact]
    public async Task SetHashAsync_輸入字串值_應設定Hash欄位()
    {
        // Arrange
        var key = $"hash_test_{Guid.NewGuid():N}";
        var field = "test_field";
        var value = "test_value";

        // Act
        var result = await _redisCacheService.SetHashAsync(key, field, value, TimeSpan.FromMinutes(30));

        // Assert
        result.Should().BeTrue();
        var retrieved = await _redisCacheService.GetHashAsync<string>(key, field);
        retrieved.Should().Be(value);
    }

    [Fact]
    public async Task SetHashAllAsync_輸入物件_應設定完整Hash()
    {
        // Arrange
        var key = $"hash_all_{Guid.NewGuid():N}";
        var session = new UserSession
        {
            UserId = "user123",
            SessionId = "session456",
            IpAddress = "192.168.1.1",
            UserAgent = "Test Browser",
            IsActive = true
        };

        // Act
        var result = await _redisCacheService.SetHashAllAsync(key, session, TimeSpan.FromHours(1));

        // Assert
        result.Should().BeTrue();
        var retrieved = await _redisCacheService.GetHashAllAsync<UserSession>(key);
        retrieved.Should().NotBeNull();
        retrieved!.UserId.Should().Be("user123");
        retrieved.SessionId.Should().Be("session456");
    }

    #endregion

    #region List 測試

    [Fact]
    public async Task ListLeftPushAsync_輸入值_應新增到List左側()
    {
        // Arrange
        var key = $"list_test_{Guid.NewGuid():N}";
        var view1 = new RecentView { ItemId = "item1", ItemType = "product", Title = "Product 1" };
        var view2 = new RecentView { ItemId = "item2", ItemType = "product", Title = "Product 2" };

        // Act
        var count1 = await _redisCacheService.ListLeftPushAsync(key, view1);
        var count2 = await _redisCacheService.ListLeftPushAsync(key, view2);

        // Assert
        count1.Should().Be(1);
        count2.Should().Be(2);

        var views = await _redisCacheService.ListRangeAsync<RecentView>(key);
        views.Should().HaveCount(2);
        views[0].ItemId.Should().Be("item2"); // 最後加入的在最前面
        views[1].ItemId.Should().Be("item1");
    }

    #endregion

    #region Set 測試

    [Fact]
    public async Task SetAddAsync_輸入值_應新增到Set()
    {
        // Arrange
        var key = $"set_test_{Guid.NewGuid():N}";
        var tag1 = "programming";
        var tag2 = "testing";
        var tag3 = "programming"; // 重複

        // Act
        var result1 = await _redisCacheService.SetAddAsync(key, tag1);
        var result2 = await _redisCacheService.SetAddAsync(key, tag2);
        var result3 = await _redisCacheService.SetAddAsync(key, tag3);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse(); // 重複項目回傳 false

        var tags = await _redisCacheService.SetMembersAsync<string>(key);
        tags.Should().HaveCount(2);
        tags.Should().Contain("programming");
        tags.Should().Contain("testing");
    }

    #endregion

    #region Sorted Set 測試

    [Fact]
    public async Task SortedSetAddAsync_輸入分數和成員_應成功新增到排序集合()
    {
        // Arrange
        var key = $"sorted_set_{Guid.NewGuid():N}";
        var entry1 = new LeaderboardEntry { UserId = "user1", Username = "Player1", Score = 100 };
        var entry2 = new LeaderboardEntry { UserId = "user2", Username = "Player2", Score = 200 };

        // Act
        var result1 = await _redisCacheService.SortedSetAddAsync(key, entry1, entry1.Score);
        var result2 = await _redisCacheService.SortedSetAddAsync(key, entry2, entry2.Score);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();

        var rankings = await _redisCacheService.SortedSetRangeWithScoresAsync<LeaderboardEntry>(
            key, 0, -1, Order.Descending);
        rankings.Should().HaveCount(2);
        rankings[0].Member.Username.Should().Be("Player2"); // 分數高的在前面
        rankings[0].Score.Should().Be(200);
    }

    #endregion

    #region TTL 與過期測試

    [Fact]
    public async Task ExpireAsync_輸入過期時間_應正確設定TTL()
    {
        // Arrange
        var key = $"expire_test_{Guid.NewGuid():N}";
        await _redisCacheService.SetStringAsync(key, "expire_value");

        // Act
        var result = await _redisCacheService.ExpireAsync(key, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().BeTrue();
        var ttl = await _redisCacheService.GetTtlAsync(key);
        ttl.Should().NotBeNull();
        ttl!.Value.TotalMinutes.Should().BeGreaterThan(4);
    }

    #endregion

    #region 資料隔離測試

    [Fact]
    public async Task 測試資料隔離_多個測試同時執行_應不互相影響()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
        var key1 = $"isolation_test:{testId}:key1";
        var key2 = $"isolation_test:{testId}:key2";

        // Act
        await _redisCacheService.SetStringAsync(key1, "value1");
        await _redisCacheService.SetStringAsync(key2, "value2");

        // Assert
        var value1 = await _redisCacheService.GetStringAsync<string>(key1);
        var value2 = await _redisCacheService.GetStringAsync<string>(key2);

        value1.Should().Be("value1");
        value2.Should().Be("value2");

        // Cleanup
        await _redisCacheService.DeleteAsync(key1);
        await _redisCacheService.DeleteAsync(key2);
    }

    #endregion
}
```
