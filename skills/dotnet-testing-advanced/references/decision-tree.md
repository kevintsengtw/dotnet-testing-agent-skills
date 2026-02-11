# 快速決策樹

### 我需要哪種進階測試？

#### 情境 1：測試 ASP.NET Core Web API

**選項 A - 基礎 API 測試**
→ `dotnet-testing-advanced-aspnet-integration-testing`

**適合**：
- 簡單的 API 端點測試
- 不需要真實資料庫（使用記憶體資料庫）
- 測試路由、模型綁定、HTTP 回應

**涵蓋內容**：
- WebApplicationFactory 使用
- TestServer 設定
- HTTP 回應驗證
- 記憶體資料庫配置

---

**選項 B - 完整的 WebAPI 流程測試**
→ `dotnet-testing-advanced-webapi-integration-testing`

**適合**：
- 完整的 CRUD API 測試
- 需要測試完整的業務流程
- 需要測試資料準備與清理

**涵蓋內容**：
- 完整的 GET、POST、PUT、DELETE 測試
- 錯誤處理驗證
- 測試基底類別模式
- 資料準備策略

---

#### 情境 2：測試需要真實資料庫

**選項 A - 關聯式資料庫（SQL Server、PostgreSQL、MySQL）**
→ `dotnet-testing-advanced-testcontainers-database`

**適合**：
- Entity Framework Core 測試
- Dapper 測試
- 真實資料庫行為驗證
- 需要測試資料庫特定功能（stored procedures、triggers 等）

**支援資料庫**：
- SQL Server
- PostgreSQL
- MySQL
- MariaDB

---

**選項 B - NoSQL 資料庫（MongoDB、Redis、Elasticsearch）**
→ `dotnet-testing-advanced-testcontainers-nosql`

**適合**：
- MongoDB 文件操作測試
- Redis 快取測試
- Elasticsearch 搜尋測試
- NoSQL 特有功能測試

**支援資料庫**：
- MongoDB
- Redis
- Elasticsearch

---

#### 情境 3：測試微服務架構

→ `dotnet-testing-advanced-aspire-testing`

**適合**：
- .NET Aspire 專案
- 分散式應用測試
- 服務間通訊測試
- 微服務整合測試

**涵蓋內容**：
- DistributedApplication 測試
- 服務依賴管理
- 跨服務測試
- 測試容器編排

---

#### 情境 4：升級或遷移測試框架

**選項 A - xUnit 升級（2.x → 3.x）**
→ `dotnet-testing-advanced-xunit-upgrade-guide`

**適合**：
- 現有專案使用 xUnit 2.x
- 想升級到 xUnit 3.x
- 了解版本差異

**涵蓋內容**：
- 重大變更說明
- 升級步驟指引
- 相容性問題處理
- 最佳實踐

---

**選項 B - 遷移到 TUnit（基礎）**
→ `dotnet-testing-advanced-tunit-fundamentals`

**適合**：
- 評估是否遷移到 TUnit
- 了解 TUnit 基礎
- 學習 TUnit 與 xUnit 的差異

**涵蓋內容**：
- TUnit 核心概念
- 與 xUnit 對比
- 遷移步驟
- 基本使用方式

---

**選項 C - TUnit 進階功能**
→ `dotnet-testing-advanced-tunit-advanced`

**適合**：
- 已使用 TUnit 基礎
- 想深入使用 TUnit 功能
- 需要平行執行、依賴注入等進階特性

**涵蓋內容**：
- 資料驅動測試
- 依賴注入
- 平行執行控制
- 進階特性
