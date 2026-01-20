using MongoDB.Bson;
using MongoDB.Driver;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using Xunit.Abstractions;

namespace YourProject.Integration.Tests.MongoDB;

/// <summary>
/// MongoDB CRUD 測試 - 展示完整的文件操作測試
/// 使用 Collection Fixture 共享容器，確保測試效能
/// </summary>
[Collection("MongoDb Collection")]
public class MongoUserServiceTests
{
    private readonly MongoUserService _mongoUserService;
    private readonly IMongoDatabase _database;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly MongoDbContainerFixture _fixture;

    public MongoUserServiceTests(MongoDbContainerFixture fixture)
    {
        _fixture = fixture;
        _database = fixture.Database;
        _fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // 建立服務實例（實際專案中應使用 DI）
        _mongoUserService = new MongoUserService(
            _database,
            Options.Create(new MongoDbSettings { UsersCollectionName = "users" }),
            NullLogger<MongoUserService>.Instance,
            _fakeTimeProvider);
    }

    #region Create 測試

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
        result.Version.Should().Be(1);
    }

    [Fact]
    public async Task CreateUserAsync_電子郵件已存在_應拋出InvalidOperationException()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";
        var user1 = new UserDocument
        {
            Username = $"user1_{Guid.NewGuid():N}",
            Email = email
        };
        var user2 = new UserDocument
        {
            Username = $"user2_{Guid.NewGuid():N}",
            Email = email  // 相同的 email
        };

        await _mongoUserService.CreateUserAsync(user1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mongoUserService.CreateUserAsync(user2));
    }

    #endregion

    #region Read 測試

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
        result.Profile.FirstName.Should().Be("Get");
    }

    [Fact]
    public async Task GetUserByIdAsync_輸入不存在的ID_應回傳null()
    {
        // Arrange
        var nonExistentId = ObjectId.GenerateNewId().ToString();

        // Act
        var result = await _mongoUserService.GetUserByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailAsync_輸入存在的Email_應回傳正確使用者()
    {
        // Arrange
        var email = $"emailtest_{Guid.NewGuid():N}@example.com";
        var user = new UserDocument
        {
            Username = $"emailtest_{Guid.NewGuid():N}",
            Email = email,
            Profile = new UserProfile { FirstName = "Email", LastName = "Test" }
        };
        await _mongoUserService.CreateUserAsync(user);

        // Act
        var result = await _mongoUserService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    #endregion

    #region Update 測試

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
    public async Task UpdateUserAsync_版本號不符_應拋出InvalidOperationException()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"versiontest_{Guid.NewGuid():N}",
            Email = $"versiontest_{Guid.NewGuid():N}@example.com"
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);
        
        // 模擬另一個使用者先更新了這個文件
        createdUser.Version = 0;  // 錯誤的版本號

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mongoUserService.UpdateUserAsync(createdUser));
    }

    #endregion

    #region Delete 測試

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

    [Fact]
    public async Task DeleteUserAsync_輸入不存在的ID_應回傳false()
    {
        // Arrange
        var nonExistentId = ObjectId.GenerateNewId().ToString();

        // Act
        var result = await _mongoUserService.DeleteUserAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}

/// <summary>
/// MongoDB BSON 序列化測試 - 驗證 BSON 序列化行為
/// </summary>
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

/// <summary>
/// MongoDB 索引測試 - 驗證索引建立與效能
/// </summary>
[Collection("MongoDb Collection")]
public class MongoIndexTests
{
    private readonly MongoDbContainerFixture _fixture;
    private readonly IMongoCollection<UserDocument> _users;
    private readonly ITestOutputHelper _output;

    public MongoIndexTests(MongoDbContainerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
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
        await _users.InsertOneAsync(user1);  // 第一次插入成功

        var exception = await Assert.ThrowsAsync<MongoWriteException>(
            () => _users.InsertOneAsync(user2));
        exception.WriteError.Category.Should().Be(ServerErrorCategory.DuplicateKey);

        _output.WriteLine("唯一索引測試通過 - 重複的 email 被正確阻擋");
    }
}
