# 技能分類地圖

### 1. 整合測試（4 個技能）- Web API 測試

| 技能名稱 | 測試範圍 | 資料庫 | 適用場景 |
|---------|---------|--------|---------|
| `dotnet-testing-advanced-aspnet-integration-testing` | WebApplicationFactory、TestServer、HTTP 回應 | 記憶體資料庫 | 基礎 API 整合測試 |
| `dotnet-testing-advanced-webapi-integration-testing` | 完整 CRUD、錯誤處理、業務流程 | 記憶體/真實 | 真實 API 專案測試 |
| `dotnet-testing-advanced-testcontainers-database` | SQL Server、PostgreSQL、MySQL | 真實（容器化） | 需要真實資料庫行為 |
| `dotnet-testing-advanced-testcontainers-nosql` | MongoDB、Redis、Elasticsearch | 真實（容器化） | NoSQL 資料庫測試 |

#### 技能詳細說明

**aspnet-integration-testing**

**核心價值**：
- 學習 WebApplicationFactory 基礎
- 理解整合測試概念
- 測試 HTTP 端點而不啟動真實伺服器

**適合情境**：
- API 端點基礎測試
- 路由驗證
- 中介軟體測試
- 不需要真實資料庫

**學習難度**：⭐⭐ 中等

**前置技能**：
- `dotnet-testing-unit-test-fundamentals`（必須）
- `dotnet-testing-awesome-assertions-guide`（建議）

---

**webapi-integration-testing**

**核心價值**：
- 完整的 API 測試流程
- 測試資料管理策略
- 錯誤處理驗證
- 真實業務場景測試

**適合情境**：
- 正式專案的 API 測試
- 完整的 CRUD 流程
- 複雜的業務邏輯驗證
- 需要測試資料準備與清理

**學習難度**：⭐⭐⭐ 中高

**前置技能**：
- `dotnet-testing-advanced-aspnet-integration-testing`（必須）
- `dotnet-testing-nsubstitute-mocking`（建議）

---

**testcontainers-database**

**核心價值**：
- 使用真實資料庫測試
- 自動化容器管理
- 測試資料庫特定功能
- 隔離的測試環境

**適合情境**：
- EF Core 測試
- Dapper 測試
- Stored Procedures 測試
- 資料庫遷移測試

**學習難度**：⭐⭐⭐ 中高

**前置技能**：
- `dotnet-testing-unit-test-fundamentals`（必須）
- Docker 基礎知識（必須）

**技術需求**：
- Docker Desktop 已安裝
- WSL2（Windows 環境）

---

**testcontainers-nosql**

**核心價值**：
- 測試 NoSQL 資料庫操作
- 容器化 NoSQL 環境
- 真實資料庫行為驗證

**適合情境**：
- MongoDB 文件操作
- Redis 快取邏輯
- Elasticsearch 搜尋功能

**學習難度**：⭐⭐⭐ 中高

**前置技能**：
- `dotnet-testing-advanced-testcontainers-database`（建議）
- NoSQL 資料庫基礎知識

---

### 2. 微服務測試（1 個技能）- 分散式系統

| 技能名稱 | 測試範圍 | 架構 | 適用場景 |
|---------|---------|------|---------|
| `dotnet-testing-advanced-aspire-testing` | .NET Aspire 分散式應用 | 微服務 | 雲原生、微服務架構 |

#### 技能詳細說明

**aspire-testing**

**核心價值**：
- 測試 .NET Aspire 專案
- 分散式應用整合測試
- 服務依賴管理
- 端到端流程驗證

**適合情境**：
- .NET Aspire 微服務專案
- 多服務協作測試
- 服務發現測試
- 分散式追蹤驗證

**學習難度**：⭐⭐⭐⭐ 高

**前置技能**：
- `dotnet-testing-advanced-aspnet-integration-testing`（必須）
- `dotnet-testing-advanced-testcontainers-database`（建議）
- .NET Aspire 基礎知識（必須）

**技術需求**：
- .NET 8+
- .NET Aspire Workload
- Docker Desktop

**涵蓋內容**：
- DistributedApplication 測試
- 服務間通訊測試
- 依賴服務管理
- 測試容器編排

---

### 3. 框架遷移（3 個技能）- 測試框架升級

| 技能名稱 | 遷移路徑 | 難度 | 適用場景 |
|---------|---------|------|---------|
| `dotnet-testing-advanced-xunit-upgrade-guide` | xUnit 2.x → 3.x | ⭐⭐ 中等 | 升級現有 xUnit 專案 |
| `dotnet-testing-advanced-tunit-fundamentals` | xUnit → TUnit（基礎） | ⭐⭐ 中等 | 評估或遷移到 TUnit |
| `dotnet-testing-advanced-tunit-advanced` | TUnit 進階功能 | ⭐⭐⭐ 中高 | 深入使用 TUnit |

#### 技能詳細說明

**xunit-upgrade-guide**

**核心價值**：
- 了解 xUnit 3.x 新特性
- 處理升級問題
- 遷移步驟指引

**適合情境**：
- 專案使用 xUnit 2.x
- 想升級到最新版本
- 了解版本差異

**學習難度**：⭐⭐ 中等

**涵蓋內容**：
- 重大變更清單
- 套件升級步驟
- 相容性問題處理
- 升級檢查清單

**何時升級**：
- 需要 xUnit 3.x 新功能
- .NET 9+ 專案
- 解決已知問題

---

**tunit-fundamentals**

**核心價值**：
- 了解 TUnit 新世代測試框架
- 學習 TUnit 核心概念
- 評估遷移可行性

**適合情境**：
- 考慮從 xUnit 遷移
- 新專案選擇測試框架
- 了解現代測試框架

**學習難度**：⭐⭐ 中等

**涵蓋內容**：
- TUnit vs xUnit 對比
- 基本測試撰寫
- 屬性與斷言
- 遷移建議

**TUnit 優勢**：
- 更好的效能
- 原生支援依賴注入
- 更靈活的測試組織
- 現代化的 API 設計

---

**tunit-advanced**

**核心價值**：
- 深入使用 TUnit 進階功能
- 平行執行控制
- 依賴注入整合
- 資料驅動測試

**適合情境**：
- 已使用 TUnit 基礎
- 需要進階功能
- 大型測試專案

**學習難度**：⭐⭐⭐ 中高

**前置技能**：
- `dotnet-testing-advanced-tunit-fundamentals`（必須）

**涵蓋內容**：
- 進階資料驅動測試
- 依賴注入容器
- 測試執行控制
- 自訂測試框架行為
