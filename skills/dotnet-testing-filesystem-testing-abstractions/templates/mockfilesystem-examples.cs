// =============================================================================
// MockFileSystem 測試範例
// Testing with MockFileSystem from System.IO.Abstractions.TestingHelpers
// =============================================================================

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace FileSystemTestingExamples.Tests;

#region 基礎 MockFileSystem 測試

/// <summary>
/// ConfigurationService 測試類別
/// 示範如何使用 MockFileSystem 進行檔案操作測試
/// </summary>
public class ConfigurationServiceTests
{
    [Fact]
    public async Task LoadConfigurationAsync_檔案存在_應回傳檔案內容()
    {
        // Arrange - 使用 Dictionary 初始化模擬檔案系統
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["config.json"] = new MockFileData("{ \"key\": \"value\" }")
        });
        
        var service = new ConfigurationService(mockFileSystem);
        
        // Act
        var result = await service.LoadConfigurationAsync("config.json");
        
        // Assert
        result.Should().Be("{ \"key\": \"value\" }");
    }
    
    [Fact]
    public async Task LoadConfigurationAsync_檔案不存在_應回傳預設值()
    {
        // Arrange - 空的檔案系統
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigurationService(mockFileSystem);
        var defaultValue = "default_config";
        
        // Act
        var result = await service.LoadConfigurationAsync("nonexistent.json", defaultValue);
        
        // Assert
        result.Should().Be(defaultValue);
    }
    
    [Fact]
    public async Task SaveConfigurationAsync_指定內容_應正確寫入檔案()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigurationService(mockFileSystem);
        var configPath = "config.json";
        var content = "{ \"setting\": true }";
        
        // Act
        await service.SaveConfigurationAsync(configPath, content);
        
        // Assert - 驗證檔案系統的最終狀態
        mockFileSystem.File.Exists(configPath).Should().BeTrue();
        var savedContent = await mockFileSystem.File.ReadAllTextAsync(configPath);
        savedContent.Should().Be(content);
    }
    
    [Fact]
    public async Task SaveConfigurationAsync_目錄不存在_應自動建立目錄()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigurationService(mockFileSystem);
        var configPath = @"C:\configs\app\settings.json";
        
        // Act
        await service.SaveConfigurationAsync(configPath, "content");
        
        // Assert
        mockFileSystem.Directory.Exists(@"C:\configs\app").Should().BeTrue();
        mockFileSystem.File.Exists(configPath).Should().BeTrue();
    }
    
    [Fact]
    public async Task LoadJsonConfigurationAsync_有效JSON_應正確反序列化()
    {
        // Arrange
        var settings = new AppSettings
        {
            ApplicationName = "Test App",
            Version = "1.0.0"
        };
        var json = JsonSerializer.Serialize(settings);
        
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["settings.json"] = new MockFileData(json)
        });
        
        var service = new ConfigurationService(mockFileSystem);
        
        // Act
        var result = await service.LoadJsonConfigurationAsync<AppSettings>("settings.json");
        
        // Assert
        result.Should().NotBeNull();
        result!.ApplicationName.Should().Be("Test App");
        result.Version.Should().Be("1.0.0");
    }
    
    [Fact]
    public async Task LoadJsonConfigurationAsync_無效JSON_應回傳Null()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["invalid.json"] = new MockFileData("{ invalid json }")
        });
        
        var service = new ConfigurationService(mockFileSystem);
        
        // Act
        var result = await service.LoadJsonConfigurationAsync<AppSettings>("invalid.json");
        
        // Assert
        result.Should().BeNull();
    }
}

/// <summary>
/// 測試用的設定類別
/// </summary>
public class AppSettings
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

#endregion

#region 目錄操作測試

/// <summary>
/// FileManagerService 測試類別
/// 示範目錄操作和檔案資訊的測試
/// </summary>
public class FileManagerServiceTests
{
    [Fact]
    public void CopyFileToDirectory_檔案存在_應成功複製()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\source\test.txt"] = new MockFileData("test content")
        });
        
        var service = new FileManagerService(mockFileSystem);
        
        // Act
        var result = service.CopyFileToDirectory(@"C:\source\test.txt", @"C:\target");
        
        // Assert
        result.Should().Be(@"C:\target\test.txt");
        mockFileSystem.File.Exists(@"C:\target\test.txt").Should().BeTrue();
        mockFileSystem.File.ReadAllText(@"C:\target\test.txt").Should().Be("test content");
    }
    
    [Fact]
    public void CopyFileToDirectory_目標目錄不存在_應自動建立()
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
    
    [Fact]
    public void CopyFileToDirectory_來源檔案不存在_應拋出FileNotFoundException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);
        
        // Act & Assert
        var action = () => service.CopyFileToDirectory(@"C:\nonexistent.txt", @"C:\target");
        action.Should().Throw<FileNotFoundException>()
              .WithMessage("*來源檔案不存在*");
    }
    
    [Fact]
    public void BackupFile_檔案存在_應建立時間戳記備份()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\data\important.txt"] = new MockFileData("important data")
        });
        
        var service = new FileManagerService(mockFileSystem);
        
        // Act
        var backupPath = service.BackupFile(@"C:\data\important.txt");
        
        // Assert
        backupPath.Should().StartWith(@"C:\data\important_");
        backupPath.Should().EndWith(".txt");
        mockFileSystem.File.Exists(backupPath).Should().BeTrue();
        mockFileSystem.File.ReadAllText(backupPath).Should().Be("important data");
    }
    
    [Fact]
    public void BackupFile_檔案不存在_應拋出FileNotFoundException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);
        
        // Act & Assert
        var action = () => service.BackupFile(@"C:\nonexistent.txt");
        action.Should().Throw<FileNotFoundException>();
    }
    
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
        var result = service.GetFileInfo(@"C:\test.txt");
        
        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("test.txt");
        result.Size.Should().Be(content.Length);
    }
    
    [Fact]
    public void GetFileInfo_檔案不存在_應回傳Null()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);
        
        // Act
        var result = service.GetFileInfo(@"C:\nonexistent.txt");
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void ListFiles_目錄有檔案_應回傳檔案清單()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\data\file1.txt"] = new MockFileData("content1"),
            [@"C:\data\file2.txt"] = new MockFileData("content2"),
            [@"C:\data\file3.csv"] = new MockFileData("content3"),
            [@"C:\other\file4.txt"] = new MockFileData("content4")
        });
        
        var service = new FileManagerService(mockFileSystem);
        
        // Act
        var result = service.ListFiles(@"C:\data", "*.txt").ToList();
        
        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.EndsWith("file1.txt"));
        result.Should().Contain(f => f.EndsWith("file2.txt"));
    }
    
    [Fact]
    public void ListFiles_目錄不存在_應回傳空清單()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);
        
        // Act
        var result = service.ListFiles(@"C:\nonexistent");
        
        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region 使用 NSubstitute 模擬錯誤情境

/// <summary>
/// 使用 NSubstitute 測試錯誤處理
/// 當需要模擬特定異常時，NSubstitute 比 MockFileSystem 更靈活
/// </summary>
public class FilePermissionServiceTests
{
    [Fact]
    public void TryReadFile_檔案存在且可讀_應回傳True並輸出內容()
    {
        // Arrange - 使用 MockFileSystem（正常情境）
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["readable.txt"] = new MockFileData("file content")
        });
        
        var service = new FilePermissionService(mockFileSystem);
        
        // Act
        var result = service.TryReadFile("readable.txt", out var content);
        
        // Assert
        result.Should().BeTrue();
        content.Should().Be("file content");
    }
    
    [Fact]
    public void TryReadFile_檔案不存在_應回傳False()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FilePermissionService(mockFileSystem);
        
        // Act
        var result = service.TryReadFile("nonexistent.txt", out var content);
        
        // Assert
        result.Should().BeFalse();
        content.Should().BeNull();
    }
    
    [Fact]
    public void TryReadFile_權限不足_應回傳False()
    {
        // Arrange - 使用 NSubstitute 模擬權限異常
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
    
    [Fact]
    public void TryReadFile_檔案被鎖定_應回傳False()
    {
        // Arrange - 使用 NSubstitute 模擬 IO 異常
        var mockFileSystem = Substitute.For<IFileSystem>();
        var mockFile = Substitute.For<IFile>();
        
        mockFileSystem.File.Returns(mockFile);
        mockFile.Exists("locked.txt").Returns(true);
        mockFile.ReadAllText("locked.txt")
                .Throws(new IOException("檔案被其他程序使用中"));
        
        var service = new FilePermissionService(mockFileSystem);
        
        // Act
        var result = service.TryReadFile("locked.txt", out var content);
        
        // Assert
        result.Should().BeFalse();
        content.Should().BeNull();
    }
    
    [Fact]
    public async Task TrySaveFileAsync_正常寫入_應回傳True()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FilePermissionService(mockFileSystem);
        
        // Act
        var result = await service.TrySaveFileAsync("output.txt", "content");
        
        // Assert
        result.Should().BeTrue();
        mockFileSystem.File.Exists("output.txt").Should().BeTrue();
    }
    
    [Fact]
    public async Task TrySaveFileAsync_目錄不存在但可建立_應成功寫入()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FilePermissionService(mockFileSystem);
        
        // Act
        var result = await service.TrySaveFileAsync(@"C:\new\folder\file.txt", "content");
        
        // Assert
        result.Should().BeTrue();
        mockFileSystem.File.Exists(@"C:\new\folder\file.txt").Should().BeTrue();
    }
}

#endregion

#region 進階測試模式

/// <summary>
/// 進階測試模式示範
/// </summary>
public class AdvancedFileSystemTestPatterns
{
    /// <summary>
    /// 測試複雜的目錄結構
    /// </summary>
    [Fact]
    public void ComplexDirectoryStructure_應正確處理()
    {
        // Arrange - 建立複雜的目錄結構
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\app\configs\app.json"] = new MockFileData("""
                {
                  "apiUrl": "https://api.test.com",
                  "timeout": 30
                }
                """),
            [@"C:\app\logs\app.log"] = new MockFileData("2024-01-01 10:00:00 INFO Application started"),
            [@"C:\app\data\users.csv"] = new MockFileData("Name,Age\nJohn,25\nJane,30"),
            [@"C:\temp\"] = new MockDirectoryData()  // 空目錄
        });
        
        // Assert - 驗證結構
        mockFileSystem.Directory.Exists(@"C:\app\configs").Should().BeTrue();
        mockFileSystem.Directory.Exists(@"C:\app\logs").Should().BeTrue();
        mockFileSystem.Directory.Exists(@"C:\temp").Should().BeTrue();
        mockFileSystem.File.Exists(@"C:\app\configs\app.json").Should().BeTrue();
    }
    
    /// <summary>
    /// 使用 AddFile 動態新增檔案
    /// </summary>
    [Fact]
    public void AddFile_動態新增檔案_應可正確讀取()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        
        // Act - 動態新增檔案
        mockFileSystem.AddFile(@"C:\dynamic\file.txt", new MockFileData("dynamic content"));
        
        // Assert
        mockFileSystem.File.Exists(@"C:\dynamic\file.txt").Should().BeTrue();
        mockFileSystem.File.ReadAllText(@"C:\dynamic\file.txt").Should().Be("dynamic content");
    }
    
    /// <summary>
    /// 測試多個檔案名稱變化
    /// </summary>
    [Theory]
    [InlineData("simple.txt")]
    [InlineData("file with spaces.txt")]
    [InlineData("file-with-hyphens.txt")]
    [InlineData("file_with_underscores.txt")]
    [InlineData("檔案.txt")]  // 中文檔名
    public void CopyFile_各種檔案名稱_應正確處理(string fileName)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var sourceFile = $@"C:\source\{fileName}";
        mockFileSystem.AddFile(sourceFile, new MockFileData("test content"));
        
        var service = new FileManagerService(mockFileSystem);
        
        // Act
        var result = service.CopyFileToDirectory(sourceFile, @"C:\target");
        
        // Assert
        result.Should().Be($@"C:\target\{fileName}");
        mockFileSystem.File.Exists(result).Should().BeTrue();
    }
    
    /// <summary>
    /// 測試檔案系統隔離 - 每個測試應使用獨立的 MockFileSystem
    /// </summary>
    [Fact]
    public void FileSystemIsolation_測試之間互不影響()
    {
        // Arrange - 第一個隔離的檔案系統
        var mockFileSystem1 = new MockFileSystem();
        mockFileSystem1.AddFile("test.txt", new MockFileData("content1"));
        
        // Arrange - 第二個隔離的檔案系統
        var mockFileSystem2 = new MockFileSystem();
        mockFileSystem2.AddFile("test.txt", new MockFileData("content2"));
        
        // Assert - 兩個檔案系統互相獨立
        mockFileSystem1.File.ReadAllText("test.txt").Should().Be("content1");
        mockFileSystem2.File.ReadAllText("test.txt").Should().Be("content2");
    }
}

#endregion

#region 測試資料輔助類別

/// <summary>
/// 測試資料輔助類別
/// 用於建立可重用的測試檔案結構
/// </summary>
public static class FileTestDataHelper
{
    /// <summary>
    /// 建立標準的測試檔案結構
    /// </summary>
    public static Dictionary<string, MockFileData> CreateTestFileStructure()
    {
        return new Dictionary<string, MockFileData>
        {
            [@"C:\app\configs\app.json"] = new MockFileData("""
                {
                  "apiUrl": "https://api.test.com",
                  "timeout": 30
                }
                """),
            [@"C:\app\logs\app.log"] = new MockFileData("2024-01-01 10:00:00 INFO Application started"),
            [@"C:\app\data\users.csv"] = new MockFileData("Name,Age\nJohn,25\nJane,30"),
            [@"C:\temp\"] = new MockDirectoryData()
        };
    }
    
    /// <summary>
    /// 建立設定檔測試結構
    /// </summary>
    public static Dictionary<string, MockFileData> CreateConfigTestStructure()
    {
        return new Dictionary<string, MockFileData>
        {
            [@"C:\config\appsettings.json"] = new MockFileData("""
                {
                  "ConnectionStrings": {
                    "DefaultConnection": "Server=localhost;Database=TestDb;"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  }
                }
                """),
            [@"C:\config\appsettings.Development.json"] = new MockFileData("""
                {
                  "Logging": {
                    "LogLevel": {
                      "Default": "Debug"
                    }
                  }
                }
                """)
        };
    }
}

/// <summary>
/// 使用測試資料輔助類別的範例
/// </summary>
public class FileTestDataHelperUsageTests
{
    [Fact]
    public void 使用預定義的測試結構()
    {
        // Arrange - 使用輔助類別建立檔案系統
        var mockFileSystem = new MockFileSystem(FileTestDataHelper.CreateTestFileStructure());
        
        // Assert
        mockFileSystem.File.Exists(@"C:\app\configs\app.json").Should().BeTrue();
        mockFileSystem.Directory.Exists(@"C:\temp").Should().BeTrue();
    }
}

#endregion
