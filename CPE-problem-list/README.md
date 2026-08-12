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

共 **53 題、111 組測試資料**，難度分佈：簡單 20、中等 27、困難 6。

其中 CPE001–CPE023 為第一批題目（全數改編自 UVa 題號）；CPE024–CPE053 為第二批新增題目，除延續 UVa 一顆星經典題之外，也加入數論、字串處理、堆疊、貪心與基礎動態規劃（DP）等主題，凡標註「原創」者為原創題目（非改編自特定 UVa 題號，但主題取材自公開的經典演算法概念）。第二批題目另外在 `.txt` 中附上 `== 參考解法 (Python) ==` 區塊，對應匯入後 CSV 的 `SolutionCode` 欄位。

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
| CPE024 | 10018 | Reverse and Add | 簡單 |
| CPE025 | 543 | Goldbach's Conjecture | 中等 |
| CPE026 | 原創 | Strong Number | 簡單 |
| CPE027 | 原創 | Simplifying Fractions | 簡單 |
| CPE028 | 10082 | WERTYU | 簡單 |
| CPE029 | 原創 | Shift Cipher | 中等 |
| CPE030 | 591 | Box of Bricks | 簡單 |
| CPE031 | 10924 | Prime Words | 簡單 |
| CPE032 | 11172 | Relational Operators | 簡單 |
| CPE033 | 10696 | f91 | 中等 |
| CPE034 | 10346 | Peter's Smokes | 中等 |
| CPE035 | 原創 | Breaking Chocolate | 簡單 |
| CPE036 | 10920 | Spiral Tap | 中等 |
| CPE037 | 原創 | Base Conversion | 中等 |
| CPE038 | 10061 | How Many Zeros and How Many Digits? | 困難 |
| CPE039 | 原創 | Anagram Groups | 中等 |
| CPE040 | 原創 | Run-Length Encoding | 簡單 |
| CPE041 | 原創 | Josephus Problem | 中等 |
| CPE042 | 原創 | Balanced Brackets | 中等 |
| CPE043 | 原創 | Longest Common Prefix | 簡單 |
| CPE044 | 原創 | Postfix Evaluation | 中等 |
| CPE045 | 原創 | Twin Primes | 中等 |
| CPE046 | 原創 | Perfect Number | 簡單 |
| CPE047 | 原創 | Narcissistic Numbers | 簡單 |
| CPE048 | 原創 | Matrix Rotation | 中等 |
| CPE049 | 原創 | Longest Increasing Subsequence | 困難 |
| CPE050 | 原創 | 0/1 Knapsack | 困難 |
| CPE051 | 原創 | Coin Change | 中等 |
| CPE052 | 原創 | Activity Selection | 中等 |
| CPE053 | 原創 | Longest Palindromic Substring | 困難 |
