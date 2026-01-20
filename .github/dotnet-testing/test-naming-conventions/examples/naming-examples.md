# 測試命名範例集

本文件收集各種測試情境的命名範例，供參考與複製使用。

---

## 1. 基本運算類測試

### Calculator 相關

```csharp
// 加法
Add_輸入1和2_應回傳3
Add_輸入負數和正數_應回傳正確結果
Add_輸入0和0_應回傳0
Add_輸入各種數值組合_應回傳正確結果

// 除法
Divide_輸入10和2_應回傳5
Divide_輸入10和0_應拋出DivideByZeroException
Divide_輸入各種有效數值_應回傳正確結果

// 乘法
Multiply_輸入3和4_應回傳12
Multiply_輸入各種數值組合_應回傳正確結果
```

---

## 2. 驗證類測試

### Email 驗證

```csharp
// 有效輸入
IsValidEmail_輸入有效Email_應回傳True
IsValidEmail_輸入有效Email格式_應回傳True

// 無效輸入
IsValidEmail_輸入null值_應回傳False
IsValidEmail_輸入空字串_應回傳False
IsValidEmail_輸入只有空白字元_應回傳False
IsValidEmail_輸入無效Email格式_應回傳False

// 網域提取
GetDomain_輸入有效Email_應回傳網域名稱
GetDomain_輸入無效Email_應回傳null
GetDomain_輸入null_應回傳null
GetDomain_輸入各種有效Email_應回傳對應網域
```

### 密碼驗證

```csharp
IsValidPassword_輸入符合規則的密碼_應回傳True
IsValidPassword_輸入少於8字元_應回傳False
IsValidPassword_輸入無大寫字母_應回傳False
IsValidPassword_輸入無數字_應回傳False
```

---

## 3. 業務邏輯測試

### Order 相關

```csharp
// 處理訂單
ProcessOrder_輸入有效訂單_應回傳處理後訂單
ProcessOrder_輸入null_應拋出ArgumentNullException
ProcessOrder_多次呼叫_每次都應該回傳新的物件實例

// 訂單號碼
GetOrderNumber_輸入有效訂單_應回傳格式化訂單號碼
GetOrderNumber_輸入null_應拋出ArgumentNullException
GetOrderNumber_輸入各種前綴和號碼組合_應回傳正確格式
```

### 價格計算

```csharp
// 折扣計算
Calculate_輸入100元和10Percent折扣_應回傳90元
Calculate_輸入負數價格_應拋出ArgumentException
Calculate_輸入無效折扣比例_應拋出ArgumentException
Calculate_輸入各種有效組合_應回傳正確結果
Calculate_輸入0元價格_應正常處理

// 含稅計算
CalculateWithTax_輸入100元和5Percent稅率_應回傳105元
CalculateWithTax_輸入負數價格_應拋出ArgumentException
CalculateWithTax_輸入負數稅率_應拋出ArgumentException
CalculateWithTax_輸入各種有效組合_應回傳正確結果
CalculateWithTax_輸入0元價格_應正常處理
```

---

## 4. 狀態變化測試

### Counter 相關

```csharp
// 遞增
Increment_從0開始_應回傳1
Increment_從0開始連續兩次_應回傳2
Increment_多次執行_應產生一致結果

// 遞減
Decrement_從0開始_應回傳負1
Decrement_從正數開始_應正確減少

// 重設
Reset_從任意值_應回傳0

// 設值
SetValue_輸入任意值_應設定正確數值
```

---

## 5. 集合操作測試

```csharp
// 新增
Add_新增項目_集合應包含該項目
Add_新增重複項目_應拋出InvalidOperationException

// 移除
Remove_移除存在項目_應回傳True
Remove_移除不存在項目_應回傳False

// 查詢
Find_輸入存在的Id_應回傳對應項目
Find_輸入不存在的Id_應回傳null
FindAll_輸入條件_應回傳符合條件的所有項目

// 計數
Count_集合為空_應回傳0
Count_集合有3個項目_應回傳3
```

---

## 6. 非同步操作測試

```csharp
// 取得資料
GetAsync_輸入有效Id_應回傳對應資料
GetAsync_輸入不存在Id_應回傳null
GetAllAsync_無資料時_應回傳空集合

// 儲存資料
SaveAsync_輸入有效實體_應成功儲存
SaveAsync_輸入null_應拋出ArgumentNullException

// 刪除資料
DeleteAsync_輸入存在Id_應成功刪除
DeleteAsync_輸入不存在Id_應回傳False
```

---

## 7. 例外處理測試命名

```csharp
// ArgumentNullException
MethodName_輸入null_應拋出ArgumentNullException

// ArgumentException
MethodName_輸入無效參數_應拋出ArgumentException
MethodName_輸入負數_應拋出ArgumentException

// InvalidOperationException
MethodName_狀態不正確時呼叫_應拋出InvalidOperationException

// 自訂例外
MethodName_業務規則違反_應拋出BusinessRuleException

// 驗證例外訊息
MethodName_輸入無效_應拋出包含正確訊息的Exception
```

---

## 8. Theory 測試命名

```csharp
// 多組有效輸入
MethodName_輸入各種有效值_應回傳正確結果
MethodName_輸入各種有效組合_應正常處理

// 多組無效輸入
MethodName_輸入各種無效值_應拋出Exception
MethodName_輸入各種無效格式_應回傳False

// 對應關係測試
MethodName_輸入各種A值_應回傳對應B值
GetDomain_輸入各種有效Email_應回傳對應網域
```

---

## 命名模板

複製以下模板，替換 `{placeholder}` 即可使用：

```csharp
// 正常路徑
{Method}_輸入{ValidInput}_應回傳{ExpectedResult}

// Null 輸入
{Method}_輸入null_應拋出ArgumentNullException

// 空值輸入
{Method}_輸入空字串_應回傳{ExpectedResult}

// 邊界條件
{Method}_輸入{BoundaryValue}_應{ExpectedBehavior}

// 例外情況
{Method}_輸入{InvalidInput}_應拋出{ExceptionType}

// 狀態變化
{Method}_從{InitialState}開始_應{ExpectedState}

// Theory 測試
{Method}_輸入各種{InputType}_應回傳{ExpectedPattern}
```
