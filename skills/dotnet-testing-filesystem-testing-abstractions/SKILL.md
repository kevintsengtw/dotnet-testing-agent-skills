---
name: dotnet-testing-filesystem-testing-abstractions
description: |
  使用 System.IO.Abstractions 測試檔案系統操作的專門技能。當需要測試 File、Directory、Path 等操作、模擬檔案系統時使用。涵蓋 IFileSystem、MockFileSystem、檔案讀寫測試、目錄操作測試等。
  Make sure to use this skill whenever the user mentions file system testing, IFileSystem, MockFileSystem, System.IO.Abstractions, or testing file/directory operations, even if they don't explicitly ask for filesystem testing guidance.
  Keywords: file testing, filesystem, 檔案測試, 檔案系統測試, IFileSystem, MockFileSystem, System.IO.Abstractions, File.ReadAllText, File.WriteAllText, Directory.CreateDirectory, Path.Combine, mock file system, 檔案抽象化
---

# 檔案系統測試：使用 System.IO.Abstractions 模擬檔案操作

## 核心原則

### 1. 檔案系統相依性的根本問題

傳統直接使用 `System.IO` 靜態類別的程式碼難以測試，原因包括：

- **速度問題**：實際磁碟 IO 比記憶體操作慢 10-100 倍
- **環境相依**：測試結果受檔案系統狀態、權限、路徑影響
- **副作用**：測試會在磁碟上留下痕跡，影響其他測試
- **並行問題**：多個測試同時操作同一檔案會產生競爭條件
- **錯誤模擬困難**：難以模擬權限不足、磁碟空間不足等異常

### 2. System.IO.Abstractions 解決方案

將 System.IO 靜態類別包裝成介面的套件，支援依賴注入和測試替身。

**必要 NuGet 套件**：

```xml
<!-- 正式環境 -->
<PackageReference Include="System.IO.Abstractions" Version="22.1.0" />

<!-- 測試專案 -->
<PackageReference Include="System.IO.Abstractions.TestingHelpers" Version="22.1.0" />
```

### 3. 重構步驟

**步驟一**：將直接使用靜態類別的程式碼改為依賴 `IFileSystem`

```csharp
// ❌ 重構前（不可測試）
public class ConfigService
{
    public string LoadConfig(string path) => File.ReadAllText(path);
}

// ✅ 重構後（可測試）
public class ConfigService
{
    private readonly IFileSystem _fileSystem;
    public ConfigService(IFileSystem fileSystem) => _fileSystem = fileSystem;
    public string LoadConfig(string path) => _fileSystem.File.ReadAllText(path);
}
```

**步驟二**：在 DI 容器中註冊真實實作

```csharp
services.AddSingleton<IFileSystem, FileSystem>();
```

**步驟三**：在測試中使用 MockFileSystem

```csharp
var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
{
    ["config.json"] = new MockFileData("{ \"key\": \"value\" }")
});
var service = new ConfigService(mockFs);
```

## MockFileSystem 測試模式

涵蓋四種核心測試模式：預設檔案狀態建立、驗證寫入結果、目錄操作測試、使用 NSubstitute 模擬 IO 異常（UnauthorizedAccessException 等）。另含進階技巧：串流操作測試、檔案資訊查詢測試、備份檔案測試。

> 完整 MockFileSystem 測試模式與進階技巧請參考 [references/mockfilesystem-patterns.md](references/mockfilesystem-patterns.md)

## 最佳實踐

### 應該這樣做

1. **使用 Path.Combine 處理路徑** — `_fileSystem.Path.Combine("configs", "app.json")`
2. **防禦性檢查檔案存在性** — 在讀取前先檢查 `_fileSystem.File.Exists()`
3. **自動建立必要目錄** — 寫入前確保目錄存在
4. **妥善處理各種 IO 異常** — UnauthorizedAccessException、IOException、DirectoryNotFoundException
5. **每個測試使用獨立的 MockFileSystem** — 確保測試隔離

### 應該避免

1. **硬編碼路徑分隔符號** — 使用 `Path.Combine` 取代 `\\` 或 `/`
2. **在單元測試中使用真實檔案系統** — 使用 MockFileSystem
3. **忽略例外處理** — 不要假設檔案一定存在

## 效能考量

- **MockFileSystem 速度**：比真實檔案操作快 10-100 倍
- **記憶體使用**：只建立測試必需的檔案，避免模擬超大檔案

## 實務整合範例

請參考 `templates/` 目錄下的完整實作：

- `configmanager-service.cs` - 設定檔管理服務（載入/儲存/備份）
- `filemanager-service.cs` - 檔案管理服務（複製/目錄操作/錯誤處理）

## 輸出格式

- 產生使用 IFileSystem 介面的服務類別
- 產生使用 MockFileSystem 的測試類別
- 包含檔案讀寫、目錄操作、路徑處理測試範例
- 提供 .csproj 套件參考（System.IO.Abstractions、System.IO.Abstractions.TestingHelpers）

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 17 - 檔案與 IO 測試：使用 System.IO.Abstractions 模擬檔案系統**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10375981
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day17

### 官方文件

- [System.IO.Abstractions GitHub](https://github.com/TestableIO/System.IO.Abstractions)
- [System.IO.Abstractions NuGet](https://www.nuget.org/packages/System.IO.Abstractions/)
- [TestingHelpers NuGet](https://www.nuget.org/packages/System.IO.Abstractions.TestingHelpers/)

### 相關技能

- `nsubstitute-mocking` - 測試替身與模擬
- `unit-test-fundamentals` - 單元測試基礎
