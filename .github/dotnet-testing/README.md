# .NET Testing Agent Skills

這是一套專為 .NET 測試開發設計的 Agent Skills 集合，基於「老派軟體工程師的測試修練 - 30 天挑戰」(2025 iThome 鐵人賽 Software Development 組冠軍作品) 提煉而成。

## 📚 關於此技能集

這些技能遵循 [agentskills.io](https://agentskills.io) 開放標準，可在支援 Agent Skills 的 AI 助理中使用，包括：

- GitHub Copilot (VS Code / CLI)
- Claude Code CLI
- 其他相容平台

每個技能專注於特定的測試任務或工作流程，可以獨立使用，也能相互組合以完成複雜的測試場景。

---

## 🎯 技能清單

### 第一階段：測試基礎與斷言

| 技能                                                      | 說明             | 主要用途                                  |
| --------------------------------------------------------- | ---------------- | ----------------------------------------- |
| [unit-test-fundamentals](./unit-test-fundamentals/)       | 單元測試基礎     | 建立符合 FIRST 原則的單元測試、3A Pattern |
| [test-naming-conventions](./test-naming-conventions/)     | 測試命名規範     | 使用三段式命名法建立清晰的測試名稱        |
| [xunit-project-setup](./xunit-project-setup/)             | xUnit 專案設定   | 建立標準化的 xUnit 測試專案結構           |
| [code-coverage-analysis](./code-coverage-analysis/)       | 程式碼覆蓋率分析 | 配置與解讀程式碼覆蓋率報告                |
| [awesome-assertions-guide](./awesome-assertions-guide/)   | 流暢斷言指南     | 使用 AwesomeAssertions 進行可讀的測試斷言 |
| [complex-object-comparison](./complex-object-comparison/) | 複雜物件比對     | 深層物件比對、欄位排除、效能最佳化        |
| [fluentvalidation-testing](./fluentvalidation-testing/)   | 驗證器測試       | FluentValidation 驗證邏輯測試與最佳實踐   |
| [nsubstitute-mocking](./nsubstitute-mocking/)             | 測試替身與模擬   | 使用 NSubstitute 建立 Mock/Stub/Spy       |
| [test-output-logging](./test-output-logging/)             | 測試輸出與日誌   | xUnit ITestOutputHelper 與 ILogger 整合   |
| [private-internal-testing](./private-internal-testing/)   | 私有成員測試策略 | Private 與 Internal 成員的測試策略        |

### 第二階段：可測試性抽象化

| 技能                                                                  | 說明         | 主要用途                                         |
| --------------------------------------------------------------------- | ------------ | ------------------------------------------------ |
| [datetime-testing-timeprovider](./datetime-testing-timeprovider/)     | 日期時間測試 | 使用 Microsoft.Bcl.TimeProvider 測試時間相關邏輯 |
| [filesystem-testing-abstractions](./filesystem-testing-abstractions/) | 檔案系統測試 | 使用 System.IO.Abstractions 模擬檔案系統操作     |

### 第三階段：測試資料生成與整合

| 技能                                                                          | 說明               | 主要用途                              |
| ----------------------------------------------------------------------------- | ------------------ | ------------------------------------- |
| [test-data-builder-pattern](./test-data-builder-pattern/)                     | Builder 模式       | 手動 Builder Pattern 建構測試資料     |
| [autofixture-basics](./autofixture-basics/)                                   | AutoFixture 基礎   | AutoFixture 基礎使用與匿名測試        |
| [autofixture-customization](./autofixture-customization/)                     | AutoFixture 自訂化 | AutoFixture 自訂化策略與進階配置      |
| [autodata-xunit-integration](./autodata-xunit-integration/)                   | AutoData 整合      | xUnit 與 AutoFixture 的 AutoData 整合 |
| [autofixture-nsubstitute-integration](./autofixture-nsubstitute-integration/) | 自動模擬整合       | AutoFixture + NSubstitute 自動模擬    |
| [bogus-fake-data](./bogus-fake-data/)                                         | 真實感測試資料     | 使用 Bogus 產生擬真的測試資料         |
| [autofixture-bogus-integration](./autofixture-bogus-integration/)             | 整合應用           | AutoFixture 與 Bogus 的整合應用       |

---

## 🌐 進階技能

整合測試與框架遷移的進階技能請參考 [dotnet-testing-advanced](../dotnet-testing-advanced/README.md)：

- **第四階段**：整合測試 (ASP.NET Core, Testcontainers, Aspire Testing)
- **第五階段**：框架遷移指南 (xUnit 3.x, TUnit)

---

## 🚀 快速開始

### 前置需求

確保您的環境支援 Agent Skills：

- **VS Code**: 安裝 VS Code 啟用 `chat.useAgentSkills` 設定
- **GitHub Copilot CLI**: 已安裝並登入
- **Claude Code CLI**: 已安裝 `code` 命令

### 啟用技能

1. **將此 repository 複製到您的專案中：**

    ```bash
    # 方法一：直接複製 .github 資料夾到您的專案
    cp -r .github/skills/dotnet-testing /your-project/.github/skills/

    # 方法二：使用 git submodule
    cd /your-project
    git submodule add https://github.com/kevintsengtw/dotnet-testing-playbook .github/skills-source
    ln -s .github/skills-source/.github/skills/dotnet-testing .github/skills/dotnet-testing
    ```

2. **在 VS Code 中使用：**
   - 開啟 Copilot Chat (`Ctrl+I` 或 `Cmd+I`)
   - 直接提問，例如：
     - "幫我建立一個測試專案"
     - "這段程式碼需要寫什麼測試？"
     - "產生程式碼覆蓋率報告"

3. **技能會自動觸發：**
   - AI 會根據您的提問內容，自動載入相關的技能
   - 您不需要手動指定技能名稱

---

## 💡 使用情境範例

### 情境 1：建立新的測試專案

當您問「如何建立 xUnit 測試專案？」時，`xunit-project-setup` 技能會自動觸發，協助您：

- 建立標準的專案結構
- 設定 csproj 檔案
- 安裝必要的 NuGet 套件
- 配置測試執行環境

### 情境 2：撰寫單元測試

當您問「幫我為這個方法寫單元測試」時，`unit-test-fundamentals` 和 `test-naming-conventions` 會協同工作：

- 使用 3A Pattern 組織測試
- 應用三段式命名法
- 遵循 FIRST 原則
- 產生適當的斷言

### 情境 3：分析測試覆蓋率

當您問「如何檢視程式碼覆蓋率？」時，`code-coverage-analysis` 技能會指導您：

- 配置 Coverlet
- 執行覆蓋率測試
- 產生 HTML 報告
- 解讀覆蓋率指標
- 在 CI/CD 中整合

---

## 🎯 技能組合建議

### 入門組合 (適合新專案)

```text
1. xunit-project-setup          → 建立測試專案
2. unit-test-fundamentals       → 學習測試基礎
3. test-naming-conventions      → 建立命名規範
```

### 品質保證組合

```text
1. code-coverage-analysis       → 監控測試覆蓋率
2. awesome-assertions-guide     → 提升斷言品質
3. test-output-logging         → 強化除錯能力
```

### 進階測試組合

```text
1. autofixture-basics               → AutoFixture 基礎
2. autofixture-customization        → 自訂化策略
3. autofixture-nsubstitute-integration → 自動模擬整合
4. nsubstitute-mocking              → 隔離外部相依
```

### 整合測試組合

請參考 [dotnet-testing-advanced](../dotnet-testing-advanced/README.md)

```text
1. aspnet-integration-testing → API 測試
2. testcontainers-database    → 資料庫容器化測試
3. aspire-testing             → Aspire 整合測試
```

---

## � 技能統計

本目錄共包含 **19 個**測試技能，涵蓋從基礎到進階的單元測試主題。

> 📝 進階技能（整合測試、框架遷移）請參考 [dotnet-testing-advanced](../dotnet-testing-advanced/README.md)

查看完整的提煉記錄：[EXTRACTION_LOG.md](../../../EXTRACTION_LOG.md)

---

## 📖 相關資源

### 原始內容

- **iThome 鐵人賽文章**: [老派軟體工程師的測試修練 - 30 天挑戰](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
- **範例程式碼**: [30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)

### Agent Skills 標準

- **官方網站**: [agentskills.io](https://agentskills.io)
- **GitHub 官方文件**: [About Agent Skills](https://docs.github.com/copilot/using-github-copilot/using-github-copilot-agent-skills)
- **VS Code 指南**: [Use Agent Skills in VS Code](https://code.visualstudio.com/docs/copilot/copilot-customization#_agent-skills)

### 社群資源

- **GitHub Awesome Copilot**: [github/awesome-copilot](https://github.com/github/awesome-copilot)
- **Anthropic Skills**: [anthropics/skills](https://github.com/anthropics/skills)

---

## 📝 授權

MIT License

---

**最後更新**: 2026-01-19
