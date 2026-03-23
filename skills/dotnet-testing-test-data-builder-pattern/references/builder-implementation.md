# Test Data Builder 完整實作範例

## 完整 UserBuilder 範例

```csharp
public class UserBuilder
{
    // 預設值：提供所有屬性的合理預設值
    private string _name = "Default User";
    private string _email = "default@example.com";
    private int _age = 25;
    private List<string> _roles = new();
    private UserSettings _settings = new()
    {
        Theme = "Light",
        Language = "en-US"
    };
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;

    // With* 方法：流暢介面設定個別屬性
    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithAge(int age)
    {
        _age = age;
        return this;
    }

    public UserBuilder WithRole(string role)
    {
        _roles.Add(role);
        return this;
    }

    public UserBuilder WithRoles(params string[] roles)
    {
        _roles.AddRange(roles);
        return this;
    }

    public UserBuilder WithSettings(UserSettings settings)
    {
        _settings = settings;
        return this;
    }

    public UserBuilder IsInactive()
    {
        _isActive = false;
        return this;
    }

    public UserBuilder CreatedOn(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    // 語意化預設建立者：提供常見情境的快速建立方法
    public static UserBuilder AUser() => new();

    public static UserBuilder AnAdminUser() => new UserBuilder()
        .WithRoles("Admin", "User");

    public static UserBuilder ARegularUser() => new UserBuilder()
        .WithRole("User");

    public static UserBuilder AnInactiveUser() => new UserBuilder()
        .IsInactive();

    // 語意化組合方法
    public UserBuilder WithValidEmail()
    {
        _email = $"{_name.Replace(" ", ".").ToLower()}@example.com";
        return this;
    }

    public UserBuilder WithAdminRights()
    {
        return WithRoles("Admin", "User");
    }

    // Build 方法：建立最終物件
    public User Build()
    {
        return new User
        {
            Name = _name,
            Email = _email,
            Age = _age,
            Roles = _roles.ToArray(),
            Settings = _settings,
            IsActive = _isActive,
            CreatedAt = _createdAt,
            ModifiedAt = _createdAt
        };
    }
}
```

## 在測試中使用 Builder

### 單一測試情境

```csharp
[Fact]
public void CreateUser_有效管理員使用者_應成功建立()
{
    // Arrange - 使用 Builder 建立測試資料
    var adminUser = UserBuilder
        .AnAdminUser()
        .WithName("John Admin")
        .WithEmail("john.admin@company.com")
        .WithAge(35)
        .Build();

    var userService = new UserService();

    // Act
    var result = userService.CreateUser(adminUser);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("John Admin", result.Name);
    Assert.Contains("Admin", result.Roles);
}
```

### 配合 Theory 使用

```csharp
public class UserValidationTests
{
    [Theory]
    [MemberData(nameof(GetUserScenarios))]
    public void ValidateUser_不同使用者情境_應回傳正確驗證結果(User user, bool expected)
    {
        // Arrange
        var validator = new UserValidator();

        // Act
        var result = validator.IsValid(user);

        // Assert
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> GetUserScenarios()
    {
        // ✅ 有效使用者情境
        yield return new object[]
        {
            UserBuilder.AUser()
                .WithName("Valid User")
                .WithEmail("valid@example.com")
                .WithAge(25)
                .Build(),
            true
        };

        // ❌ 無效使用者情境 - 空名稱
        yield return new object[]
        {
            UserBuilder.AUser()
                .WithName("")
                .Build(),
            false
        };

        // ❌ 無效使用者情境 - 年齡過小
        yield return new object[]
        {
            UserBuilder.AUser()
                .WithAge(10)
                .Build(),
            false
        };

        // ❌ 無效使用者情境 - 無效 Email
        yield return new object[]
        {
            UserBuilder.AUser()
                .WithEmail("invalid-email")
                .Build(),
            false
        };
    }
}
```
