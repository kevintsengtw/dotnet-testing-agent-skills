# Skill boundary management

這份文件處理多 skill 之間的邊界、衝突與 handoff。

## 為什麼重要

真實 over-trigger 很多不是因為 skill 太通用，而是因為兩個 skill 都像自己該接。

## Overlap matrix

對每個 skill，列出：
- 最接近的 2-5 個 skill 或 workflow
- 哪些 query 應由本 skill 接
- 哪些 query 應由別的 skill 接
- handoff 條件

範例欄位：
- Neighbor
- Shared vocabulary
- This skill wins when
- Neighbor wins when
- Negative trigger to add

## 設計原則

1) 先做 scope，後做 wording
- 如果 in-scope / out-of-scope 不清楚，description 再怎麼改都會互相搶。

2) 盡量用 outcome 分界
- 比起用工具名分界，用「使用者想完成什麼」通常更穩。

3) 需要時明示 handoff
- 若兩個 skill 都可能相關，先由哪個 skill 接，再在流程中 hand off。

## 何時該拆 skill

- 同一 description 同時造成 under-trigger 和 over-trigger
- 兩組 use cases 沒有共用 workflow
- 一組高自由度，一組高約束
- 兩組 query 只共享少數關鍵詞

## 測試方式

- 每個鄰近 skill 至少準備 2-3 個界線案例
- 對每個案例寫清楚為何本 skill 應接或不應接
- 記錄是否發生 query stealing
