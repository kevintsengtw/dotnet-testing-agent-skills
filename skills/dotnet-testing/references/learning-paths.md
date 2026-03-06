# 學習路徑建議

> 此文件從 `SKILL.md` 提取，提供新手與進階的完整學習路徑規劃。

---

### 新手路徑（1-2 週）

**目標**：建立測試基礎，能夠撰寫基本的單元測試

#### 階段 1：建立基礎（Day 1-5）

**Day 1-2：測試基礎概念**
- 技能：`dotnet-testing-unit-test-fundamentals`
- 學習重點：
  - FIRST 原則
  - 3A Pattern
  - [Fact] 與 [Theory] 的使用
  - 基本斷言方法
- 實作練習：為簡單的 Calculator 類別寫測試

**Day 3：命名規範**
- 技能：`dotnet-testing-test-naming-conventions`
- 學習重點：
  - 三段式命名法
  - 中文命名建議
  - 測試類別命名
- 實作練習：重新命名 Day 1-2 的測試

**Day 4-5：建立測試專案**
- 技能：`dotnet-testing-xunit-project-setup`
- 學習重點：
  - 專案結構規劃
  - .csproj 設定
  - NuGet 套件管理
  - xunit.runner.json 設定
- 實作練習：為現有專案建立測試專案

#### 階段 2：提升品質（Day 6-10）

**Day 6-7：流暢斷言**
- 技能：`dotnet-testing-awesome-assertions-guide`
- 學習重點：
  - AwesomeAssertions 基礎
  - 集合斷言
  - 例外斷言
  - 物件比對
- 實作練習：改寫之前的測試，使用 AwesomeAssertions

**Day 8：測試輸出**
- 技能：`dotnet-testing-test-output-logging`
- 學習重點：
  - ITestOutputHelper 使用
  - 整合 ILogger
  - 除錯技巧
- 實作練習：為複雜測試加入輸出

**Day 9-10：處理依賴**
- 技能：`dotnet-testing-nsubstitute-mocking`
- 學習重點：
  - Mock vs Stub vs Spy
  - 基本模擬設定
  - 驗證呼叫
  - Returns 和 Throws
- 實作練習：測試有 Repository 依賴的 Service

#### 階段 3：自動化測試資料（Day 11-14）

**Day 11-12：AutoFixture 基礎**
- 技能：`dotnet-testing-autofixture-basics`
- 學習重點：
  - 匿名測試資料
  - Create 和 CreateMany
  - 減少測試樣板程式碼
- 實作練習：使用 AutoFixture 簡化測試

**Day 13-14：整合應用**
- 技能：
  - `dotnet-testing-autodata-xunit-integration`
  - `dotnet-testing-autofixture-nsubstitute-integration`
- 學習重點：
  - AutoData 屬性
  - 自動建立 Mock
  - 組合使用
- 實作練習：綜合應用 AutoFixture + NSubstitute

#### 階段 4：總結與實戰（Day 15）

- 為一個小型專案建立完整測試
- 應用所學的所有技能
- 設定 Code Coverage

---

### 進階路徑（2-3 週）

**前置條件**：完成新手路徑

#### 第一週：測試資料專精

**Day 1-2：AutoFixture 進階**
- 技能：`dotnet-testing-autofixture-customization`
- 學習重點：
  - Customizations
  - Specimens
  - Behaviors
  - 自訂生成規則

**Day 3-4：擬真資料**
- 技能：`dotnet-testing-bogus-fake-data`
- 學習重點：
  - Faker 使用
  - 真實感資料生成
  - 本地化資料

**Day 5：Builder Pattern**
- 技能：`dotnet-testing-test-data-builder-pattern`
- 學習重點：
  - 建立 Test Data Builder
  - Fluent Interface
  - 預設值設定

**Day 6-7：整合應用**
- 技能：`dotnet-testing-autofixture-bogus-integration`
- 學習重點：
  - 整合 AutoFixture 與 Bogus
  - 選擇合適的工具

#### 第二週：特殊場景處理

**Day 1-2：時間測試**
- 技能：`dotnet-testing-datetime-testing-timeprovider`
- 學習重點：
  - TimeProvider 抽象化
  - FakeTimeProvider 使用
  - 時區處理

**Day 3-4：檔案系統測試**
- 技能：`dotnet-testing-filesystem-testing-abstractions`
- 學習重點：
  - IFileSystem 介面
  - MockFileSystem 使用
  - 檔案操作測試

**Day 5-6：複雜物件比對**
- 技能：`dotnet-testing-complex-object-comparison`
- 學習重點：
  - 深層物件比對
  - 排除特定屬性
  - 自訂比對規則

**Day 7：私有成員測試**
- 技能：`dotnet-testing-private-internal-testing`
- 學習重點：
  - InternalsVisibleTo
  - 測試策略
  - 何時應避免

#### 第三週：品質與整合

**Day 1-2：驗證測試**
- 技能：`dotnet-testing-fluentvalidation-testing`
- 學習重點：
  - TestHelper 使用
  - 驗證規則測試
  - 錯誤訊息驗證

**Day 3-4：程式碼覆蓋率**
- 技能：`dotnet-testing-code-coverage-analysis`
- 學習重點：
  - Coverlet 使用
  - 報告產生
  - CI/CD 整合

**Day 5-7：綜合實戰**
- 為中型專案建立完整測試
- 應用所有學到的技能
- 建立測試最佳實踐文件

---

### 快速參考：我該先學什麼？

**我是新手，完全沒寫過測試**
→ 新手路徑階段 1：unit-test-fundamentals → test-naming-conventions → xunit-project-setup

**我會寫基礎測試，想提升品質**
→ 新手路徑階段 2：awesome-assertions-guide → nsubstitute-mocking

**我想提升測試效率**
→ 進階路徑第一週：測試資料生成系列

**我遇到特殊場景（時間、檔案系統）**
→ 進階路徑第二週：特殊場景處理系列

**我想評估測試品質**
→ code-coverage-analysis
