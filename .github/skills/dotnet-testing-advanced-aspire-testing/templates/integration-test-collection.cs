namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// 整合測試集合定義
/// 使用 Collection Fixture 在所有測試類別間共享 AspireAppFixture
/// 避免每個測試類別重複啟動容器，提升測試效能
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<AspireAppFixture>
{
    /// <summary>
    /// 測試集合名稱
    /// </summary>
    public const string Name = "Integration Tests";
    
    // 這個類別不需要實作任何程式碼
    // 它只是用來定義 Collection Fixture
    // 所有標記為 [Collection("Integration Tests")] 的測試類別
    // 都會共享同一個 AspireAppFixture 實例
}

// 使用方式：
// [Collection(IntegrationTestCollection.Name)]
// public class MyControllerTests : IntegrationTestBase
// {
//     public MyControllerTests(AspireAppFixture fixture) : base(fixture) { }
// }
