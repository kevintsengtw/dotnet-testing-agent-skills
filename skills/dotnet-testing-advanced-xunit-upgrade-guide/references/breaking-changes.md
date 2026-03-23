# xUnit 3.x 破壞性變更完整清單

## 1. 測試專案變成可執行檔

```xml
<!-- xUnit 2.x (Library) -->
<PropertyGroup>
  <OutputType>Library</OutputType>
</PropertyGroup>

<!-- xUnit 3.x (Exe) - 必須變更 -->
<PropertyGroup>
  <OutputType>Exe</OutputType>
</PropertyGroup>
```

## 2. async void 測試不再支援

```csharp
// ❌ xUnit 2.x - 3.x 中會失敗
[Fact]
public async void 測試某個非同步功能()
{
    var result = await SomeAsyncMethod();
    Assert.True(result);
}

// ✅ xUnit 3.x - 正確寫法
[Fact]
public async Task 測試某個非同步功能()
{
    var result = await SomeAsyncMethod();
    Assert.True(result);
}
```

## 3. IAsyncLifetime 變更

在 xUnit 3.x 中，`IAsyncLifetime` 繼承 `IAsyncDisposable`。如果同時實作 `IAsyncLifetime` 和 `IDisposable`，只會呼叫 `DisposeAsync`，不會呼叫 `Dispose`。

```csharp
// ⚠️ 需要注意的模式
public class MyTestClass : IAsyncLifetime, IDisposable
{
    public async Task InitializeAsync() { /* ... */ }
    public async Task DisposeAsync() { /* 會被呼叫 */ }
    public void Dispose() { /* 在 3.x 中不會被呼叫 */ }
}

// ✅ 建議：將清理邏輯統一放在 DisposeAsync
public class MyTestClass : IAsyncLifetime
{
    public async Task InitializeAsync() { /* 初始化 */ }
    public async Task DisposeAsync() { /* 所有清理邏輯 */ }
}
```

## 4. SkippableFact/SkippableTheory 移除

```csharp
// ❌ xUnit 2.x - 已移除
[SkippableFact]
public void 可跳過的測試()
{
    Skip.If(某個條件, "跳過原因");
    // 測試邏輯
}

// ✅ xUnit 3.x - 使用 Assert.Skip
[Fact]
public void 可跳過的測試()
{
    if (某個條件)
    {
        Assert.Skip("跳過原因");
    }
    // 測試邏輯
}
```

## 5. 僅支援 SDK-style 專案

檢查專案檔案開頭是否為：

```xml
<Project Sdk="Microsoft.NET.Sdk">
```

如果是傳統格式，必須先轉換為 SDK-style。

## 6. 自訂 DataAttribute 方法簽名變更

xUnit 3.x 中 `DataAttribute` 方法簽名已變更：`GetData(MethodInfo)` → `GetDataAsync(MethodInfo, DisposalTracker)`，回傳型別改為 `Task<IReadOnlyCollection<ITheoryDataRow>>`。
