# CPE 題庫 (大學程式能力檢定)

本資料夾收錄 **CPE（Collegiate Programming Examination，大學程式能力檢定，國立中山大學主辦）** 的經典練習題。

## 來源與說明

- CPE 的考題全數出自 [UVa Online Judge](https://onlinejudge.org/)，本題庫改編自 CPE 必考的 **UVa「一顆星」經典題集**，涵蓋輸入輸出、字串處理、數學、質數、進位制、模擬等基礎主題。
- 每題都附有**參考解並實際執行驗證**，因此 `TestInput` 與 `ExpectedOutput` 的對應關係保證正確，可直接用於本系統的自動判題。
- 題目敘述以繁體中文重新撰寫，並標註對應的 UVa 題號，方便對照原題與延伸練習。

> 這些是廣為流傳的公開教學題目。若要準備正式 CPE 考試，建議至 [中山大學 CPE 官網](https://cpe.cse.nsysu.edu.tw/) 練習完整題庫。

## 檔案內容

| 檔案 | 說明 |
|------|------|
| `CPE-problems.csv` | 可直接匯入本系統的題庫檔（UTF-8，無 BOM）。每一列為一組測試資料，相同 `ProblemCode` 的列會合併成同一題。 |
| `CPExxx.txt` | 各題的人類可讀版本：題目敘述 + 範例輸入 / 輸出。 |

共 **23 題、46 組測試資料**，難度分佈：簡單 8、中等 13、困難 2。

## 如何匯入本系統

1. 開啟應用程式，左側選單點選「匯入」。
2. 切換到「表單匯入」分頁。
3. 點「選擇 CSV / XLS / XLSX」，選擇本資料夾的 `CPE-problems.csv`。
4. 點「預覽 / 驗證」確認無誤後，點「匯入 / 更新」。

匯入後即可在「已載入題庫」與「題目」頁面看到 `ExamType = CPE` 的題目。

## 題目清單

| 題號 | UVa | 標題 | 難度 |
|------|-----|------|------|
| CPE001 | 100 | The 3n + 1 Problem | 中等 |
| CPE002 | 10055 | Hashmat the Brave Warrior | 簡單 |
| CPE003 | 10071 | Back to High School Physics | 簡單 |
| CPE004 | 10812 | Beat the Spread! | 簡單 |
| CPE005 | 10041 | Vito's Family | 中等 |
| CPE006 | 10035 | Primary Arithmetic | 中等 |
| CPE007 | 10929 | You Can Say 11 | 中等 |
| CPE008 | 10783 | Odd Sum | 簡單 |
| CPE009 | 11332 | Summing Digits | 簡單 |
| CPE010 | 10038 | Jolly Jumpers | 中等 |
| CPE011 | 10252 | Common Permutation | 中等 |
| CPE012 | 272 | TeX Quotes | 簡單 |
| CPE013 | 10093 | An Easy Problem! | 困難 |
| CPE014 | 11461 | Square Numbers | 簡單 |
| CPE015 | 10931 | Parity | 簡單 |
| CPE016 | 10235 | Simply Emirp | 中等 |
| CPE017 | 10008 | What's Cryptanalysis? | 中等 |
| CPE018 | 10420 | List of Conquests | 中等 |
| CPE019 | 11417 | GCD | 中等 |
| CPE020 | 10922 | 2 the 9s | 中等 |
| CPE021 | 948 | Fibonaccimal Base | 困難 |
| CPE022 | 10190 | Divide, But Not Quite Conquer! | 中等 |
| CPE023 | 12019 | Doom's Day Algorithm | 中等 |
