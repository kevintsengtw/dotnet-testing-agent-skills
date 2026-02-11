# FluentValidation 核心測試模式 - 詳細參考

> 此文件包含 FluentValidation 驗證器測試的完整程式碼範例與詳細說明。
> 主文件：[SKILL.md](../SKILL.md)

---

## 模式 1：基本欄位驗證

### 驗證器範例

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("使用者名稱不可為 null 或空白")
            .Length(3, 20).WithMessage("使用者名稱長度必須在 3 到 20 個字元之間")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("使用者名稱只能包含字母、數字和底線");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不可為 null 或空白")
            .EmailAddress().WithMessage("電子郵件格式不正確")
            .MaximumLength(100).WithMessage("電子郵件長度不能超過 100 個字元");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("年齡必須大於或等於 18 歲")
            .LessThanOrEqualTo(120).WithMessage("年齡必須小於或等於 120 歲");
    }
}
```

### 測試範例

```csharp
public class UserValidatorTests
{
    private readonly UserValidator _validator;

    public UserValidatorTests()
    {
        _validator = new UserValidator();
    }

    [Fact]
    public void Validate_有效使用者名稱_應該通過驗證()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "valid_user123" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_空白使用者名稱_應該驗證失敗()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱不可為 null 或空白");
    }
}
```

---

## 模式 2：參數化測試

```csharp
[Theory]
[InlineData("", "使用者名稱不可為 null 或空白")]
[InlineData("ab", "使用者名稱長度必須在 3 到 20 個字元之間")]
[InlineData("a_very_long_username_exceeds_limit", "使用者名稱長度必須在 3 到 20 個字元之間")]
[InlineData("user@name", "使用者名稱只能包含字母、數字和底線")]
public void Validate_無效使用者名稱_應該回傳對應錯誤(string username, string expectedError)
{
    // Arrange
    var request = new UserRegistrationRequest { Username = username };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Username)
          .WithErrorMessage(expectedError);
}

[Theory]
[InlineData("user123")]
[InlineData("valid_user")]
[InlineData("TEST_User_99")]
public void Validate_有效使用者名稱_應該通過驗證(string username)
{
    // Arrange
    var request = new UserRegistrationRequest { Username = username };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.Username);
}
```

---

## 模式 3：跨欄位驗證

### 密碼與確認密碼

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼不可為 null 或空白")
            .Length(8, 50).WithMessage("密碼長度必須在 8 到 50 個字元之間")
            .Must(BeComplexPassword).WithMessage("密碼必須包含大小寫字母和數字");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("確認密碼必須與密碼相同");
    }

    private bool BeComplexPassword(string password)
    {
        return !string.IsNullOrEmpty(password) && 
               Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
    }
}
```

### 測試範例

```csharp
[Fact]
public void Validate_密碼與確認密碼不一致_應該驗證失敗()
{
    // Arrange
    var request = new UserRegistrationRequest
    {
        Password = "Password123",
        ConfirmPassword = "DifferentPass456"
    };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
          .WithErrorMessage("確認密碼必須與密碼相同");
}

[Theory]
[InlineData("weak", "密碼長度必須在 8 到 50 個字元之間")]
[InlineData("weakpass", "密碼必須包含大小寫字母和數字")]
[InlineData("WEAKPASS123", "密碼必須包含大小寫字母和數字")]
public void Validate_弱密碼_應該驗證失敗(string password, string expectedError)
{
    // Arrange
    var request = new UserRegistrationRequest
    {
        Password = password,
        ConfirmPassword = password
    };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
          .WithErrorMessage(expectedError);
}
```

---

## 模式 4：時間相依驗證

### 年齡與生日一致性驗證

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly TimeProvider _timeProvider;

    public UserValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        RuleFor(x => x.BirthDate)
            .Must((request, birthDate) => IsAgeConsistentWithBirthDate(birthDate, request.Age))
            .WithMessage("生日與年齡不一致");
    }

    private bool IsAgeConsistentWithBirthDate(DateTime birthDate, int age)
    {
        var currentDate = _timeProvider.GetLocalNow().Date;
        var calculatedAge = currentDate.Year - birthDate.Year;

        if (birthDate.Date > currentDate.AddYears(-calculatedAge))
        {
            calculatedAge--;
        }

        return calculatedAge == age;
    }
}
```

### 測試範例

```csharp
public class UserValidatorTests
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly UserValidator _validator;

    public UserValidatorTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 1, 1));
        _validator = new UserValidator(_fakeTimeProvider);
    }

    [Fact]
    public void Validate_年齡與生日一致_應該通過驗證()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            BirthDate = new DateTime(1990, 1, 1),
            Age = 34 // 2024 - 1990 = 34
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Validate_年齡與生日不一致_應該驗證失敗()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            BirthDate = new DateTime(1990, 1, 1),
            Age = 25 // 錯誤的年齡
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
              .WithErrorMessage("生日與年齡不一致");
    }

    [Fact]
    public void Validate_生日尚未到達_年齡計算應該正確()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 2, 1));
        var validator = new UserValidator(_fakeTimeProvider);

        var request = new UserRegistrationRequest
        {
            BirthDate = new DateTime(1990, 6, 15), // 生日在今年尚未到達
            Age = 33 // 2024 - 1990 - 1 = 33
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }
}
```

---

## 模式 5：條件式驗證

### 驗證器定義

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        // 電話號碼為可選，但如果有填就必須是有效格式
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^09\d{8}$").WithMessage("電話號碼格式不正確")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
```

### 測試範例

```csharp
[Fact]
public void Validate_電話號碼為空_應該跳過驗證()
{
    // Arrange
    var request = new UserRegistrationRequest { PhoneNumber = null };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
}

[Fact]
public void Validate_電話號碼格式錯誤_應該驗證失敗()
{
    // Arrange
    var request = new UserRegistrationRequest { PhoneNumber = "123456789" };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
          .WithErrorMessage("電話號碼格式不正確");
}

[Theory]
[InlineData("0912345678")]
[InlineData("0987654321")]
public void Validate_有效電話號碼_應該通過驗證(string phoneNumber)
{
    // Arrange
    var request = new UserRegistrationRequest { PhoneNumber = phoneNumber };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
}
```

---

## 模式 6：非同步驗證

### 驗證器定義

```csharp
public interface IUserService
{
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailRegisteredAsync(string email);
}

public class UserAsyncValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly IUserService _userService;

    public UserAsyncValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(x => x.Username)
            .MustAsync(async (username, cancellation) =>
                await _userService.IsUsernameAvailableAsync(username))
            .WithMessage("使用者名稱已被使用");

        RuleFor(x => x.Email)
            .MustAsync(async (email, cancellation) =>
                !await _userService.IsEmailRegisteredAsync(email))
            .WithMessage("此電子郵件已被註冊");
    }
}
```

### 測試範例

```csharp
public class UserAsyncValidatorTests
{
    private readonly IUserService _mockUserService;
    private readonly UserAsyncValidator _validator;

    public UserAsyncValidatorTests()
    {
        _mockUserService = Substitute.For<IUserService>();
        _validator = new UserAsyncValidator(_mockUserService);
    }

    [Fact]
    public async Task ValidateAsync_使用者名稱可用_應該通過驗證()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "newuser123" };

        _mockUserService.IsUsernameAvailableAsync("newuser123")
                       .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        await _mockUserService.Received(1).IsUsernameAvailableAsync("newuser123");
    }

    [Fact]
    public async Task ValidateAsync_使用者名稱已被使用_應該驗證失敗()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "existinguser" };

        _mockUserService.IsUsernameAvailableAsync("existinguser")
                       .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱已被使用");
        await _mockUserService.Received(1).IsUsernameAvailableAsync("existinguser");
    }

    [Fact]
    public async Task ValidateAsync_外部服務拋出例外_應該正確處理()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "testuser" };

        _mockUserService.IsUsernameAvailableAsync("testuser")
                       .Returns(Task.FromException<bool>(new TimeoutException("服務逾時")));

        // Act & Assert
        await _validator.TestValidateAsync(request)
                       .Should().ThrowAsync<TimeoutException>();
    }
}
```

---

## 模式 7：集合驗證

### 驗證器定義

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("角色清單不可為 null 或空陣列")
            .Must(roles => roles == null || roles.All(role => IsValidRole(role)))
            .WithMessage("包含無效的角色");
    }

    private bool IsValidRole(string role)
    {
        var validRoles = new[] { "User", "Admin", "Manager", "Support" };
        return validRoles.Contains(role);
    }
}
```

### 測試範例

```csharp
[Fact]
public void Validate_空的角色清單_應該驗證失敗()
{
    // Arrange
    var request = new UserRegistrationRequest { Roles = new List<string>() };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Roles)
          .WithErrorMessage("角色清單不可為 null 或空陣列");
}

[Theory]
[InlineData("InvalidRole")]
[InlineData("SuperUser")]
public void Validate_無效角色_應該驗證失敗(string invalidRole)
{
    // Arrange
    var request = new UserRegistrationRequest
    {
        Roles = new List<string> { "User", invalidRole }
    };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Roles)
          .WithErrorMessage("包含無效的角色");
}

[Theory]
[InlineData(new[] { "User" })]
[InlineData(new[] { "Admin" })]
[InlineData(new[] { "User", "Manager" })]
public void Validate_有效角色_應該通過驗證(string[] roles)
{
    // Arrange
    var request = new UserRegistrationRequest { Roles = roles.ToList() };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.Roles);
}
```
