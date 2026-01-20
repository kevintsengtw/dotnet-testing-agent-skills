using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TestDataBuilderPattern.AdvancedScenarios
{
    // ===== 進階 Domain Models =====
    
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsAvailable { get; set; }
        public string Category { get; set; }
    }
    
    public class Order
    {
        public int Id { get; set; }
        public User Customer { get; set; }
        public List<Product> Products { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
    }
    
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
    
    // ===== Product Builder =====
    
    public class ProductBuilder
    {
        private int _id = 1;
        private string _name = "Default Product";
        private decimal _price = 100m;
        private int _stock = 10;
        private bool _isAvailable = true;
        private string _category = "General";
        
        public ProductBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
        
        public ProductBuilder WithName(string name)
        {
            _name = name;
            return this;
        }
        
        public ProductBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }
        
        public ProductBuilder WithStock(int stock)
        {
            _stock = stock;
            return this;
        }
        
        public ProductBuilder IsUnavailable()
        {
            _isAvailable = false;
            return this;
        }
        
        public ProductBuilder InCategory(string category)
        {
            _category = category;
            return this;
        }
        
        // 預設建立者
        public static ProductBuilder AProduct() => new();
        
        public static ProductBuilder AnExpensiveProduct() => new ProductBuilder()
            .WithPrice(1000m);
        
        public static ProductBuilder ADiscountedProduct() => new ProductBuilder()
            .WithPrice(50m);
        
        public static ProductBuilder AnOutOfStockProduct() => new ProductBuilder()
            .WithStock(0)
            .IsUnavailable();
        
        public Product Build()
        {
            return new Product
            {
                Id = _id,
                Name = _name,
                Price = _price,
                Stock = _stock,
                IsAvailable = _isAvailable,
                Category = _category
            };
        }
    }
    
    // ===== Order Builder (組合多個 Builder) =====
    
    public class OrderBuilder
    {
        private int _id = 1;
        private User _customer = UserBuilder.AUser().Build();
        private List<Product> _products = new();
        private decimal _totalAmount = 0m;
        private DateTime _orderDate = DateTime.UtcNow;
        private OrderStatus _status = OrderStatus.Pending;
        
        public OrderBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
        
        public OrderBuilder ForCustomer(User customer)
        {
            _customer = customer;
            return this;
        }
        
        public OrderBuilder WithProduct(Product product)
        {
            _products.Add(product);
            _totalAmount += product.Price;
            return this;
        }
        
        public OrderBuilder WithProducts(params Product[] products)
        {
            _products.AddRange(products);
            _totalAmount = _products.Sum(p => p.Price);
            return this;
        }
        
        public OrderBuilder OnDate(DateTime orderDate)
        {
            _orderDate = orderDate;
            return this;
        }
        
        public OrderBuilder WithStatus(OrderStatus status)
        {
            _status = status;
            return this;
        }
        
        // 預設建立者
        public static OrderBuilder AnOrder() => new();
        
        public static OrderBuilder ACompletedOrder() => new OrderBuilder()
            .WithStatus(OrderStatus.Delivered);
        
        public static OrderBuilder ACancelledOrder() => new OrderBuilder()
            .WithStatus(OrderStatus.Cancelled);
        
        public Order Build()
        {
            return new Order
            {
                Id = _id,
                Customer = _customer,
                Products = _products,
                TotalAmount = _totalAmount,
                OrderDate = _orderDate,
                Status = _status
            };
        }
    }
    
    // ===== 統一測試資料管理 =====
    
    public static class TestData
    {
        public static class Users
        {
            public static User John => UserBuilder.AUser()
                .WithName("John Doe")
                .WithEmail("john@example.com")
                .WithAge(30)
                .Build();
            
            public static User Admin => UserBuilder.AnAdminUser()
                .WithName("Admin User")
                .WithEmail("admin@company.com")
                .Build();
            
            public static User Premium => UserBuilder.APremiumUser()
                .WithName("Premium Customer")
                .WithEmail("premium@example.com")
                .Build();
        }
        
        public static class Products
        {
            public static Product Laptop => ProductBuilder.AProduct()
                .WithName("Laptop")
                .WithPrice(1000m)
                .WithStock(5)
                .InCategory("Electronics")
                .Build();
            
            public static Product Mouse => ProductBuilder.AProduct()
                .WithName("Wireless Mouse")
                .WithPrice(50m)
                .WithStock(20)
                .InCategory("Electronics")
                .Build();
            
            public static Product Book => ProductBuilder.AProduct()
                .WithName("Programming Book")
                .WithPrice(40m)
                .WithStock(15)
                .InCategory("Books")
                .Build();
        }
    }
    
    // ===== 進階使用範例 =====
    
    public class AdvancedBuilderScenarios
    {
        public void Scenario1_BuilderComposition()
        {
            // 組合多個 Builder：建立複雜的訂單
            var order = OrderBuilder.AnOrder()
                .ForCustomer(UserBuilder.APremiumUser()
                    .WithName("Alice Premium")
                    .Build())
                .WithProducts(
                    ProductBuilder.AProduct().WithName("Laptop").WithPrice(1000m).Build(),
                    ProductBuilder.AProduct().WithName("Mouse").WithPrice(50m).Build()
                )
                .WithStatus(OrderStatus.Confirmed)
                .Build();
            
            Console.WriteLine($"Order Total: ${order.TotalAmount}, Customer: {order.Customer.Name}");
            // Output: Order Total: $1050, Customer: Alice Premium
        }
        
        public void Scenario2_UsingSharedTestData()
        {
            // 使用統一管理的測試資料
            var order = OrderBuilder.AnOrder()
                .ForCustomer(TestData.Users.John)
                .WithProducts(TestData.Products.Laptop, TestData.Products.Mouse)
                .Build();
            
            Console.WriteLine($"Products: {order.Products.Count}, Total: ${order.TotalAmount}");
            // Output: Products: 2, Total: $1050
        }
        
        public void Scenario3_EdgeCaseScenarios()
        {
            // 建立邊界情境的測試資料
            
            // 空訂單
            var emptyOrder = OrderBuilder.AnOrder()
                .ForCustomer(TestData.Users.John)
                .Build();
            
            // 高價訂單
            var expensiveOrder = OrderBuilder.AnOrder()
                .WithProducts(
                    ProductBuilder.AnExpensiveProduct().Build(),
                    ProductBuilder.AnExpensiveProduct().Build()
                )
                .Build();
            
            // 已取消的舊訂單
            var oldCancelledOrder = OrderBuilder.ACancelledOrder()
                .OnDate(DateTime.UtcNow.AddMonths(-6))
                .Build();
        }
    }
    
    // ===== xUnit Theory 整合範例 =====
    
    public class OrderValidationTests
    {
        [Theory]
        [MemberData(nameof(GetOrderScenarios))]
        public void ValidateOrder_不同訂單情境_應回傳正確驗證結果(Order order, bool expectedValid)
        {
            // Arrange
            var validator = new OrderValidator();
            
            // Act
            var isValid = validator.IsValid(order);
            
            // Assert
            Assert.Equal(expectedValid, isValid);
        }
        
        public static IEnumerable<object[]> GetOrderScenarios()
        {
            // ✅ 有效訂單：一般使用者購買產品
            yield return new object[]
            {
                OrderBuilder.AnOrder()
                    .ForCustomer(TestData.Users.John)
                    .WithProduct(TestData.Products.Laptop)
                    .Build(),
                true
            };
            
            // ❌ 無效訂單：空訂單（沒有產品）
            yield return new object[]
            {
                OrderBuilder.AnOrder()
                    .ForCustomer(TestData.Users.John)
                    .Build(),
                false
            };
            
            // ❌ 無效訂單：無效的客戶
            yield return new object[]
            {
                OrderBuilder.AnOrder()
                    .ForCustomer(UserBuilder.AUser().WithName("").Build())
                    .WithProduct(TestData.Products.Laptop)
                    .Build(),
                false
            };
            
            // ❌ 無效訂單：包含缺貨產品
            yield return new object[]
            {
                OrderBuilder.AnOrder()
                    .ForCustomer(TestData.Users.John)
                    .WithProduct(ProductBuilder.AnOutOfStockProduct().Build())
                    .Build(),
                false
            };
        }
    }
    
    // ===== Mock Validator (示範用) =====
    
    public class OrderValidator
    {
        public bool IsValid(Order order)
        {
            if (order.Products == null || !order.Products.Any())
                return false;
            
            if (string.IsNullOrWhiteSpace(order.Customer?.Name))
                return false;
            
            if (order.Products.Any(p => !p.IsAvailable))
                return false;
            
            return true;
        }
    }
}
