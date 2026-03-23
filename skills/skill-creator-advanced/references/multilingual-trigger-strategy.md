# Multilingual trigger strategy

這份文件說明如何為 skill 做多語言與混合語料的 trigger 設計。

## 為什麼需要

很多 skill 在作者測試時看起來能觸發，但上線後失敗，原因不是 description 本身錯，而是：
- 使用者會中英混用
- 工具名、檔名、deliverable 常保留英文
- 同一件事在不同語言下的說法差很多

## 最低覆蓋

若 skill 面向一般使用者，至少測：
- `zh`：純中文
- `en`：純英文
- `mixed`：中文句子 + 英文工具名 / 檔名 / 功能詞

## 設計原則

1) description 不要只偏單一語言
- 如果常見工具名是英文，就不要只寫中文意圖詞。

2) 用真實說法，不要只做翻譯
- 「做 benchmark」和「跑基準測試」都可能出現。

3) 測縮寫與俗稱
- 例如 ROI、CI、PDF、Slack、MCP。

4) 注意地區口語
- 同樣是中文，不同地區的常用詞未必相同。

## 測試矩陣

每個主要 use case 至少準備：
- 1 個中文 prompt
- 1 個英文 prompt
- 1 個 mixed prompt

## 常見失敗

- 中文能觸發，但 mixed prompt 失敗
- 只要有英文工具名就誤觸發其他 skill
- description 塞太多雙語詞，結果過寬而 over-trigger

## 修法

- 補常見 mixed trigger phrases
- 把容易和別的 skill 重疊的跨語言詞做 negative triggers
- 若 description 已過寬，先收 scope，再補 multilingual wording
