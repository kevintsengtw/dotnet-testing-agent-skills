// =============================================================================
// 串流處理與設定檔管理實務範例
// Stream Processing and Configuration Management Examples
// =============================================================================

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using Xunit;

namespace FileSystemTestingExamples;

#region 串流處理服務

/// <summary>
/// 串流處理服務
/// 示範如何處理大型檔案，使用串流而非一次載入整個檔案
/// </summary>
public class StreamProcessorService
{
    private readonly IFileSystem _fileSystem;
    
    public StreamProcessorService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    /// <summary>
    /// 計算檔案行數（使用串流，記憶體效率高）
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <returns>行數</returns>
    public async Task<int> CountLinesAsync(string filePath)
    {
        using var stream = _fileSystem.File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        
        int lineCount = 0;
        while (await reader.ReadLineAsync() != null)
        {
            lineCount++;
        }
        
        return lineCount;
    }
    
    /// <summary>
    /// 逐行處理大型檔案
    /// </summary>
    /// <param name="inputPath">輸入檔案路徑</param>
    /// <param name="outputPath">輸出檔案路徑</param>
    /// <param name="processor">每行的處理函式</param>
    public async Task ProcessLargeFileAsync(
        string inputPath, 
        string outputPath, 
        Func<string, string> processor)
    {
        using var inputStream = _fileSystem.File.OpenRead(inputPath);
        using var outputStream = _fileSystem.File.Create(outputPath);
        using var reader = new StreamReader(inputStream);
        using var writer = new StreamWriter(outputStream);
        
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var processedLine = processor(line);
            await writer.WriteLineAsync(processedLine);
        }
    }
    
    /// <summary>
    /// 取得檔案統計資訊
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <returns>統計資訊</returns>
    public async Task<FileStatistics> GetFileStatisticsAsync(string filePath)
    {
        var stats = new FileStatistics();
        
        using var stream = _fileSystem.File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            stats.LineCount++;
            stats.CharacterCount += line.Length;
            stats.WordCount += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
        
        return stats;
    }
    
    /// <summary>
    /// 檔案統計資訊
    /// </summary>
    public class FileStatistics
    {
        public int LineCount { get; set; }
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
    }
}

#endregion

#region 設定檔管理服務

/// <summary>
/// 整合設定檔管理服務
/// 示範完整的設定檔生命週期管理
/// </summary>
public class ConfigManagerService
{
    private readonly IFileSystem _fileSystem;
    private readonly string _configDirectory;
    
    public ConfigManagerService(IFileSystem fileSystem, string configDirectory = "config")
    {
        _fileSystem = fileSystem;
        _configDirectory = configDirectory;
    }
    
    /// <summary>
    /// 初始化設定目錄
    /// </summary>
    public void InitializeConfigDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_configDirectory) && 
            !_fileSystem.Directory.Exists(_configDirectory))
        {
            _fileSystem.Directory.CreateDirectory(_configDirectory);
        }
    }
    
    /// <summary>
    /// 載入應用程式設定
    /// </summary>
    /// <returns>應用程式設定</returns>
    public async Task<AppSettings> LoadAppSettingsAsync()
    {
        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");

        if (!_fileSystem.File.Exists(configPath))
        {
            // 檔案不存在時建立預設設定
            var defaultSettings = new AppSettings();
            await SaveAppSettingsAsync(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var jsonContent = await _fileSystem.File.ReadAllTextAsync(configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(jsonContent);
            return settings ?? new AppSettings();
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }
    
    /// <summary>
    /// 儲存應用程式設定
    /// </summary>
    /// <param name="settings">應用程式設定</param>
    public async Task SaveAppSettingsAsync(AppSettings settings)
    {
        InitializeConfigDirectory();

        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");
        var jsonContent = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await _fileSystem.File.WriteAllTextAsync(configPath, jsonContent);
    }
    
    /// <summary>
    /// 備份現有設定
    /// </summary>
    /// <returns>備份檔案路徑</returns>
    public string BackupConfiguration()
    {
        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");

        if (!_fileSystem.File.Exists(configPath))
        {
            throw new FileNotFoundException("找不到要備份的設定檔案");
        }

        var backupDirectory = _fileSystem.Path.Combine(_configDirectory, "backup");
        if (!_fileSystem.Directory.Exists(backupDirectory))
        {
            _fileSystem.Directory.CreateDirectory(backupDirectory);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"appsettings_{timestamp}.json";
        var backupPath = _fileSystem.Path.Combine(backupDirectory, backupFileName);

        _fileSystem.File.Copy(configPath, backupPath);
        return backupPath;
    }
    
    /// <summary>
    /// 列出所有備份
    /// </summary>
    /// <returns>備份檔案路徑清單</returns>
    public IEnumerable<string> ListBackups()
    {
        var backupDirectory = _fileSystem.Path.Combine(_configDirectory, "backup");
        
        if (!_fileSystem.Directory.Exists(backupDirectory))
        {
            return Enumerable.Empty<string>();
        }
        
        return _fileSystem.Directory.GetFiles(backupDirectory, "appsettings_*.json")
                          .OrderByDescending(f => f);
    }
    
    /// <summary>
    /// 從備份還原設定
    /// </summary>
    /// <param name="backupPath">備份檔案路徑</param>
    public async Task RestoreFromBackupAsync(string backupPath)
    {
        if (!_fileSystem.File.Exists(backupPath))
        {
            throw new FileNotFoundException($"備份檔案不存在: {backupPath}");
        }
        
        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");
        var content = await _fileSystem.File.ReadAllTextAsync(backupPath);
        await _fileSystem.File.WriteAllTextAsync(configPath, content);
    }

    /// <summary>
    /// 應用程式設定
    /// </summary>
    public class AppSettings
    {
        public string ApplicationName { get; set; } = "FileSystem Testing Demo";
        public string Version { get; set; } = "1.0.0";
        public DatabaseSettings Database { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }

    /// <summary>
    /// 資料庫設定
    /// </summary>
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = "Server=localhost;Database=TestDb;";
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// 日誌設定
    /// </summary>
    public class LoggingSettings
    {
        public string Level { get; set; } = "Information";
        public bool EnableFileLogging { get; set; } = true;
        public string LogDirectory { get; set; } = "logs";
    }
}

#endregion

#region 測試類別

/// <summary>
/// 串流處理服務測試
/// </summary>
public class StreamProcessorServiceTests
{
    [Fact]
    public async Task CountLinesAsync_多行檔案_應回傳正確行數()
    {
        // Arrange
        var content = "Line 1\nLine 2\nLine 3\nLine 4";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["test.txt"] = new MockFileData(content)
        });
        
        var service = new StreamProcessorService(mockFileSystem);
        
        // Act
        var result = await service.CountLinesAsync("test.txt");
        
        // Assert
        result.Should().Be(4);
    }
    
    [Fact]
    public async Task CountLinesAsync_空檔案_應回傳零()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["empty.txt"] = new MockFileData("")
        });
        
        var service = new StreamProcessorService(mockFileSystem);
        
        // Act
        var result = await service.CountLinesAsync("empty.txt");
        
        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public async Task ProcessLargeFileAsync_處理每一行_應正確轉換並寫入()
    {
        // Arrange
        var inputContent = "hello\nworld\ntest";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["input.txt"] = new MockFileData(inputContent)
        });
        
        var service = new StreamProcessorService(mockFileSystem);
        
        // Act - 將每一行轉換為大寫
        await service.ProcessLargeFileAsync("input.txt", "output.txt", line => line.ToUpper());
        
        // Assert
        var outputContent = mockFileSystem.File.ReadAllText("output.txt");
        outputContent.Should().Contain("HELLO");
        outputContent.Should().Contain("WORLD");
        outputContent.Should().Contain("TEST");
    }
    
    [Fact]
    public async Task GetFileStatisticsAsync_應回傳正確統計資訊()
    {
        // Arrange
        var content = "Hello World\nThis is a test\nThird line";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["stats.txt"] = new MockFileData(content)
        });
        
        var service = new StreamProcessorService(mockFileSystem);
        
        // Act
        var result = await service.GetFileStatisticsAsync("stats.txt");
        
        // Assert
        result.LineCount.Should().Be(3);
        result.WordCount.Should().Be(8); // Hello, World, This, is, a, test, Third, line
    }
}

/// <summary>
/// 設定檔管理服務測試
/// </summary>
public class ConfigManagerServiceTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly ConfigManagerService _service;
    
    public ConfigManagerServiceTests()
    {
        _mockFileSystem = new MockFileSystem();
        _service = new ConfigManagerService(_mockFileSystem, "test-config");
    }
    
    [Fact]
    public async Task LoadAppSettingsAsync_設定檔不存在_應回傳並建立預設設定()
    {
        // Act
        var result = await _service.LoadAppSettingsAsync();
        
        // Assert
        result.Should().NotBeNull();
        result.ApplicationName.Should().Be("FileSystem Testing Demo");
        result.Version.Should().Be("1.0.0");
        result.Database.Should().NotBeNull();
        result.Logging.Should().NotBeNull();
        
        // 應該建立預設設定檔
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.File.Exists(configPath).Should().BeTrue();
    }
    
    [Fact]
    public async Task SaveAppSettingsAsync_儲存設定_應正確寫入檔案()
    {
        // Arrange
        var settings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Test App",
            Version = "2.0.0",
            Database = new ConfigManagerService.DatabaseSettings
            {
                ConnectionString = "Server=test;Database=TestDb;",
                TimeoutSeconds = 60
            }
        };
        
        // Act
        await _service.SaveAppSettingsAsync(settings);
        
        // Assert
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.File.Exists(configPath).Should().BeTrue();
        
        var savedContent = await _mockFileSystem.File.ReadAllTextAsync(configPath);
        var savedSettings = JsonSerializer.Deserialize<ConfigManagerService.AppSettings>(savedContent);
        
        savedSettings!.ApplicationName.Should().Be("Test App");
        savedSettings.Version.Should().Be("2.0.0");
        savedSettings.Database.ConnectionString.Should().Be("Server=test;Database=TestDb;");
        savedSettings.Database.TimeoutSeconds.Should().Be(60);
    }
    
    [Fact]
    public async Task LoadAppSettingsAsync_設定檔存在_應回傳正確設定()
    {
        // Arrange
        var settings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Existing App",
            Version = "3.0.0"
        };
        
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.AddFile(configPath, new MockFileData(json));
        
        // Act
        var result = await _service.LoadAppSettingsAsync();
        
        // Assert
        result.ApplicationName.Should().Be("Existing App");
        result.Version.Should().Be("3.0.0");
    }
    
    [Fact]
    public void BackupConfiguration_設定檔存在_應建立備份檔案()
    {
        // Arrange
        var settings = new ConfigManagerService.AppSettings();
        var json = JsonSerializer.Serialize(settings);
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.AddFile(configPath, new MockFileData(json));
        
        // Act
        var backupPath = _service.BackupConfiguration();
        
        // Assert
        _mockFileSystem.File.Exists(backupPath).Should().BeTrue();
        backupPath.Should().StartWith(@"test-config\backup\appsettings_");
        backupPath.Should().EndWith(".json");
        
        var backupContent = _mockFileSystem.File.ReadAllText(backupPath);
        backupContent.Should().Be(json);
    }
    
    [Fact]
    public void BackupConfiguration_設定檔不存在_應拋出FileNotFoundException()
    {
        // Act & Assert
        var action = () => _service.BackupConfiguration();
        action.Should().Throw<FileNotFoundException>()
              .WithMessage("*找不到要備份的設定檔案*");
    }
    
    [Fact]
    public void ListBackups_有多個備份_應按時間降序回傳()
    {
        // Arrange
        _mockFileSystem.AddFile(@"test-config\backup\appsettings_20240101_100000.json", 
            new MockFileData("{}"));
        _mockFileSystem.AddFile(@"test-config\backup\appsettings_20240102_100000.json", 
            new MockFileData("{}"));
        _mockFileSystem.AddFile(@"test-config\backup\appsettings_20240103_100000.json", 
            new MockFileData("{}"));
        
        // Act
        var backups = _service.ListBackups().ToList();
        
        // Assert
        backups.Should().HaveCount(3);
        backups[0].Should().Contain("20240103"); // 最新的在前面
    }
    
    [Fact]
    public void ListBackups_沒有備份_應回傳空清單()
    {
        // Act
        var backups = _service.ListBackups();
        
        // Assert
        backups.Should().BeEmpty();
    }
    
    [Fact]
    public async Task RestoreFromBackupAsync_備份存在_應還原設定()
    {
        // Arrange
        var originalSettings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Original App"
        };
        var backupSettings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Backup App"
        };
        
        var configPath = @"test-config\appsettings.json";
        var backupPath = @"test-config\backup\appsettings_backup.json";
        
        _mockFileSystem.AddFile(configPath, 
            new MockFileData(JsonSerializer.Serialize(originalSettings)));
        _mockFileSystem.AddFile(backupPath, 
            new MockFileData(JsonSerializer.Serialize(backupSettings)));
        
        // Act
        await _service.RestoreFromBackupAsync(backupPath);
        
        // Assert
        var restoredContent = await _mockFileSystem.File.ReadAllTextAsync(configPath);
        var restoredSettings = JsonSerializer.Deserialize<ConfigManagerService.AppSettings>(restoredContent);
        restoredSettings!.ApplicationName.Should().Be("Backup App");
    }
    
    [Fact]
    public async Task RestoreFromBackupAsync_備份不存在_應拋出FileNotFoundException()
    {
        // Act & Assert
        var action = async () => await _service.RestoreFromBackupAsync(@"nonexistent.json");
        await action.Should().ThrowAsync<FileNotFoundException>();
    }
}

#endregion

#region 整合測試範例

/// <summary>
/// 完整工作流程整合測試
/// 示範設定檔的完整生命週期
/// </summary>
public class ConfigManagerIntegrationTests
{
    [Fact]
    public async Task 完整設定檔生命週期_建立修改備份還原()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigManagerService(mockFileSystem, "app-config");
        
        // Step 1: 載入預設設定（應該自動建立）
        var settings = await service.LoadAppSettingsAsync();
        settings.ApplicationName.Should().Be("FileSystem Testing Demo");
        
        // Step 2: 修改設定
        settings.ApplicationName = "Modified App";
        settings.Database.ConnectionString = "Server=production;Database=ProdDb;";
        await service.SaveAppSettingsAsync(settings);
        
        // Step 3: 建立備份
        var backupPath = service.BackupConfiguration();
        mockFileSystem.File.Exists(backupPath).Should().BeTrue();
        
        // Step 4: 再次修改設定
        settings.ApplicationName = "Another Modification";
        await service.SaveAppSettingsAsync(settings);
        
        // Step 5: 從備份還原
        await service.RestoreFromBackupAsync(backupPath);
        
        // Assert: 驗證還原後的設定
        var restoredSettings = await service.LoadAppSettingsAsync();
        restoredSettings.ApplicationName.Should().Be("Modified App");
        restoredSettings.Database.ConnectionString.Should().Be("Server=production;Database=ProdDb;");
    }
}

#endregion
