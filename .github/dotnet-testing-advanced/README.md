# .NET Testing Advanced Agent Skills

這是專為 .NET 進階測試開發設計的 Agent Skills 集合，涵蓋整合測試與框架遷移指南，基於「老派軟體工程師的測試修練 - 30 天挑戰」(2025 iThome 鐵人賽 Software Development 組冠軍作品) 提煉而成。

## 📚 關於此技能集

這些技能專注於進階測試主題，包含：

- **整合測試**：使用 WebApplicationFactory、Testcontainers、.NET Aspire Testing 進行完整的系統整合測試
- **框架遷移**：xUnit 版本升級、TUnit 新世代測試框架遷移指南

這些技能遵循 [agentskills.io](https://agentskills.io) 開放標準，可在支援 Agent Skills 的 AI 助理中使用。

---

## 🎯 技能清單

### 第四階段：整合測試

| 技能                                                        | 說明                   | 主要用途                                    |
| ----------------------------------------------------------- | ---------------------- | ------------------------------------------- |
| [aspnet-integration-testing](./aspnet-integration-testing/) | ASP.NET 整合測試       | ASP.NET Core WebApplicationFactory 整合測試 |
| [testcontainers-database](./testcontainers-database/)       | 資料庫容器化測試         | 使用 Testcontainers 進行容器化資料庫測試  |
| [testcontainers-nosql](./testcontainers-nosql/)             | NoSQL 容器化測試       | Testcontainers 整合 MongoDB 與 Redis 測試   |
| [webapi-integration-testing](./webapi-integration-testing/) | WebApi 整合測試        | WebApi 服務整合測試完整流程                 |
| [aspire-testing](./aspire-testing/)                         | Aspire Testing 框架 | .NET Aspire Testing 整合測試框架          |

### 第五階段：框架遷移指南

| 技能                                          | 說明           | 主要用途                        |
| --------------------------------------------- | -------------- | ------------------------------- |
| [xunit-upgrade-guide](./xunit-upgrade-guide/) | xUnit 版本升級 | xUnit 2.9.x 到 3.x 的升級指南   |
| [tunit-fundamentals](./tunit-fundamentals/)   | TUnit 入門基礎 | TUnit 新世代測試框架入門        |
| [tunit-advanced](./tunit-advanced/)           | TUnit 進階應用 | TUnit 資料驅動與整合測試      |

---

## 🚀 快速開始

### 前置需求

在使用進階測試技能前，建議先熟悉基礎測試技能 ([dotnet-testing](../dotnet-testing/README.md))：

- 單元測試基礎 (`unit-test-fundamentals`)
- xUnit 專案設定 (`xunit-project-setup`)
- NSubstitute 模擬 (`nsubstitute-mocking`)
- AwesomeAssertions 斷言 (`awesome-assertions-guide`)

### 環境需求

進階測試技能可能需要以下額外環境：

- **Docker Desktop**：用於 Testcontainers 容器化測試
- **.NET Aspire**：用於分散式應用程式測試
- **容器執行環境**：WSL2、Docker、Podman 等

---

## 💡 使用情境範例

### 情境 1：建立 Web API 整合測試

當您問「如何測試 Web API 端點？」時，`aspnet-integration-testing` 技能會協助您：

- 建立 `WebApplicationFactory<T>` 測試環境
- 配置記憶體資料庫
- 使用 `AwesomeAssertions.Web` 驗證 HTTP 回應
- 實作測試資料準備與清理

### 情境 2：容器化資料庫測試

當您問「如何在測試中使用真實資料庫？」時，`testcontainers-database` 技能會指導您：

- 使用 Testcontainers 啟動 PostgreSQL/MSSQL 容器
- 配置資料庫連線與初始化
- 管理容器生命週期
- 處理平行測試的資料隔離

### 情境 3：遷移到 xUnit 3.x

當您問「如何升級 xUnit 到 3.x？」時，`xunit-upgrade-guide` 技能會提供：

- 版本差異對照表
- 破壞性變更清單
- 逐步遷移指南
- 常見問題解決方案

---

## 🎯 技能組合建議

### 整合測試完整組合

```text
1. aspnet-integration-testing      → API 端點測試基礎
2. testcontainers-database         → 資料庫容器化
3. testcontainers-nosql            → NoSQL 與快取測試
4. webapi-integration-testing      → 完整 API 測試流程
5. aspire-testing                  → 分散式應用測試
```

### 框架遷移組合

```text
1. xunit-upgrade-guide → xUnit 版本升級
2. tunit-fundamentals  → TUnit 入門基礎
3. tunit-advanced      → TUnit 進階應用
```

---

## � 技能統計

本目錄共包含 **8 個**進階測試技能，涵蓋整合測試與框架遷移主題。

查看完整的提煉記錄：[EXTRACTION_LOG.md](../../../EXTRACTION_LOG.md)

---

## 📖 相關資源

### 基礎技能

- **dotnet-testing/**：單元測試相關技能 (19 個技能)
- 詳見 [dotnet-testing/README.md](../dotnet-testing/README.md)

### 原始內容

- **iThome 鐵人賽文章**: [老派軟體工程師的測試修練 - 30 天挑戰](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
- **範例程式碼**: [30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)

### 外部資源

- [Microsoft.AspNetCore.Mvc.Testing](https://docs.microsoft.com/aspnet/core/test/integration-tests)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [.NET Aspire Testing](https://learn.microsoft.com/dotnet/aspire/testing/testing-overview)
- [TUnit 官方文件](https://tunit.dev/)

---

## 📝 授權

MIT License

---

**最後更新**: 2026-01-19
