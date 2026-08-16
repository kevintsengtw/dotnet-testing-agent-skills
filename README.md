# .NET Testing Agent Skills

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![xUnit](https://img.shields.io/badge/xUnit-2.x-5C2D91)](https://xunit.net/)
[![xUnit](https://img.shields.io/badge/xUnit-3.x-5C2D91)](https://xunit.net/)

> 🏆 基於 **2025 iThome 鐵人賽 Software Development 組冠軍作品**「老派軟體工程師的測試修練 - 30 天挑戰」提煉而成

專為 .NET 開發者打造的 AI Agent Skills 集合，涵蓋從單元測試到整合測試的完整最佳實踐。讓 GitHub Copilot、Claude 等 AI 助理自動為您提供專業的測試指導！

---

## ✨ 特色

- 🎯 **符合官方規範**：29 個 skills 的 description 包含 Keywords 關鍵字，AI 根據對話內容自動載入
- � **符合 Anthropic 最佳實踐**：依據官方「[The Complete Guide to Building Skills for Claude](https://claude.com/blog/complete-guide-to-building-skills-for-claude)」全面優化，採用漸進式揭露 (Progressive Disclosure) 架構
- 📚 **29 個精煉技能**：包含 2 個總覽技能 + 27 個專業技能，涵蓋單元測試、模擬、測試資料生成、整合測試等
- 🔧 **即用範本**：提供完整的專案結構與程式碼範例
- 🌐 **多平台支援**：GitHub Copilot、Claude Code、Cursor 等
- 📖 **中文友善**：完整的繁體中文文件與命名建議
- 📦 **標準化結構**：符合 Claude Code skills 標準，支援 npx skills install 安裝
- 📊 **2026-02-01 全面優化**：Description（含 Keywords）、入口導航全面強化
- 📐 **2026-02-11 Anthropic 規範優化**：依據官方 Skill 建立指南，全面調整 29 個 SKILL.md 結構與內容
- 🔬 **2026-03-07 Skill-Creator 規範優化**：使用新版 skill-creator 檢視並優化全部 29 個 SKILL.md 的寫作風格與結構
- 🔧 **2026-03-11 Skill-Creator 驗證測試後微調**：針對 3 個 skills 進行驗證測試後的回應品質微調
- 📦 **2026-03-23 NuGet 套件版本升級 + Skill 結構重構**：全面升級測試相關套件版本，12 個 SKILL.md 瘦身並將程式碼範例拆至 references/
- 🔄 **2026-03-31 NuGet 套件版本同步更新**：14 個 NuGet 套件升級至最新穩定版，修正 Testcontainers Wait Strategy 與 FluentValidation 套件參考問題

---

## 🚀 快速開始

### 方法一：使用 npx skills install（推薦）

```bash
# 從 GitHub 直接安裝到 Claude Code 全域 skills
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 或安裝到當前工作區
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git --workspace
```

### 方法二：直接複製

#### 複製到 GitHub Copilot（VS Code）

**Linux / macOS (Bash)**
```bash
# 1. Clone 此 repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 2. 複製到您的專案（GitHub Copilot 使用 .github/skills）
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# 3. VS Code v1.109+ 已預設啟用 Agent Skills，無需手動設定
#    舊版 VS Code：設定 → 搜尋 "chat.useAgentSkills" → 勾選啟用
```

**Windows (PowerShell)**
```powershell
# 1. Clone 此 repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 2. 複製到您的專案（GitHub Copilot 使用 .github/skills）
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.github\" -Recurse

# 3. VS Code v1.109+ 已預設啟用 Agent Skills，無需手動設定
#    舊版 VS Code：設定 → 搜尋 "chat.useAgentSkills" → 勾選啟用
```

#### 複製到 Claude Code

**Linux / macOS (Bash)**
```bash
# 複製到 Claude Code 工作區 skills
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/

# 或複製到全域 skills
cp -r dotnet-testing-agent-skills/skills ~/.config/claude/
```

**Windows (PowerShell)**
```powershell
# 複製到 Claude Code 工作區 skills
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.claude\" -Recurse

# 或複製到全域 skills
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "$env:APPDATA\claude\" -Recurse
```

### 方法三：Git Submodule

```bash
cd /your-project

# 對 GitHub Copilot：加入 submodule 到 .github/skills
git submodule add https://github.com/kevintsengtw/dotnet-testing-agent-skills .github/skills
cd .github/skills && cp -r skills/* . && cd ../..

# 對 Claude Code：加入 submodule 到 .claude/skills
git submodule add https://github.com/kevintsengtw/dotnet-testing-agent-skills .claude/skills
cd .claude/skills && cp -r skills/* . && cd ../..
```

### 方法四：選擇性複製

只需要特定技能？

#### Linux / macOS (Bash)

```bash
# 只複製單元測試基礎
cp -r dotnet-testing-agent-skills/skills/dotnet-testing-unit-test-fundamentals /your-project/.github/skills/

# 只複製 AutoFixture 系列
cp -r dotnet-testing-agent-skills/skills/dotnet-testing-autofixture-* /your-project/.github/skills/

# 只複製總覽 skills
cp -r dotnet-testing-agent-skills/skills/dotnet-testing /your-project/.github/skills/
cp -r dotnet-testing-agent-skills/skills/dotnet-testing-advanced /your-project/.github/skills/
```

#### Windows (PowerShell)

```powershell
# 只複製單元測試基礎
Copy-Item -Path "dotnet-testing-agent-skills\skills\dotnet-testing-unit-test-fundamentals" -Destination "\your-project\.github\skills\" -Recurse

# 只複製 AutoFixture 系列
Get-ChildItem -Path "dotnet-testing-agent-skills\skills\dotnet-testing-autofixture-*" | Copy-Item -Destination "\your-project\.github\skills\" -Recurse

# 只複製總覽 skills
Copy-Item -Path "dotnet-testing-agent-skills\skills\dotnet-testing" -Destination "\your-project\.github\skills\" -Recurse
Copy-Item -Path "dotnet-testing-agent-skills\skills\dotnet-testing-advanced" -Destination "\your-project\.github\skills\" -Recurse
```

---

## 目錄結構

```text
skills/
├── dotnet-testing/                              # ⭐ 總覽：基礎技能導航（19 個子技能）
├── dotnet-testing-advanced/                     # ⭐ 總覽：進階技能導航（8 個子技能）
├── dotnet-testing-unit-test-fundamentals/
├── dotnet-testing-test-naming-conventions/
├── dotnet-testing-xunit-project-setup/
├── dotnet-testing-awesome-assertions-guide/
├── dotnet-testing-complex-object-comparison/
├── dotnet-testing-code-coverage-analysis/
├── dotnet-testing-nsubstitute-mocking/
├── dotnet-testing-test-output-logging/
├── dotnet-testing-private-internal-testing/
├── dotnet-testing-fluentvalidation-testing/
├── dotnet-testing-datetime-testing-timeprovider/
├── dotnet-testing-filesystem-testing-abstractions/
├── dotnet-testing-test-data-builder-pattern/
├── dotnet-testing-autofixture-basics/
├── dotnet-testing-autofixture-customization/
├── dotnet-testing-autodata-xunit-integration/
├── dotnet-testing-autofixture-nsubstitute-integration/
├── dotnet-testing-bogus-fake-data/
├── dotnet-testing-autofixture-bogus-integration/
├── dotnet-testing-advanced-aspnet-integration-testing/
├── dotnet-testing-advanced-testcontainers-database/
├── dotnet-testing-advanced-testcontainers-nosql/
├── dotnet-testing-advanced-webapi-integration-testing/
├── dotnet-testing-advanced-aspire-testing/
├── dotnet-testing-advanced-xunit-upgrade-guide/
├── dotnet-testing-advanced-tunit-fundamentals/
└── dotnet-testing-advanced-tunit-advanced/
```

> **注意**：
> - Skills 採用扁平結構，使用前綴命名來區分基礎技能 (`dotnet-testing-*`) 與進階技能 (`dotnet-testing-advanced-*`)
> - ⭐ 兩個總覽 skills 提供智能導航，自動推薦適合的子技能組合
> - 各技能目錄可能包含 `templates/`（範本）與 `references/`（參考文件）子目錄，採用漸進式揭露架構
> - 安裝後，skills 會根據目標環境複製到對應位置（`.github/skills/` 或 `.claude/skills/`）

---

## 📖 快速參考指南（推薦）

為了讓您快速上手並充分運用這些 skills，我們提供了完整的參考指南：

### `SKILLS_QUICK_REFERENCE.md` (v2.0.0 合併版)

**Skills 快速參考指南** - 整合 AI Agent 與開發者使用指引

**給 AI Agent 的指引**：
- 🔍 **關鍵字快速對應表**：20 個常用場景的關鍵字 → Skill 映射
- 🚀 **AI Agent 工作流程範本**：4 步驟自動化流程說明

**給開發者的參考**：
- 💬 **Prompt 模板**：3 種推薦的對話模板
- 🎯 **常見情境組合**：3 個完整的使用情境範例
- 📋 **完整技能目錄**：27 個 skills 分類清單
- 📊 **Skills 優化狀態**：Keywords 整合、預期效果說明

**使用方式**：
```bash
# 方法 1：複製為參考檔（推薦）
cp SKILLS_QUICK_REFERENCE.md /your-project/SKILLS_QUICK_REFERENCE.md

# 方法 2：整合到既有文件
# 如果專案已有類似的參考文件，建議手動將內容整合進去
```

**👉 為什麼需要這個指南？**

雖然總覽 skills (`dotnet-testing` 和 `dotnet-testing-advanced`) 提供智能導航，但快速參考指南能：

1. ✅ **快速查詢**：不確定用哪個 skill 時立即找到答案
2. ✅ **範例 Prompt**：提供可直接複製使用的對話範例
3. ✅ **情境組合**：展示實際的多 skill 搭配使用案例
4. ✅ **降低學習門檻**：不需記住所有 skill 名稱

---

## 技能清單

### 🎯 總覽技能 (2 個) - 新增！

> **NEW!** 兩個總覽 skills 提供智能導航，當您不確定使用哪個技能時，它們會自動分析需求並推薦適合的技能組合。

| 技能 | 說明 | 何時使用 |
|------|------|---------|
| `dotnet-testing` | 基礎測試技能總覽與引導中心 | 詢問「如何寫 .NET 測試」、「測試入門」等一般性問題時自動觸發 |
| `dotnet-testing-advanced` | 進階測試技能總覽與引導中心 | 詢問「整合測試」、「API 測試」、「微服務測試」等進階需求時自動觸發 |

**總覽 skills 的價值**：
- ✅ **智能推薦**：根據您的具體需求，推薦 1-4 個最適合的子技能組合
- ✅ **學習路徑**：提供循序漸進的學習建議（新手路徑、進階路徑）
- ✅ **決策支援**：透過決策樹快速找到需要的技能
- ✅ **範例導向**：每個任務都有完整的提示詞範例

### 基礎技能 (19 個)

<details>
<summary>第一階段：測試基礎與斷言 (10 個)</summary>

| 技能 | 說明 |
|------|------|
| `dotnet-testing-unit-test-fundamentals` | FIRST 原則、3A Pattern、測試金字塔 |
| `dotnet-testing-test-naming-conventions` | 三段式命名法、中文命名建議 |
| `dotnet-testing-xunit-project-setup` | xUnit 專案結構、配置、套件管理 |
| `dotnet-testing-awesome-assertions-guide` | FluentAssertions 流暢斷言 |
| `dotnet-testing-complex-object-comparison` | 深層物件比對技巧 |
| `dotnet-testing-code-coverage-analysis` | Coverlet 覆蓋率分析與報告 |
| `dotnet-testing-nsubstitute-mocking` | Mock/Stub/Spy 測試替身 |
| `dotnet-testing-test-output-logging` | ITestOutputHelper 與 ILogger 整合 |
| `dotnet-testing-private-internal-testing` | Private/Internal 成員測試策略 |
| `dotnet-testing-fluentvalidation-testing` | FluentValidation 驗證器測試 |

</details>

<details>
<summary>第二階段：可測試性抽象化 (2 個)</summary>

| 技能 | 說明 |
|------|------|
| `dotnet-testing-datetime-testing-timeprovider` | TimeProvider 時間抽象化 |
| `dotnet-testing-filesystem-testing-abstractions` | System.IO.Abstractions 檔案系統測試 |

</details>

<details>
<summary>第三階段：測試資料生成 (7 個)</summary>

| 技能 | 說明 |
|------|------|
| `dotnet-testing-test-data-builder-pattern` | 手動 Builder Pattern |
| `dotnet-testing-autofixture-basics` | AutoFixture 基礎與匿名測試資料 |
| `dotnet-testing-autofixture-customization` | AutoFixture 自訂化策略 |
| `dotnet-testing-autodata-xunit-integration` | AutoData 與 xUnit Theory 整合 |
| `dotnet-testing-autofixture-nsubstitute-integration` | AutoFixture + NSubstitute 自動模擬 |
| `dotnet-testing-bogus-fake-data` | Bogus 擬真資料產生 |
| `dotnet-testing-autofixture-bogus-integration` | AutoFixture 與 Bogus 整合 |

</details>

### 進階技能 (8 個)

<details>
<summary>第四階段：整合測試 (5 個)</summary>

| 技能 | 說明 |
|------|------|
| `dotnet-testing-advanced-aspnet-integration-testing` | WebApplicationFactory 整合測試 |
| `dotnet-testing-advanced-testcontainers-database` | PostgreSQL/MSSQL 容器化測試 |
| `dotnet-testing-advanced-testcontainers-nosql` | MongoDB/Redis 容器化測試 |
| `dotnet-testing-advanced-webapi-integration-testing` | WebAPI 完整整合測試流程 |
| `dotnet-testing-advanced-aspire-testing` | .NET Aspire Testing 框架 |

</details>

<details>
<summary>第五階段：框架遷移 (3 個)</summary>

| 技能 | 說明 |
|------|------|
| `dotnet-testing-advanced-xunit-upgrade-guide` | xUnit 2.9.x → 3.x 升級指南 |
| `dotnet-testing-advanced-tunit-fundamentals` | TUnit 新世代測試框架入門 |
| `dotnet-testing-advanced-tunit-advanced` | TUnit 進階應用 |

</details>

---

## 使用範例

設定完成後，您只需要自然地對話：

```text
👤：幫我建立一個 xUnit 測試專案

🤖：[自動載入 dotnet-testing-xunit-project-setup 技能]
    我將協助您建立標準的 xUnit 測試專案結構...
    
    1. 建立專案檔案
    2. 配置必要套件
    3. 設定測試執行環境
```

```text
👤：為這個 Service 寫單元測試，它有依賴需要 Mock

🤖：[自動載入 dotnet-testing-unit-test-fundamentals + dotnet-testing-nsubstitute-mocking 技能]
    根據測試最佳實踐，我將建立符合 3A Pattern 的測試...
```

更多使用情境請參考 [完整使用手冊](SKILLS_USAGE_GUIDE.md)。

---

## 學習資源

### 原始內容

這些 Agent Skills 是從以下內容提煉而成：

- 📖 **iThome 鐵人賽系列文章**：[老派軟體工程師的測試修練 - 30 天挑戰](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)  
  🏆 2025 iThome 鐵人賽 Software Development 組冠軍
  
- 💻 **完整範例程式碼**：[30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)  
  包含所有範例專案的可執行程式碼

### 深入學習文件

本專案提供五份完整的 Agent Skills 教學文件，幫助你深入理解如何打造專業的 AI 技能包：

- **[Agent Skills：從架構設計到實戰應用](docs/Agent_Skills_Mastery.pdf)**  
  完整涵蓋 Agent Skills 從理論到實踐的系統性教材。整合架構設計、模組化設計與實戰應用，提供從基礎概念到進階整合的完整學習路徑。

- **[Claude Code Skills: 讓 AI 變身專業工匠](docs/Agent_Skills_Architecture.pdf)**  
  深入解析 Agent Skills 的架構設計、運作原理與最佳實踐。從基礎概念到進階應用，完整說明如何將 AI 從通才訓練成專才。

- **[Agent Skills: 打造模組化 AI 專業技能包](docs/Agent_Skills_Modular_Mastery.pdf)**  
  詳細說明如何設計模組化的技能結構，包含 SKILL.md 撰寫規範、漸進式揭露機制、以及與其他客製化工具（Custom Instructions、MCP、Prompt Files）的比較與整合。

- **[Agent Skills 實戰: 打造 .NET 測試自動化專家](docs/Agent_Skills_.NET_Testing_Expert.pdf)**  
  從零開始的實作教學，一步步引導你建立第一個 Agent Skill。涵蓋完整的開發流程、觸發機制、以及如何在 VS Code 中成功啟用並測試你的技能。

- **[.NET Testing：寫得更好、跑得更快](docs/NET_Testing_Write_Better_Run_Faster.pdf)**  
  結合 NikiforovAll 的 `dotnet-test` skill，專注於測試執行優化與除錯。教你如何使用 Build-First 策略提升效能、透過精準過濾執行特定測試案例、以及運用 Blame Mode (`--blame-hang` / `--blame-crash`) 診斷測試卡死或崩潰問題。此技能可與 `kevintsengtw/dotnet-testing-agent-skills` 互補，前者專注於「如何執行測試」，後者專注於「如何撰寫測試」。  
  **相關資源**：[NikiforovAll dotnet-test skill](https://github.com/NikiforovAll/claude-code-rules/tree/main/plugins/handbook-dotnet/skills/dotnet-test) | [2 MUST USE features for dotnet test debugging](https://www.youtube.com/watch?v=JTmIO21KmGw)

### 30 天挑戰完整索引

<details>
<summary>📚 第一階段：測試基礎與斷言 (Day 01-09)</summary>

| Day | 主題 | 文章 | 範例 |
|-----|------|------|------|
| 01 | 老派工程師的測試啟蒙 - 為什麼我們需要測試？ | [連結](https://ithelp.ithome.com.tw/articles/10373888) | [day01/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day01) |
| 02 | xUnit 框架深度解析 - 從生態概觀到實戰專案 | [連結](https://ithelp.ithome.com.tw/articles/10373952) | [day02/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day02) |
| 03 | xUnit 進階功能與測試資料管理 | [連結](https://ithelp.ithome.com.tw/articles/10374064) | [day03/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day03) |
| 04 | AwesomeAssertions 基礎應用與實戰技巧 | [連結](https://ithelp.ithome.com.tw/articles/10374188) | [day04/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day04) |
| 05 | AwesomeAssertions 進階技巧與複雜情境應用 | [連結](https://ithelp.ithome.com.tw/articles/10374425) | [day05/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day05) |
| 06 | Code Coverage 程式碼涵蓋範圍實戰指南 | [連結](https://ithelp.ithome.com.tw/articles/10374467) | - |
| 07 | 依賴替代入門 - 使用 NSubstitute | [連結](https://ithelp.ithome.com.tw/articles/10374593) | [day07/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day07) |
| 08 | 測試輸出與記錄 - xUnit ITestOutputHelper 與 ILogger | [連結](https://ithelp.ithome.com.tw/articles/10374711) | [day08/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day08) |
| 09 | 測試私有與內部成員 - Private 與 Internal 的測試策略 | [連結](https://ithelp.ithome.com.tw/articles/10374866) | [day09/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day09) |

</details>

<details>
<summary>🔧 第二階段：測試資料生成 (Day 10-18)</summary>

| Day | 主題 | 文章 | 範例 |
|-----|------|------|------|
| 10 | AutoFixture 基礎：自動產生測試資料 | [連結](https://ithelp.ithome.com.tw/articles/10375018) | [day10/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day10) |
| 11 | AutoFixture 進階：自訂化測試資料生成策略 | [連結](https://ithelp.ithome.com.tw/articles/10375153) | [day11/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day11) |
| 12 | 結合 AutoData：xUnit 與 AutoFixture 的整合應用 | [連結](https://ithelp.ithome.com.tw/articles/10375296) | [day12/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day12) |
| 13 | NSubstitute 與 AutoFixture 的整合應用 | [連結](https://ithelp.ithome.com.tw/articles/10375419) | [day13/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day13) |
| 14 | Bogus 入門：與 AutoFixture 的差異比較 | [連結](https://ithelp.ithome.com.tw/articles/10375501) | [day14/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day14) |
| 15 | AutoFixture 與 Bogus 的整合應用 | [連結](https://ithelp.ithome.com.tw/articles/10375620) | [day15/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day15) |
| 16 | 測試日期與時間：Microsoft.Bcl.TimeProvider 取代 DateTime | [連結](https://ithelp.ithome.com.tw/articles/10375821) | [day16/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day16) |
| 17 | 檔案與 IO 測試：使用 System.IO.Abstractions 模擬檔案系統 | [連結](https://ithelp.ithome.com.tw/articles/10375981) | [day17/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day17) |
| 18 | 驗證測試：FluentValidation Test Extensions | [連結](https://ithelp.ithome.com.tw/articles/10376147) | [day18/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day18) |

</details>

<details>
<summary>🔗 第三階段：整合測試 (Day 19-25)</summary>

| Day | 主題 | 文章 | 範例 |
|-----|------|------|------|
| 19 | 整合測試入門：基礎架構與應用場景 | [連結](https://ithelp.ithome.com.tw/articles/10376335) | [day19/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day19) |
| 20 | Testcontainers 初探：使用 Docker 架設測試環境 | [連結](https://ithelp.ithome.com.tw/articles/10376401) | [day20/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day20) |
| 21 | Testcontainers 整合測試：MSSQL + EF Core 以及 Dapper | [連結](https://ithelp.ithome.com.tw/articles/10376524) | [day21/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day21) |
| 22 | Testcontainers 整合測試：MongoDB 及 Redis 基礎到進階 | [連結](https://ithelp.ithome.com.tw/articles/10376740) | [day22/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day22) |
| 23 | 整合測試實戰：WebApi 服務的整合測試 | [連結](https://ithelp.ithome.com.tw/articles/10376873) | [day23/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day23) |
| 24 | .NET Aspire Testing 入門基礎介紹 | [連結](https://ithelp.ithome.com.tw/articles/10377071) | [day24/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day24) |
| 25 | .NET Aspire 整合測試實戰：從 Testcontainers 到 Aspire | [連結](https://ithelp.ithome.com.tw/articles/10377197) | [day25/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day25) |

</details>

<details>
<summary>🚀 第四階段：框架遷移與進階應用 (Day 26-30)</summary>

| Day | 主題 | 文章 | 範例 |
|-----|------|------|------|
| 26 | xUnit 升級指南：從 2.9.x 到 3.x 的轉換 | [連結](https://ithelp.ithome.com.tw/articles/10377477) | [day26/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day26) |
| 27 | GitHub Copilot 測試實戰：AI 輔助測試開發指南 | [連結](https://ithelp.ithome.com.tw/articles/10377577) | [day27/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day27) |
| 28 | TUnit 入門 - 下世代 .NET 測試框架探索 | [連結](https://ithelp.ithome.com.tw/articles/10377828) | [day28/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day28) |
| 29 | TUnit 進階應用：資料驅動測試與依賴注入深度實戰 | [連結](https://ithelp.ithome.com.tw/articles/10377970) | [day29/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day29) |
| 30 | TUnit 進階應用 - 執行控制與測試品質和 ASP.NET Core 整合 | [連結](https://ithelp.ithome.com.tw/articles/10378176) | [day30/](https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day30) |

</details>

### 學習路徑

```mermaid
graph LR
    A[📖 閱讀文章] --> B[💻 執行範例] --> C[🤖 使用 Agent Skills]
    A --> D[理解概念]
    B --> E[實作練習]
    C --> F[AI 輔助開發]
    
    style A fill:#e1f5ff
    style B fill:#fff4e1
    style C fill:#e8f5e9
    style D fill:#f3e5f5
    style E fill:#fff9c4
    style F fill:#f1f8e9
```

---

## 環境需求

### 基礎技能

- .NET 8.0 SDK 或更新版本
- VS Code / Visual Studio / Rider
- GitHub Copilot 或其他支援 Agent Skills 的 AI 助理

### 進階技能（整合測試）

- Docker Desktop
- WSL2（Windows 環境）
- .NET Aspire Workload（用於 Aspire Testing）

---

## 支援的 AI 平台

| 平台 | 支援狀態 | 說明 |
|------|---------|------|
| GitHub Copilot (VS Code) | ✅ 完整支援 | v1.109+ 預設啟用，需使用 Agent Mode |
| GitHub Copilot CLI | ✅ 完整支援 | - |
| Claude Code CLI | ✅ 完整支援 | 使用 `/plugin` 指令 |
| Cursor | ✅ 完整支援 | - |
| Claude.ai (Web) | ⚠️ 部分支援 | 需手動貼上技能內容 |

---

## 相關連結

> ⚠️ **GitHub Copilot 使用者請務必閱讀**：[GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md) 包含 VS Code v1.109 Agent Skills 正式發佈 (GA) 的重要更新說明，包括預設啟用、彈性搜尋路徑、多 AI 工具共用工作區等關鍵變更。

- **GitHub Copilot 更新紀錄**：[GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md) — Copilot 使用者必讀
- **完整使用手冊**：[SKILLS_USAGE_GUIDE.md](SKILLS_USAGE_GUIDE.md)
- **Agent Skills 標準**：[agentskills.io](https://agentskills.io)
- **Anthropic Skill 建立完整指南**：[The Complete Guide to Building Skills for Claude](https://claude.com/blog/complete-guide-to-building-skills-for-claude) — 本專案 2026-02-11 優化依據
  - [PDF 版本](https://resources.anthropic.com/hubfs/The-Complete-Guide-to-Building-Skill-for-Claude.pdf?hsLang=en)
  - [Anthropic Skill Authoring Best Practices](https://platform.claude.com/docs/agent-skills/skill-authoring-best-practices)
- **GitHub Copilot Agent Skills 文件**：[官方說明](https://docs.github.com/copilot/using-github-copilot/using-github-copilot-agent-skills)

---

## 2026-08-16 修正 AwesomeAssertions 幻覺 API 名稱 (v2.4.2)

修正 3 個 Skills、8 處錯誤的斷言方法名稱。**這些名稱在 AwesomeAssertions 中並不存在，複製受影響的範本會直接造成 CS1061 編譯錯誤。**

### 為什麼會發生

這三個名稱是 **FluentAssertions 5.x 時代的舊式命名**。AwesomeAssertions 承接的是 FluentAssertions 7 的 API，只保留 `...ThanOrEqualTo` 形式，舊名少了中間的 `Than`：

| 錯誤名稱（不存在） | 正確名稱 |
| --- | --- |
| `BeGreaterOrEqualTo(n)` | `BeGreaterThanOrEqualTo(n)` |
| `BeLessOrEqualTo(n)` | `BeLessThanOrEqualTo(n)` |
| `HaveCountGreaterOrEqualTo(n)` | `HaveCountGreaterThanOrEqualTo(n)` |

這類錯誤特別難以察覺 —— 名稱本身讀起來完全合理，語意也正確，只是那個版本的 API 從來沒有這樣命名過。它不會在審閱時被抓到，只會在使用者實際編譯時才爆出來。

### 查證方式

本次不是憑記憶或語感判斷，而是**以組件反射列舉 AwesomeAssertions 9.5.0 的 636 個公開成員名稱**建立比對基準，再抽出全部 Skills 中含 `.Should()` 的敘述逐一比對。同時反射確認了 AwesomeAssertions.Web 的 54 個狀態碼斷言（`Be200Ok`、`Be404NotFound` 等）**全部存在，並非誤用**。

### 受影響的 Skills

| Skill | 處數 | 檔案 |
| --- | --- | --- |
| `dotnet-testing-autodata-xunit-integration` | 5 | `references/collection-size-attribute.md`、`templates/advanced-patterns.cs`（2 處）、`templates/external-data-integration.cs`、`templates/autodata-attributes.cs` |
| `dotnet-testing-autofixture-customization` | 1 | `templates/dataannotations-integration.cs` |
| `dotnet-testing-bogus-fake-data` | 2 | `templates/basic-usage.cs`、`templates/advanced-patterns.cs` |

修正皆為單純的名稱替換，語意與斷言行為完全不變，**不影響任何既有的正確用法**。Skill 名稱、數量、目錄結構均未更動。

### 已知問題（尚未修正）

`dotnet-testing-awesome-assertions-guide` 與 `dotnet-testing-complex-object-comparison` 另有同類的幻覺 API（如 `WithMaxRecursionDepth`、`ExcludingNestedObjects`、`RespectingRuntimeTypes`、`HaveLengthGreaterThan`、`BePositiveInfinity`），本次**未一併修正** —— 這兩個 Skill 正在進行 v3 全面重寫，屆時會整份取代。若您正在使用這兩個 Skill，請留意下列對應：

| 錯誤 | 正確 |
| --- | --- |
| `WithMaxRecursionDepth(n)` | `AllowingInfiniteRecursion()` |
| `ExcludingNestedObjects` | `Excluding(x => x.Child)` |
| `RespectingRuntimeTypes` | `PreferringRuntimeMemberTypes()` |
| `HaveLengthGreaterThan(n)` | `text.Length.Should().BeGreaterThan(n)` |
| `BePositiveInfinity()` | `Should().Be(double.PositiveInfinity)` |

> 詳細變更請參閱：[v2.4.2 Release Notes](https://github.com/kevintsengtw/dotnet-testing-agent-skills/releases/tag/v2.4.2)

---

## 2026-03-31 NuGet 套件版本同步更新 (v2.4.1)

全面同步 14 個 NuGet 套件至最新穩定版，涵蓋 12 個 Skills、19 個檔案，並改善 Testcontainers Wait Strategy 與修正 FluentValidation 套件參考。

### NuGet 套件版本升級

| 套件 | 舊版本 | 新版本 | 影響 Skills |
|------|--------|--------|------------- |
| `Aspire.Hosting.Testing` | 9.1.0 | **13.1.3** | aspire-testing |
| `xunit.runner.visualstudio` | 2.8.2 / 3.0.0 / 3.0.1 | **3.1.5** | aspire-testing, aspnet-integration-testing, testcontainers-database, testcontainers-nosql, webapi-integration-testing, code-coverage-analysis, xunit-project-setup |
| `Microsoft.Data.SqlClient` | 6.1.4 | **7.0.0** | testcontainers-database |
| `MongoDB.Driver` | 3.7.0 | **3.7.1** | testcontainers-nosql |
| `MongoDB.Bson` | 3.7.0 | **3.7.1** | testcontainers-nosql |
| `StackExchange.Redis` | 2.11.8 | **2.12.8** | aspire-testing, testcontainers-nosql |
| `Microsoft.Bcl.TimeProvider` | 10.0.3 | **10.0.5** | testcontainers-nosql |
| `Microsoft.Extensions.TimeProvider.Testing` | 10.3.0 | **10.4.0** | testcontainers-nosql, tunit-fundamentals, webapi-integration-testing, fluentvalidation-testing |
| `TUnit` | 0.57.24 / 1.19.57 | **1.24.0** | tunit-advanced, tunit-fundamentals |
| `Microsoft.Testing.Extensions.TrxReport` | 2.0.2 | **2.1.0** | tunit-fundamentals |
| `Npgsql` | 10.0.1 | **10.0.2** | aspire-testing, webapi-integration-testing |
| `AutoFixture.Xunit3` | 4.18.1 | **4.19.0** | xunit-upgrade-guide |
| `AwesomeAssertions.Web` | 9.4.0 | **1.9.6** | webapi-integration-testing |
| `Microsoft.Extensions.Time.Testing` | *(已移除)* | → `Microsoft.Extensions.TimeProvider.Testing` | fluentvalidation-testing |

### Testcontainers Wait Strategy 改善

將 `UntilPortIsAvailable()` 替換為應用層健康檢查策略，避免「端口已開但服務尚未就緒」的競態條件：

| 容器 | 舊策略 | 新策略 |
|------|--------|--------|
| PostgreSQL | `UntilPortIsAvailable(5432)` | `UntilCommandIsCompleted("pg_isready")` |
| MongoDB | `UntilPortIsAvailable(27017)` | `UntilCommandIsCompleted("mongosh --eval 'db.runCommand({ ping: 1 })'")` |
| SQL Server | `UntilPortIsAvailable(1433)` + 日誌偵測 | 僅保留 `UntilMessageIsLogged(...)` |

### FluentValidation 套件參考修正

- 移除獨立的 `FluentValidation.TestHelper` 套件參考（TestHelper API 已內含於 `FluentValidation` 主套件）
- 修正套件名稱：`Microsoft.Extensions.Time.Testing` → `Microsoft.Extensions.TimeProvider.Testing`
- 新增注意事項說明，引導使用者正確使用 `using FluentValidation.TestHelper;`

> 詳細變更請參閱：[v2.4.1 Release Notes](https://github.com/kevintsengtw/dotnet-testing-agent-skills/releases/tag/v2.4.1)

---

## 2026-03-23 NuGet 套件版本升級 + Skill 結構重構 (v2.4.0)

使用 Anthropic 官方 skill-creator 重新調整 12 個 dotnet-testing 系列 SKILL.md，並全面升級測試相關 NuGet 套件版本：

### Skill 結構重構

| 優化項目 | 說明 | 影響範圍 |
|---------|------|---------|
| **SKILL.md 瘦身** | 大段程式碼範例拆至 `references/` 目錄，降低 Context Window 佔用 | 12 個檔案 |
| **templates 更新** | 各 skill 的 .csproj 範本同步更新套件版本 | 多個 templates |
| **references 新增** | 新增詳細參考文件，包含完整範例與進階用法 | 多個 references |

### NuGet 套件版本升級（v2.4.0）

| 套件 | 舊版本 | 新版本 |
|------|--------|--------|
| TUnit | 0.57.24 | 1.19.57 (1.x 正式版) |
| Testcontainers | — | 4.11.0 |
| xunit.v3 | 3.0.1 | 3.2.2 |
| xunit.runner.visualstudio | — | 3.1.5 |
| AwesomeAssertions | — | 9.4.0 |
| Microsoft.NET.Test.Sdk | — | 18.3.0 |
| coverlet.collector | — | 8.0.1 |
| FluentValidation | 11.11.0 | 12.1.1 |
| Respawn | 6.2.1 | 7.0.0 |
| Microsoft.Data.SqlClient | 5.2.2 | 6.1.4 |
| System.IO.Abstractions | 21.* | 22.1.0 |
| StackExchange.Redis | — | 最新版 |
| Dapper | — | 最新版 |
| MongoDB.Driver | — | 最新版 |

> 此次更新確保所有 skill 的範本與參考內容使用最新穩定版套件。

---

## 2026-03-11 Skill-Creator 驗證測試後微調 (v2.3.1)

透過 skill-creator 進行驗證測試後，針對三個 skills 的回應品質進行微調：

| Skill | 調整內容 |
|-------|--------|
| **dotnet-testing-complex-object-comparison** | 強化循環參照處理說明：改為「務必提及循環參照處理」，即使使用者未明確問到，也應主動說明 `IgnoringCyclicReferences()` 和 `WithMaxRecursionDepth(n)` 的用法 |
| **dotnet-testing-private-internal-testing** | 新增「回覆策略」區塊：要求回覆必須完整涵蓋三種路徑（重構建議、InternalsVisibleTo 方案、反射測試方案），不可只推薦其中一種而忽略其他 |
| **dotnet-testing-test-naming-conventions** | 命名範例表格新增英文命名對照：三段式命名同時展示中文與英文兩種風格，讓使用者依團隊慣例選擇 |

> 此次微調基於 skill-creator 驗證測試結果，確保 AI 回應更完整、更符合實務需求。

---

## 2026-03-07 Skill-Creator 規範優化 (v2.3.0)

使用新版 skill-creator 工具對全部 29 個 SKILL.md 進行檢視與優化調整：

| 優化項目 | 說明 | 影響範圍 |
|---------|------|---------|
| **移除 Emoji 裝飾** | 移除標題與內容中的裝飾性 emoji，採用乾淨專業的寫作風格 | ~20 個檔案 |
| **AI 指引段落自然化** | 總覽技能的 AI Agent 指引改為自然語氣，用理解替代命令 | 2 個檔案 |
| **DO/DON'T 風格調整** | 強硬的 DO/DON'T 段落改為「推薦做法」/「常見誤區」並補充理由 | 4 個檔案 |
| **補齊 related skills** | 缺少相關技能段落的檔案補上「相關技能」資訊 | 10 個檔案 |
| **流程範例精簡** | 移除冗餘的「錯誤流程」反面教材，僅保留正確流程範例 | 2 個檔案 |
| **H1 標題一致性** | 統一半形/全形冒號使用 | 3 個檔案 |

> 此次優化依據 Anthropic skill-creator 核心原則：「用解釋 WHY 取代強硬的 MUST」、「保持精簡」、「漸進式揭露」。

---

## 2026-02-11 Anthropic 規範優化

依據 Anthropic 官方「[The Complete Guide to Building Skills for Claude](https://claude.com/blog/complete-guide-to-building-skills-for-claude)」與「[Skill Authoring Best Practices](https://platform.claude.com/docs/agent-skills/skill-authoring-best-practices)」，對全部 29 個 SKILL.md 進行結構性優化：

| 優化項目 | 說明 | 影響範圍 |
|---------|------|---------|
| **漸進式揭露** | 所有 SKILL.md 精簡至 ≤500 行，詳細內容提取至 `references/` 目錄 | 21 個檔案 |
| **H1 標題中文化** | 統一為中文標題格式 | 3 個檔案 |
| **首段 H2 標準化** | 統一為 `## 適用情境` 作為首段 | 17 個檔案 |
| **觸發句描述** | 所有 description 加入「當需要…時使用」觸發句 | 14 個檔案 |
| **related_skills 元資料** | 所有子技能補上 `related_skills` 欄位 | 26 個檔案 |
| **末段 H2 標準化** | 統一為 `## 參考資源` 作為末段 | 9 個檔案 |
| **補建範本檔案** | 為缺少範本的技能新增 templates | 1 個檔案 |

**核心架構變更 — 漸進式揭露 (Progressive Disclosure)**：

```text
SKILL.md (≤500 行)          ← AI 主要讀取的指令檔
├── references/              ← 詳細參考內容（按需讀取）
│   ├── detailed-patterns.md
│   └── advanced-examples.md
├── templates/               ← 程式碼範本
└── scripts/                 ← 輔助腳本
```

> 此架構確保 AI 的 Context Window 不會被過長的指令檔佔用，同時保留完整的參考資源供需要時存取。

---

## 技能組合建議

### 新手入門

```mermaid
graph LR
    A[dotnet-testing-unit-test-fundamentals] --> B[dotnet-testing-test-naming-conventions]
    B --> C[dotnet-testing-xunit-project-setup]
    C --> D[dotnet-testing-awesome-assertions-guide]
    
    style A fill:#e3f2fd
    style B fill:#e8f5e9
    style C fill:#fff3e0
    style D fill:#f3e5f5
```

### 效率提升

```mermaid
graph LR
    A[dotnet-testing-autofixture-basics] --> B[dotnet-testing-autofixture-customization]
    B --> C[dotnet-testing-autofixture-nsubstitute-integration]
    C --> D[dotnet-testing-autodata-xunit-integration]
    
    style A fill:#e1f5ff
    style B fill:#e8f5e9
    style C fill:#fff9c4
    style D fill:#f1f8e9
```

### 整合測試

```mermaid
graph LR
    A[dotnet-testing-advanced-aspnet-integration-testing] --> B[dotnet-testing-advanced-testcontainers-database]
    B --> C[dotnet-testing-advanced-webapi-integration-testing]
    C --> D[dotnet-testing-advanced-aspire-testing]
    
    style A fill:#e8eaf6
    style B fill:#e0f2f1
    style C fill:#fff8e1
    style D fill:#fce4ec
```

---

## 貢獻

歡迎提交 Issues 和 Pull Requests！

如果您發現技能內容有誤或想要新增新技能，請：
1. Fork 本專案
2. 建立您的 feature branch
3. 提交 Pull Request

---

## 授權

MIT License - 自由使用與修改

---

## 致謝

感謝所有在 iThome 鐵人賽期間給予支持與回饋的讀者們！

---

**作者**：Kevin Tseng  
**最後更新**：2026-03-23
