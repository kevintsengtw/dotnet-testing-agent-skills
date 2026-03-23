# Regression gates

這份文件說明 skill 在 benchmark 後如何用機械式門檻決定是否可發版。

## 為什麼要 gates

如果沒有門檻，benchmark 很容易變成「看報表感覺還行」。

## 常見 gates

- `min_pass_rate_delta`
  - with-skill 的平均 pass rate 至少要比 baseline 好多少
- `max_time_increase_seconds`
  - 平均耗時最多可增加多少秒
- `max_token_increase`
  - 平均 token 最多可增加多少
- `require_non_negative_pass_rate`
  - 不允許 with-skill 比 baseline 更差

## 設定原則

1) 先保守，再收緊
- 一開始可以只要求不退步，之後再要求有顯著提升。

2) 對高成本 skill 設時間與 token 上限
- 否則容易用大量成本換小幅提升。

3) 對高風險 skill 設更嚴格 pass rate 門檻
- 例如法律、醫療、金流、資料轉換。

## 建議檔案

把門檻寫進：
- `assets/evals/regression_gates.json`

再用：

```bash
python scripts/check_regression_gates.py <benchmark.json> --config assets/evals/regression_gates.json
```

## 不要犯的錯

- benchmark 跑完才臨時想門檻
- 每次改版都換門檻，導致無法比較
- 只看 pass rate，不看成本與維護負擔
