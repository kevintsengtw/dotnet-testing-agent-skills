// =============================================================================
// 檔案系統抽象化基礎：從不可測試到可測試的重構
// System.IO.Abstractions Basics - Refactoring for Testability
// =============================================================================

using System.IO.Abstractions;
using System.Text.Json;

namespace FileSystemTestingExamples;

#region 問題示範：不可測試的程式碼

/// <summary>
/// ❌ 不可測試的設定服務（反面教材）
/// 直接使用 System.IO 靜態類別，無法進行單元測試
/// </summary>
public class LegacyConfigurationService
{
    /// <summary>
    /// 載入設定檔 - 直接依賴檔案系統
    /// </summary>
    public string LoadConfig(string configPath)
    {
        // ❌ 問題：無法在測試中控制檔案內容
        return File.ReadAllText(configPath);
    }
    
    /// <summary>
    /// 儲存設定檔 - 會在磁碟上產生副作用
    /// </summary>
    public void SaveConfig(string configPath, string content)
    {
        // ❌ 問題：測試會真的寫入磁碟，影響其他測試
        File.WriteAllText(configPath, content);
    }
    
    /// <summary>
    /// 檢查設定檔是否存在
    /// </summary>
    public bool ConfigExists(string configPath)
    {
        // ❌ 問題：依賴真實檔案系統狀態
        return File.Exists(configPath);
    }
}

/*
 * LegacyConfigurationService 的問題：
 * 
 * 1. 速度問題：磁碟 IO 比記憶體操作慢 10-100 倍
 * 2. 環境相依：測試結果受檔案系統狀態影響
 * 3. 副作用：會在磁碟上留下檔案，影響其他測試
 * 4. 並行問題：多個測試同時操作同一檔案會產生競爭
 * 5. 錯誤模擬困難：無法輕易模擬權限不足等異常
 */

#endregion

#region 解決方案：使用 IFileSystem 抽象化

/// <summary>
/// ✅ 可測試的設定服務
/// 透過依賴注入 IFileSystem 實現可測試性
/// </summary>
public class ConfigurationService
{
    private readonly IFileSystem _fileSystem;
    
    /// <summary>
    /// 建構函式注入 IFileSystem
    /// </summary>
    /// <param name="fileSystem">檔案系統抽象介面</param>
    public ConfigurationService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }
    
    /// <summary>
    /// 載入設定值，如果檔案不存在則回傳預設值
    /// </summary>
    /// <param name="filePath">設定檔路徑</param>
    /// <param name="defaultValue">預設值（當檔案不存在或讀取失敗時使用）</param>
    /// <returns>設定內容</returns>
    public async Task<string> LoadConfigurationAsync(string filePath, string defaultValue = "")
    {
        // ✅ 使用注入的 _fileSystem 而非靜態 File 類別
        if (!_fileSystem.File.Exists(filePath))
        {
            return defaultValue;
        }

        try
        {
            return await _fileSystem.File.ReadAllTextAsync(filePath);
        }
        catch (Exception)
        {
            // 讀取失敗時回傳預設值
            return defaultValue;
        }
    }
    
    /// <summary>
    /// 儲存設定到檔案，自動建立必要的目錄結構
    /// </summary>
    /// <param name="filePath">設定檔路徑</param>
    /// <param name="value">要儲存的值</param>
    public async Task SaveConfigurationAsync(string filePath, string value)
    {
        // ✅ 使用 Path.GetDirectoryName 處理路徑
        var directory = _fileSystem.Path.GetDirectoryName(filePath);
        
        // ✅ 自動建立目錄（如果不存在）
        if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
        {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        await _fileSystem.File.WriteAllTextAsync(filePath, value);
    }
    
    /// <summary>
    /// 載入 JSON 格式的設定檔
    /// </summary>
    /// <typeparam name="T">設定物件類型</typeparam>
    /// <param name="filePath">設定檔路徑</param>
    /// <returns>反序列化後的設定物件，失敗時回傳 null</returns>
    public async Task<T?> LoadJsonConfigurationAsync<T>(string filePath) where T : class
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return default;
        }

        try
        {
            var jsonContent = await _fileSystem.File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonContent);
        }
        catch (Exception)
        {
            return default;
        }
    }
    
    /// <summary>
    /// 儲存 JSON 格式的設定檔
    /// </summary>
    /// <typeparam name="T">設定物件類型</typeparam>
    /// <param name="filePath">設定檔路徑</param>
    /// <param name="settings">要儲存的設定物件</param>
    public async Task SaveJsonConfigurationAsync<T>(string filePath, T settings) where T : class
    {
        var directory = _fileSystem.Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
        {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        var jsonContent = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await _fileSystem.File.WriteAllTextAsync(filePath, jsonContent);
    }
}

#endregion

#region 檔案管理服務

/// <summary>
/// 檔案管理服務，提供檔案和目錄的進階操作
/// </summary>
public class FileManagerService
{
    private readonly IFileSystem _fileSystem;
    
    public FileManagerService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    /// <summary>
    /// 複製檔案到指定目錄
    /// </summary>
    /// <param name="sourceFilePath">來源檔案路徑</param>
    /// <param name="targetDirectory">目標目錄</param>
    /// <returns>目標檔案的完整路徑</returns>
    /// <exception cref="FileNotFoundException">來源檔案不存在</exception>
    public string CopyFileToDirectory(string sourceFilePath, string targetDirectory)
    {
        // 檢查來源檔案是否存在
        if (!_fileSystem.File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException($"來源檔案不存在: {sourceFilePath}");
        }

        // 自動建立目標目錄
        if (!_fileSystem.Directory.Exists(targetDirectory))
        {
            _fileSystem.Directory.CreateDirectory(targetDirectory);
        }

        // 使用 Path.GetFileName 取得檔案名稱
        var fileName = _fileSystem.Path.GetFileName(sourceFilePath);
        var targetFilePath = _fileSystem.Path.Combine(targetDirectory, fileName);

        // 複製檔案（覆寫既有檔案）
        _fileSystem.File.Copy(sourceFilePath, targetFilePath, overwrite: true);
        return targetFilePath;
    }
    
    /// <summary>
    /// 備份檔案（加上時間戳記）
    /// </summary>
    /// <param name="filePath">要備份的檔案路徑</param>
    /// <returns>備份檔案的完整路徑</returns>
    /// <exception cref="FileNotFoundException">檔案不存在</exception>
    public string BackupFile(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException($"檔案不存在: {filePath}");
        }

        var directory = _fileSystem.Path.GetDirectoryName(filePath);
        var fileNameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(filePath);
        var extension = _fileSystem.Path.GetExtension(filePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var backupFileName = $"{fileNameWithoutExtension}_{timestamp}{extension}";
        var backupFilePath = _fileSystem.Path.Combine(directory ?? "", backupFileName);

        _fileSystem.File.Copy(filePath, backupFilePath);
        return backupFilePath;
    }
    
    /// <summary>
    /// 取得檔案資訊
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <returns>檔案資訊，如果檔案不存在則回傳 null</returns>
    public FileInfoData? GetFileInfo(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return null;
        }

        // 使用 IFileInfo 取得檔案詳細資訊
        var fileInfo = _fileSystem.FileInfo.New(filePath);
        return new FileInfoData
        {
            Name = fileInfo.Name,
            FullPath = fileInfo.FullName,
            Size = fileInfo.Length,
            CreationTime = fileInfo.CreationTime,
            LastWriteTime = fileInfo.LastWriteTime,
            IsReadOnly = fileInfo.IsReadOnly
        };
    }
    
    /// <summary>
    /// 列出目錄中的所有檔案
    /// </summary>
    /// <param name="directoryPath">目錄路徑</param>
    /// <param name="searchPattern">搜尋模式（預設 *.*）</param>
    /// <returns>檔案路徑清單</returns>
    public IEnumerable<string> ListFiles(string directoryPath, string searchPattern = "*.*")
    {
        if (!_fileSystem.Directory.Exists(directoryPath))
        {
            return Enumerable.Empty<string>();
        }

        return _fileSystem.Directory.GetFiles(directoryPath, searchPattern);
    }
    
    /// <summary>
    /// 確保目錄存在，如果不存在則建立
    /// </summary>
    /// <param name="directoryPath">目錄路徑</param>
    /// <returns>目錄是否成功建立或已存在</returns>
    public bool EnsureDirectoryExists(string directoryPath)
    {
        try
        {
            if (!_fileSystem.Directory.Exists(directoryPath))
            {
                _fileSystem.Directory.CreateDirectory(directoryPath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 檔案資訊資料類別
    /// </summary>
    public class FileInfoData
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public bool IsReadOnly { get; set; }
    }
}

#endregion

#region 檔案權限服務（錯誤處理示範）

/// <summary>
/// 檔案權限服務，示範如何處理各種 IO 異常
/// </summary>
public class FilePermissionService
{
    private readonly IFileSystem _fileSystem;
    
    public FilePermissionService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    /// <summary>
    /// 嘗試讀取檔案，處理各種可能的異常
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <param name="content">讀取到的內容（如果成功）</param>
    /// <returns>是否成功讀取</returns>
    public bool TryReadFile(string filePath, out string? content)
    {
        content = null;
        
        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                return false;
            }
                
            content = _fileSystem.File.ReadAllText(filePath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // 權限不足
            return false;
        }
        catch (IOException)
        {
            // 檔案被鎖定或其他 IO 錯誤
            return false;
        }
    }
    
    /// <summary>
    /// 嘗試寫入檔案，自動建立目錄並處理異常
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <param name="content">要寫入的內容</param>
    /// <returns>是否成功寫入</returns>
    public async Task<bool> TrySaveFileAsync(string filePath, string content)
    {
        try
        {
            await _fileSystem.File.WriteAllTextAsync(filePath, content);
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            // 嘗試建立目錄後重試
            var directory = _fileSystem.Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
            {
                return false;
            }
            
            try
            {
                _fileSystem.Directory.CreateDirectory(directory);
                await _fileSystem.File.WriteAllTextAsync(filePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}

#endregion

#region DI 註冊範例

/*
 * 在 ASP.NET Core 中的 DI 註冊方式：
 * 
 * // Program.cs 或 Startup.cs
 * 
 * // 註冊 IFileSystem 的真實實作
 * services.AddSingleton<IFileSystem, FileSystem>();
 * 
 * // 註冊使用 IFileSystem 的服務
 * services.AddScoped<ConfigurationService>();
 * services.AddScoped<FileManagerService>();
 * services.AddScoped<FilePermissionService>();
 */

#endregion
