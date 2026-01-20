// FluentValidation 測試範本
// 此範本展示基本的 FluentValidation 驗證器測試模式

using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FluentValidationTestingExample;

// ==================== 測試資料模型 ====================

public class UserRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool AgreeToTerms { get; set; }
}

// ==================== 驗證器實作 ====================

public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly TimeProvider _timeProvider;

    public UserRegistrationValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        // 使用者名稱驗證
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("使用者名稱不可為 null 或空白")
            .Length(3, 20).WithMessage("使用者名稱長度必須在 3 到 20 個字元之間")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("使用者名稱只能包含字母、數字和底線");

        // 電子郵件驗證
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不可為 null 或空白")
            .EmailAddress().WithMessage("電子郵件格式不正確")
            .MaximumLength(100).WithMessage("電子郵件長度不能超過 100 個字元");

        // 密碼驗證
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼不可為 null 或空白")
            .Length(8, 50).WithMessage("密碼長度必須在 8 到 50 個字元之間")
            .Must(BeComplexPassword).WithMessage("密碼必須包含大小寫字母和數字");

        // 確認密碼驗證
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("確認密碼必須與密碼相同");

        // 年齡驗證
        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("年齡必須大於或等於 18 歲")
            .LessThanOrEqualTo(120).WithMessage("年齡必須小於或等於 120 歲");

        // 生日與年齡一致性驗證
        RuleFor(x => x.BirthDate)
            .Must((request, birthDate) => IsAgeConsistentWithBirthDate(birthDate, request.Age))
            .WithMessage("生日與年齡不一致");

        // 電話號碼驗證（選填）
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^09\d{8}$").WithMessage("電話號碼格式不正確")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        // 角色驗證
        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("角色清單不可為 null 或空陣列")
            .Must(roles => roles == null || roles.All(role => IsValidRole(role)))
            .WithMessage("包含無效的角色");

        // 同意條款驗證
        RuleFor(x => x.AgreeToTerms)
            .Equal(true).WithMessage("必須同意使用條款");
    }

    private bool BeComplexPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // 必須包含大小寫字母和數字
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
    }

    private bool IsAgeConsistentWithBirthDate(DateTime birthDate, int age)
    {
        var currentDate = _timeProvider.GetLocalNow().Date;
        var calculatedAge = currentDate.Year - birthDate.Year;

        // 如果今年生日還沒到，年齡要減 1
        if (birthDate.Date > currentDate.AddYears(-calculatedAge))
        {
            calculatedAge--;
        }

        return calculatedAge == age;
    }

    private bool IsValidRole(string role)
    {
        var validRoles = new[] { "User", "Admin", "Manager", "Support" };
        return validRoles.Contains(role);
    }
}

// ==================== 測試類別 ====================

public class UserRegistrationValidatorTests
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly UserRegistrationValidator _validator;

    public UserRegistrationValidatorTests()
    {
        // 設定假的時間提供者為 2024-01-01
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 1, 1));
        
        // 建立驗證器實例
        _validator = new UserRegistrationValidator(_fakeTimeProvider);
    }

    // ==================== 使用者名稱驗證測試 ====================

    [Fact]
    public void Validate_有效的使用者名稱_應該通過驗證()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = "valid_user123";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_空的使用者名稱_應該驗證失敗(string username)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = username;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱不可為 null 或空白");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Validate_過短的使用者名稱_應該驗證失敗(string username)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = username;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱長度必須在 3 到 20 個字元之間");
    }

    [Fact]
    public void Validate_過長的使用者名稱_應該驗證失敗()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = "a_very_long_username_that_exceeds_limit";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱長度必須在 3 到 20 個字元之間");
    }

    [Theory]
    [InlineData("user@name")]
    [InlineData("user-name")]
    [InlineData("user name")]
    [InlineData("user#123")]
    public void Validate_包含特殊字元的使用者名稱_應該驗證失敗(string username)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = username;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱只能包含字母、數字和底線");
    }

    // ==================== Email 驗證測試 ====================

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_空的Email_應該驗證失敗(string email)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = email;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("電子郵件不可為 null 或空白");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user name@example.com")]
    public void Validate_無效的Email格式_應該驗證失敗(string email)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = email;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("電子郵件格式不正確");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@example.com")]
    [InlineData("user+tag@example.co.uk")]
    public void Validate_有效的Email_應該通過驗證(string email)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = email;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // ==================== 密碼驗證測試 ====================

    [Theory]
    [InlineData("weak")]
    [InlineData("weakpass")]
    [InlineData("WEAKPASS123")]
    [InlineData("weakpass123")]
    [InlineData("WeakPass")]
    public void Validate_弱密碼_應該驗證失敗(string password)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("StrongPass123")]
    [InlineData("MyP@ssw0rd")]
    [InlineData("Test1234Aa")]
    public void Validate_強密碼_應該通過驗證(string password)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_密碼與確認密碼不一致_應該驗證失敗()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = "Password123";
        request.ConfirmPassword = "DifferentPass456";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
              .WithErrorMessage("確認密碼必須與密碼相同");
    }

    // ==================== 年齡驗證測試 ====================

    [Theory]
    [InlineData(17)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_年齡小於18_應該驗證失敗(int age)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Age = age;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Age)
              .WithErrorMessage("年齡必須大於或等於 18 歲");
    }

    [Theory]
    [InlineData(121)]
    [InlineData(150)]
    public void Validate_年齡大於120_應該驗證失敗(int age)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Age = age;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Age)
              .WithErrorMessage("年齡必須小於或等於 120 歲");
    }

    [Theory]
    [InlineData(18)]
    [InlineData(30)]
    [InlineData(120)]
    public void Validate_有效年齡_應該通過驗證(int age)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Age = age;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Age);
    }

    // ==================== 生日與年齡一致性驗證測試 ====================

    [Fact]
    public void Validate_年齡與生日一致_應該通過驗證()
    {
        // Arrange - 現在是 2024-01-01，生日是 1990-01-01，年齡應該是 34
        var request = CreateValidRequest();
        request.BirthDate = new DateTime(1990, 1, 1);
        request.Age = 34;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Validate_年齡與生日不一致_應該驗證失敗()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BirthDate = new DateTime(1990, 1, 1);
        request.Age = 25; // 錯誤的年齡

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
              .WithErrorMessage("生日與年齡不一致");
    }

    [Fact]
    public void Validate_生日尚未到達_年齡計算應該正確()
    {
        // Arrange - 設定現在是 2024-02-01，生日是 1990-06-15（今年生日還沒到）
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 2, 1));
        var validator = new UserRegistrationValidator(_fakeTimeProvider);

        var request = CreateValidRequest();
        request.BirthDate = new DateTime(1990, 6, 15);
        request.Age = 33; // 2024 - 1990 - 1 = 33（因為生日還沒到）

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    // ==================== 電話號碼驗證測試 ====================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_電話號碼為空_應該跳過驗證(string phoneNumber)
    {
        // Arrange
        var request = CreateValidRequest();
        request.PhoneNumber = phoneNumber;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("0812345678")]
    [InlineData("091234567")]
    [InlineData("09123456789")]
    public void Validate_無效的電話號碼格式_應該驗證失敗(string phoneNumber)
    {
        // Arrange
        var request = CreateValidRequest();
        request.PhoneNumber = phoneNumber;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
              .WithErrorMessage("電話號碼格式不正確");
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("0987654321")]
    [InlineData("0900000000")]
    public void Validate_有效的電話號碼_應該通過驗證(string phoneNumber)
    {
        // Arrange
        var request = CreateValidRequest();
        request.PhoneNumber = phoneNumber;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // ==================== 角色驗證測試 ====================

    [Fact]
    public void Validate_空的角色清單_應該驗證失敗()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Roles = new List<string>();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Roles)
              .WithErrorMessage("角色清單不可為 null 或空陣列");
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("SuperUser")]
    [InlineData("Guest")]
    public void Validate_無效的角色_應該驗證失敗(string invalidRole)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Roles = new List<string> { "User", invalidRole };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Roles)
              .WithErrorMessage("包含無效的角色");
    }

    [Fact]
    public void Validate_有效的角色組合_應該通過驗證()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Roles = new List<string> { "User", "Admin", "Manager" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Roles);
    }

    // ==================== 同意條款驗證測試 ====================

    [Fact]
    public void Validate_未同意條款_應該驗證失敗()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AgreeToTerms = false;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AgreeToTerms)
              .WithErrorMessage("必須同意使用條款");
    }

    [Fact]
    public void Validate_已同意條款_應該通過驗證()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AgreeToTerms = true;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AgreeToTerms);
    }

    // ==================== 整體驗證測試 ====================

    [Fact]
    public void Validate_完全有效的請求_應該通過所有驗證()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ==================== 輔助方法 ====================

    /// <summary>
    /// 建立一個有效的使用者註冊請求作為測試基礎
    /// </summary>
    private UserRegistrationRequest CreateValidRequest()
    {
        return new UserRegistrationRequest
        {
            Username = "testuser123",
            Email = "test@example.com",
            Password = "TestPass123",
            ConfirmPassword = "TestPass123",
            BirthDate = new DateTime(1990, 1, 1),
            Age = 34, // 根據 2024-01-01 計算
            PhoneNumber = "0912345678",
            Roles = new List<string> { "User" },
            AgreeToTerms = true
        };
    }
}
