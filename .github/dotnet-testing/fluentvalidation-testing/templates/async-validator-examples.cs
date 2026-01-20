// 非同步驗證器範例
// 此範本展示如何測試需要外部服務的非同步驗證

using FluentValidation;
using FluentValidation.TestHelper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluentValidationAsyncExample;

// ==================== 外部服務介面 ====================

public interface IUserService
{
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailRegisteredAsync(string email);
}

// ==================== 測試資料模型 ====================

public class UserRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// ==================== 非同步驗證器實作 ====================

public class UserRegistrationAsyncValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly IUserService _userService;

    public UserRegistrationAsyncValidator(IUserService userService)
    {
        _userService = userService;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        // 使用者名稱基本驗證
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("使用者名稱不可為 null 或空白")
            .Length(3, 20).WithMessage("使用者名稱長度必須在 3 到 20 個字元之間");

        // 使用者名稱唯一性驗證（非同步）
        RuleFor(x => x.Username)
            .MustAsync(async (username, cancellation) =>
                await _userService.IsUsernameAvailableAsync(username))
            .WithMessage("使用者名稱已被使用")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        // 電子郵件基本驗證
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不可為 null 或空白")
            .EmailAddress().WithMessage("電子郵件格式不正確");

        // 電子郵件唯一性驗證（非同步）
        RuleFor(x => x.Email)
            .MustAsync(async (email, cancellation) =>
                !await _userService.IsEmailRegisteredAsync(email))
            .WithMessage("此電子郵件已被註冊")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

// ==================== 測試類別 ====================

public class UserRegistrationAsyncValidatorTests
{
    private readonly IUserService _mockUserService;
    private readonly UserRegistrationAsyncValidator _validator;

    public UserRegistrationAsyncValidatorTests()
    {
        // 建立 Mock 服務
        _mockUserService = Substitute.For<IUserService>();
        
        // 建立驗證器實例
        _validator = new UserRegistrationAsyncValidator(_mockUserService);
    }

    // ==================== 使用者名稱可用性測試 ====================

    [Fact]
    public async Task ValidateAsync_使用者名稱可用_應該通過驗證()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "newuser123",
            Email = "new@example.com",
            Password = "Password123"
        };

        // Mock 回傳使用者名稱可用
        _mockUserService.IsUsernameAvailableAsync("newuser123")
                       .Returns(Task.FromResult(true));

        // Mock 回傳電子郵件未被註冊
        _mockUserService.IsEmailRegisteredAsync("new@example.com")
                       .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);

        // 驗證是否正確呼叫了服務
        await _mockUserService.Received(1).IsUsernameAvailableAsync("newuser123");
        await _mockUserService.Received(1).IsEmailRegisteredAsync("new@example.com");
    }

    [Fact]
    public async Task ValidateAsync_使用者名稱已被使用_應該驗證失敗()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "existinguser",
            Email = "new@example.com",
            Password = "Password123"
        };

        // Mock 回傳使用者名稱不可用
        _mockUserService.IsUsernameAvailableAsync("existinguser")
                       .Returns(Task.FromResult(false));

        // Mock 回傳電子郵件未被註冊
        _mockUserService.IsEmailRegisteredAsync("new@example.com")
                       .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱已被使用");

        await _mockUserService.Received(1).IsUsernameAvailableAsync("existinguser");
    }

    [Fact]
    public async Task ValidateAsync_空白使用者名稱_應該跳過非同步驗證()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "", // 空白使用者名稱
            Email = "test@example.com",
            Password = "Password123"
        };

        // Mock 設定（不應該被呼叫）
        _mockUserService.IsUsernameAvailableAsync(Arg.Any<string>())
                       .Returns(Task.FromResult(true));

        _mockUserService.IsEmailRegisteredAsync("test@example.com")
                       .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱不可為 null 或空白");

        // 驗證沒有呼叫非同步檢查（因為基本驗證已失敗）
        await _mockUserService.DidNotReceive().IsUsernameAvailableAsync(Arg.Any<string>());
    }

    // ==================== Email 可用性測試 ====================

    [Fact]
    public async Task ValidateAsync_Email未被註冊_應該通過驗證()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "available@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("testuser")
                       .Returns(Task.FromResult(true));

        _mockUserService.IsEmailRegisteredAsync("available@example.com")
                       .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        await _mockUserService.Received(1).IsEmailRegisteredAsync("available@example.com");
    }

    [Fact]
    public async Task ValidateAsync_Email已被註冊_應該驗證失敗()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("testuser")
                       .Returns(Task.FromResult(true));

        _mockUserService.IsEmailRegisteredAsync("existing@example.com")
                       .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("此電子郵件已被註冊");

        await _mockUserService.Received(1).IsEmailRegisteredAsync("existing@example.com");
    }

    // ==================== 例外處理測試 ====================

    [Fact]
    public async Task ValidateAsync_服務拋出例外_應該正確傳遞()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        // Mock 拋出例外
        _mockUserService.IsUsernameAvailableAsync("testuser")
                       .Throws(new TimeoutException("資料庫連線逾時"));

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(async () =>
            await _validator.TestValidateAsync(request));
    }

    [Fact]
    public async Task ValidateAsync_服務暫時無法使用_應該正確處理()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        // Mock 回傳失敗的 Task
        _mockUserService.IsUsernameAvailableAsync("testuser")
                       .Returns(Task.FromException<bool>(new InvalidOperationException("服務暫時無法使用")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _validator.TestValidateAsync(request));

        await _mockUserService.Received(1).IsUsernameAvailableAsync("testuser");
    }

    // ==================== CancellationToken 測試 ====================

    [Fact]
    public async Task ValidateAsync_使用CancellationToken_應該正確傳遞()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        var cts = new CancellationTokenSource();

        _mockUserService.IsUsernameAvailableAsync("testuser")
                       .Returns(Task.FromResult(true));

        _mockUserService.IsEmailRegisteredAsync("test@example.com")
                       .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(request, strategy =>
        {
            strategy.IncludeAllRuleSets();
        }, cts.Token);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ==================== 整合測試 ====================

    [Fact]
    public async Task ValidateAsync_使用者名稱和Email都已被使用_應該同時顯示兩個錯誤()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("existinguser")
                       .Returns(Task.FromResult(false));

        _mockUserService.IsEmailRegisteredAsync("existing@example.com")
                       .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("使用者名稱已被使用");

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("此電子郵件已被註冊");

        await _mockUserService.Received(1).IsUsernameAvailableAsync("existinguser");
        await _mockUserService.Received(1).IsEmailRegisteredAsync("existing@example.com");
    }
}

// ==================== 進階範例：條件式非同步驗證 ====================

public class OrderValidator : AbstractValidator<OrderRequest>
{
    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;

    public OrderValidator(IInventoryService inventoryService, IPaymentService paymentService)
    {
        _inventoryService = inventoryService;
        _paymentService = paymentService;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        // 基本驗證
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("產品 ID 不可為空");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("數量必須大於 0");

        // 庫存檢查（非同步）
        RuleFor(x => x)
            .MustAsync(async (order, cancellation) =>
                await _inventoryService.IsStockAvailableAsync(order.ProductId, order.Quantity))
            .WithMessage("庫存不足")
            .When(x => !string.IsNullOrEmpty(x.ProductId) && x.Quantity > 0);

        // 付款方式驗證（條件式非同步）
        RuleFor(x => x.PaymentMethod)
            .MustAsync(async (order, paymentMethod, cancellation) =>
                await _paymentService.IsPaymentMethodValidAsync(paymentMethod, order.Amount))
            .WithMessage("此付款方式不適用於此訂單金額")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod));
    }
}

public class OrderRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public interface IInventoryService
{
    Task<bool> IsStockAvailableAsync(string productId, int quantity);
}

public interface IPaymentService
{
    Task<bool> IsPaymentMethodValidAsync(string paymentMethod, decimal amount);
}

// ==================== 條件式非同步驗證測試 ====================

public class OrderValidatorTests
{
    private readonly IInventoryService _mockInventoryService;
    private readonly IPaymentService _mockPaymentService;
    private readonly OrderValidator _validator;

    public OrderValidatorTests()
    {
        _mockInventoryService = Substitute.For<IInventoryService>();
        _mockPaymentService = Substitute.For<IPaymentService>();
        _validator = new OrderValidator(_mockInventoryService, _mockPaymentService);
    }

    [Fact]
    public async Task ValidateAsync_庫存充足且付款方式有效_應該通過驗證()
    {
        // Arrange
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 5,
            PaymentMethod = "CreditCard",
            Amount = 1000m
        };

        _mockInventoryService.IsStockAvailableAsync("PROD001", 5)
                            .Returns(Task.FromResult(true));

        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 1000m)
                          .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(order);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        await _mockInventoryService.Received(1).IsStockAvailableAsync("PROD001", 5);
        await _mockPaymentService.Received(1).IsPaymentMethodValidAsync("CreditCard", 1000m);
    }

    [Fact]
    public async Task ValidateAsync_庫存不足_應該驗證失敗()
    {
        // Arrange
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 100,
            PaymentMethod = "CreditCard",
            Amount = 10000m
        };

        _mockInventoryService.IsStockAvailableAsync("PROD001", 100)
                            .Returns(Task.FromResult(false));

        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 10000m)
                          .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(order);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("庫存不足");
    }

    [Fact]
    public async Task ValidateAsync_產品ID為空_應該跳過庫存檢查()
    {
        // Arrange
        var order = new OrderRequest
        {
            ProductId = "", // 空的產品 ID
            Quantity = 5,
            PaymentMethod = "CreditCard",
            Amount = 1000m
        };

        // Mock 設定（不應該被呼叫）
        _mockInventoryService.IsStockAvailableAsync(Arg.Any<string>(), Arg.Any<int>())
                            .Returns(Task.FromResult(true));

        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 1000m)
                          .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(order);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);

        // 驗證沒有呼叫庫存檢查
        await _mockInventoryService.DidNotReceive().IsStockAvailableAsync(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ValidateAsync_付款方式不適用_應該驗證失敗()
    {
        // Arrange
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 1,
            PaymentMethod = "Cash",
            Amount = 100000m // 高額訂單不能使用現金
        };

        _mockInventoryService.IsStockAvailableAsync("PROD001", 1)
                            .Returns(Task.FromResult(true));

        _mockPaymentService.IsPaymentMethodValidAsync("Cash", 100000m)
                          .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(order);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod)
              .WithErrorMessage("此付款方式不適用於此訂單金額");
    }

    [Fact]
    public async Task ValidateAsync_同時驗證多個非同步規則_效能測試()
    {
        // Arrange
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 5,
            PaymentMethod = "CreditCard",
            Amount = 1000m
        };

        // Mock 設定帶延遲以模擬真實情況
        _mockInventoryService.IsStockAvailableAsync("PROD001", 5)
                            .Returns(async _ =>
                            {
                                await Task.Delay(100);
                                return true;
                            });

        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 1000m)
                          .Returns(async _ =>
                          {
                              await Task.Delay(100);
                              return true;
                          });

        // Act
        var startTime = DateTime.UtcNow;
        var result = await _validator.TestValidateAsync(order);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // 驗證非同步驗證是否並行執行（總時間應該接近較長的一個，而非兩者相加）
        Assert.True(elapsed.TotalMilliseconds < 300, 
            $"驗證時間 {elapsed.TotalMilliseconds}ms 過長，可能未並行執行");
    }
}
