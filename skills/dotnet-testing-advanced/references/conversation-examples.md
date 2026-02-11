# 引導對話範例

以下範例展示 AI 如何與您互動，幫助您選擇正確的進階技能。

### 範例 1：API 測試需求

**您**：我想測試 Web API

**AI**：我會協助您建立 Web API 測試！請先確認您的測試需求：

**1. API 複雜度**：
- [ ] 簡單的 API（幾個端點）
- [ ] 中型 API（完整的 CRUD）
- [ ] 複雜 API（多層架構、複雜業務邏輯）

**2. 資料庫需求**：
- [ ] 不需要資料庫
- [ ] 記憶體資料庫即可
- [ ] 需要真實資料庫

**3. 資料庫類型**（如需要真實資料庫）：
- [ ] SQL Server / PostgreSQL / MySQL
- [ ] MongoDB / Redis / Elasticsearch
- [ ] 兩者都有

請告訴我您的選擇，我會推薦適合的技能組合。

---

**您**：中型 API，需要真實的 SQL Server 資料庫

**AI**：完美！根據您的需求，我推薦以下技能組合：

**第一步：學習基礎整合測試（1-2 天）**
`dotnet-testing-advanced-aspnet-integration-testing`
- 理解 WebApplicationFactory
- 學習 HTTP 測試基礎

**第二步：加入真實資料庫（2 天）**
`dotnet-testing-advanced-testcontainers-database`
- 使用 Testcontainers 設定 SQL Server
- 學習資料庫測試隔離

**第三步：完整流程測試（2-3 天）**
`dotnet-testing-advanced-webapi-integration-testing`
- 實作完整的 CRUD 測試
- 測試資料管理策略
- 錯誤處理驗證

**預期成果**：
完成後您將能夠為 API 專案建立完整的整合測試套件，使用真實的 SQL Server 資料庫驗證行為。

需要我使用這些 skills 協助您開始嗎？

---

### 範例 2：微服務測試

**您**：我的專案是 .NET Aspire 微服務架構，要怎麼測試？

**AI**：.NET Aspire 微服務測試需要特殊的方法！我推薦使用 `dotnet-testing-advanced-aspire-testing` skill。

**前置確認**：
1. 您已安裝 .NET Aspire Workload？
   - 如果沒有：`dotnet workload install aspire`

2. 您的專案有哪些服務？
   - API Service
   - Worker Service
   - 其他服務

3. 是否使用資料庫？
   - 如果有，我們會搭配 `dotnet-testing-advanced-testcontainers-database`

**測試重點**：
- DistributedApplication 測試
- 服務間通訊驗證
- 依賴服務管理
- 端到端流程測試

需要我使用 `dotnet-testing-advanced-aspire-testing` skill 協助您建立測試嗎？

---

### 範例 3：框架升級

**您**：我的專案使用 xUnit 2.9，應該升級到 3.x 嗎？

**AI**：這是個好問題！我會協助您評估。讓我先了解您的情況：

**1. 升級動機**：
- [ ] 需要 xUnit 3.x 的新功能
- [ ] 解決已知問題
- [ ] 只是想跟上最新版本
- [ ] 其他原因

**2. 專案規模**：
- [ ] 小型（< 100 個測試）
- [ ] 中型（100-500 個測試）
- [ ] 大型（> 500 個測試）

**3. 升級時程壓力**：
- [ ] 不急，可以慢慢遷移
- [ ] 需要儘快完成

---

**您**：中型專案，主要是想跟上最新版本，不急

**AI**：了解！我推薦使用 `dotnet-testing-advanced-xunit-upgrade-guide` skill。

**升級計劃**：

**階段 1：了解差異（1 天）**
- 閱讀重大變更清單
- 了解哪些功能受影響
- 評估遷移工作量

**階段 2：準備升級（半天）**
- 備份目前程式碼
- 更新 .csproj 套件版本
- 檢查編譯錯誤

**階段 3：處理相容性（1-2 天）**
- 修正編譯錯誤
- 處理執行階段問題
- 驗證所有測試通過

**總工時估計**：2-3 天

**風險評估**：⭐⭐ 中低
xUnit 3.x 升級通常很順利，大部分變更都有向後相容。

需要我使用 `dotnet-testing-advanced-xunit-upgrade-guide` skill 協助您開始升級嗎？

---

### 範例 4：TUnit 評估

**您**：我聽說 TUnit 很不錯，應該遷移嗎？

**AI**：TUnit 確實是很有潛力的測試框架！讓我協助您評估。

**TUnit 主要優勢**：
1. ⚡ **更好的效能**：平行執行優化
2. 💉 **原生依賴注入**：不需要額外設定
3. 🎯 **現代化 API**：更直覺的設計
4. 🔧 **靈活性**：更多自訂選項

**評估重點**：

**1. 專案狀況**：
- 新專案 → 可以考慮直接使用 TUnit
- 現有專案 → 需評估遷移成本

**2. 團隊技能**：
- 團隊對 xUnit 熟悉度
- 學習新框架的意願

**3. 遷移成本**：
- 測試數量
- 特殊測試模式（如 Theory、ClassFixture 等）

**我的建議**：

**如果是新專案**：
使用 `dotnet-testing-advanced-tunit-fundamentals` skill 了解 TUnit，考慮直接採用。

**如果是現有專案**：
1. 先用 `dotnet-testing-advanced-tunit-fundamentals` 了解基礎
2. 試驗性遷移一個測試檔案
3. 評估實際遷移工作量
4. 決定是否全面遷移

需要我協助您評估嗎？
