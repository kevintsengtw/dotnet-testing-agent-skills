# .NET Testing Skills 快速參考指南

> 💡 **給 AI Agent 和開發者的提示**：這是完整的 Skills 使用指南，包含關鍵字對應、使用範例和最佳實踐

---

## 📖 關於本指南

本指南整合了兩個重要面向：
- 🤖 **AI Agent 使用指引**：關鍵字對應表、工作流程範本
- 👨‍💻 **開發者快速參考**：Prompt 模板、情境組合建議

**重要提醒**：
- ✅ **Triggers 自動匹配**：27 個 skills 已優化，包含 400-520 個 triggers，AI 會根據對話內容自動載入
- ✅ **入口 Skills**：不確定時可使用 `dotnet-testing` 或 `dotnet-testing-advanced` 獲得智能推薦
- ✅ **本指南僅供參考**：主要用於人類開發者查詢，AI 主要依靠 triggers 自動匹配

---

## 🎯 總覽 Skills（不確定時先用這個！）

| Skill 名稱 | 用途 | 何時使用 |
|-----------|------|---------|
| `dotnet-testing` | 基礎技能導航（19 個子技能） | 一般性測試問題、不確定用哪個基礎技能時 |
| `dotnet-testing-advanced` | 進階技能導航（8 個子技能） | 整合測試、API 測試、微服務等進階需求 |

**總覽 Skills 的價值**：自動分析需求，推薦 1-4 個最適合的技能組合，提供學習路徑與範例。

---

## 🔍 關鍵字快速對應表

> 💡 **給 AI Agent**：這個表格幫助您快速找到對應的 Skill。然而，由於已優化 triggers，大部分情況下會自動匹配。

| 當使用者提到... | 應使用的 Skill | Skill 完整路徑 |
|----------------|---------------|---------------|
| **Validator、驗證器、FluentValidation、CreateUserValidator** | `dotnet-testing-fluentvalidation-testing` | `/skills/dotnet-testing/dotnet-testing-fluentvalidation-testing/` |
| **AutoFixture、測試資料生成、CreateMany** | `dotnet-testing-autofixture-basics` | `/skills/dotnet-testing/dotnet-testing-autofixture-basics/` |
| **Mock、模擬、NSubstitute、Substitute.For** | `dotnet-testing-nsubstitute-mocking` | `/skills/dotnet-testing/dotnet-testing-nsubstitute-mocking/` |
| **時間測試、DateTime、TimeProvider、FakeTimeProvider** | `dotnet-testing-datetime-testing-timeprovider` | `/skills/dotnet-testing/dotnet-testing-datetime-testing-timeprovider/` |
| **檔案測試、File、IFileSystem、MockFileSystem** | `dotnet-testing-filesystem-testing-abstractions` | `/skills/dotnet-testing/dotnet-testing-filesystem-testing-abstractions/` |
| **Bogus、假資料、Faker、擬真資料** | `dotnet-testing-bogus-fake-data` | `/skills/dotnet-testing/dotnet-testing-bogus-fake-data/` |
| **Builder、Test Data Builder、WithXxx** | `dotnet-testing-test-data-builder-pattern` | `/skills/dotnet-testing/dotnet-testing-test-data-builder-pattern/` |
| **FluentAssertions、Should()、BeEquivalentTo** | `dotnet-testing-awesome-assertions-guide` | `/skills/dotnet-testing/dotnet-testing-awesome-assertions-guide/` |
| **物件比對、深層比較、DTO 比對** | `dotnet-testing-complex-object-comparison` | `/skills/dotnet-testing/dotnet-testing-complex-object-comparison/` |
| **API 測試、WebApplicationFactory、整合測試** | `dotnet-testing-advanced-aspnet-integration-testing` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-aspnet-integration-testing/` |
| **WebAPI 測試、CRUD 測試、端點測試** | `dotnet-testing-advanced-webapi-integration-testing` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-webapi-integration-testing/` |
| **Testcontainers、容器測試、SQL Server 容器** | `dotnet-testing-advanced-testcontainers-database` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-testcontainers-database/` |
| **MongoDB、Redis、Elasticsearch、NoSQL** | `dotnet-testing-advanced-testcontainers-nosql` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-testcontainers-nosql/` |
| **Aspire、微服務、DistributedApplication** | `dotnet-testing-advanced-aspire-testing` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-aspire-testing/` |
| **xUnit 升級、xUnit 3.x** | `dotnet-testing-advanced-xunit-upgrade-guide` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-xunit-upgrade-guide/` |
| **TUnit、新測試框架** | `dotnet-testing-advanced-tunit-fundamentals` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-tunit-fundamentals/` |
| **測試覆蓋率、Coverlet、Code Coverage** | `dotnet-testing-code-coverage-analysis` | `/skills/dotnet-testing/dotnet-testing-code-coverage-analysis/` |
| **私有成員、internal、InternalsVisibleTo** | `dotnet-testing-private-internal-testing` | `/skills/dotnet-testing/dotnet-testing-private-internal-testing/` |
| **AutoData、[AutoData]、Theory** | `dotnet-testing-autodata-xunit-integration` | `/skills/dotnet-testing/dotnet-testing-autodata-xunit-integration/` |
| **ITestOutputHelper、測試輸出、ILogger** | `dotnet-testing-test-output-logging` | `/skills/dotnet-testing/dotnet-testing-test-output-logging/` |

---

## 📋 完整技能目錄

### 基礎測試技能（dotnet-testing）- 19 個技能

#### 🎓 測試基礎
1. `dotnet-testing-unit-test-fundamentals` - 單元測試基礎（FIRST 原則、3A Pattern）
2. `dotnet-testing-test-naming-conventions` - 測試命名規範
3. `dotnet-testing-xunit-project-setup` - xUnit 專案建置

#### 📦 測試資料生成
4. `dotnet-testing-autofixture-basics` - AutoFixture 基礎
5. `dotnet-testing-autofixture-customization` - AutoFixture 客製化
6. `dotnet-testing-bogus-fake-data` - Bogus 假資料生成
7. `dotnet-testing-test-data-builder-pattern` - Test Data Builder Pattern
8. `dotnet-testing-autofixture-bogus-integration` - AutoFixture + Bogus 整合

#### 🎭 測試替身
9. `dotnet-testing-nsubstitute-mocking` - NSubstitute Mock 框架
10. `dotnet-testing-autofixture-nsubstitute-integration` - AutoFixture + NSubstitute 整合

#### ✅ 斷言驗證
11. `dotnet-testing-awesome-assertions-guide` - FluentAssertions 流暢斷言
12. `dotnet-testing-complex-object-comparison` - 複雜物件比對
13. `dotnet-testing-fluentvalidation-testing` - FluentValidation 測試 ⭐

#### 🔧 特殊場景
14. `dotnet-testing-datetime-testing-timeprovider` - 時間相關測試
15. `dotnet-testing-filesystem-testing-abstractions` - 檔案系統測試
16. `dotnet-testing-private-internal-testing` - 私有/內部成員測試

#### 📊 測試度量
17. `dotnet-testing-code-coverage-analysis` - 程式碼覆蓋率分析

#### 🔗 框架整合
18. `dotnet-testing-autodata-xunit-integration` - AutoData + xUnit 整合
19. `dotnet-testing-test-output-logging` - 測試輸出與日誌

---

### 進階測試技能（dotnet-testing-advanced）- 8 個技能

#### 🌐 整合測試
1. `dotnet-testing-advanced-aspnet-integration-testing` - ASP.NET Core 整合測試
2. `dotnet-testing-advanced-webapi-integration-testing` - WebAPI 完整測試
3. `dotnet-testing-advanced-testcontainers-database` - Testcontainers 資料庫測試
4. `dotnet-testing-advanced-testcontainers-nosql` - Testcontainers NoSQL 測試

#### ☁️ 微服務測試
5. `dotnet-testing-advanced-aspire-testing` - .NET Aspire 測試

#### 🔄 框架遷移
6. `dotnet-testing-advanced-xunit-upgrade-guide` - xUnit 升級指南
7. `dotnet-testing-advanced-tunit-fundamentals` - TUnit 基礎
8. `dotnet-testing-advanced-tunit-advanced` - TUnit 進階

---

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

---

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

---

## 🎯 常見情境快速指引

### 情境 1：為 Validator 建立測試
**觸發關鍵字**：Validator、驗證器、CreateUserValidator、UpdateProductValidator

**使用 Skill**：
```
/skill dotnet-testing-fluentvalidation-testing
```

**為什麼**：此 skill 專門處理 FluentValidation 測試，包含：
- TestHelper 完整使用
- ShouldHaveValidationErrorFor 方法
- ShouldNotHaveValidationErrorFor 方法
- 複雜驗證規則測試
- 自訂驗證器測試

---

### 情境 2：需要產生大量測試資料
**觸發關鍵字**：測試資料、AutoFixture、CreateMany、假資料

**使用 Skill**：
- 自動化資料 → `dotnet-testing-autofixture-basics`
- 擬真資料 → `dotnet-testing-bogus-fake-data`
- 兩者結合 → `dotnet-testing-autofixture-bogus-integration`

---

### 情境 3：有外部依賴需要模擬
**觸發關鍵字**：Mock、模擬、Repository、Service 依賴

**使用 Skill**：
```
/skill dotnet-testing-nsubstitute-mocking
```

如果已使用 AutoFixture：
```
/skill dotnet-testing-autofixture-nsubstitute-integration
```

---

### 情境 4：測試時間相關邏輯
**觸發關鍵字**：DateTime、時間測試、過期時間、TimeProvider

**使用 Skill**：
```
/skill dotnet-testing-datetime-testing-timeprovider
```

---

### 情境 5：測試 Web API
**觸發關鍵字**：API 測試、Controller 測試、整合測試、端點測試

**使用 Skill**：
- 基礎 API 測試 → `dotnet-testing-advanced-aspnet-integration-testing`
- 完整 CRUD 測試 → `dotnet-testing-advanced-webapi-integration-testing`
- 需要真實資料庫 → `dotnet-testing-advanced-testcontainers-database`

---

## 🚀 AI Agent 工作流程範本

```
步驟 1: 接收使用者需求
   └─> 使用者：「請幫我建立 CreateUserValidator 的測試」

步驟 2: Triggers 自動匹配 ✅
   └─> 系統根據關鍵字「Validator」自動觸發
   └─> 載入 `dotnet-testing-fluentvalidation-testing`

步驟 3: 執行 Skill 指引 ✅
   └─> 按照 skill 內容建立測試
   └─> 使用 TestHelper
   └─> 撰寫驗證規則測試

步驟 4: 回應使用者 ✅
   └─> 提供完整的測試程式碼
   └─> 說明測試涵蓋的情境
```

**重要提醒**：由於 triggers 優化（27 個 skills，總計 400-520 個 triggers），大部分情況下 AI 會自動載入正確的 skill，無需手動指定。

---

## 📚 參考資源

### 入口指南
如果不確定使用哪個 skill，可先查看入口指南：
- [基礎測試總覽](/skills/dotnet-testing/SKILL.md)
- [進階測試總覽](/skills/dotnet-testing-advanced/SKILL.md)

這些檔案包含：
- 完整的決策樹
- 學習路徑建議
- 常見任務映射表
- 技能組合建議

### 原始資料來源
- **iThome 鐵人賽**：[老派軟體工程師的測試修練](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
- **範例程式碼**：[30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)

---

## ⚠️ 重要提醒

### 對於 AI Agent
1. **Triggers 自動匹配最重要** - 27 個 skills 已優化 triggers（400-520 個），會自動觸發
2. **遵循 skill 指引** - 每個 skill 都包含最佳實踐和完整範例
3. **不確定時使用入口 skill** - `dotnet-testing` 或 `dotnet-testing-advanced` 會提供智能推薦
4. **本指南僅供參考** - AI 主要依靠 triggers，不會主動讀取此檔案

### 對於開發者
1. **明確指定 skill** - 如果知道要用哪個 skill，直接在對話中提及
2. **提供關鍵字** - 多提供相關關鍵字幫助 AI 匹配 skill
3. **參考入口指南** - 不確定時查看 dotnet-testing 或 dotnet-testing-advanced 的 SKILL.md
4. **使用 Prompt 模板** - 參考上方的模板範例來引導 AI

---

## 📊 Skills 優化狀態

本 Skills 集合已完成全面優化（2026-01-27）：

### Triggers 統計
- **27 個 Skills** 全面優化
- **400-520 個 Triggers** 涵蓋各種使用場景
- **平均 15-19 個 Triggers** 每個 skill

### 預期效果
- AI 能找到對應 Skill：**70-85%**（相比優化前提升 100-140%）
- AI 主動載入 Skill：**50-70%**（相比優化前提升 150-233%）
- AI 遵循 Skill 指引：**80-90%**（相比優化前提升 50-60%）

詳細資訊請參考：[OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md)

---

**最後更新**：2026-01-27
**維護者**：Kevin Tseng
**版本**：2.0.0（合併版）
