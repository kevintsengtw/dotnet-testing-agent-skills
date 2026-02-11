# 測試實作範例

> 此文件從 [SKILL.md](../SKILL.md) 提取，包含 AutoFixture + NSubstitute 整合的完整測試實作範例。

---

## 基本測試：無需設定相依行為

當測試只需要驗證 SUT 本身的邏輯（如參數驗證）時：

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task IsExistsAsync_輸入的ShipperId為0時_應拋出ArgumentOutOfRangeException(
    ShipperService sut)
{
    // Arrange
    var shipperId = 0;

    // Act
    var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => sut.IsExistsAsync(shipperId));

    // Assert
    exception.Message.Should().Contain(nameof(shipperId));
}
```

## 進階測試：設定相依行為

使用 `[Frozen]` 取得相依性並設定其行為：

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task IsExistsAsync_輸入的ShipperId_資料不存在_應回傳false(
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut)
{
    // Arrange
    var shipperId = 99;
    shipperRepository.IsExistsAsync(Arg.Any<int>()).Returns(false);

    // Act
    var actual = await sut.IsExistsAsync(shipperId);

    // Assert
    actual.Should().BeFalse();
}
```

## 使用自動產生的測試資料

AutoFixture 同時產生 SUT 和測試資料：

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task GetAsync_輸入的ShipperId_資料有存在_應回傳model(
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut,
    ShipperModel model)  // AutoFixture 自動產生
{
    // Arrange
    var shipperId = model.ShipperId;
    shipperRepository.IsExistsAsync(Arg.Any<int>()).Returns(true);
    shipperRepository.GetAsync(Arg.Any<int>()).Returns(model);

    // Act
    var actual = await sut.GetAsync(shipperId);

    // Assert
    actual.Should().NotBeNull();
    actual.ShipperId.Should().Be(shipperId);
}
```

## 參數化測試：InlineAutoData

結合固定測試值與自動產生的 SUT：

```csharp
[Theory]
[InlineAutoDataWithCustomization(0, 10, nameof(from))]
[InlineAutoDataWithCustomization(-1, 10, nameof(from))]
[InlineAutoDataWithCustomization(1, 0, nameof(size))]
[InlineAutoDataWithCustomization(1, -1, nameof(size))]
public async Task GetCollectionAsync_from與size輸入不合規格內容_應拋出ArgumentOutOfRangeException(
    int from,
    int size,
    string parameterName,
    ShipperService sut)  // 自動產生
{
    // Act
    var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => sut.GetCollectionAsync(from, size));

    // Assert
    exception.Message.Should().Contain(parameterName);
}
```

## 使用 CollectionSize 控制集合大小

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task GetAllAsync_資料表裡有10筆資料_回傳的集合裡有10筆(
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut,
    [CollectionSize(10)] IEnumerable<ShipperModel> models)
{
    // Arrange
    shipperRepository.GetAllAsync().Returns(models);

    // Act
    var actual = await sut.GetAllAsync();

    // Assert
    actual.Should().NotBeEmpty();
    actual.Should().HaveCount(10);
}
```

## 複雜資料設定：使用 IFixture

當需要精確控制測試資料時：

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task SearchAsync_companyName輸入資料_有符合條件的資料_回傳集合應包含符合條件的資料(
    IFixture fixture,
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut)
{
    // Arrange
    const string companyName = "test";
    
    var models = fixture.Build<ShipperModel>()
                        .With(x => x.CompanyName, companyName)
                        .CreateMany(1);

    shipperRepository.GetTotalCountAsync().Returns(1);
    shipperRepository.SearchAsync(Arg.Any<string>(), Arg.Any<string>())
                     .Returns(models);

    // Act
    var actual = await sut.SearchAsync(companyName, string.Empty);

    // Assert
    actual.Should().NotBeEmpty();
    actual.Should().HaveCount(1);
    actual.Any(x => x.CompanyName == companyName).Should().BeTrue();
}
```

## Nullable 參考類型處理

測試 null 或空值參數時的處理方式：

```csharp
[Theory]
[InlineAutoDataWithCustomization(null!, null!)]
[InlineAutoDataWithCustomization("", "")]
[InlineAutoDataWithCustomization("   ", "   ")]
public async Task SearchAsync_companyName與phone都為空白_應拋出ArgumentException(
    string? companyName,
    string? phone,
    ShipperService sut)
{
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => sut.SearchAsync(companyName!, phone!));
    
    exception.Message.Should().Contain("companyName 與 phone 不可都為空白");
}
```

**處理說明**：

1. **參數宣告使用 `string?`**：因為測試需要傳入 `null` 值
2. **InlineAutoData 中使用 `null!`**：告訴編譯器這是刻意的測試資料
3. **方法呼叫使用 `!` 運算子**：在測試中使用 null-forgiving 運算子
