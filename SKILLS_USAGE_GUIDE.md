# .NET Testing Agent Skills 使用手冊

本手冊詳細說明如何在各種 AI 助理平台上使用 .NET Testing Agent Skills，並提供完整的技能清單、使用情境與最佳實踐。

---

## 📋 目錄

> 💡 **快速開始**：想要快速查詢？請參考 [SKILLS_QUICK_REFERENCE.md](SKILLS_QUICK_REFERENCE.md) 快速參考指南

1. [簡介](#簡介)
2. [支援平台與環境需求](#支援平台與環境需求)
3. [安裝與設定](#安裝與設定)
4. [技能清單總覽](#技能清單總覽)
5. [使用方式](#使用方式)
6. [技能詳細說明](#技能詳細說明)
7. [使用情境範例](#使用情境範例)
8. [技能組合建議](#技能組合建議)
9. [常見問題 FAQ](#常見問題-faq)
10. [進階使用](#進階使用)

---

## 簡介

### 什麼是 .NET Testing Agent Skills？

這是一套專為 .NET 測試開發設計的 Agent Skills 集合，基於「老派軟體工程師的測試修練 - 30 天挑戰」(2025 iThome 鐵人賽 Software Development 組冠軍作品) 提煉而成。

**遵循 [agentskills.io](https://agentskills.io) 開放標準**，這些技能是**跨平台通用**的，可在 GitHub Copilot、Claude、Cursor 等多種 AI 工具中使用。

這些技能讓 AI 助理能夠：

- 🎯 **自動識別**：根據您的對話內容，自動載入相關的測試技能（透過 description 中的 Keywords 優化關鍵字）
- 📚 **提供專業指導**：給予符合業界最佳實踐的測試建議
- 🔧 **生成程式碼**：產出符合規範的測試程式碼與專案結構
- 🔄 **整合工作流程**：協助完成從單元測試到整合測試的完整流程
- 🌐 **跨平台支援**：skills 內容符合開放標準，可在任何支援 agentskills.io 的 AI 工具使用

### Skills 最新優化（2026-02-01）

本 Skills 集合已完成全面優化，**符合 [agentskills.io](https://agentskills.io/specification) 官方規範**：

- ✅ **29 個 Skills 全面優化**：description 包含核心說明 + Keywords 關鍵字
- ✅ **Description 多行格式**：清晰說明核心功能、適用場景、涵蓋範圍，並整合觸發關鍵字
- ✅ **入口 Skills 導航**：兩個總覽 skills 提供智能推薦
- ✅ **符合官方規範**：移除非標準 `triggers` 欄位，改用 `description` 中的 `Keywords:` 格式

**詳細資訊**：請參考 [OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md)

### 技能涵蓋範圍

| 類別                               | 技能數量  | 涵蓋主題                           |
| ---------------------------------- | --------- | ---------------------------------- |
| 總覽技能                           | 2 個      | 智能導航與技能推薦                 |
| 基礎技能 (dotnet-testing)          | 19 個     | 單元測試、斷言、模擬、測試資料生成 |
| 進階技能 (dotnet-testing-advanced) | 8 個      | 整合測試、容器化測試、框架遷移     |
| **總計**                           | **29 個** | 總覽引導 + Day 01-30 完整內容      |

---

## 支援平台與環境需求

### 支援的 AI 助理平台

> 💡 本 Skills 集合遵循 [agentskills.io](https://agentskills.io) 開放標準，可在多種 AI 平台使用

| 平台                           | 支援狀態    | 備註                                                                  |
| ------------------------------ | ----------- | --------------------------------------------------------------------- |
| **GitHub Copilot (VS Code)**   | ✅ 完整支援 | 複製到 `.github/skills/`（v1.109+ 也支援 `.claude/skills/` 等多路徑） |
| **GitHub Copilot CLI**         | ✅ 完整支援 | 同上                                                                  |
| **Claude Desktop**             | ✅ 完整支援 | 使用 `/plugin` 指令或複製到專案                                       |
| **Claude Code CLI**            | ✅ 完整支援 | 複製到 `.claude/skills/`                                              |
| **Cursor**                     | ✅ 完整支援 | 複製到 `.cursor/skills/`                                              |
| **其他支援 Agent Skills 工具** | ✅ 通用     | 符合 agentskills.io 標準，查閱該工具文件                              |

> 💡 **GitHub Copilot v1.109+ 更新**：Agent Skills 已正式 GA 並預設啟用，且支援彈性搜尋路徑。詳見 [GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md)。

### 環境需求

#### 基礎技能

- .NET 8.0 SDK 或更新版本
- 任意支援的 IDE (VS Code, Visual Studio, Rider)

#### 進階技能 (整合測試)

- Docker Desktop (用於 Testcontainers)
- WSL2 (Windows 環境)
- .NET Aspire Workload (用於 Aspire Testing)

---

## 安裝與設定

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

# 2. 複製到您的專案（GitHub Copilot 官方推薦路徑 .github/skills）
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# 完成！VS Code v1.109+ 已預設啟用 Agent Skills，無需額外設定
```

**Windows (PowerShell)**
```powershell
# 1. Clone 此 repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 2. 複製到您的專案（GitHub Copilot 官方推薦路徑 .github/skills）
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.github\" -Recurse

# 完成！VS Code v1.109+ 已預設啟用 Agent Skills，無需額外設定
```

> 💡 **v1.109+ 多工具共用**：如果你同時使用 GitHub Copilot 和 Claude Code，只需複製到 `.claude/skills/` 一個位置即可，兩個工具都能存取。詳見 [GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md)。

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

### VS Code 設定

**VS Code v1.109+（2026-02-04 之後）**：Agent Skills 已正式 GA 並**預設啟用**，不需要手動設定。

**VS Code v1.109 之前**：需手動啟用 Agent Skills 支援：

1. 開啟設定 (`Ctrl+,` 或 `Cmd+,`)
2. 搜尋 `chat.useAgentSkills`
3. 確認已勾選啟用

> 💡 更多 v1.109 更新資訊（彈性搜尋路徑、多工具共用、診斷工具等），請參考 [GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md)。

---

## 技能清單總覽

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

---

### 基礎技能 (dotnet-testing) - 19 個

#### 第一階段：測試基礎與斷言

| 技能名稱                                   | 說明                                    | 來源       |
| ------------------------------------------ | --------------------------------------- | ---------- |
| `dotnet-testing-unit-test-fundamentals`    | 單元測試基礎、FIRST 原則、3A Pattern    | Day 01     |
| `dotnet-testing-test-naming-conventions`   | 三段式測試命名規範                      | Day 01     |
| `dotnet-testing-xunit-project-setup`       | xUnit 測試專案設定與結構                | Day 02, 03 |
| `dotnet-testing-awesome-assertions-guide`  | AwesomeAssertions 流暢斷言指南          | Day 04, 05 |
| `dotnet-testing-complex-object-comparison` | 複雜物件深層比對技巧                    | Day 05     |
| `dotnet-testing-code-coverage-analysis`    | 程式碼覆蓋率分析與報告                  | Day 06     |
| `dotnet-testing-nsubstitute-mocking`       | NSubstitute 測試替身 (Mock/Stub/Spy)    | Day 07     |
| `dotnet-testing-test-output-logging`       | xUnit ITestOutputHelper 與 ILogger 整合 | Day 08     |
| `dotnet-testing-private-internal-testing`  | Private/Internal 成員測試策略           | Day 09     |
| `dotnet-testing-fluentvalidation-testing`  | FluentValidation 驗證器測試             | Day 18     |

#### 第二階段：可測試性抽象化

| 技能名稱                                         | 說明                                | 來源   |
| ------------------------------------------------ | ----------------------------------- | ------ |
| `dotnet-testing-datetime-testing-timeprovider`   | TimeProvider 時間抽象化測試         | Day 16 |
| `dotnet-testing-filesystem-testing-abstractions` | System.IO.Abstractions 檔案系統測試 | Day 17 |

#### 第三階段：測試資料生成與整合

| 技能名稱                                             | 說明                               | 來源   |
| ---------------------------------------------------- | ---------------------------------- | ------ |
| `dotnet-testing-test-data-builder-pattern`           | 手動 Builder Pattern 測試資料建構  | Day 03 |
| `dotnet-testing-autofixture-basics`                  | AutoFixture 基礎與匿名測試資料     | Day 10 |
| `dotnet-testing-autofixture-customization`           | AutoFixture 自訂化策略             | Day 11 |
| `dotnet-testing-autodata-xunit-integration`          | AutoData 與 xUnit 整合             | Day 12 |
| `dotnet-testing-autofixture-nsubstitute-integration` | AutoFixture + NSubstitute 自動模擬 | Day 13 |
| `dotnet-testing-bogus-fake-data`                     | Bogus 擬真測試資料產生             | Day 14 |
| `dotnet-testing-autofixture-bogus-integration`       | AutoFixture 與 Bogus 整合應用      | Day 15 |

### 進階技能 (dotnet-testing-advanced)

#### 第四階段：整合測試

| 技能名稱                                             | 說明                                        | 來源       |
| ---------------------------------------------------- | ------------------------------------------- | ---------- |
| `dotnet-testing-advanced-aspnet-integration-testing` | ASP.NET Core WebApplicationFactory 整合測試 | Day 19     |
| `dotnet-testing-advanced-testcontainers-database`    | Testcontainers 資料庫容器化測試             | Day 20, 21 |
| `dotnet-testing-advanced-testcontainers-nosql`       | Testcontainers MongoDB/Redis 測試           | Day 22     |
| `dotnet-testing-advanced-webapi-integration-testing` | WebAPI 完整整合測試流程                     | Day 23     |
| `dotnet-testing-advanced-aspire-testing`             | .NET Aspire Testing 框架                    | Day 24, 25 |

#### 第五階段：框架遷移指南

| 技能名稱                                      | 說明                         | 來源       |
| --------------------------------------------- | ---------------------------- | ---------- |
| `dotnet-testing-advanced-xunit-upgrade-guide` | xUnit 2.9.x 到 3.x 升級指南  | Day 26     |
| `dotnet-testing-advanced-tunit-fundamentals`  | TUnit 新世代測試框架入門     | Day 28     |
| `dotnet-testing-advanced-tunit-advanced`      | TUnit 資料驅動與整合測試進階 | Day 29, 30 |

---

## 使用方式

### 自動觸發模式

技能會根據您的對話內容自動載入。您只需要自然地提問：

```text
👤：幫我建立一個 xUnit 測試專案

🤖：[自動載入 dotnet-testing-xunit-project-setup 技能]
   我將協助您建立標準的 xUnit 測試專案結構...
```

### 常見觸發語句

#### 總覽技能觸發

| 您說的話                 | 觸發的技能                    |
| ------------------------ | ----------------------------- |
| "我想學習 .NET 測試"     | `dotnet-testing`              |
| "如何寫 .NET 測試"       | `dotnet-testing`              |
| "測試入門"               | `dotnet-testing`              |
| "建立整合測試"           | `dotnet-testing-advanced`     |
| "API 測試怎麼做"         | `dotnet-testing-advanced`     |
| "微服務測試"             | `dotnet-testing-advanced`     |
| "使用 Testcontainers"    | `dotnet-testing-advanced`     |

#### 專業技能觸發

| 您說的話                 | 觸發的技能                                                                        |
| ------------------------ | --------------------------------------------------------------------------------- |
| "建立測試專案"           | `dotnet-testing-xunit-project-setup`                                              |
| "為這個方法寫單元測試"   | `dotnet-testing-unit-test-fundamentals`, `dotnet-testing-test-naming-conventions` |
| "產生測試資料"           | `dotnet-testing-autofixture-basics` 或 `dotnet-testing-bogus-fake-data`           |
| "這個類別有 Mock 需求"   | `dotnet-testing-nsubstitute-mocking`                                              |
| "檢查程式碼覆蓋率"       | `dotnet-testing-code-coverage-analysis`                                           |
| "使用 Docker 測試資料庫" | `dotnet-testing-advanced-testcontainers-database`                                 |

### 明確指定技能

您也可以明確要求使用特定技能：

```text
👤：使用 dotnet-testing-test-naming-conventions 技能來檢查我的測試命名

👤：參考 dotnet-testing-autofixture-nsubstitute-integration 技能來重構這個測試
```

---

## 技能詳細說明

### dotnet-testing-unit-test-fundamentals

**用途**：建立符合最佳實踐的單元測試

**核心內容**：

- FIRST 原則 (Fast, Isolated, Repeatable, Self-validating, Timely)
- 3A Pattern (Arrange, Act, Assert)
- 測試金字塔概念
- 測試覆蓋的正確心態

**觸發情境**：

- 詢問單元測試基礎概念
- 請求為方法撰寫測試
- 詢問測試最佳實踐

---

### dotnet-testing-test-naming-conventions

**用途**：建立清晰且一致的測試命名

**核心內容**：

- 三段式命名法：`方法名_情境_預期結果`
- 中文命名建議
- 避免模糊的命名

**範例**：

```csharp
// ✅ 好的命名
public void Add_輸入正數_應回傳正確加總()

// ❌ 避免的命名
public void TestAdd()
```

---

### dotnet-testing-xunit-project-setup

**用途**：建立標準化的 xUnit 測試專案

**核心內容**：

- 專案結構建議
- .csproj 設定範本
- 必要 NuGet 套件
- xunit.runner.json 配置

**產出**：

- 完整的測試專案結構
- 正確的套件參考
- 測試執行配置

---

### dotnet-testing-nsubstitute-mocking

**用途**：使用 NSubstitute 建立測試替身

**核心內容**：

- Mock vs Stub vs Spy 概念
- NSubstitute 基本語法
- 回傳值設定
- 行為驗證
- 引數匹配器

**範例**：

```csharp
var repository = Substitute.For<IUserRepository>();
repository.GetById(Arg.Any<int>()).Returns(new User { Name = "Test" });
repository.Received(1).GetById(1);
```

---

### dotnet-testing-autofixture-basics

**用途**：使用 AutoFixture 自動產生測試資料

**核心內容**：

- Fixture 基本使用
- Create 與 CreateMany
- 匿名變數概念
- 循環參考處理

**適用情境**：

- 物件有多個屬性需要填充
- 測試不關心資料的具體值
- 需要大量測試資料

---

### dotnet-testing-advanced-testcontainers-database

**用途**：使用 Testcontainers 進行容器化資料庫測試

**核心內容**：

- PostgreSQL/MSSQL 容器設定
- EF Core 整合
- Dapper 整合
- 測試資料隔離策略
- Collection Fixture 容器共享

**前置需求**：

- Docker Desktop
- WSL2 (Windows)

---

## 使用情境範例

### 情境 1：建立新的測試專案

**您的需求**：從零開始建立一個 xUnit 測試專案

**對話流程**：

```text
👤：我想為 MyProject 建立測試專案

🤖：我將協助您建立測試專案。根據 dotnet-testing-xunit-project-setup 技能的指導...

    1. 建立專案結構
    2. 配置 csproj 檔案
    3. 安裝必要套件
    4. 設定測試執行環境
```

### 情境 2：為現有方法寫測試

**您的需求**：為 Calculator.Add 方法寫測試

**對話流程**：

```text
👤：幫我為這個方法寫單元測試
    public int Add(int a, int b) => a + b;

🤖：根據 dotnet-testing-unit-test-fundamentals 與 dotnet-testing-test-naming-conventions 技能，
    我將建立符合 3A Pattern 的測試...

    [Fact]
    public void Add_輸入正數_應回傳正確加總()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(2, 3);

        // Assert
        result.Should().Be(5);
    }
```

### 情境 3：重構測試以使用 AutoFixture

**您的需求**：簡化測試資料準備

**對話流程**：

```text
👤：這個測試有太多手動設定，可以簡化嗎？

🤖：根據 dotnet-testing-autofixture-basics 技能，我們可以使用 AutoFixture 
    自動產生不重要的測試資料...

    [Theory, AutoData]
    public void CreateUser_有效資料_應成功建立(User user)
    {
        // user 由 AutoFixture 自動產生
        var result = _service.Create(user);
        result.Should().NotBeNull();
    }
```

### 情境 4：建立整合測試環境

**您的需求**：測試需要真實的 PostgreSQL 資料庫

**對話流程**：

```text
👤：我想用真實資料庫來測試 Repository

🤖：根據 dotnet-testing-advanced-testcontainers-database 技能，我將協助您設定 
    Testcontainers 環境...

    1. 安裝 Testcontainers.PostgreSql 套件
    2. 建立 DatabaseFixture 類別
    3. 配置 Collection Fixture
    4. 實作測試基底類別
```

---

## 技能組合建議

> 💡 **提示**：不確定該用哪些技能？可以直接詢問總覽技能（`dotnet-testing` 或 `dotnet-testing-advanced`），它們會根據您的需求自動推薦最適合的技能組合！

### 🆕 新手入門組合

適合剛開始學習 .NET 測試的開發者：

```text
1. dotnet-testing-unit-test-fundamentals    → 理解測試基礎
2. dotnet-testing-test-naming-conventions   → 建立命名習慣
3. dotnet-testing-xunit-project-setup       → 設定測試環境
4. dotnet-testing-awesome-assertions-guide  → 學習流暢斷言
```

### 🎯 品質保證組合

適合需要提升測試品質的團隊：

```text
1. dotnet-testing-code-coverage-analysis       → 監控覆蓋率
2. dotnet-testing-complex-object-comparison    → 精準斷言
3. dotnet-testing-test-output-logging          → 除錯支援
4. dotnet-testing-fluentvalidation-testing     → 驗證邏輯測試
```

### 🚀 效率提升組合

適合想要加速測試開發的開發者：

```text
1. dotnet-testing-autofixture-basics                    → 自動化測試資料
2. dotnet-testing-autofixture-customization             → 自訂化策略
3. dotnet-testing-autofixture-nsubstitute-integration   → 自動 Mock
4. dotnet-testing-autodata-xunit-integration            → Theory 整合
```

### 🔗 整合測試組合

適合需要建立整合測試的專案：

```text
1. dotnet-testing-advanced-aspnet-integration-testing    → API 測試基礎
2. dotnet-testing-advanced-testcontainers-database       → 資料庫容器化
3. dotnet-testing-advanced-testcontainers-nosql          → NoSQL 測試
4. dotnet-testing-advanced-webapi-integration-testing    → 完整流程
```

### 🔄 框架遷移組合

適合計畫升級或遷移測試框架的團隊：

```text
1. dotnet-testing-advanced-xunit-upgrade-guide   → xUnit 3.x 升級
2. dotnet-testing-advanced-tunit-fundamentals    → TUnit 入門
3. dotnet-testing-advanced-tunit-advanced        → TUnit 進階
```

---

## 常見問題 FAQ

### Q1：技能沒有自動觸發怎麼辦？

**A**：確認以下事項：

1. 技能目錄結構正確（GitHub Copilot 使用 `.github/skills/`，Claude Code 使用 `.claude/skills/`）
2. VS Code v1.109 之前的版本需確認已啟用 `chat.useAgentSkills` 設定（v1.109+ 已預設啟用）
3. 每個技能資料夾都有 `SKILL.md` 檔案
4. 嘗試更明確地描述您的需求，或直接詢問「我想學習 .NET 測試」來觸發總覽技能
5. 使用 VS Code v1.109+ 的 Chat Customization Diagnostics 診斷功能確認 skills 載入狀態（在 Chat 面板中右鍵點擊）

---

### Q2：可以同時使用多個技能嗎？

**A**：是的，AI 會根據對話內容自動組合相關技能。例如，當您詢問「為這個服務寫單元測試」時，可能同時觸發：

- `dotnet-testing-unit-test-fundamentals` - 測試結構
- `dotnet-testing-test-naming-conventions` - 命名規範
- `dotnet-testing-nsubstitute-mocking` - 依賴模擬

---

### Q3：技能內容會佔用太多 Token 嗎？

**A**：不會。Agent Skills 使用「漸進式載入」機制：

1. 平時只讀取技能名稱和描述 (約 30-50 tokens)
2. 需要時才載入完整內容
3. 不使用的技能不會消耗 Token

---

### Q4：可以修改技能內容嗎？

**A**：當然可以！技能就是 Markdown 檔案，您可以：

- 根據團隊規範調整建議
- 新增專案特定的範例
- 移除不適用的部分
- 新增自訂技能

---

### Q5：這些技能適用於 NUnit 或 MSTest 嗎？

**A**：這些技能主要針對 xUnit 設計，但許多概念是通用的：

- 測試命名規範
- 3A Pattern
- 測試資料生成策略
- 模擬技巧

如需 NUnit/MSTest 支援，可以修改技能內容或建立新技能。

---

### Q6：如何更新到最新版本的技能？

**A**：根據您的安裝方式：

**npx skills install**：重新執行安裝指令

```bash
# 更新全域 skills
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 更新工作區 skills
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git --workspace
```

**直接複製**：重新下載並複製

```bash
# 對 GitHub Copilot
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# 對 Claude Code
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/
```

**Git Submodule**：

```bash
cd .github/skills  # 或 .claude/skills
git pull origin main
```

---

## 進階使用

### 建立自訂技能

您可以根據團隊需求建立專屬技能：

```markdown
---
name: my-team-testing-standards
description: 我們團隊的測試規範。當被要求寫測試時使用此技能。
---

# 團隊測試規範

## 命名規則
- 使用中文命名
- 必須使用三段式

## 必要斷言
- 使用 AwesomeAssertions
- 禁止使用 Assert.Equal 直接比較物件

## 模擬規範
- 使用 NSubstitute
- 禁止使用 Moq
```

### 與 MCP 工具整合

技能可以教導 AI 如何使用 MCP 工具：

```markdown
## 使用 Microsoft Learn MCP

當需要查詢官方文件時：
1. 使用 microsoft_docs_search 搜尋相關主題
2. 使用 microsoft_docs_fetch 取得完整內容
3. 使用 microsoft_code_sample_search 尋找程式碼範例
```

### 技能組合檔案

建立一個「組合技能」來整合多個相關技能：

```markdown
---
name: complete-testing-workflow
description: 完整的測試工作流程，從單元測試到整合測試
---

# 完整測試工作流程

本技能整合以下技能的精華：
- dotnet-testing-unit-test-fundamentals
- dotnet-testing-autofixture-basics
- dotnet-testing-nsubstitute-mocking
- dotnet-testing-advanced-testcontainers-database

## 工作流程步驟
1. 確認測試類型(單元/整合)
2. 設定測試資料
3. 建立必要的模擬
4. 撰寫測試
5. 驗證覆蓋率
```

---

## 相關資源

### 專案文件

- **[SKILLS_QUICK_REFERENCE.md](SKILLS_QUICK_REFERENCE.md)**：快速參考指南（推薦！）
  - 關鍵字快速對應表
  - Prompt 模板與使用範例
  - 常見情境快速指引
  - Skills 優化狀態說明

- **[OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md)**：Skills 優化總結報告
  - 詳細的優化策略與成果
  - Keywords 整合說明
  - 預期效果分析

### 原始內容

- **iThome 鐵人賽文章**：[老派軟體工程師的測試修練 - 30 天挑戰](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
- **完整範例程式碼**：[30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)
- **公開 Repository**：[dotnet-testing-agent-skills](https://github.com/kevintsengtw/dotnet-testing-agent-skills)

### Agent Skills 標準

- **官方網站**：[agentskills.io](https://agentskills.io)
- **GitHub 文件**：[About Agent Skills](https://docs.github.com/copilot/using-github-copilot/using-github-copilot-agent-skills)
- **Anthropic Skills**：[anthropics/skills](https://github.com/anthropics/skills)

### 技能目錄結構

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
> - 安裝後，skills 會根據目標環境複製到對應位置（`.github/skills/` 或 `.claude/skills/`）

---

## 授權

MIT License

---

**最後更新**：2026-02-09
