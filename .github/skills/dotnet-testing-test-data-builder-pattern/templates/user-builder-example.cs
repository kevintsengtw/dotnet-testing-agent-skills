using System;
using System.Collections.Generic;
using System.Linq;

namespace TestDataBuilderPattern.Examples
{
    // ===== Domain Models =====
    
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string[] Roles { get; set; }
        public UserSettings Settings { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
    
    public class UserSettings
    {
        public string Theme { get; set; }
        public string Language { get; set; }
        public string[] FeatureFlags { get; set; }
    }
    
    // ===== User Builder Implementation =====
    
    public class UserBuilder
    {
        // 預設值：提供所有屬性的合理預設值
        private int _id = 1;
        private string _name = "Default User";
        private string _email = "default@example.com";
        private int _age = 25;
        private List<string> _roles = new();
        private UserSettings _settings = new() 
        { 
            Theme = "Light", 
            Language = "en-US",
            FeatureFlags = Array.Empty<string>()
        };
        private bool _isActive = true;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _modifiedAt = DateTime.UtcNow;
        
        // With* 方法：流暢介面設定個別屬性
        public UserBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
        
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
            _modifiedAt = createdAt;
            return this;
        }
        
        public UserBuilder ModifiedOn(DateTime modifiedAt)
        {
            _modifiedAt = modifiedAt;
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
        
        public static UserBuilder APremiumUser() => new UserBuilder()
            .WithRoles("Premium", "User")
            .WithSettings(new UserSettings 
            { 
                Theme = "Dark", 
                Language = "en-US",
                FeatureFlags = new[] { "AdvancedSearch", "PrioritySupport" }
            });
        
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
        
        public UserBuilder WithDarkTheme()
        {
            _settings.Theme = "Dark";
            return this;
        }
        
        // Build 方法：建立最終物件
        public User Build()
        {
            return new User
            {
                Id = _id,
                Name = _name,
                Email = _email,
                Age = _age,
                Roles = _roles.ToArray(),
                Settings = _settings,
                IsActive = _isActive,
                CreatedAt = _createdAt,
                ModifiedAt = _modifiedAt
            };
        }
    }
    
    // ===== Usage Examples =====
    
    public class UserBuilderExamples
    {
        public void Example1_BasicUsage()
        {
            // 基本使用：建立一個使用預設值的使用者
            var user = UserBuilder.AUser().Build();
            
            Console.WriteLine($"User: {user.Name}, Email: {user.Email}");
            // Output: User: Default User, Email: default@example.com
        }
        
        public void Example2_CustomizeProperties()
        {
            // 自訂屬性：只修改需要的屬性
            var user = UserBuilder.AUser()
                .WithName("John Doe")
                .WithEmail("john.doe@company.com")
                .WithAge(30)
                .Build();
            
            Console.WriteLine($"User: {user.Name}, Age: {user.Age}");
            // Output: User: John Doe, Age: 30
        }
        
        public void Example3_PresetScenarios()
        {
            // 使用預設情境：快速建立特定類型的使用者
            var adminUser = UserBuilder.AnAdminUser()
                .WithName("Admin Smith")
                .Build();
            
            Console.WriteLine($"Roles: {string.Join(", ", adminUser.Roles)}");
            // Output: Roles: Admin, User
            
            var premiumUser = UserBuilder.APremiumUser()
                .WithName("Premium Jones")
                .Build();
            
            Console.WriteLine($"Features: {string.Join(", ", premiumUser.Settings.FeatureFlags)}");
            // Output: Features: AdvancedSearch, PrioritySupport
        }
        
        public void Example4_FluentChaining()
        {
            // 流暢介面鏈式呼叫：多個設定串連
            var user = UserBuilder.AUser()
                .WithName("Alice")
                .WithValidEmail()  // 根據名稱自動產生 Email
                .WithAge(28)
                .WithRole("Manager")
                .WithDarkTheme()
                .Build();
            
            Console.WriteLine($"Email: {user.Email}, Theme: {user.Settings.Theme}");
            // Output: Email: alice@example.com, Theme: Dark
        }
        
        public void Example5_InvalidScenarios()
        {
            // 建立無效資料用於測試驗證邏輯
            var userWithEmptyName = UserBuilder.AUser()
                .WithName("")
                .Build();
            
            var userWithInvalidEmail = UserBuilder.AUser()
                .WithEmail("invalid-email")
                .Build();
            
            var tooYoungUser = UserBuilder.AUser()
                .WithAge(10)
                .Build();
            
            // 這些物件可用於測試驗證器的錯誤處理
        }
    }
}
