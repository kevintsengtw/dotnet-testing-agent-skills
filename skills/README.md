# .NET Testing Agent Skills

這個目錄包含 29 個 .NET 測試相關的 Agent Skills（2 個總覽技能 + 27 個專業技能）。

## 🎯 總覽 Skills

這兩個總覽 skills 提供智能導航，自動分析需求並推薦適合的技能組合：

- **[dotnet-testing](dotnet-testing/)** - 基礎測試技能總覽（19 個子技能）
- **[dotnet-testing-advanced](dotnet-testing-advanced/)** - 進階測試技能總覽（8 個子技能）

## 📦 安裝方式

### 使用 npx skills install（推薦）

```bash
# 從 GitHub 安裝到 Claude Code 全域
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 或安裝到當前工作區
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git --workspace
```

### 手動安裝

#### 對 GitHub Copilot（VS Code）

複製 `skills/` 目錄到專案的 `.github/skills/`：

```bash
cp -r skills/* /your-project/.github/skills/
```

#### 對 Claude Code

複製 `skills/` 目錄到工作區或全域：

```bash
# 工作區
cp -r skills/* /your-project/.claude/skills/

# 全域
cp -r skills/* ~/.config/claude/skills/
```

## 📚 Skills 清單

### 總覽技能（2 個）

| 技能 | 說明 |
|------|------|
| `dotnet-testing` | 基礎測試技能總覽與引導中心 |
| `dotnet-testing-advanced` | 進階測試技能總覽與引導中心 |

### 基礎技能（19 個）

- `dotnet-testing-unit-test-fundamentals`
- `dotnet-testing-test-naming-conventions`
- `dotnet-testing-xunit-project-setup`
- `dotnet-testing-awesome-assertions-guide`
- `dotnet-testing-complex-object-comparison`
- `dotnet-testing-code-coverage-analysis`
- `dotnet-testing-nsubstitute-mocking`
- `dotnet-testing-test-output-logging`
- `dotnet-testing-private-internal-testing`
- `dotnet-testing-fluentvalidation-testing`
- `dotnet-testing-datetime-testing-timeprovider`
- `dotnet-testing-filesystem-testing-abstractions`
- `dotnet-testing-test-data-builder-pattern`
- `dotnet-testing-autofixture-basics`
- `dotnet-testing-autofixture-customization`
- `dotnet-testing-autodata-xunit-integration`
- `dotnet-testing-autofixture-nsubstitute-integration`
- `dotnet-testing-bogus-fake-data`
- `dotnet-testing-autofixture-bogus-integration`

### 進階技能（8 個）

- `dotnet-testing-advanced-aspnet-integration-testing`
- `dotnet-testing-advanced-testcontainers-database`
- `dotnet-testing-advanced-testcontainers-nosql`
- `dotnet-testing-advanced-webapi-integration-testing`
- `dotnet-testing-advanced-aspire-testing`
- `dotnet-testing-advanced-xunit-upgrade-guide`
- `dotnet-testing-advanced-tunit-fundamentals`
- `dotnet-testing-advanced-tunit-advanced`

## 📖 詳細文檔

請參閱專案根目錄的 [PUBLIC_REPO_README.md](../PUBLIC_REPO_README.md) 獲取完整的使用說明和學習資源。

## 🏆 來源

基於「老派軟體工程師的測試修練 - 30 天挑戰」（2025 iThome 鐵人賽 Software Development 組冠軍作品）提煉而成。

## 📄 授權

MIT License
