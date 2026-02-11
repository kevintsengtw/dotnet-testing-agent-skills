# MongoDB 容器化測試

> 本文件從 [SKILL.md](../SKILL.md) 擷取，包含 MongoDB 容器化測試的完整程式碼範例與詳細說明。

---

### MongoDB Container Fixture

使用 Collection Fixture 模式共享容器，節省 80% 以上的測試時間：

```csharp
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace YourProject.Integration.Tests.Fixtures;

/// <summary>
/// MongoDB 容器 Fixture - 實作 IAsyncLifetime 管理容器生命週期
/// </summary>
public class MongoDbContainerFixture : IAsyncLifetime
{
    private MongoDbContainer? _container;

    public IMongoDatabase Database { get; private set; } = null!;
    public string ConnectionString { get; private set; } = string.Empty;
    public string DatabaseName { get; } = "testdb";

    public async Task InitializeAsync()
    {
        // 使用 MongoDB 7.0 確保功能完整性
        _container = new MongoDbBuilder()
                     .WithImage("mongo:7.0")
                     .WithPortBinding(27017, true)
                     .Build();

        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();
        var client = new MongoClient(ConnectionString);
        Database = client.GetDatabase(DatabaseName);
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// 清空資料庫中的所有集合 - 用於測試間隔離
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var collections = await Database.ListCollectionNamesAsync();
        await collections.ForEachAsync(async collectionName =>
        {
            await Database.DropCollectionAsync(collectionName);
        });
    }
}

/// <summary>
/// 定義使用 MongoDB Fixture 的測試集合
/// </summary>
[CollectionDefinition("MongoDb Collection")]
public class MongoDbCollectionFixture : ICollectionFixture<MongoDbContainerFixture>
{
    // 此類別不需要實作，僅用於標記集合
}
```

### MongoDB 文件模型設計

建立包含巢狀物件、陣列、字典等複雜結構的文件模型：

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourProject.Core.Models.Mongo;

/// <summary>
/// 使用者文件 - 展示 MongoDB 複雜文件結構
/// </summary>
public class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("username")]
    [BsonRequired]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    [BsonRequired]
    public string Email { get; set; } = string.Empty;

    [BsonElement("profile")]
    public UserProfile Profile { get; set; } = new();

    [BsonElement("addresses")]
    public List<Address> Addresses { get; set; } = new();

    [BsonElement("skills")]
    public List<Skill> Skills { get; set; } = new();

    [BsonElement("preferences")]
    public Dictionary<string, object> Preferences { get; set; } = new();

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;

    [BsonElement("version")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// 樂觀鎖定版本遞增
    /// </summary>
    public void IncrementVersion(DateTime updateTime)
    {
        Version++;
        UpdatedAt = updateTime;
    }
}

/// <summary>
/// 使用者檔案 - 巢狀文件範例
/// </summary>
public class UserProfile
{
    [BsonElement("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("last_name")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("birth_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? BirthDate { get; set; }

    [BsonElement("bio")]
    public string Bio { get; set; } = string.Empty;

    [BsonElement("social_links")]
    public Dictionary<string, string> SocialLinks { get; set; } = new();

    [BsonIgnore]
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// 地址模型 - 用於地理空間查詢
/// </summary>
public class Address
{
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // "home", "work", "other"

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("country")]
    public string Country { get; set; } = string.Empty;

    [BsonElement("location")]
    public GeoLocation? Location { get; set; }

    [BsonElement("is_primary")]
    public bool IsPrimary { get; set; }
}

/// <summary>
/// 地理位置 - GeoJSON 格式
/// </summary>
public class GeoLocation
{
    [BsonElement("type")]
    public string Type { get; set; } = "Point";

    [BsonElement("coordinates")]
    public double[] Coordinates { get; set; } = new double[2]; // [longitude, latitude]

    public static GeoLocation CreatePoint(double longitude, double latitude)
    {
        return new GeoLocation
        {
            Type = "Point",
            Coordinates = new[] { longitude, latitude }
        };
    }
}

/// <summary>
/// 技能模型 - 陣列查詢範例
/// </summary>
public class Skill
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("level")]
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;

    [BsonElement("years_experience")]
    public int YearsExperience { get; set; }

    [BsonElement("verified")]
    public bool Verified { get; set; }
}

/// <summary>
/// 技能等級列舉
/// </summary>
public enum SkillLevel
{
    [BsonRepresentation(BsonType.String)]
    Beginner,

    [BsonRepresentation(BsonType.String)]
    Intermediate,

    [BsonRepresentation(BsonType.String)]
    Advanced,

    [BsonRepresentation(BsonType.String)]
    Expert
}
```

### BSON 序列化測試

驗證 BSON 序列化行為：

```csharp
using MongoDB.Bson;
using AwesomeAssertions;

namespace YourProject.Integration.Tests.MongoDB;

public class MongoBsonTests
{
    [Fact]
    public void ObjectId產生_應產生有效的ObjectId()
    {
        // Arrange & Act
        var objectId = ObjectId.GenerateNewId();

        // Assert
        objectId.Should().NotBeNull();
        objectId.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        objectId.ToString().Should().HaveLength(24);
    }

    [Fact]
    public void BsonDocument建立_當傳入null值_應正確處理()
    {
        // Arrange
        var doc = new BsonDocument
        {
            ["name"] = "John",
            ["email"] = BsonNull.Value,
            ["age"] = 25
        };

        // Act
        var json = doc.ToJson();

        // Assert
        json.Should().Contain("\"email\" : null");
        doc["email"].IsBsonNull.Should().BeTrue();
    }

    [Fact]
    public void BsonArray操作_當使用複雜陣列_應正確處理()
    {
        // Arrange
        var skills = new BsonArray
        {
            new BsonDocument { ["name"] = "C#", ["level"] = 5 },
            new BsonDocument { ["name"] = "MongoDB", ["level"] = 3 }
        };

        var doc = new BsonDocument
        {
            ["userId"] = ObjectId.GenerateNewId(),
            ["skills"] = skills
        };

        // Act
        var skillsArray = doc["skills"].AsBsonArray;
        var firstSkill = skillsArray[0].AsBsonDocument;

        // Assert
        skillsArray.Should().HaveCount(2);
        firstSkill["name"].AsString.Should().Be("C#");
        firstSkill["level"].AsInt32.Should().Be(5);
    }
}
```

### MongoDB CRUD 測試

```csharp
using MongoDB.Driver;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;

namespace YourProject.Integration.Tests.MongoDB;

[Collection("MongoDb Collection")]
public class MongoUserServiceTests
{
    private readonly MongoUserService _mongoUserService;
    private readonly IMongoDatabase _database;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public MongoUserServiceTests(MongoDbContainerFixture fixture)
    {
        _database = fixture.Database;
        _fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        
        // 建立服務實例
        _mongoUserService = new MongoUserService(
            _database, 
            Options.Create(new MongoDbSettings { UsersCollectionName = "users" }),
            NullLogger<MongoUserService>.Instance,
            _fakeTimeProvider);
    }

    [Fact]
    public async Task CreateUserAsync_輸入有效使用者_應成功建立使用者()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"testuser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Profile = new UserProfile
            {
                FirstName = "Test",
                LastName = "User",
                Bio = "Test user bio"
            }
        };

        // Act
        var result = await _mongoUserService.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);
    }

    [Fact]
    public async Task GetUserByIdAsync_輸入存在的ID_應回傳正確使用者()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"gettest_{Guid.NewGuid():N}",
            Email = $"gettest_{Guid.NewGuid():N}@example.com",
            Profile = new UserProfile { FirstName = "Get", LastName = "Test" }
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);

        // Act
        var result = await _mongoUserService.GetUserByIdAsync(createdUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task UpdateUserAsync_使用樂觀鎖定_應成功更新版本號()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"updatetest_{Guid.NewGuid():N}",
            Email = $"updatetest_{Guid.NewGuid():N}@example.com"
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);
        createdUser.Profile.Bio = "Updated bio";

        // Act
        var result = await _mongoUserService.UpdateUserAsync(createdUser);

        // Assert
        result.Should().NotBeNull();
        result!.Version.Should().Be(2);
        result.Profile.Bio.Should().Be("Updated bio");
    }

    [Fact]
    public async Task DeleteUserAsync_輸入存在的ID_應成功刪除使用者()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"deletetest_{Guid.NewGuid():N}",
            Email = $"deletetest_{Guid.NewGuid():N}@example.com"
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);

        // Act
        var result = await _mongoUserService.DeleteUserAsync(createdUser.Id);

        // Assert
        result.Should().BeTrue();
        
        var deletedUser = await _mongoUserService.GetUserByIdAsync(createdUser.Id);
        deletedUser.Should().BeNull();
    }
}
```

### MongoDB 索引測試

```csharp
using MongoDB.Driver;
using AwesomeAssertions;
using System.Diagnostics;

namespace YourProject.Integration.Tests.MongoDB;

[Collection("MongoDb Collection")]
public class MongoIndexTests
{
    private readonly IMongoCollection<UserDocument> _users;
    private readonly ITestOutputHelper _output;

    public MongoIndexTests(MongoDbContainerFixture fixture, ITestOutputHelper output)
    {
        _users = fixture.Database.GetCollection<UserDocument>("index_test_users");
        _output = output;
    }

    [Fact]
    public async Task CreateUniqueIndex_電子郵件唯一索引_應防止重複插入()
    {
        // Arrange - 確保集合為空
        await _users.DeleteManyAsync(FilterDefinition<UserDocument>.Empty);

        // 建立唯一索引
        var indexKeysDefinition = Builders<UserDocument>.IndexKeys.Ascending(u => u.Email);
        var indexOptions = new CreateIndexOptions { Unique = true };
        await _users.Indexes.CreateOneAsync(
            new CreateIndexModel<UserDocument>(indexKeysDefinition, indexOptions));

        var uniqueEmail = $"unique_{Guid.NewGuid():N}@example.com";
        var user1 = new UserDocument { Username = "user1", Email = uniqueEmail };
        var user2 = new UserDocument { Username = "user2", Email = uniqueEmail };

        // Act & Assert
        await _users.InsertOneAsync(user1); // 第一次插入成功

        var exception = await Assert.ThrowsAsync<MongoWriteException>(
            () => _users.InsertOneAsync(user2));
        exception.WriteError.Category.Should().Be(ServerErrorCategory.DuplicateKey);

        _output.WriteLine("唯一索引測試通過 - 重複的 email 被正確阻擋");
    }

    [Fact]
    public async Task CompoundIndex_複合索引查詢效能_應提升查詢速度()
    {
        // Arrange - 確保集合為空
        await _users.DeleteManyAsync(FilterDefinition<UserDocument>.Empty);

        // 插入測試資料
        var testUsers = Enumerable.Range(0, 1000)
            .Select(i => new UserDocument
            {
                Username = $"user_{i:D4}",
                Email = $"user{i:D4}_{Guid.NewGuid():N}@example.com",
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i % 365)
            })
            .ToList();

        await _users.InsertManyAsync(testUsers);

        // 建立複合索引
        var compoundIndex = Builders<UserDocument>.IndexKeys
            .Ascending(u => u.IsActive)
            .Descending(u => u.CreatedAt);
        await _users.Indexes.CreateOneAsync(new CreateIndexModel<UserDocument>(compoundIndex));

        // 測試查詢效能
        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(u => u.IsActive, true),
            Builders<UserDocument>.Filter.Gte(u => u.CreatedAt, DateTime.UtcNow.AddDays(-100))
        );

        var stopwatch = Stopwatch.StartNew();
        var results = await _users.Find(filter).ToListAsync();
        stopwatch.Stop();

        _output.WriteLine($"查詢時間: {stopwatch.ElapsedMilliseconds}ms, 結果數量: {results.Count}");
        
        // Assert
        results.Should().NotBeEmpty();
    }
}
```
