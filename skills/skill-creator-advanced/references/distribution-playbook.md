# Distribution & sharing playbook（發布與分享）

這份文件整理技能發布時常見的交付物與最佳做法。

## 1) 交付物清單

最少：
- skill folder（含 SKILL.md + 必要 scripts/references/assets）
- 打包後的 .skill 檔

建議（放在 skill folder 外面，例如 repo root）：
- README（安裝與使用說明）
- screenshots / demo GIF
- Releases（提供 zip/.skill 下載）

## 2) 安裝路徑（概念）

個人使用者：
- 下載 skill folder
-（如需要）zip
- 於產品設定頁上傳 / 或放到對應目錄

組織層級：
- 由管理者 workspace-wide 部署
- 設定自動更新與集中管理

## 3) Repo 結構建議

- repo root：README.md、CHANGELOG（可選）、release notes、示例
- skill folder：只放 SKILL.md + scripts/ references/ assets/ + 必要 license

避免：在 skill folder 放 README.md（容易造成混亂且不建議）。

## 4) 版本策略

- 在 frontmatter 的頂層 `version:` 維護日期版本（例如 2026.3.9）
- Release notes 於 repo 或外部文件
- 重大變更：調整 triggers/範圍需清楚公告

## 5) 定位與文案

原則：強調 outcome，而不是「這是一個有 YAML 的資料夾」。

也要說清楚 MCP + skills 的價值（如果適用）：
- MCP 提供工具/資料 access
- skill 提供 workflow 與 best practices
