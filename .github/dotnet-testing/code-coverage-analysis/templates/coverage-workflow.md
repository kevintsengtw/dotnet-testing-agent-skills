# 程式碼覆蓋率工作流程指南

本文件說明在不同環境下執行程式碼覆蓋率分析的完整工作流程。

---

## 工作流程概覽

```text
設定專案 → 執行測試 → 收集覆蓋率 → 產生報告 → 分析結果 → 改善測試
```

---

## 方法一：使用 .NET CLI（推薦用於 CI/CD）

### 步驟 1：確認專案設定

確認測試專案已安裝必要套件：

```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.3">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### 步驟 2：執行測試並收集覆蓋率

**基本執行：**

```powershell
# 執行測試並收集覆蓋率
dotnet test --collect:"XPlat Code Coverage"
```

**指定輸出目錄：**

```powershell
# 指定覆蓋率結果輸出位置
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

**使用 runsettings 檔案：**

```powershell
# 使用進階設定
dotnet test --settings coverage.runsettings
```

### 步驟 3：產生 HTML 報告

安裝 ReportGenerator 工具：

```powershell
# 全域安裝
dotnet tool install -g dotnet-reportgenerator-globaltool

# 或本地安裝
dotnet new tool-manifest
dotnet tool install dotnet-reportgenerator-globaltool
```

產生報告：

```powershell
# 產生 HTML 報告
reportgenerator `
  -reports:"**\coverage.cobertura.xml" `
  -targetdir:"coveragereport" `
  -reporttypes:Html

# 開啟報告
start coveragereport\index.html
```

### 步驟 4：分析結果

開啟生成的 HTML 報告，檢查：

1. **整體覆蓋率**：Line Coverage、Branch Coverage
2. **模組覆蓋率**：各專案的覆蓋率分布
3. **檔案覆蓋率**：識別覆蓋率低的檔案
4. **風險區域**：標記為紅色的未覆蓋程式碼

---

## 方法二：使用 Visual Studio + Fine Code Coverage

### 步驟 1：安裝 Fine Code Coverage

1. 開啟 Visual Studio
2. 延伸模組 → 管理延伸模組
3. 搜尋 "Fine Code Coverage"
4. 安裝後重新啟動

### 步驟 2：設定選項

1. 工具 → 選項 → Fine Code Coverage
2. 啟用以下設定：
   - Run (Common) → Enable：`True`
   - Editor Colouring Line Highlighting：`True`

### 步驟 3：執行測試

1. 開啟測試總管（測試 → 測試總管）
2. 執行所有測試或特定測試
3. Fine Code Coverage 會自動顯示結果

### 步驟 4：檢視覆蓋率

**開啟 Fine Code Coverage 視窗：**

- 檢視 → 其他視窗 → Fine Code Coverage

**啟用編輯器指示器：**

- 工具 → FCC Toggle Indicators

**顏色標示：**

- 綠色：已被測試覆蓋
- 黃色：部分覆蓋（部分分支未測試）
- 紅色：未被覆蓋

### 步驟 5：改善覆蓋率

根據紅色標示：

1. 識別未覆蓋的程式碼區塊
2. 分析是否需要測試
3. 撰寫新的測試案例
4. 重新執行測試驗證

---

## 方法三：使用 VS Code

### 步驟 1：安裝擴充套件

確認已安裝 C# Dev Kit：

1. 按 `Ctrl+Shift+X` 開啟擴充功能
2. 搜尋 "C# Dev Kit"
3. 安裝並重新載入

### 步驟 2：開啟測試總管

1. 點選活動列的燒杯圖示
2. 或執行命令：`Testing: Focus on 測試總管 View`

### 步驟 3：執行覆蓋率測試

在測試總管中：

1. 點選「執行涵蓋範圍測試」圖示
2. 等待測試完成

### 步驟 4：檢視結果

**測試涵蓋範圍視圖：**

- 顯示樹狀結構的覆蓋率資訊
- 各檔案的覆蓋率百分比

**編輯器內顯示：**

- 綠色：已覆蓋的程式碼
- 紅色：未覆蓋的程式碼
- 執行次數：顯示每行被執行的次數

**檔案總管顯示：**

- 直接在檔案名稱旁顯示覆蓋率百分比

### 步驟 5：切換內嵌涵蓋範圍

使用快捷鍵 `Ctrl+; Ctrl+Shift+I` 或執行命令：

- `Test: Show Inline Coverage (測試: 切換內嵌涵蓋範圍)`

---

## 方法四：CI/CD 整合

### GitHub Actions

建立 `.github/workflows/test-coverage.yml`：

```yaml
name: Test with Coverage

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run tests with coverage
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Install ReportGenerator
        run: dotnet tool install -g dotnet-reportgenerator-globaltool
      
      - name: Generate coverage report
        run: |
          reportgenerator \
            -reports:**/coverage.cobertura.xml \
            -targetdir:coverage \
            -reporttypes:Html;Cobertura
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v4
        with:
          files: coverage/Cobertura.xml
          fail_ci_if_error: true
```

### Azure DevOps

建立 `azure-pipelines.yml`：

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      packageType: 'sdk'
      version: '9.0.x'
  
  - task: DotNetCoreCLI@2
    displayName: 'Restore packages'
    inputs:
      command: 'restore'
  
  - task: DotNetCoreCLI@2
    displayName: 'Build solution'
    inputs:
      command: 'build'
      arguments: '--no-restore'
  
  - task: DotNetCoreCLI@2
    displayName: 'Run tests with coverage'
    inputs:
      command: 'test'
      arguments: '--no-build --collect:"XPlat Code Coverage"'
      publishTestResults: true
  
  - task: PublishCodeCoverageResults@1
    displayName: 'Publish coverage report'
    inputs:
      codeCoverageTool: 'Cobertura'
      summaryFileLocation: '$(Agent.TempDirectory)/**/*coverage.cobertura.xml'
      reportDirectory: '$(Build.SourcesDirectory)/coverage'
```

---

## 覆蓋率改善策略

### 階段一：建立基準線（目標 60-70%）

1. **識別核心模組**：
   - 業務邏輯
   - 資料驗證
   - 計算邏輯

2. **撰寫基本測試**：
   - 主要流程測試
   - 基本邊界測試

3. **執行覆蓋率**：
   - 建立初始基準

### 階段二：補充關鍵測試（目標 70-80%）

1. **分析缺口**：
   - 檢視紅色區域
   - 識別關鍵未覆蓋程式碼

2. **補充測試**：
   - 邊界條件測試
   - 異常情境測試
   - 分支覆蓋測試

3. **持續監控**：
   - 每次 PR 檢查覆蓋率變化

### 階段三：細緻化測試（目標 80-85%）

1. **深入分析**：
   - 檢查黃色區域（部分覆蓋）
   - 確認所有分支都有測試

2. **品質提升**：
   - 改善測試斷言
   - 增加邊界條件
   - 測試錯誤處理

3. **排除不必要的程式碼**：
   - 使用 `[ExcludeFromCodeCoverage]`
   - 在 runsettings 中排除

### 階段四：維護與監控

1. **設定 CI/CD 閘門**：
   - 覆蓋率不能下降
   - 新程式碼必須有測試

2. **定期檢視**：
   - 每週檢視覆蓋率報告
   - 識別風險區域

3. **持續改善**：
   - 重構高複雜度程式碼
   - 補充遺漏的測試

---

## 覆蓋率報告解讀

### 關鍵指標

1. **Line Coverage（行覆蓋率）**
   - 計算：執行的程式碼行數 / 總程式碼行數
   - 建議：≥ 70%

2. **Branch Coverage（分支覆蓋率）**
   - 計算：執行的分支數 / 總分支數
   - 建議：≥ 60%
   - **比行覆蓋率更重要**

3. **Method Coverage（方法覆蓋率）**
   - 計算：執行的方法數 / 總方法數
   - 建議：≥ 75%

### 顏色標示含義

| 顏色 | 覆蓋率範圍 | 狀態   | 建議行動             |
| ---- | ---------- | ------ | -------------------- |
| 綠色 | ≥ 75%      | 良好   | 維持現狀             |
| 黃色 | 50-74%     | 警告   | 評估是否需要補充測試 |
| 紅色 | < 50%      | 危險   | 優先補充測試         |
| 灰色 | N/A        | 已排除 | 確認排除設定是否正確 |

---

## 常見問題排除

### 問題 1：覆蓋率顯示 0%

**可能原因：**

- 未安裝 `coverlet.collector`
- runsettings 設定錯誤
- 測試未實際執行

**解決方案：**

```powershell
# 確認套件安裝
dotnet list package | Select-String "coverlet"

# 重新安裝
dotnet add package coverlet.collector

# 清除快取重新測試
dotnet clean
dotnet test --collect:"XPlat Code Coverage"
```

### 問題 2：ReportGenerator 找不到覆蓋率檔案

**解決方案：**

```powershell
# 使用絕對路徑
reportgenerator `
  -reports:"$(Get-Location)\TestResults\**\coverage.cobertura.xml" `
  -targetdir:"coveragereport" `
  -reporttypes:Html

# 或指定完整路徑
reportgenerator `
  -reports:"C:\Projects\MyApp\TestResults\{guid}\coverage.cobertura.xml" `
  -targetdir:"coveragereport" `
  -reporttypes:Html
```

### 問題 3：VS Code 無法顯示覆蓋率

**解決方案：**

1. 確認已安裝 C# Dev Kit
2. 重新執行「執行涵蓋範圍測試」
3. 檢查是否產生 lcov 檔案
4. 重新載入視窗（`Ctrl+Shift+P` → `Reload Window`）

### 問題 4：CI/CD 中覆蓋率失敗

**解決方案：**

```yaml
# 在 GitHub Actions 中偵錯
- name: Display coverage files
  run: |
    echo "Coverage files:"
    find . -name "coverage.cobertura.xml"
    
- name: Run tests with verbose output
  run: dotnet test --verbosity detailed --collect:"XPlat Code Coverage"
```

---

## 檢查清單

執行覆蓋率分析前，確認以下項目：

### 專案設定

- [ ] 已安裝 `coverlet.collector` 套件
- [ ] 測試專案設定正確（`IsTestProject=true`）
- [ ] runsettings 檔案格式正確（如果使用）

### 執行測試

- [ ] 所有測試都通過
- [ ] 使用正確的收集器參數
- [ ] 覆蓋率檔案有成功產生

### 檢視報告

- [ ] 報告檔案可以正常開啟
- [ ] 覆蓋率數據符合預期
- [ ] 已識別需要改善的區域

### CI/CD 整合

- [ ] Pipeline 可以成功執行
- [ ] 覆蓋率報告有正確上傳
- [ ] 閘門設定符合團隊標準

---

## 最佳實踐

### DO（應該做）

✅ 定期檢視覆蓋率報告
✅ 專注於分支覆蓋率而非行覆蓋率
✅ 排除不必要的程式碼（如自動產生的程式碼）
✅ 在 CI/CD 中整合覆蓋率檢查
✅ 設定合理的覆蓋率目標（70-85%）
✅ 結合複雜度指標評估測試需求

### DON'T（不應該做）

❌ 將覆蓋率當作 KPI
❌ 為了衝數字而寫沒有斷言的測試
❌ 追求 100% 覆蓋率
❌ 忽略測試品質只看覆蓋率數字
❌ 測試簡單的 getter/setter
❌ 在沒有分析的情況下盲目補充測試

---

## 相關資源

- [Coverlet 官方文件](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator 文件](https://github.com/danielpalme/ReportGenerator)
- [Fine Code Coverage](https://github.com/FortuneN/FineCodeCoverage)
- [Microsoft 覆蓋率文件](https://learn.microsoft.com/dotnet/core/testing/unit-testing-code-coverage)
