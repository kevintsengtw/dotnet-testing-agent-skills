# 跨平台使用指南

本 repository 包含的 Agent Skills 遵循 [agentskills.io](https://agentskills.io) 開放標準，**可在多種 AI 平台使用**。

## 🌐 支援的 AI 平台

### GitHub Copilot (VS Code)
\\\ash
# 複製到 .github/skills/ (GitHub Copilot 的標準路徑)
cp -r dotnet-testing-agent-skills/.github/skills /your-project/.github/
\\\

### Claude Desktop / Claude Code CLI
\\\ash
# 方法 1: 使用 /plugin 指令直接載入
# 在 Claude 對話中輸入: /plugin path/to/skill/SKILL.md

# 方法 2: 複製到專案中，讓 Claude Code CLI 自動識別
cp -r dotnet-testing-agent-skills/.github/skills /your-project/.claude/skills/
\\\

### Cursor
\\\ash
# 複製到 Cursor 的 skills 目錄
cp -r dotnet-testing-agent-skills/.github/skills /your-project/.cursor/skills/
\\\

### 其他支援 Agent Skills 的 AI 工具
\\\ash
# 將 skills 複製到該工具指定的目錄
# 查閱該工具的文件以確認正確路徑
\\\

## 📂 Skills 內容結構

每個 skill 都包含：
- \SKILL.md\ - 技能定義檔案（符合 agentskills.io 標準）
- \	emplates/\ - 程式碼範本與範例

這些檔案是**平台無關**的，可以在任何支援 Agent Skills 標準的 AI 工具中使用。

## 💡 直接使用單一 Skill

如果您的 AI 工具支援直接載入 skill 檔案：

\\\ash
# 只需要指向 SKILL.md 檔案
# 例如：Claude Desktop 可以直接開啟
dotnet-testing-agent-skills/.github/skills/dotnet-testing/unit-test-fundamentals/SKILL.md
\\\

## 🔗 為什麼放在 .github/skills/?

雖然這個 repository 將 skills 組織在 \.github/skills/\ 下（GitHub Copilot 的標準路徑），但這**不限制**您只能在 GitHub Copilot 使用。您可以：

1. 直接複製整個 \skills/\ 資料夾到任何路徑
2. 只複製需要的 skill 資料夾
3. 讓您的 AI 工具指向這個 repository 的路徑

關鍵是 **SKILL.md 檔案本身符合開放標準**，與存放位置無關！
