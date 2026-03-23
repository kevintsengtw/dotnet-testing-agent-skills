# 循環複雜度範例與測試策略

## 循環複雜度（Cyclomatic Complexity）

**定義：** 程式中獨立邏輯路徑的數量

**與測試案例的關係：**

- 循環複雜度 = 至少需要的測試案例數量
- 每個 if、for、while、case、&&、|| 都會增加複雜度

## 範例

```csharp
public int Max(int[] array)
{
    if (array == null || array.Length == 0)  // +2 (null 判斷 + 長度判斷)
    {
        throw new ArgumentException("array must not be empty.");
    }

    int max = array[0];

    for (int i = 1; i < array.Length; i++)  // +1 (迴圈)
    {
        if (array[i] > max)  // +1 (條件判斷)
        {
            max = array[i];
        }
    }

    return max;  // +1 (方法本身)
}
// 總複雜度 = 5
```

## 測試策略

循環複雜度為 5，至少需要 5 個測試案例：

1. 傳入 null → 測試 `array == null`
2. 傳入空陣列 → 測試 `array.Length == 0`
3. 單一元素 → 不進入迴圈
4. 最大值在開頭 → 迴圈不更新 max
5. 最大值在中間 → 迴圈更新 max

## Visual Studio 擴充套件

**CodeMaintainability：**

- 顯示可維護性指標
- 計算循環複雜度
- 評估程式碼品質

**CodeMaid：**

- Spade 功能：視覺化程式碼結構
- 顯示每個方法的複雜度
- 幫助識別需要重構的程式碼
