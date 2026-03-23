# AutoFixture 基本使用範例

## Fixture 類別與 Create<T>()

`Fixture` 是 AutoFixture 的核心類別，提供自動產生測試資料的能力：

```csharp
using AutoFixture;

[Fact]
public void AutoFixture_基本使用_應產生有效資料()
{
    // Arrange
    var fixture = new Fixture();

    // Act - 產生基本型別
    var id = fixture.Create<int>();           // 隨機正整數
    var name = fixture.Create<string>();      // 類似 GUID 格式的字串
    var price = fixture.Create<decimal>();    // 隨機十進位數
    var isActive = fixture.Create<bool>();    // 隨機布林值
    var date = fixture.Create<DateTime>();    // 隨機日期時間
    var guid = fixture.Create<Guid>();        // 新的 GUID

    // Assert
    id.Should().BePositive();
    name.Should().NotBeNullOrEmpty();
    guid.Should().NotBe(Guid.Empty);
}
```

## CreateMany<T>() 產生集合

```csharp
[Fact]
public void CreateMany_產生集合_應有多個元素()
{
    var fixture = new Fixture();

    // 預設產生 3 個元素
    var products = fixture.CreateMany<Product>().ToList();

    // 指定數量
    var moreProducts = fixture.CreateMany<Product>(10).ToList();

    products.Should().HaveCount(3);
    moreProducts.Should().HaveCount(10);
}
```

## 複雜物件自動建構

AutoFixture 能夠自動建構複雜的物件結構：

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Address Address { get; set; }        // 巢狀物件
    public List<Order> Orders { get; set; }     // 集合屬性
}

[Fact]
public void 複雜物件_應完整建構所有層級()
{
    var fixture = new Fixture();

    var customer = fixture.Create<Customer>();

    // 所有屬性自動填入值
    customer.Should().NotBeNull();
    customer.Id.Should().BePositive();
    customer.Name.Should().NotBeNullOrEmpty();
    customer.Address.Should().NotBeNull();
    customer.Address.Street.Should().NotBeNullOrEmpty();
    customer.Orders.Should().NotBeEmpty();
}
```
