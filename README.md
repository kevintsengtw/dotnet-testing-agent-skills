# .NET Testing Agent Skills

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![xUnit](https://img.shields.io/badge/xUnit-3.x-5C2D91)](https://xunit.net/)
[![Agent Skills](https://img.shields.io/badge/Agent_Skills-Standard-blue)](https://agentskills.io)

> 🏆 基於 **2025 iThome 鐵人賽 Software Development 組冠軍作品**「老派軟體工程師的測試修練 - 30 天挑戰」提煉而成

**跨平台通用的 AI Agent Skills 集合**，遵循 [agentskills.io](https://agentskills.io) 開放標準。涵蓋從單元測試到整合測試的完整最佳實踐，可在 **GitHub Copilot、Claude、Cursor** 等多種 AI 工具中使用！

---

## ✨ 特色

- 🌐 **跨平台通用**：符合 agentskills.io 開放標準，可在多種 AI 工具使用
- 🎯 **自動觸發**：AI 根據對話內容自動載入相關技能
- 📚 **27 個精煉技能**：涵蓋單元測試、模擬、測試資料生成、整合測試等
- 🔧 **即用範本**：提供完整的專案結構與程式碼範例
- 📖 **中文友善**：完整的繁體中文文件與命名建議

---

## 🌐 支援的 AI 平台

本 repository 的 Agent Skills 可以在以下 AI 平台使用：

| 平台 | 支援狀態 | 使用方式 |
|------|----------|----------|
| **GitHub Copilot (VS Code)** | ✅ 完整支援 | 複製到 \.github/skills/\ |
| **GitHub Copilot CLI** | ✅ 完整支援 | 同上 |
| **Claude Desktop** | ✅ 完整支援 | 使用 \/plugin\ 指令或複製到專案 |
| **Claude Code CLI** | ✅ 完整支援 | 複製到 \.claude/skills/\ |
| **Cursor** | ✅ 完整支援 | 複製到 \.cursor/skills/\ |
| **其他支援 Agent Skills 的工具** | ✅ 通用 | 查閱該工具的文件 |

> 💡 詳細的跨平台使用說明請參考 [PLATFORM_GUIDE.md](PLATFORM_GUIDE.md)

---

## 🚀 快速開始

### GitHub Copilot (VS Code)

\\\ash
# 1. Clone 此 repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 2. 複製到您的專案
cp -r dotnet-testing-agent-skills/.github/skills /your-project/.github/

# 3. 在 VS Code 中啟用 Agent Skills
# 設定 → 搜尋 \"chat.useAgentSkills\" → 勾選啟用
\\\

### Claude Desktop / Code CLI

\\\ash
# 方法 1: 直接使用 /plugin 指令
# 在 Claude 對話中：/plugin path/to/skill/SKILL.md

# 方法 2: 複製到專案
cp -r dotnet-testing-agent-skills/.github/skills /your-project/.claude/skills/
\\\

### Cursor

\\\ash
# 複製到 Cursor 的 skills 目錄
cp -r dotnet-testing-agent-skills/.github/skills /your-project/.cursor/skills/
\\\

### Git Submodule（適用於所有平台）

\\\ash
cd /your-project

# 加入 submodule
git submodule add https://github.com/kevintsengtw/dotnet-testing-agent-skills .agent-skills

# 根據您使用的 AI 平台建立符號連結
ln -s .agent-skills/.github/skills .github/skills          # GitHub Copilot
ln -s .agent-skills/.github/skills .claude/skills          # Claude
ln -s .agent-skills/.github/skills .cursor/skills          # Cursor
\\\

### 選擇性複製

只需要特定技能？

\\\ash
# 只複製單元測試基礎
cp -r dotnet-testing-agent-skills/.github/skills/dotnet-testing/unit-test-fundamentals /your-project/.ai-skills/

# 只複製 AutoFixture 系列
cp -r dotnet-testing-agent-skills/.github/skills/dotnet-testing/autofixture-* /your-project/.ai-skills/
\\\

---
# 只複製 AutoFixture 系列
cp -r dotnet-testing-agent-skills/.github/skills/dotnet-testing/autofixture-* /your-project/.github/skills/
```

---

## 📦 技能清單

### 基礎技能 (19 個)

<details>
<summary>第一階段：測試基礎與斷言 (10 個)</summary>

| 技能 | 說明 |
|------|------|
| `unit-test-fundamentals` | FIRST 原則、3A Pattern、測試金字塔 |
| `test-naming-conventions` | 三段式命名法、中文命名建議 |
| `xunit-project-setup` | xUnit 專案結構、配置、套件管理 |
| `awesome-assertions-guide` | FluentAssertions 流暢斷言 |
| `complex-object-comparison` | 深層物件比對技巧 |
| `code-coverage-analysis` | Coverlet 覆蓋率分析與報告 |
| `nsubstitute-mocking` | Mock/Stub/Spy 測試替身 |
| `test-output-logging` | ITestOutputHelper 與 ILogger 整合 |
| `private-internal-testing` | Private/Internal 成員測試策略 |
| `fluentvalidation-testing` | FluentValidation 驗證器測試 |

</details>

<details>
<summary>第二階段：可測試性抽象化 (2 個)</summary>

| 技能 | 說明 |
|------|------|
| `datetime-testing-timeprovider` | TimeProvider 時間抽象化 |
| `filesystem-testing-abstractions` | System.IO.Abstractions 檔案系統測試 |

</details>

<details>
<summary>第三階段：測試資料生成 (7 個)</summary>

| 技能 | 說明 |
|------|------|
| `test-data-builder-pattern` | 手動 Builder Pattern |
| `autofixture-basics` | AutoFixture 基礎與匿名測試資料 |
| `autofixture-customization` | AutoFixture 自訂化策略 |
| `autodata-xunit-integration` | AutoData 與 xUnit Theory 整合 |
| `autofixture-nsubstitute-integration` | AutoFixture + NSubstitute 自動模擬 |
| `bogus-fake-data` | Bogus 擬真資料產生 |
| `autofixture-bogus-integration` | AutoFixture 與 Bogus 整合 |

</details>

### 進階技能 (8 個)

<details>
<summary>第四階段：整合測試 (5 個)</summary>

| 技能 | 說明 |
|------|------|
| `aspnet-integration-testing` | WebApplicationFactory 整合測試 |
| `testcontainers-database` | PostgreSQL/MSSQL 容器化測試 |
| `testcontainers-nosql` | MongoDB/Redis 容器化測試 |
| `webapi-integration-testing` | WebAPI 完整整合測試流程 |
| `aspire-testing` | .NET Aspire Testing 框架 |

</details>

<details>
<summary>第五階段：框架遷移 (3 個)</summary>

| 技能 | 說明 |
|------|------|
| `xunit-upgrade-guide` | xUnit 2.9.x → 3.x 升級指南 |
| `tunit-fundamentals` | TUnit 新世代測試框架入門 |
| `tunit-advanced` | TUnit 進階應用 |

</details>

---

## 💡 使用範例

設定完成後，您只需要自然地對話：

```text
👤：幫我建立一個 xUnit 測試專案

🤖：[自動載入 xunit-project-setup 技能]
    我將協助您建立標準的 xUnit 測試專案結構...
    
    1. 建立專案檔案
    2. 配置必要套件
    3. 設定測試執行環境
```

```text
👤：為這個 Service 寫單元測試，它有依賴需要 Mock

🤖：[自動載入 unit-test-fundamentals + nsubstitute-mocking 技能]
    根據測試最佳實踐，我將建立符合 3A Pattern 的測試...
```

更多使用情境請參考 [完整使用手冊](SKILLS_USAGE_GUIDE.md)。

---

## 🎓 學習資源

### 原始內容

這些 Agent Skills 是從以下內容提煉而成：

- 📖 **iThome 鐵人賽系列文章**：[老派軟體工程師的測試修練 - 30 天挑戰](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)  
  🏆 2025 iThome 鐵人賽 Software Development 組冠軍
  
- 💻 **完整範例程式碼**：[30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)  
  包含所有範例專案的可執行程式碼

### 學習路徑

```
閱讀文章 → 執行範例 → 使用 Agent Skills
   ↓           ↓              ↓
  理解概念    實作練習      AI 輔助開發
```

---

## 📋 環境需求

### 基礎技能
- .NET 8.0 SDK 或更新版本
- VS Code / Visual Studio / Rider
- GitHub Copilot 或其他支援 Agent Skills 的 AI 助理

### 進階技能（整合測試）
- Docker Desktop
- WSL2（Windows 環境）
- .NET Aspire Workload（用於 Aspire Testing）

---

## 🤖 支援的 AI 平台

| 平台 | 支援狀態 | 說明 |
|------|----------|------|
| GitHub Copilot (VS Code) | ✅ 完整支援 | 需啟用 Agent Mode |
| GitHub Copilot CLI | ✅ 完整支援 | - |
| Claude Code CLI | ✅ 完整支援 | 使用 `/plugin` 指令 |
| Cursor | ✅ 完整支援 | - |
| Claude.ai (Web) | ⚠️ 部分支援 | 需手動貼上技能內容 |

---

## 🔗 相關連結

- **完整使用手冊**：[SKILLS_USAGE_GUIDE.md](SKILLS_USAGE_GUIDE.md)
- **Agent Skills 標準**：[agentskills.io](https://agentskills.io)
- **GitHub Copilot Agent Skills 文件**：[官方說明](https://docs.github.com/copilot/using-github-copilot/using-github-copilot-agent-skills)

---

## 🛠️ 技能組合建議

### 🆕 新手入門
```
unit-test-fundamentals → test-naming-conventions 
→ xunit-project-setup → awesome-assertions-guide
```

### 🚀 效率提升
```
autofixture-basics → autofixture-customization 
→ autofixture-nsubstitute-integration → autodata-xunit-integration
```

### 🔗 整合測試
```
aspnet-integration-testing → testcontainers-database 
→ webapi-integration-testing → aspire-testing
```

---

## 🤝 貢獻

歡迎提交 Issues 和 Pull Requests！

如果您發現技能內容有誤或想要新增新技能，請：
1. Fork 本專案
2. 建立您的 feature branch
3. 提交 Pull Request

---

## 📄 授權

MIT License - 自由使用與修改

---

## 🙏 致謝

感謝所有在 iThome 鐵人賽期間給予支持與回饋的讀者們！

---

**作者**：Kevin Tseng  
**最後更新**：2026-01-20

