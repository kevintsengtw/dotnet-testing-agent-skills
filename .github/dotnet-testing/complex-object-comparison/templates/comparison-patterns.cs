using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Xunit;

namespace DotNetTesting.ComplexObjectComparison.Templates;

/// <summary>
/// 複雜物件比對的常見模式與範例
/// </summary>
public class ComparisonPatterns
{
    #region Test Models

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public AuditInfo AuditInfo { get; set; } = new();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class AuditInfo
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class TreeNode
    {
        public string Value { get; set; } = string.Empty;
        public TreeNode? Parent { get; set; }
        public List<TreeNode> Children { get; set; } = new();
    }

    public class DataRecord
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsProcessed { get; set; }
    }

    #endregion

    #region 模式 1: 基本深層物件比對

    [Fact]
    public void Pattern1_深層物件完整比對_範例()
    {
        // Arrange
        var expected = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            TotalAmount = 1059.97m,
            Items = new List<OrderItem>
            {
                new() { Id = 1, ProductName = "Laptop", Quantity = 1, Price = 999.99m },
                new() { Id = 2, ProductName = "Mouse", Quantity = 2, Price = 29.99m }
            }
        };

        // Act
        var actual = GetOrderFromService(1);

        // Assert
        // 基本用法：完整深層比對
        actual.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region 模式 2: 排除動態欄位

    [Fact]
    public void Pattern2_排除時間戳記與自動生成欄位_範例()
    {
        // Arrange
        var original = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            TotalAmount = 999.99m,
            CreatedAt = DateTime.Now.AddDays(-1),
            UpdatedAt = DateTime.Now.AddDays(-1)
        };

        // Act
        var updated = UpdateOrder(original);

        // Assert
        // 排除會自動變動的欄位
        updated.Should().BeEquivalentTo(original, options =>
            options.Excluding(o => o.UpdatedAt)      // 自動更新時間
                   .Excluding(o => o.Status)         // 狀態可能改變
        );

        // 單獨驗證動態欄位
        updated.UpdatedAt.Should().BeAfter(original.UpdatedAt);
    }

    #endregion

    #region 模式 3: 巢狀物件排除

    [Fact]
    public void Pattern3_排除巢狀物件時間欄位_範例()
    {
        // Arrange
        var expected = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            CreatedAt = DateTime.Now,
            Items = new List<OrderItem>
            {
                new() { Id = 1, ProductName = "Laptop", AddedAt = DateTime.Now }
            },
            AuditInfo = new AuditInfo
            {
                CreatedBy = "system",
                CreatedAt = DateTime.Now
            }
        };

        // Act
        var actual = GetOrderFromService(1);

        // Assert
        // 使用路徑模式排除所有時間戳記
        actual.Should().BeEquivalentTo(expected, options =>
            options.Excluding(ctx => ctx.Path.EndsWith("At"))
                   .Excluding(ctx => ctx.Path.EndsWith("Time"))
        );
    }

    #endregion

    #region 模式 4: 循環參照處理

    [Fact]
    public void Pattern4_處理循環參照結構_範例()
    {
        // Arrange
        // 建立具有父子雙向參照的樹狀結構
        var parent = new TreeNode { Value = "Root" };
        var child1 = new TreeNode { Value = "Child1", Parent = parent };
        var child2 = new TreeNode { Value = "Child2", Parent = parent };
        parent.Children = new List<TreeNode> { child1, child2 };

        // Act
        var actualTree = GetTreeFromService("Root");

        // Assert
        // 處理循環參照
        actualTree.Should().BeEquivalentTo(parent, options =>
            options.IgnoringCyclicReferences()
                   .WithMaxRecursionDepth(10)
        );
    }

    #endregion

    #region 模式 5: 選擇性屬性比對

    [Fact]
    public void Pattern5_只比對關鍵屬性_範例()
    {
        // Arrange
        var expected = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            TotalAmount = 999.99m,
            // 其他屬性省略
        };

        // Act
        var actual = GetOrderFromService(1);

        // Assert
        // 只比對關鍵業務屬性
        actual.Should().BeEquivalentTo(expected, options =>
            options.Including(o => o.Id)
                   .Including(o => o.CustomerName)
                   .Including(o => o.TotalAmount)
        );
    }

    #endregion

    #region 模式 6: 集合順序控制

    [Fact]
    public void Pattern6_集合順序比對控制_範例()
    {
        // Arrange
        var expectedItems = new[]
        {
            new OrderItem { Id = 1, ProductName = "Laptop" },
            new OrderItem { Id = 2, ProductName = "Mouse" }
        };

        // Act
        var orderedItems = GetOrderedItems();
        var unorderedItems = GetUnorderedItems();

        // Assert
        // 嚴格順序比對
        orderedItems.Should().BeEquivalentTo(expectedItems, options =>
            options.WithStrictOrdering()
        );

        // 寬鬆比對（不考慮順序）
        unorderedItems.Should().BeEquivalentTo(expectedItems, options =>
            options.WithoutStrictOrdering()
        );
    }

    #endregion

    #region 模式 7: 大量資料抽樣驗證

    [Fact]
    public void Pattern7_大量資料抽樣驗證_範例()
    {
        // Arrange
        var largeDataset = Enumerable.Range(1, 100000)
            .Select(i => new DataRecord
            {
                Id = i,
                Value = $"Record_{i}",
                Timestamp = DateTime.Now
            })
            .ToList();

        // Act
        var processed = ProcessLargeDataset(largeDataset);

        // Assert
        // 1. 驗證總數
        processed.Should().HaveCount(largeDataset.Count);

        // 2. 抽樣驗證（驗證 1000 筆）
        var sampleSize = 1000;
        var sampleIndices = Enumerable.Range(0, sampleSize)
            .Select(i => Random.Shared.Next(processed.Count))
            .Distinct()
            .ToList();

        foreach (var index in sampleIndices)
        {
            processed[index].Should().BeEquivalentTo(largeDataset[index], options =>
                options.Excluding(r => r.Timestamp)
                       .Excluding(r => r.IsProcessed)
            );
        }

        // 3. 統計驗證
        processed.Count(r => r.IsProcessed).Should().Be(processed.Count);
    }

    #endregion

    #region 模式 8: Entity Framework 實體比對

    [Fact]
    public void Pattern8_EF實體比對_範例()
    {
        // Arrange
        var expected = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            TotalAmount = 999.99m
        };

        // Act
        var actual = GetOrderFromDatabase(1);

        // Assert
        // 排除 EF 追蹤屬性與導航屬性
        actual.Should().BeEquivalentTo(expected, options =>
            options.ExcludingMissingMembers()      // 排除額外的 EF 屬性
                   .Excluding(o => o.CreatedAt)
                   .Excluding(o => o.UpdatedAt)
                   .Excluding(o => o.AuditInfo)    // 可能的導航屬性
        );
    }

    #endregion

    #region 模式 9: AssertionScope 批次驗證

    [Fact]
    public void Pattern9_批次驗證多個物件_範例()
    {
        // Arrange
        var orders = GetMultipleOrders();

        // Assert
        // 使用 AssertionScope 一次顯示所有失敗
        using (new AssertionScope())
        {
            foreach (var order in orders)
            {
                order.Id.Should().BeGreaterThan(0, "訂單 ID 必須大於 0");
                order.CustomerName.Should().NotBeNullOrEmpty("客戶名稱不可為空");
                order.TotalAmount.Should().BeGreaterThan(0, "總金額必須大於 0");
                order.Items.Should().NotBeEmpty("訂單必須包含至少一個項目");
            }
        }
    }

    #endregion

    #region 模式 10: 錯誤訊息優化

    [Fact]
    public void Pattern10_提供有意義的錯誤訊息_範例()
    {
        // Arrange
        var expected = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            Status = "Pending"
        };

        // Act
        var actual = GetOrderFromService(1);

        // Assert
        // 使用 because 提供上下文
        actual.Should().BeEquivalentTo(expected, options =>
            options.Excluding(o => o.CreatedAt)
                   .Because("CreatedAt 是系統自動生成的時間戳記")
                   .Excluding(o => o.UpdatedAt)
                   .Because("UpdatedAt 會隨著訂單更新而變動")
        );

        // 個別屬性也可以加上說明
        actual.Status.Should().Be(expected.Status,
            "因為新建立的訂單預設狀態應為 Pending");
    }

    #endregion

    #region Helper Methods (Stub Implementations)

    private Order GetOrderFromService(int id) => new()
    {
        Id = id,
        CustomerName = "John Doe",
        TotalAmount = 1059.97m,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now,
        Status = "Pending",
        Items = new List<OrderItem>
        {
            new() { Id = 1, ProductName = "Laptop", Quantity = 1, Price = 999.99m, AddedAt = DateTime.Now },
            new() { Id = 2, ProductName = "Mouse", Quantity = 2, Price = 29.99m, AddedAt = DateTime.Now }
        },
        AuditInfo = new AuditInfo
        {
            CreatedBy = "system",
            CreatedAt = DateTime.Now
        }
    };

    private Order UpdateOrder(Order order)
    {
        order.UpdatedAt = DateTime.Now;
        order.Status = "Processing";
        return order;
    }

    private TreeNode GetTreeFromService(string value)
    {
        var parent = new TreeNode { Value = value };
        var child1 = new TreeNode { Value = "Child1", Parent = parent };
        var child2 = new TreeNode { Value = "Child2", Parent = parent };
        parent.Children = new List<TreeNode> { child1, child2 };
        return parent;
    }

    private List<OrderItem> GetOrderedItems() => new()
    {
        new() { Id = 1, ProductName = "Laptop" },
        new() { Id = 2, ProductName = "Mouse" }
    };

    private List<OrderItem> GetUnorderedItems() => new()
    {
        new() { Id = 2, ProductName = "Mouse" },
        new() { Id = 1, ProductName = "Laptop" }
    };

    private List<DataRecord> ProcessLargeDataset(List<DataRecord> data) =>
        data.Select(r => new DataRecord
        {
            Id = r.Id,
            Value = r.Value,
            Timestamp = DateTime.Now,
            IsProcessed = true
        }).ToList();

    private Order GetOrderFromDatabase(int id) => GetOrderFromService(id);

    private List<Order> GetMultipleOrders() => new()
    {
        new() { Id = 1, CustomerName = "John", TotalAmount = 999, Items = new() { new() { ProductName = "Item1" } } },
        new() { Id = 2, CustomerName = "Jane", TotalAmount = 1500, Items = new() { new() { ProductName = "Item2" } } }
    };

    #endregion
}
