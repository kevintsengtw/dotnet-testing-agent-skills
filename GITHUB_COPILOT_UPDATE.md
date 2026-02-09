# GitHub Copilot Agent Skills 更新紀錄

本文件記錄 VS Code / GitHub Copilot 的重要更新，以及這些更新對使用 .NET Testing Agent Skills 的影響與注意事項。

**目標讀者**：使用 GitHub Copilot 搭配本專案 dotnet-testing Agent Skills 的開發者

---

## 📋 更新紀錄

| 日期       | VS Code 版本 | 重點變更                                                                     |
| ---------- | ------------ | ---------------------------------------------------------------------------- |
| 2026-02-04 | v1.109       | [Agent Skills GA、彈性搜尋路徑、多工具共用工作區](#vs-code-v1109-2026-02-04) |

---

## VS Code v1.109 (2026-02-04)

**參考來源**：[VS Code v1.109 Release Notes - Agent Customization](https://code.visualstudio.com/updates/v1_109#_agent-customization)

---

### 🎯 重點摘要 (TL;DR)

- ✅ **Agent Skills 正式 GA**：預設啟用，不再需要手動開啟 `chat.useAgentSkills` 設定
- ✅ **已在使用的 Copilot 使用者不需異動**：skills 繼續放在 `.github/skills/` 即可，完全不受影響
- ✅ **彈性搜尋路徑**：VS Code 現在會同時搜尋 `.github/skills/`、`.claude/skills/` 及其他路徑
- ✅ **多工具共用**：同時使用多個 AI 工具時，skills 只需放在一個位置，不再需要重複複製或建立 symlink
- ✅ **診斷工具**：新增 Chat Customization Diagnostics，方便排查 skills 載入問題

> 💡 **對純 GitHub Copilot 使用者**：如果你只使用 GitHub Copilot，skills 繼續放在 `.github/skills/`（官方推薦路徑）即可，**不需要做任何變更**。這次更新主要簡化的是同時使用多個 AI 工具的情境。

---

### 📋 更新前 vs 更新後對照表

| 項目                  | 更新前 (< v1.109)                          | 更新後 (v1.109+)                                                               |
| --------------------- | ------------------------------------------ | ------------------------------------------------------------------------------ |
| Agent Skills 啟用方式 | 需手動啟用 `chat.useAgentSkills`           | 預設啟用 (GA)                                                                  |
| Skill 搜尋路徑        | 僅搜尋 `.github/skills/`                   | 自動搜尋 `.github/skills/`、`.claude/skills/`、`~/.copilot/skills/` 等多個路徑 |
| 多 AI 工具共用        | 需為每個工具分別複製 skills 或建立 symlink | 單一目錄即可供多個工具使用                                                     |
| 自訂搜尋路徑          | 不支援                                     | 透過 `chat.agentSkillsLocations` 設定自訂路徑                                  |
| 偵錯工具              | 無                                         | Chat Customization Diagnostics 診斷視圖                                        |
| Extension 分發        | 不支援                                     | 透過 `chatSkills` contribution point 打包分發                                  |
| 組織層級指令          | 不支援                                     | `github.copilot.chat.organizationInstructions.enabled`                         |

---

### 🚀 Agent Skills 正式發佈 (GA)

VS Code v1.109 將 Agent Skills 從實驗性功能正式升級為 **Generally Available (GA)**，並且**預設啟用**。

這代表：

- **不再需要**到 VS Code 設定中搜尋 `chat.useAgentSkills` 並手動勾選啟用
- 更新到 v1.109 後，Agent Skills 功能會自動運作
- 之前已啟用 `chat.useAgentSkills` 的使用者不受影響，設定仍然存在但已無需手動管理

> ⚠️ **注意**：[SKILLS_USAGE_GUIDE.md](SKILLS_USAGE_GUIDE.md) 中「VS Code 設定」章節提到的三步驟啟用流程（開啟設定 → 搜尋 `chat.useAgentSkills` → 勾選啟用），在 v1.109+ 版本中已不再需要執行。

---

### 🔍 彈性 Skill 搜尋路徑

v1.109 之前，GitHub Copilot 僅會在 `.github/skills/` 目錄中搜尋 skills。更新後，VS Code 會自動在以下多個路徑搜尋：

| 搜尋路徑                               | 範圍      | 說明                                                |
| -------------------------------------- | --------- | --------------------------------------------------- |
| **`.github/skills/`**                  | Workspace | **GitHub Copilot 官方推薦路徑**（維持不變）         |
| `.claude/skills/`                      | Workspace | Claude Code 預設路徑，現在 Copilot 也會自動搜尋     |
| `~/.copilot/skills/`                   | User Home | 使用者全域 Copilot skills                           |
| `~/.claude/skills/`                    | User Home | 使用者全域 Claude skills，Copilot 也會搜尋          |
| `chat.agentSkillsLocations` 設定的路徑 | 自訂      | 透過 VS Code 設定指定的自訂路徑                     |

> ✅ **對純 Copilot 使用者**：`.github/skills/` 仍然是 GitHub Copilot 的官方推薦路徑。如果你只使用 GitHub Copilot，skills 繼續放在 `.github/skills/` 即可，**完全不需要異動**。

新增的搜尋路徑主要惠及同時使用多個 AI 工具的使用者——例如已經將 skills 安裝在 `.claude/skills/` 的 Claude Code 使用者，現在 GitHub Copilot 也能自動找到並載入這些 skills，無需額外複製。

#### 純 GitHub Copilot 使用者的目錄結構（推薦，與過去相同）

```plaintext
your-project/
├── .github/
│   └── skills/                          ← GitHub Copilot 官方推薦路徑
│       ├── dotnet-testing/
│       ├── dotnet-testing-advanced/
│       ├── dotnet-testing-unit-test-fundamentals/
│       ├── dotnet-testing-autofixture-basics/
│       └── ... (共 29 個 skills)
├── src/
│   └── MyProject/
└── tests/
    └── MyProject.Tests/
```

---

### 🌐 多 AI 工具共用工作區

> 💡 **本章節適用於同時使用多個 AI 工具（如 GitHub Copilot + Claude Code）的使用者**。如果你只使用 GitHub Copilot，skills 繼續放在 `.github/skills/` 即可，可跳過本節。

這是 v1.109 最具影響力的變化之一。過去在同一個 workspace 中使用多個 AI 工具時，需要為每個工具準備各自的 skills 目錄：

#### 過去的做法（v1.109 之前）

```plaintext
your-project/
├── .github/
│   └── skills/              ← GitHub Copilot 使用
│       ├── dotnet-testing/
│       └── ... (29 個 skills)
├── .claude/
│   └── skills/              ← Claude Code 使用
│       ├── dotnet-testing/
│       └── ... (29 個 skills，重複複製)
└── .cursor/
    └── skills/              ← Cursor 使用
        ├── dotnet-testing/
        └── ... (29 個 skills，再次重複複製)
```

這種做法的問題：

- ❌ 需要維護多份相同的 skills 副本
- ❌ 更新時需要同步修改多個目錄
- ❌ 或者需要建立 symlink 來避免重複，增加維護複雜度

#### 現在的做法（v1.109+）

由於 GitHub Copilot 現在會自動搜尋 `.claude/skills/` 等多個路徑，同時使用多個 AI 工具時只需要將 skills 放在**一個位置**：

```plaintext
your-project/
├── .claude/
│   └── skills/              ← 一份 skills 同時供 Copilot 和 Claude Code 使用
│       ├── dotnet-testing/
│       └── ... (29 個 skills)
├── src/
└── tests/
```

> ✅ GitHub Copilot v1.109+ → 自動搜尋 `.claude/skills/` ✅
> ✅ Claude Code → 原本就支援 `.claude/skills/` ✅
> ✅ 不需要 symlink，不需要重複複製 ✅

---

#### 依使用情境選擇 Skills 目錄位置

##### 情境 A：只使用 GitHub Copilot（推薦維持現狀）

使用 GitHub Copilot 官方推薦路徑 `.github/skills/`，**與過去完全相同，不需要任何變更**。

**Linux / macOS (Bash)**

```bash
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 複製到 GitHub Copilot 官方推薦路徑
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# 完成！
```

**Windows (PowerShell)**

```powershell
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 複製到 GitHub Copilot 官方推薦路徑
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.github\" -Recurse

# 完成！
```

##### 情境 B：同時使用 GitHub Copilot + Claude Code

將 skills 放在 `.claude/skills/`，兩個工具都能直接存取：

**Linux / macOS (Bash)**

```bash
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 只需複製到一個位置，兩個工具都能使用
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/

# 完成！GitHub Copilot v1.109+ 和 Claude Code 都能使用
```

**Windows (PowerShell)**

```powershell
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 只需複製到一個位置，兩個工具都能使用
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.claude\" -Recurse

# 完成！GitHub Copilot v1.109+ 和 Claude Code 都能使用
```

##### 情境 C：使用自訂共用目錄（適用於多工具或團隊場景）

透過 `chat.agentSkillsLocations` 設定指定一個與 AI 工具無關的共用目錄名稱：

```bash
# 建立共用 skills 目錄
cp -r dotnet-testing-agent-skills/skills /your-project/shared-skills/
```

在 `.vscode/settings.json` 中設定：

```json
{
  "chat.agentSkillsLocations": [
    "./shared-skills"
  ]
}
```

##### 情境 D：使用 User Home 目錄（全域安裝，跨所有專案）

將 skills 安裝到使用者的 home 目錄，所有專案都能使用：

**Linux / macOS (Bash)**

```bash
# 安裝到全域 Copilot skills 路徑
cp -r dotnet-testing-agent-skills/skills/* ~/.copilot/skills/

# 或安裝到全域 Claude skills 路徑（Copilot v1.109+ 也會搜尋此處）
cp -r dotnet-testing-agent-skills/skills/* ~/.claude/skills/
```

**Windows (PowerShell)**

```powershell
# 安裝到全域 Copilot skills 路徑
Copy-Item -Path "dotnet-testing-agent-skills\skills\*" -Destination "$HOME\.copilot\skills\" -Recurse

# 或安裝到全域 Claude skills 路徑（Copilot v1.109+ 也會搜尋此處）
Copy-Item -Path "dotnet-testing-agent-skills\skills\*" -Destination "$HOME\.claude\skills\" -Recurse
```

---

#### AI 工具 × 目錄對應表

以下表格整理各 AI 工具預設讀取的 skills 目錄：

| 目錄位置                             | GitHub Copilot (< v1.109) | GitHub Copilot (v1.109+) | Claude Code | Cursor |
| ------------------------------------ | ------------------------- | ------------------------ | ----------- | ------ |
| `.github/skills/`                    | ✅                        | ✅                       | ❌          | ❌     |
| `.claude/skills/`                    | ❌                        | ✅                       | ✅          | ❌     |
| `.cursor/skills/`                    | ❌                        | ❌                       | ❌          | ✅     |
| `~/.copilot/skills/`                 | ❌                        | ✅                       | ❌          | ❌     |
| `~/.claude/skills/`                  | ❌                        | ✅                       | ✅          | ❌     |
| `chat.agentSkillsLocations` 自訂路徑 | ❌                        | ✅                       | ❌          | ❌     |

> 💡 **建議**：純 GitHub Copilot 使用者繼續使用 `.github/skills/`（官方推薦）即可。如果同時使用 GitHub Copilot + Claude Code，將 skills 放在 `.claude/skills/` 是最簡單的共用方式。如果還需要支援 Cursor，可以使用 `chat.agentSkillsLocations` 加入 `.cursor/skills/`，或者只為 Cursor 額外複製一份。

---

### ⚙️ chat.agentSkillsLocations 自訂路徑設定

`chat.agentSkillsLocations` 是 v1.109 新增的 VS Code 設定，允許你指定額外的 skills 搜尋路徑。

#### 基本使用

在 `.vscode/settings.json` 中加入設定：

```json
{
  "chat.agentSkillsLocations": [
    "./shared-skills",
    "./vendor/dotnet-testing-agent-skills/skills"
  ]
}
```

#### 搭配 Git Submodule

如果使用 Git Submodule 方式安裝 skills，可以直接指向 submodule 路徑：

```bash
# 加入 submodule
git submodule add https://github.com/kevintsengtw/dotnet-testing-agent-skills vendor/dotnet-testing-agent-skills
```

```json
{
  "chat.agentSkillsLocations": [
    "./vendor/dotnet-testing-agent-skills/skills"
  ]
}
```

#### 團隊共享

將 `.vscode/settings.json` 提交到版本控制中，團隊成員 clone 後即可自動套用 skills 搜尋路徑設定，無需個別設定。

---

### 🔧 Chat Customization Diagnostics 診斷工具

v1.109 新增了 Chat Customization Diagnostics 診斷視圖，方便確認 skills 是否正確載入。

#### 使用方式

1. 開啟 GitHub Copilot Chat 面板
2. 在 Chat 面板中**右鍵點擊**
3. 選擇 **Chat Customization Diagnostics**
4. 檢視已載入的 skills 清單、檔案路徑與錯誤訊息

#### 診斷內容

- 列出所有已載入的 customization files
- 顯示每個 skill 的載入狀態（成功 / 失敗）
- 如有錯誤，顯示具體的錯誤訊息

> 💡 **除錯技巧**：如果你的 .NET Testing Skills 沒有自動觸發，使用 Chat Customization Diagnostics 來確認 VS Code 是否正確載入了 skills。常見問題包括：目錄路徑錯誤、`SKILL.md` 檔案格式問題、frontmatter 語法錯誤等。

---

### 📦 其他 v1.109 相關更新

#### Extension 分發 Skills

v1.109 新增了 `chatSkills` contribution point，VS Code Extension 可以打包並分發 skills。這意味著未來 .NET Testing Agent Skills 有可能以 **VS Code Extension** 的形式發布，讓使用者一鍵安裝。

#### 組織層級指令

透過 `github.copilot.chat.organizationInstructions.enabled` 設定，組織可以統一配置 Copilot 指令，確保團隊成員使用一致的 AI 行為規範。如果你的團隊要統一導入 .NET Testing Skills，可以搭配此功能在組織層級推廣。

#### `/init` 指令

新增的 `/init` slash command 會自動分析專案結構並產生 workspace instructions。這個功能與 Agent Skills **互補**：

- `/init` 產生的是專案層級的通用指令（coding style、project conventions）
- Agent Skills 提供的是領域專業知識（.NET 測試最佳實踐、框架使用指南）
- 兩者可以同時使用，不會衝突

---

### 📝 安裝方式更新建議

更新到 VS Code v1.109 後，安裝 .NET Testing Agent Skills 的步驟更為簡化：

#### 純 GitHub Copilot 使用者（與過去相同）

**Linux / macOS (Bash)**

```bash
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: 複製到 GitHub Copilot 官方推薦路徑
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# 完成！v1.109 後不需要再手動啟用 chat.useAgentSkills
```

**Windows (PowerShell)**

```powershell
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: 複製到 GitHub Copilot 官方推薦路徑
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.github\" -Recurse

# 完成！v1.109 後不需要再手動啟用 chat.useAgentSkills
```

#### 同時使用 GitHub Copilot + Claude Code（新增簡化流程）

**Linux / macOS (Bash)**

```bash
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: 複製到一個位置即可（兩個工具共用）
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/

# 完成！GitHub Copilot v1.109+ 和 Claude Code 都能使用
```

**Windows (PowerShell)**

```powershell
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: 複製到一個位置即可（兩個工具共用）
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.claude\" -Recurse

# 完成！GitHub Copilot v1.109+ 和 Claude Code 都能使用
```

#### 與過去安裝流程的差異

| 步驟                 | 過去 (< v1.109)                      | 現在 (v1.109+)                                                |
| -------------------- | ------------------------------------ | ------------------------------------------------------------- |
| 1. Clone repo        | 相同                                 | 相同                                                          |
| 2. 複製 skills       | 需分別複製到各工具的目錄             | 純 Copilot: `.github/skills/`（不變）；多工具: 一個位置即可   |
| 3. 啟用 Agent Skills | 手動到設定啟用 `chat.useAgentSkills` | **不需要**，預設已啟用                                        |
| 4. 驗證載入          | 無工具可用                           | 使用 Chat Customization Diagnostics                           |

---

### ❓ 常見問題 FAQ

#### Q1：我已經將 skills 安裝在 `.github/skills/`，需要移動嗎？

**A**：**不需要**。`.github/skills/` 是 GitHub Copilot 的官方推薦路徑，v1.109 更新後仍然完全支援。如果你只使用 GitHub Copilot，維持現狀即可，不需要做任何異動。只有當你同時使用 Claude Code 等其他 AI 工具，且希望減少重複複製時，才需要考慮調整目錄位置。

---

#### Q2：我需要移除 `chat.useAgentSkills` 設定嗎？

**A**：不需要。這個設定在 v1.109 中仍然存在，只是已經預設為啟用狀態。保留或移除都不影響功能。

---

#### Q3：我同時使用 Cursor，這次更新有幫助嗎？

**A**：部分有幫助。Cursor 仍然從 `.cursor/skills/` 讀取 skills，但你可以透過 `chat.agentSkillsLocations` 讓 GitHub Copilot 也搜尋 `.cursor/skills/`，減少一份重複。完整三工具共用的最佳方式是使用策略 2（自訂共用目錄）。

---

#### Q4：Skills 的自動觸發（Keywords）機制有改變嗎？

**A**：沒有。Skills 透過 `description` 中的 Keywords 自動匹配觸發的機制完全不變。v1.109 改變的只是 skills 的**搜尋路徑**，不影響 skills 的內容格式和觸發邏輯。

---

#### Q5：如何確認 skills 是否正確載入？

**A**：使用 v1.109 新增的 Chat Customization Diagnostics：

1. 開啟 Copilot Chat 面板
2. 右鍵點擊 → 選擇 Chat Customization Diagnostics
3. 確認 29 個 dotnet-testing skills 都出現在已載入清單中

如果有 skills 未載入，檢查：

- 目錄路徑是否正確
- 每個 skill 資料夾中是否有 `SKILL.md` 檔案
- `SKILL.md` 的 frontmatter 格式是否正確

---

### 📚 相關文件

#### 專案文件

- [SKILLS_USAGE_GUIDE.md](SKILLS_USAGE_GUIDE.md) — 完整使用手冊（安裝、技能清單、使用情境、FAQ）
- [SKILLS_QUICK_REFERENCE.md](SKILLS_QUICK_REFERENCE.md) — 快速參考指南（關鍵字對照、Prompt 模板）
- [OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md) — Skills 優化總結報告

#### 外部連結

- [VS Code v1.109 Release Notes - Agent Customization](https://code.visualstudio.com/updates/v1_109#_agent-customization)
- [agentskills.io 官方規範](https://agentskills.io)
- [GitHub Copilot Agent Skills 文件](https://docs.github.com/copilot/using-github-copilot/using-github-copilot-agent-skills)

---

**最後更新**：2026-02-09
