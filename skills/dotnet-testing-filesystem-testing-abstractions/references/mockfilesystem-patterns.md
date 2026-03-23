# MockFileSystem 測試模式與進階技巧

## 模式一：預設檔案狀態

```csharp
[Fact]
public async Task LoadConfig_檔案存在_應回傳內容()
{
    // Arrange - 建立預設的檔案系統狀態
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        ["config.json"] = new MockFileData("{ \"key\": \"value\" }"),
        [@"C:\data\users.csv"] = new MockFileData("Name,Age\nJohn,25"),
        [@"C:\logs\"] = new MockDirectoryData()  // 空目錄
    });

    var service = new ConfigService(mockFileSystem);

    // Act
    var result = await service.LoadConfigAsync("config.json");

    // Assert
    result.Should().Contain("key");
}
```

## 模式二：驗證寫入結果

```csharp
[Fact]
public async Task SaveConfig_指定內容_應正確寫入()
{
    // Arrange
    var mockFileSystem = new MockFileSystem();
    var service = new ConfigService(mockFileSystem);

    // Act
    await service.SaveConfigAsync("output.json", "{ \"saved\": true }");

    // Assert - 驗證檔案系統的最終狀態
    mockFileSystem.File.Exists("output.json").Should().BeTrue();
    var content = await mockFileSystem.File.ReadAllTextAsync("output.json");
    content.Should().Contain("saved");
}
```

## 模式三：測試目錄操作

```csharp
[Fact]
public void CopyFile_目標目錄不存在_應自動建立()
{
    // Arrange
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        [@"C:\source\file.txt"] = new MockFileData("content")
    });
    var service = new FileManagerService(mockFileSystem);

    // Act
    service.CopyFileToDirectory(@"C:\source\file.txt", @"C:\target\subfolder");

    // Assert
    mockFileSystem.Directory.Exists(@"C:\target\subfolder").Should().BeTrue();
    mockFileSystem.File.Exists(@"C:\target\subfolder\file.txt").Should().BeTrue();
}
```

## 模式四：使用 NSubstitute 模擬錯誤

當需要模擬特定異常時，MockFileSystem 支援有限，可使用 NSubstitute：

```csharp
[Fact]
public void TryReadFile_權限不足_應回傳False()
{
    // Arrange
    var mockFileSystem = Substitute.For<IFileSystem>();
    var mockFile = Substitute.For<IFile>();

    mockFileSystem.File.Returns(mockFile);
    mockFile.Exists("protected.txt").Returns(true);
    mockFile.ReadAllText("protected.txt")
            .Throws(new UnauthorizedAccessException("存取被拒"));

    var service = new FilePermissionService(mockFileSystem);

    // Act
    var result = service.TryReadFile("protected.txt", out var content);

    // Assert
    result.Should().BeFalse();
    content.Should().BeNull();
}
```

---

## 進階測試技巧

### 串流操作測試

```csharp
[Fact]
public async Task CountLines_多行檔案_應回傳正確行數()
{
    // Arrange
    var content = "Line 1\nLine 2\nLine 3\nLine 4";
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        ["data.txt"] = new MockFileData(content)
    });

    var processor = new StreamProcessorService(mockFileSystem);

    // Act
    var result = await processor.CountLinesAsync("data.txt");

    // Assert
    result.Should().Be(4);
}
```

### 檔案資訊測試

```csharp
[Fact]
public void GetFileInfo_檔案存在_應回傳正確資訊()
{
    // Arrange
    var content = "Hello, World!";
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        [@"C:\test.txt"] = new MockFileData(content)
    });

    var service = new FileManagerService(mockFileSystem);

    // Act
    var info = service.GetFileInfo(@"C:\test.txt");

    // Assert
    info.Should().NotBeNull();
    info!.Name.Should().Be("test.txt");
    info.Size.Should().Be(content.Length);
}
```

### 備份檔案測試

```csharp
[Fact]
public void BackupFile_檔案存在_應建立時間戳記備份()
{
    // Arrange
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        [@"C:\data\important.txt"] = new MockFileData("重要資料")
    });

    var service = new FileManagerService(mockFileSystem);

    // Act
    var backupPath = service.BackupFile(@"C:\data\important.txt");

    // Assert
    backupPath.Should().StartWith(@"C:\data\important_");
    backupPath.Should().EndWith(".txt");
    mockFileSystem.File.Exists(backupPath).Should().BeTrue();
}
```
