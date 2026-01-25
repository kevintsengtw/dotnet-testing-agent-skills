# .NET Testing Skills 快速參考指南

> 💡 **給 AI 助理的提示**：當用戶請求下列任務時，請主動引用對應的 Agent Skills

## 🎯 總覽 Skills（不確定時先用這個！）

| Skill 名稱 | 用途 | 何時使用 |
|-----------|------|---------|
| `dotnet-testing` | 基礎技能導航（19 個子技能） | 一般性測試問題、不確定用哪個基礎技能時 |
| `dotnet-testing-advanced` | 進階技能導航（8 個子技能） | 整合測試、API 測試、微服務等進階需求 |

**總覽 Skills 的價值**：自動分析需求，推薦 1-4 個最適合的技能組合，提供學習路徑與範例。

## 📋 快速查詢表

| 我想做什麼         | 使用這些 Skills                                                                                                                  | 範例 Prompt                                                                        |
| ------------------ | -------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| 🆕 建立測試專案    | `dotnet-testing-xunit-project-setup`                                                                                             | 「使用 xunit-project-setup skill 為我的專案建立測試結構」                          |
| ✅ 寫單元測試      | `dotnet-testing-unit-test-fundamentals`<br>`dotnet-testing-test-naming-conventions`<br>`dotnet-testing-awesome-assertions-guide` | 「使用 unit-test-fundamentals 和 test-naming-conventions skills 為這個方法寫測試」 |
| 🎭 Mock 依賴      | `dotnet-testing-nsubstitute-mocking`                                                                                             | 「使用 nsubstitute-mocking skill 來模擬這個 repository」                           |
| 🔧 產生測試資料   | `dotnet-testing-autofixture-basics`<br>`dotnet-testing-bogus-fake-data`                                                          | 「使用 autofixture-basics skill 自動產生測試資料」                                 |
| 🔗 整合測試 (API) | `dotnet-testing-advanced-aspnet-integration-testing`                                                                             | 「使用 aspnet-integration-testing skill 建立 API 測試」                            |
| 🐳 容器化測試     | `dotnet-testing-advanced-testcontainers-database`                                                                                | 「使用 testcontainers-database skill 設定資料庫測試」                              |
| 📊 檢查覆蓋率     | `dotnet-testing-code-coverage-analysis`                                                                                          | 「使用 code-coverage-analysis skill 分析測試覆蓋率」                               |
| 🔄 升級 xUnit     | `dotnet-testing-advanced-xunit-upgrade-guide`                                                                                    | 「使用 xunit-upgrade-guide skill 協助升級到 xUnit 3.x」                            |

## 🎯 常見情境組合

### 情境 1：從零開始建立測試專案

```text
請使用以下 skills 協助我建立完整的測試專案：
1. dotnet-testing-xunit-project-setup - 建立專案結構
2. dotnet-testing-test-naming-conventions - 設定命名規範
3. dotnet-testing-unit-test-fundamentals - 建立第一個測試
```

### 情境 2：為有依賴的服務寫測試

```text
請使用以下 skills 為這個服務類別建立測試：
1. dotnet-testing-unit-test-fundamentals - 測試結構
2. dotnet-testing-nsubstitute-mocking - 模擬依賴
3. dotnet-testing-autofixture-basics - 產生測試資料
4. dotnet-testing-awesome-assertions-guide - 撰寫斷言
```

### 情境 3：建立整合測試

```text
請使用以下 skills 建立完整的整合測試：
1. dotnet-testing-advanced-testcontainers-database - 設定資料庫容器
2. dotnet-testing-advanced-aspnet-integration-testing - API 測試基礎
3. dotnet-testing-advanced-webapi-integration-testing - 完整流程
```

## 💬 推薦的 Prompt 模板

### 模板 1：明確指定 Skills

```text
請參考 {skill-name} skill 來協助我 {任務描述}
```

**範例**：

```text
請參考 dotnet-testing-nsubstitute-mocking skill 來協助我為 UserService 建立 Mock Repository
```

### 模板 2：多個 Skills 組合

```text
請使用以下 skills：
- {skill-1} - {用途}
- {skill-2} - {用途}
- {skill-3} - {用途}

來協助我 {任務描述}
```

### 模板 3：探索性詢問

```text
我想要 {任務描述}，請建議我應該使用哪些 skills？
```

## 📚 完整 Skills 清單

### 基礎技能 (19 個)

<details>
<summary>點擊展開</summary>

1. `dotnet-testing-unit-test-fundamentals`
2. `dotnet-testing-test-naming-conventions`
3. `dotnet-testing-xunit-project-setup`
4. `dotnet-testing-awesome-assertions-guide`
5. `dotnet-testing-complex-object-comparison`
6. `dotnet-testing-code-coverage-analysis`
7. `dotnet-testing-nsubstitute-mocking`
8. `dotnet-testing-test-output-logging`
9. `dotnet-testing-private-internal-testing`
10. `dotnet-testing-fluentvalidation-testing`
11. `dotnet-testing-datetime-testing-timeprovider`
12. `dotnet-testing-filesystem-testing-abstractions`
13. `dotnet-testing-test-data-builder-pattern`
14. `dotnet-testing-autofixture-basics`
15. `dotnet-testing-autofixture-customization`
16. `dotnet-testing-autodata-xunit-integration`
17. `dotnet-testing-autofixture-nsubstitute-integration`
18. `dotnet-testing-bogus-fake-data`
19. `dotnet-testing-autofixture-bogus-integration`

</details>

### 進階技能 (8 個)

<details>
<summary>點擊展開</summary>

1. `dotnet-testing-advanced-aspnet-integration-testing`
2. `dotnet-testing-advanced-testcontainers-database`
3. `dotnet-testing-advanced-testcontainers-nosql`
4. `dotnet-testing-advanced-webapi-integration-testing`
5. `dotnet-testing-advanced-aspire-testing`
6. `dotnet-testing-advanced-xunit-upgrade-guide`
7. `dotnet-testing-advanced-tunit-fundamentals`
8. `dotnet-testing-advanced-tunit-advanced`

</details>

---

**提示**：將此檔案加入書籤，需要時快速查詢！
