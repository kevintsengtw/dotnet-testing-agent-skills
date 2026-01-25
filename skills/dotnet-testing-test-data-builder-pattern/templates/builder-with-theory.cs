using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TestDataBuilderPattern.TheoryExamples
{
    // ===== Domain Models =====
    
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public CustomerType Type { get; set; }
        public decimal CreditLimit { get; set; }
        public bool IsVerified { get; set; }
    }
    
    public enum CustomerType
    {
        Regular,
        Premium,
        VIP
    }
    
    // ===== Customer Builder =====
    
    public class CustomerBuilder
    {
        private int _id = 1;
        private string _name = "Default Customer";
        private string _email = "customer@example.com";
        private CustomerType _type = CustomerType.Regular;
        private decimal _creditLimit = 1000m;
        private bool _isVerified = true;
        
        public CustomerBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
        
        public CustomerBuilder WithName(string name)
        {
            _name = name;
            return this;
        }
        
        public CustomerBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }
        
        public CustomerBuilder OfType(CustomerType type)
        {
            _type = type;
            return this;
        }
        
        public CustomerBuilder WithCreditLimit(decimal limit)
        {
            _creditLimit = limit;
            return this;
        }
        
        public CustomerBuilder Unverified()
        {
            _isVerified = false;
            return this;
        }
        
        // 預設建立者
        public static CustomerBuilder ACustomer() => new();
        
        public static CustomerBuilder ARegularCustomer() => new CustomerBuilder()
            .OfType(CustomerType.Regular)
            .WithCreditLimit(1000m);
        
        public static CustomerBuilder APremiumCustomer() => new CustomerBuilder()
            .OfType(CustomerType.Premium)
            .WithCreditLimit(5000m);
        
        public static CustomerBuilder AVIPCustomer() => new CustomerBuilder()
            .OfType(CustomerType.VIP)
            .WithCreditLimit(10000m);
        
        public Customer Build()
        {
            return new Customer
            {
                Id = _id,
                Name = _name,
                Email = _email,
                Type = _type,
                CreditLimit = _creditLimit,
                IsVerified = _isVerified
            };
        }
    }
    
    // ===== Builder 配合 xUnit Theory 測試 =====
    
    public class CustomerServiceTests
    {
        // 範例 1：使用 MemberData 配合 Builder 測試不同客戶類型
        [Theory]
        [MemberData(nameof(GetCustomerTypeScenarios))]
        public void CalculateDiscount_不同客戶類型_應回傳對應折扣(Customer customer, decimal expectedDiscount)
        {
            // Arrange
            var service = new CustomerService();
            
            // Act
            var discount = service.CalculateDiscount(customer);
            
            // Assert
            Assert.Equal(expectedDiscount, discount);
        }
        
        public static IEnumerable<object[]> GetCustomerTypeScenarios()
        {
            // Regular 客戶：無折扣
            yield return new object[]
            {
                CustomerBuilder.ARegularCustomer()
                    .WithName("Regular John")
                    .Build(),
                0m
            };
            
            // Premium 客戶：5% 折扣
            yield return new object[]
            {
                CustomerBuilder.APremiumCustomer()
                    .WithName("Premium Jane")
                    .Build(),
                0.05m
            };
            
            // VIP 客戶：10% 折扣
            yield return new object[]
            {
                CustomerBuilder.AVIPCustomer()
                    .WithName("VIP Alice")
                    .Build(),
                0.10m
            };
        }
        
        // 範例 2：測試客戶驗證邏輯
        [Theory]
        [MemberData(nameof(GetCustomerValidationScenarios))]
        public void ValidateCustomer_不同情境_應回傳正確驗證結果(Customer customer, bool expectedValid, string description)
        {
            // Arrange
            var validator = new CustomerValidator();
            
            // Act
            var isValid = validator.IsValid(customer);
            
            // Assert
            Assert.Equal(expectedValid, isValid);
        }
        
        public static IEnumerable<object[]> GetCustomerValidationScenarios()
        {
            // ✅ 有效客戶
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Valid Customer")
                    .WithEmail("valid@example.com")
                    .Build(),
                true,
                "有效的一般客戶"
            };
            
            // ❌ 無效客戶 - 空名稱
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("")
                    .WithEmail("test@example.com")
                    .Build(),
                false,
                "名稱為空"
            };
            
            // ❌ 無效客戶 - 無效 Email
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Test User")
                    .WithEmail("invalid-email")
                    .Build(),
                false,
                "Email 格式錯誤"
            };
            
            // ❌ 無效客戶 - 未驗證
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Unverified User")
                    .WithEmail("unverified@example.com")
                    .Unverified()
                    .Build(),
                false,
                "客戶未驗證"
            };
            
            // ❌ 無效客戶 - 信用額度為負
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Negative Credit")
                    .WithCreditLimit(-100m)
                    .Build(),
                false,
                "信用額度為負數"
            };
        }
        
        // 範例 3：測試信用額度核准邏輯
        [Theory]
        [MemberData(nameof(GetCreditApprovalScenarios))]
        public void ApproveCredit_不同額度與客戶類型_應回傳正確核准結果(
            Customer customer, 
            decimal requestAmount, 
            bool expectedApproved)
        {
            // Arrange
            var service = new CustomerService();
            
            // Act
            var approved = service.ApproveCreditRequest(customer, requestAmount);
            
            // Assert
            Assert.Equal(expectedApproved, approved);
        }
        
        public static IEnumerable<object[]> GetCreditApprovalScenarios()
        {
            // ✅ Regular 客戶 - 請求金額在限額內
            yield return new object[]
            {
                CustomerBuilder.ARegularCustomer().Build(),
                500m,
                true
            };
            
            // ❌ Regular 客戶 - 請求金額超過限額
            yield return new object[]
            {
                CustomerBuilder.ARegularCustomer().Build(),
                1500m,
                false
            };
            
            // ✅ Premium 客戶 - 請求較高金額
            yield return new object[]
            {
                CustomerBuilder.APremiumCustomer().Build(),
                4000m,
                true
            };
            
            // ❌ Premium 客戶 - 請求金額超過限額
            yield return new object[]
            {
                CustomerBuilder.APremiumCustomer().Build(),
                6000m,
                false
            };
            
            // ✅ VIP 客戶 - 請求高額
            yield return new object[]
            {
                CustomerBuilder.AVIPCustomer().Build(),
                9000m,
                true
            };
            
            // ❌ 未驗證客戶 - 拒絕任何請求
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .Unverified()
                    .Build(),
                100m,
                false
            };
        }
        
        // 範例 4：使用 ClassData 配合 Builder
        [Theory]
        [ClassData(typeof(CustomerUpgradeTestData))]
        public void UpgradeCustomer_符合條件_應升級客戶等級(
            Customer customer, 
            CustomerType expectedType)
        {
            // Arrange
            var service = new CustomerService();
            
            // Act
            var upgradedCustomer = service.UpgradeCustomer(customer);
            
            // Assert
            Assert.Equal(expectedType, upgradedCustomer.Type);
        }
    }
    
    // ===== ClassData 實作配合 Builder =====
    
    public class CustomerUpgradeTestData : TheoryData<Customer, CustomerType>
    {
        public CustomerUpgradeTestData()
        {
            // Regular -> Premium 升級條件：信用額度 >= 2000
            Add(
                CustomerBuilder.ARegularCustomer()
                    .WithCreditLimit(2000m)
                    .Build(),
                CustomerType.Premium
            );
            
            // Premium -> VIP 升級條件：信用額度 >= 7000
            Add(
                CustomerBuilder.APremiumCustomer()
                    .WithCreditLimit(7000m)
                    .Build(),
                CustomerType.VIP
            );
            
            // 不符合升級條件：維持原等級
            Add(
                CustomerBuilder.ARegularCustomer()
                    .WithCreditLimit(1000m)
                    .Build(),
                CustomerType.Regular
            );
        }
    }
    
    // ===== Mock Services (示範用) =====
    
    public class CustomerService
    {
        public decimal CalculateDiscount(Customer customer)
        {
            return customer.Type switch
            {
                CustomerType.Regular => 0m,
                CustomerType.Premium => 0.05m,
                CustomerType.VIP => 0.10m,
                _ => 0m
            };
        }
        
        public bool ApproveCreditRequest(Customer customer, decimal amount)
        {
            if (!customer.IsVerified)
                return false;
            
            return amount <= customer.CreditLimit;
        }
        
        public Customer UpgradeCustomer(Customer customer)
        {
            var newType = customer.Type;
            
            if (customer.Type == CustomerType.Regular && customer.CreditLimit >= 2000m)
                newType = CustomerType.Premium;
            else if (customer.Type == CustomerType.Premium && customer.CreditLimit >= 7000m)
                newType = CustomerType.VIP;
            
            return new Customer
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Type = newType,
                CreditLimit = customer.CreditLimit,
                IsVerified = customer.IsVerified
            };
        }
    }
    
    public class CustomerValidator
    {
        public bool IsValid(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
                return false;
            
            if (!IsValidEmail(customer.Email))
                return false;
            
            if (!customer.IsVerified)
                return false;
            
            if (customer.CreditLimit < 0)
                return false;
            
            return true;
        }
        
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains("@");
        }
    }
}
