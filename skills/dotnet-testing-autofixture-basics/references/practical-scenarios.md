# AutoFixture 實務應用場景：完整程式碼範例

> 本文件從 SKILL.md `## 實務應用場景` 提取，包含 Entity 測試、DTO 驗證、大量資料測試的完整程式碼範例。

## Entity 測試

```csharp
[Theory]
[InlineData(0, CustomerLevel.Bronze)]
[InlineData(15000, CustomerLevel.Silver)]
[InlineData(60000, CustomerLevel.Gold)]
[InlineData(120000, CustomerLevel.Diamond)]
public void GetLevel_不同消費金額_應回傳正確等級(decimal totalSpent, CustomerLevel expected)
{
    var fixture = new Fixture();
    var customer = fixture.Build<Customer>()
        .With(x => x.TotalSpent, totalSpent)
        .Create();

    var level = customer.GetLevel();

    level.Should().Be(expected);
}
```

## DTO 驗證

```csharp
[Fact]
public void ValidateRequest_有效資料_應通過驗證()
{
    var fixture = new Fixture();
    
    var request = fixture.Build<CreateCustomerRequest>()
        .With(x => x.Name, fixture.Create<string>()[..50])
        .With(x => x.Email, fixture.Create<MailAddress>().Address)
        .With(x => x.Age, Random.Shared.Next(18, 78))
        .Create();

    var context = new ValidationContext(request);
    var results = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(request, context, results, true);

    isValid.Should().BeTrue();
}
```

## 大量資料測試

```csharp
[Fact]
public void ProcessBatch_大量資料_應正確處理()
{
    var fixture = new Fixture();
    var records = fixture.CreateMany<DataRecord>(1000).ToList();
    var processor = new DataProcessor();

    var stopwatch = Stopwatch.StartNew();
    var result = processor.ProcessBatch(records);
    stopwatch.Stop();

    result.ProcessedCount.Should().Be(1000);
    result.ErrorCount.Should().Be(0);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
}
```
