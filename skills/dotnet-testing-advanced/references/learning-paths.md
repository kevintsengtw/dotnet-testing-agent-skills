# 學習路徑建議

### 整合測試入門（1 週）

**目標**：掌握整合測試基礎，能為 Web API 建立測試

#### Day 1-2：基礎整合測試

**技能**：`dotnet-testing-advanced-aspnet-integration-testing`

**學習重點**：
- WebApplicationFactory 概念
- TestServer 使用
- HTTP 回應驗證
- 記憶體資料庫配置

**實作練習**：
- 為簡單的 API 建立整合測試
- 測試 GET、POST 端點

---

#### Day 3-4：完整 WebAPI 測試

**技能**：`dotnet-testing-advanced-webapi-integration-testing`

**學習重點**：
- 完整 CRUD 測試
- 測試基底類別設計
- 資料準備與清理
- 錯誤處理驗證

**實作練習**：
- 為完整的 Controller 建立測試
- 實作測試資料管理

---

#### Day 5-6：容器化資料庫測試

**技能**：`dotnet-testing-advanced-testcontainers-database`

**學習重點**：
- Testcontainers 概念
- SQL Server 容器設定
- 資料庫遷移執行
- 測試隔離

**實作練習**：
- 為 Repository 建立測試
- 使用真實資料庫

---

#### Day 7：NoSQL 資料庫測試

**技能**：`dotnet-testing-advanced-testcontainers-nosql`

**學習重點**：
- MongoDB 容器設定
- Redis 容器設定
- NoSQL 特定測試模式

**實作練習**：
- 測試 MongoDB Repository
- 測試 Redis Cache Service

---

### 微服務測試專精（3-5 天）

**前置條件**：完成整合測試入門

**技能**：`dotnet-testing-advanced-aspire-testing`

**學習重點**：
- .NET Aspire 架構理解
- DistributedApplication 測試
- 服務依賴管理
- 分散式測試策略

**實作練習**：
- 測試 Aspire 微服務專案
- 驗證服務間通訊

---

### 框架遷移路徑（依需求）

#### xUnit 升級（1-2 天）

**技能**：`dotnet-testing-advanced-xunit-upgrade-guide`

**學習重點**：
- xUnit 3.x 新特性
- 重大變更處理
- 升級步驟
- 相容性測試

**實施步驟**：
1. 了解版本差異
2. 更新套件
3. 處理編譯錯誤
4. 驗證測試執行

---

#### TUnit 遷移（2-5 天）

**基礎（2-3 天）**
技能：`dotnet-testing-advanced-tunit-fundamentals`

**學習重點**：
- TUnit 核心概念
- 與 xUnit 對比
- 基本測試撰寫
- 遷移建議

---

**進階（2-3 天）**
技能：`dotnet-testing-advanced-tunit-advanced`

**學習重點**：
- 資料驅動測試
- 依賴注入
- 平行執行控制
- 進階特性
