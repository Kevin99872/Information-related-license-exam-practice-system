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

共 **203 題、411 組測試資料**，難度分佈：簡單 96、中等 87、困難 20。

其中 CPE001–CPE023 為第一批題目（全數改編自 UVa 題號）；CPE024–CPE053 為第二批新增題目，除延續 UVa 一顆星經典題之外，也加入數論、字串處理、堆疊、貪心與基礎動態規劃（DP）等主題。CPE054–CPE103 為第三批新增題目：CPE054–CPE077 改編自「CPE 一顆星選集 49 題」中尚未收錄的題號，CPE078–CPE103 為原創題目。CPE104–CPE153 為第四批新增題目：CPE104–CPE124 改編自其他經典 UVa 題號（如 The Blocks Problem、Graph Connectivity、LC-Display 等），CPE125–CPE153 為原創題目，涵蓋 BFS/DFS、矩陣運算、字串演算法、貪心與動態規劃等主題。CPE154–CPE203 為第五批新增題目：CPE154–CPE169 改編自其他經典 UVa 題號（如 Unix ls、Palindromes、ShellSort 等），CPE170–CPE203 為原創題目，涵蓋圖論（Dijkstra、拓樸排序、最小生成樹）、雙指標、位元運算與更多動態規劃主題。所有改編題目皆直接從 UVa 官方題目卷 PDF (`onlinejudge.org/external/`) 抓取原文後翻譯改編。凡標註「原創」者為原創題目（非改編自特定 UVa 題號，但主題取材自公開的經典演算法概念）。第二批之後的題目皆在 `.txt` 中附上 `== 參考解法 (Python) ==` 區塊，並經實際執行驗證，對應匯入後 CSV 的 `SolutionCode` 欄位。

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
| CPE054 | 118 | Mutant Flatworld Explorers | 困難 |
| CPE055 | 299 | Train Swapping | 中等 |
| CPE056 | 490 | Rotating Sentences | 中等 |
| CPE057 | 10019 | Funny Encryption Method | 簡單 |
| CPE058 | 10050 | Hartals | 中等 |
| CPE059 | 10056 | What is the Probability? | 中等 |
| CPE060 | 10057 | A mid-summer night's dream | 困難 |
| CPE061 | 10062 | Tell me the frequencies! | 簡單 |
| CPE062 | 10101 | Bangla Numbers | 困難 |
| CPE063 | 10170 | The Hotel with Infinite Rooms | 困難 |
| CPE064 | 10189 | Minesweeper | 中等 |
| CPE065 | 10193 | All You Need Is Love | 中等 |
| CPE066 | 10221 | Satellites | 中等 |
| CPE067 | 10222 | Decode the Mad man | 中等 |
| CPE068 | 10226 | Hardwood Species | 簡單 |
| CPE069 | 10242 | Fourth Point!! | 中等 |
| CPE070 | 10268 | 498-bis | 簡單 |
| CPE071 | 10409 | Die Game | 中等 |
| CPE072 | 10415 | Eb Alto Saxophone Player | 中等 |
| CPE073 | 10908 | Largest Square | 困難 |
| CPE074 | 11005 | Cheapest Base | 困難 |
| CPE075 | 11063 | B2-Sequence | 簡單 |
| CPE076 | 11150 | Cola | 中等 |
| CPE077 | 11321 | Sort! Sort!! And Sort!!! | 困難 |
| CPE078 | 原創 | Digital Root | 簡單 |
| CPE079 | 原創 | Prime Count in Range | 簡單 |
| CPE080 | 原創 | GCD and LCM of an Array | 簡單 |
| CPE081 | 原創 | Two Sum Pair Count | 簡單 |
| CPE082 | 原創 | Kadane's Maximum Subarray | 中等 |
| CPE083 | 原創 | Merge Intervals | 中等 |
| CPE084 | 原創 | Spiral Matrix Traversal | 中等 |
| CPE085 | 原創 | Matrix Transpose | 簡單 |
| CPE086 | 原創 | Circular Queue Simulation | 中等 |
| CPE087 | 原創 | Next Greater Element | 中等 |
| CPE088 | 原創 | Roman Numeral to Integer | 簡單 |
| CPE089 | 原創 | Integer to Roman Numeral | 簡單 |
| CPE090 | 原創 | Morse Code Decoder | 簡單 |
| CPE091 | 原創 | ROT13 Cipher | 簡單 |
| CPE092 | 原創 | Set Union and Intersection | 簡單 |
| CPE093 | 原創 | Longest Common Subsequence | 中等 |
| CPE094 | 原創 | Subset Sum Existence | 中等 |
| CPE095 | 原創 | Tower of Hanoi | 簡單 |
| CPE096 | 原創 | Kth Largest Element | 簡單 |
| CPE097 | 原創 | Counting Sort | 簡單 |
| CPE098 | 原創 | Fixed Window Maximum Sum | 簡單 |
| CPE099 | 原創 | Leap Year and Day of Year | 簡單 |
| CPE100 | 原創 | 12-Hour to 24-Hour Time | 簡單 |
| CPE101 | 原創 | Pascal's Triangle Row | 簡單 |
| CPE102 | 原創 | Modular Exponentiation | 簡單 |
| CPE103 | 原創 | Binary Search Insert Position | 簡單 |
| CPE104 | 101 | The Blocks Problem | 困難 |
| CPE105 | 459 | Graph Connectivity | 中等 |
| CPE106 | 468 | Key to Success | 中等 |
| CPE107 | 483 | Word Scramble | 簡單 |
| CPE108 | 573 | The Snail | 中等 |
| CPE109 | 624 | CD | 困難 |
| CPE110 | 706 | LC-Display | 困難 |
| CPE111 | 10025 | The ?1?2?...?n=k problem | 中等 |
| CPE112 | 10474 | Where is the Marble? | 簡單 |
| CPE113 | 10905 | Children's Game | 簡單 |
| CPE114 | 10921 | Find the Telephone | 簡單 |
| CPE115 | 10925 | Krakovia | 中等 |
| CPE116 | 10935 | Throwing cards away I | 簡單 |
| CPE117 | 10970 | Big Chocolate | 簡單 |
| CPE118 | 11044 | Searching for Nessy | 簡單 |
| CPE119 | 11364 | Optimal Parking | 簡單 |
| CPE120 | 11384 | Help is needed for Dexter | 簡單 |
| CPE121 | 11498 | Division of Nlogonia | 中等 |
| CPE122 | 11559 | Event Planning | 中等 |
| CPE123 | 11729 | Commando War | 中等 |
| CPE124 | 11039 | Building designing | 困難 |
| CPE125 | 原創 | Grid Maze Shortest Path | 中等 |
| CPE126 | 原創 | Island Count | 中等 |
| CPE127 | 原創 | Anagram Pair Check | 簡單 |
| CPE128 | 原創 | Palindrome Check | 簡單 |
| CPE129 | 原創 | Vowel and Consonant Count | 簡單 |
| CPE130 | 原創 | Most Frequent Word | 簡單 |
| CPE131 | 原創 | Generalized Caesar Cipher | 簡單 |
| CPE132 | 原創 | Binary to Decimal | 簡單 |
| CPE133 | 原創 | Decimal to Custom Base | 簡單 |
| CPE134 | 原創 | Prime Factorization | 簡單 |
| CPE135 | 原創 | Sum and Count of Divisors | 簡單 |
| CPE136 | 原創 | Trailing Zeros in Factorial | 簡單 |
| CPE137 | 原創 | Happy Number Check | 簡單 |
| CPE138 | 原創 | Matrix Addition | 簡單 |
| CPE139 | 原創 | Matrix Multiplication | 中等 |
| CPE140 | 原創 | Determinant of a Square Matrix | 困難 |
| CPE141 | 原創 | Anagram Palindrome Check | 簡單 |
| CPE142 | 原創 | Run-Length Decode | 簡單 |
| CPE143 | 原創 | String Rotation Check | 簡單 |
| CPE144 | 原創 | Longest Palindromic Prefix | 簡單 |
| CPE145 | 原創 | Edit Distance | 中等 |
| CPE146 | 原創 | Unique Paths in a Grid | 簡單 |
| CPE147 | 原創 | Minimum Coins for Change | 中等 |
| CPE148 | 原創 | Job Sequencing with Deadlines | 中等 |
| CPE149 | 原創 | Fractional Knapsack | 中等 |
| CPE150 | 原創 | Minimum Cost to Merge Piles | 中等 |
| CPE151 | 原創 | Nth Catalan Number | 簡單 |
| CPE152 | 原創 | nCr mod p | 簡單 |
| CPE153 | 原創 | Nth Fibonacci mod m | 簡單 |
| CPE154 | 400 | Unix ls | 困難 |
| CPE155 | 401 | Palindromes | 中等 |
| CPE156 | 455 | Periodic Strings | 簡單 |
| CPE157 | 458 | The Decoder | 簡單 |
| CPE158 | 465 | Overflow | 中等 |
| CPE159 | 494 | Kindergarten Counting Game | 簡單 |
| CPE160 | 499 | What's The Frequency, Kenneth? | 中等 |
| CPE161 | 1225 | Digit Counting | 簡單 |
| CPE162 | 1585 | Score | 簡單 |
| CPE163 | 10152 | ShellSort | 困難 |
| CPE164 | 10370 | Above Average | 簡單 |
| CPE165 | 10424 | Love Calculator | 中等 |
| CPE166 | 10763 | Foreign Exchange | 簡單 |
| CPE167 | 11078 | Open Credit System | 簡單 |
| CPE168 | 11340 | Newspaper | 簡單 |
| CPE169 | 12100 | Printer Queue | 簡單 |
| CPE170 | 原創 | Count Inversions (Merge Sort) | 中等 |
| CPE171 | 原創 | Bubble Sort Pass Count | 簡單 |
| CPE172 | 原創 | Binary Search Exact Match | 簡單 |
| CPE173 | 原創 | Longest Substring Without Repeating Characters | 中等 |
| CPE174 | 原創 | Container With Most Water | 中等 |
| CPE175 | 原創 | 3Sum Triple Count | 中等 |
| CPE176 | 原創 | Minimum Insertions to Balance Parentheses | 中等 |
| CPE177 | 原創 | Prefix Expression Evaluation | 中等 |
| CPE178 | 原創 | Linked List Cycle Detection | 中等 |
| CPE179 | 原創 | Dijkstra Shortest Path | 中等 |
| CPE180 | 原創 | Topological Sort | 中等 |
| CPE181 | 原創 | Minimum Spanning Tree (Kruskal) | 中等 |
| CPE182 | 原創 | Climbing Stairs Ways | 簡單 |
| CPE183 | 原創 | Word Break | 中等 |
| CPE184 | 原創 | Maximum Product Subarray | 中等 |
| CPE185 | 原創 | Trapping Rain Water | 中等 |
| CPE186 | 原創 | Best Time to Buy and Sell Stock | 簡單 |
| CPE187 | 原創 | Majority Element | 簡單 |
| CPE188 | 原創 | Move Zeroes to End | 簡單 |
| CPE189 | 原創 | Rotate Array by K | 簡單 |
| CPE190 | 原創 | Missing Number in 1..N | 簡單 |
| CPE191 | 原創 | Find Duplicate Number | 簡單 |
| CPE192 | 原創 | Power Set Generation | 簡單 |
| CPE193 | 原創 | Next Permutation | 中等 |
| CPE194 | 原創 | Diagonal Matrix Traversal | 中等 |
| CPE195 | 原創 | Sudoku Block Validity Check | 中等 |
| CPE196 | 原創 | Magic Square Validation | 簡單 |
| CPE197 | 原創 | Nth Ugly Number | 簡單 |
| CPE198 | 原創 | Longest Consecutive Sequence | 中等 |
| CPE199 | 原創 | Maximum Overlapping Intervals | 中等 |
| CPE200 | 原創 | Water Jug Problem | 中等 |
| CPE201 | 原創 | Single Number (XOR Trick) | 簡單 |
| CPE202 | 原創 | Permutation in String | 中等 |
| CPE203 | 原創 | Longest Common Substring | 中等 |
