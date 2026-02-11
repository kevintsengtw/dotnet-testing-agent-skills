# 測試生命週期管理

## 建構式與 Dispose 模式

```csharp
public class BasicLifecycleTests : IDisposable
{
    private readonly Calculator _calculator;

    public BasicLifecycleTests()
    {
        _calculator = new Calculator();
    }

    [Test]
    public async Task Add_基本測試()
    {
        await Assert.That(_calculator.Add(1, 2)).IsEqualTo(3);
    }

    public void Dispose()
    {
        // 清理資源
    }
}
```

## Before / After 屬性

TUnit 提供更細緻的生命週期控制：

```csharp
public class LifecycleTests
{
    private static TestDatabase? _database;

    // 類別層級：所有測試執行前只執行一次
    [Before(Class)]
    public static async Task ClassSetup()
    {
        _database = new TestDatabase();
        await _database.InitializeAsync();
    }

    // 測試層級：每個測試執行前都會執行
    [Before(Test)]
    public async Task TestSetup()
    {
        await _database!.ClearDataAsync();
    }

    [Test]
    public async Task 測試使用者建立()
    {
        var userService = new UserService(_database!);
        var user = await userService.CreateUserAsync("test@example.com");
        await Assert.That(user.Id).IsNotEqualTo(Guid.Empty);
    }

    // 測試層級：每個測試執行後都會執行
    [After(Test)]
    public async Task TestTearDown()
    {
        // 記錄測試結果
    }

    // 類別層級：所有測試執行後只執行一次
    [After(Class)]
    public static async Task ClassTearDown()
    {
        if (_database != null)
        {
            await _database.DisposeAsync();
        }
    }
}
```

## 生命週期屬性種類

| 屬性                 | 類型     | 說明                     |
| -------------------- | -------- | ------------------------ |
| `[Before(Test)]`     | 實例方法 | 每個測試執行前           |
| `[Before(Class)]`    | 靜態方法 | 類別中第一個測試執行前   |
| `[Before(Assembly)]` | 靜態方法 | 組件中第一個測試執行前   |
| `[After(Test)]`      | 實例方法 | 每個測試執行後           |
| `[After(Class)]`     | 靜態方法 | 類別中最後一個測試執行後 |
| `[After(Assembly)]`  | 靜態方法 | 組件中最後一個測試執行後 |

## 執行順序

```text
1. Before(Class)
2. 建構式
3. Before(Test)
4. 測試方法
5. After(Test)
6. Dispose
7. After(Class)
```
