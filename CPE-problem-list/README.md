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

共 **500 題、982 組測試資料**，難度分佈：簡單 200、中等 255、困難 45。

其中 CPE001–CPE023 為第一批題目（全數改編自 UVa 題號）；CPE024–CPE053 為第二批新增題目，除延續 UVa 一顆星經典題之外，也加入數論、字串處理、堆疊、貪心與基礎動態規劃（DP）等主題。CPE054–CPE103 為第三批新增題目：CPE054–CPE077 改編自「CPE 一顆星選集 49 題」中尚未收錄的題號，CPE078–CPE103 為原創題目。CPE104–CPE153 為第四批新增題目：CPE104–CPE124 改編自其他經典 UVa 題號（如 The Blocks Problem、Graph Connectivity、LC-Display 等），CPE125–CPE153 為原創題目，涵蓋 BFS/DFS、矩陣運算、字串演算法、貪心與動態規劃等主題。CPE154–CPE203 為第五批新增題目：CPE154–CPE169 改編自其他經典 UVa 題號（如 Unix ls、Palindromes、ShellSort 等），CPE170–CPE203 為原創題目，涵蓋圖論（Dijkstra、拓樸排序、最小生成樹）、雙指標、位元運算與更多動態規劃主題。CPE204–CPE253 為第六批新增題目：CPE204–CPE218 改編自其他經典 UVa 題號（如 500!、Biorhythms、Friends、Modular Fibonacci 等），CPE219–CPE253 為原創題目，涵蓋回溯法（N 皇后）、更多動態規劃（雞蛋掉落、切割鋼條、交錯字串）、數論（尤拉函數、中國剩餘定理、擴展歐幾里得）與貪心演算法。CPE254–CPE303 為第七批新增題目：CPE254–CPE263 改編自其他經典 UVa 題號（如 Anagram、Matrix Chain Multiplication、Rails、Potentiometers 等），CPE264–CPE303 為原創題目，涵蓋字串比對（KMP、Z-Algorithm）、計算幾何（凸包、最近點對、多邊形面積）、資料結構設計（LRU Cache、Min Stack、Trie）與更多回溯法主題。CPE304–CPE353 為第八批新增題目：CPE304–CPE311 改編自其他經典 UVa 題號（如 Sum It Up、Game of Sum、The Dragon of Loowater、Prerequisites? 等），CPE312–CPE353 為原創題目，涵蓋進階圖論（Floyd-Warshall、Bellman-Ford、Tarjan 割點、強連通分量）、進階資料結構（線段樹、稀疏表）與更多動態規劃（最大正方形、解碼方式、完全平方數）主題。CPE354–CPE403 為第九批新增題目：CPE354–CPE358 改編自其他經典 UVa 題號（如 Ants、Fire!、A Walk Through the Forest 等），CPE359–CPE403 為原創題目，涵蓋網路流（最大流、二分圖匹配）、二元樹系列（BST 驗證/刪除/第 K 小、鏡像判斷、路徑總和）與更多貪心、回溯法主題。CPE404–CPE453 為第十批新增題目：CPE404–CPE409 改編自其他經典 UVa 題號（如 Rare Order、Dark roads、Master-Mind Hints 等），CPE410–CPE453 為原創題目，涵蓋動態規劃還原解（LIS/LCS/背包還原實際解）、更多二元樹系列（層序、寬度、BST 兩數之和）與陣列/字串經典題型（同構字串、樣式比對、第一個缺失正整數）。CPE454–CPE500 為第十一批（最終批）新增題目，將題庫擴充至滿額 500 題：CPE454–CPE456 改編自其他經典 UVa 題號（Is It A Tree?、Secret Research、Pizza Cutting），CPE457–CPE500 為原創題目，涵蓋最短路徑/最小生成樹的路徑與邊還原（Dijkstra、Kruskal）、進階動態規劃（3xN 骨牌鋪磚、最長回文子序列還原、加權區間排程、LIS 計數）、滑動視窗與雙指標系列（乘積小於 K 的子陣列、最大連續 1 的個數 III、最短未排序子陣列）、字串堆疊模擬（巢狀字串解碼、基本計算機、移除括號、退格字串比較）、二元樹系列（路徑總和 II、展開為鏈結串列、層序右指標）、二分搜尋於答案空間（船運能力、可可吃香蕉）與圖論橋接演算法（Tarjan 求橋）等主題。所有改編題目皆直接從 UVa 官方題目卷 PDF (`onlinejudge.org/external/`) 抓取原文後翻譯改編。凡標註「原創」者為原創題目（非改編自特定 UVa 題號，但主題取材自公開的經典演算法概念）。第二批之後的題目皆在 `.txt` 中附上 `== 參考解法 (Python) ==` 區塊，並經實際執行驗證，對應匯入後 CSV 的 `SolutionCode` 欄位。

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
| CPE204 | 424 | Integer Inquiry | 簡單 |
| CPE205 | 484 | The Department of Redundancy Department | 簡單 |
| CPE206 | 495 | Fibonacci Freeze | 簡單 |
| CPE207 | 623 | 500! | 簡單 |
| CPE208 | 756 | Biorhythms | 中等 |
| CPE209 | 993 | Product of digits | 簡單 |
| CPE210 | 1584 | Circular Sequence | 簡單 |
| CPE211 | 10105 | Polynomial coefficients | 簡單 |
| CPE212 | 10161 | Ant on a Chessboard | 中等 |
| CPE213 | 10183 | How many Fibs? | 中等 |
| CPE214 | 10229 | Modular Fibonacci | 困難 |
| CPE215 | 10344 | 23 Out of 5 | 中等 |
| CPE216 | 10608 | Friends | 簡單 |
| CPE217 | 11565 | Simple Equations | 中等 |
| CPE218 | 11827 | Maximum GCD | 簡單 |
| CPE219 | 原創 | N-Queens Count | 困難 |
| CPE220 | 原創 | Bipartite Graph Check | 中等 |
| CPE221 | 原創 | Longest Path in a DAG | 困難 |
| CPE222 | 原創 | Egg Drop Problem | 困難 |
| CPE223 | 原創 | Rod Cutting Problem | 中等 |
| CPE224 | 原創 | Longest Bitonic Subsequence | 中等 |
| CPE225 | 原創 | Interleaving String Check | 中等 |
| CPE226 | 原創 | Wildcard Pattern Matching | 困難 |
| CPE227 | 原創 | Kth Permutation Sequence | 中等 |
| CPE228 | 原創 | Gray Code Generation | 簡單 |
| CPE229 | 原創 | Sum of Two Squares Check | 簡單 |
| CPE230 | 原創 | Integer Square Root | 簡單 |
| CPE231 | 原創 | Extended Euclidean Algorithm | 簡單 |
| CPE232 | 原創 | Chinese Remainder Theorem | 中等 |
| CPE233 | 原創 | Euler's Totient Function | 簡單 |
| CPE234 | 原創 | Longest Wiggle Subsequence | 中等 |
| CPE235 | 原創 | Remove Duplicates from Sorted Array | 簡單 |
| CPE236 | 原創 | Sort Colors (Dutch National Flag) | 簡單 |
| CPE237 | 原創 | Maximum Sum Rectangle in 2D Matrix | 困難 |
| CPE238 | 原創 | Generate Spiral Matrix | 簡單 |
| CPE239 | 原創 | Zigzag Conversion | 簡單 |
| CPE240 | 原創 | Big Number String Multiplication | 簡單 |
| CPE241 | 原創 | Valid Roman Numeral Check | 簡單 |
| CPE242 | 原創 | Excel Column Title | 簡單 |
| CPE243 | 原創 | Excel Column Number | 簡單 |
| CPE244 | 原創 | Reverse Integer with Overflow Check | 簡單 |
| CPE245 | 原創 | Kth Missing Positive Number | 簡單 |
| CPE246 | 原創 | Trie Prefix Count | 中等 |
| CPE247 | 原創 | Longest Palindromic Subsequence | 中等 |
| CPE248 | 原創 | Minimum Path Sum in Grid | 簡單 |
| CPE249 | 原創 | Count Set Bits in Range | 簡單 |
| CPE250 | 原創 | Divisor Count via Sieve | 簡單 |
| CPE251 | 原創 | Longest Arithmetic Subsequence | 中等 |
| CPE252 | 原創 | Candy Distribution | 中等 |
| CPE253 | 原創 | Gas Station Circular Tour | 中等 |
| CPE254 | 156 | Ananagrams | 中等 |
| CPE255 | 195 | Anagram | 中等 |
| CPE256 | 442 | Matrix Chain Multiplication | 中等 |
| CPE257 | 514 | Rails | 中等 |
| CPE258 | 580 | Critical Mass | 簡單 |
| CPE259 | 10820 | Send a Table | 中等 |
| CPE260 | 11029 | Leading and Trailing | 中等 |
| CPE261 | 11991 | Easy Problem from Rujia Liu? | 中等 |
| CPE262 | 12086 | Potentiometers | 中等 |
| CPE263 | 12356 | Army buddies | 中等 |
| CPE264 | 原創 | KMP String Matching | 中等 |
| CPE265 | 原創 | Z-Algorithm Pattern Count | 中等 |
| CPE266 | 原創 | Largest Rectangle in Histogram | 困難 |
| CPE267 | 原創 | Sliding Window Maximum (Deque) | 中等 |
| CPE268 | 原創 | Heap Sort | 簡單 |
| CPE269 | 原創 | Radix Sort | 簡單 |
| CPE270 | 原創 | Selection Sort Minimum Swaps | 簡單 |
| CPE271 | 原創 | Quick Sort Comparison Count | 中等 |
| CPE272 | 原創 | Median of Two Sorted Arrays | 中等 |
| CPE273 | 原創 | Search in Rotated Sorted Array | 中等 |
| CPE274 | 原創 | Find Peak Element | 中等 |
| CPE275 | 原創 | Kth Smallest Element in Sorted Matrix | 中等 |
| CPE276 | 原創 | Matrix Power | 中等 |
| CPE277 | 原創 | LRU Cache Simulation | 中等 |
| CPE278 | 原創 | Huffman Encoding Table | 中等 |
| CPE279 | 原創 | Minimum Size Subarray Sum | 中等 |
| CPE280 | 原創 | Subarray Sum Equals K | 中等 |
| CPE281 | 原創 | Maximum Circular Subarray Sum | 中等 |
| CPE282 | 原創 | Combination Sum | 中等 |
| CPE283 | 原創 | Letter Combinations of a Phone Number | 簡單 |
| CPE284 | 原創 | BST Insert and Inorder Traversal | 簡單 |
| CPE285 | 原創 | Generate Valid Parentheses | 中等 |
| CPE286 | 原創 | Smallest Prime Factor Sieve Factorization | 簡單 |
| CPE287 | 原創 | Convex Hull (Monotone Chain) | 困難 |
| CPE288 | 原創 | Closest Pair of Points | 中等 |
| CPE289 | 原創 | Line Segment Intersection Check | 中等 |
| CPE290 | 原創 | Polygon Area (Shoelace Formula) | 簡單 |
| CPE291 | 原創 | Point in Polygon Test | 中等 |
| CPE292 | 原創 | Rat in a Maze Path Count | 中等 |
| CPE293 | 原創 | Tree Height from Parent Array | 簡單 |
| CPE294 | 原創 | Min Stack | 中等 |
| CPE295 | 原創 | Queue Using Two Stacks | 簡單 |
| CPE296 | 原創 | Infix to Postfix Conversion | 中等 |
| CPE297 | 原創 | Balanced Binary Tree Height Check | 中等 |
| CPE298 | 原創 | Unique Permutations | 中等 |
| CPE299 | 原創 | Modular Inverse (Extended Euclidean) | 簡單 |
| CPE300 | 原創 | Count Numbers Without Repeated Digits | 困難 |
| CPE301 | 原創 | Longest Substring with At Most K Distinct Characters | 中等 |
| CPE302 | 原創 | Conway's Game of Life | 中等 |
| CPE303 | 原創 | Word Search in Grid | 中等 |
| CPE304 | 574 | Sum It Up | 中等 |
| CPE305 | 10465 | Homer Simpson | 中等 |
| CPE306 | 10891 | Game of Sum | 困難 |
| CPE307 | 11292 | The Dragon of Loowater | 簡單 |
| CPE308 | 11054 | Wine trading in Gergovia | 簡單 |
| CPE309 | 11947 | Cancer or Scorpio | 中等 |
| CPE310 | 11258 | String Partition | 中等 |
| CPE311 | 10919 | Prerequisites? | 簡單 |
| CPE312 | 原創 | Segment Tree Range Minimum Query | 中等 |
| CPE313 | 原創 | Sparse Table Range Minimum Query | 中等 |
| CPE314 | 原創 | Miller-Rabin Primality Test | 中等 |
| CPE315 | 原創 | Nim Game | 簡單 |
| CPE316 | 原創 | Longest Repeated Substring | 中等 |
| CPE317 | 原創 | Count Distinct Substrings | 簡單 |
| CPE318 | 原創 | Product of Array Except Self | 簡單 |
| CPE319 | 原創 | Merge Sorted Array In-Place | 簡單 |
| CPE320 | 原創 | Floyd-Warshall All-Pairs Shortest Path | 中等 |
| CPE321 | 原創 | Bellman-Ford with Negative Cycle Detection | 中等 |
| CPE322 | 原創 | Articulation Points (Cut Vertices) | 困難 |
| CPE323 | 原創 | Strongly Connected Components | 困難 |
| CPE324 | 原創 | Eulerian Path/Circuit Existence Check | 中等 |
| CPE325 | 原創 | Diameter of a Tree | 中等 |
| CPE326 | 原創 | Lowest Common Ancestor | 中等 |
| CPE327 | 原創 | Sudoku Solver | 困難 |
| CPE328 | 原創 | Longest Common Subsequence of Three Strings | 中等 |
| CPE329 | 原創 | Palindrome Partitioning Minimum Cuts | 中等 |
| CPE330 | 原創 | Maximum Sum Increasing Subsequence | 中等 |
| CPE331 | 原創 | Find Missing and Duplicate Number | 中等 |
| CPE332 | 原創 | Job Scheduling to Minimize Lateness | 中等 |
| CPE333 | 原創 | Single Number II | 中等 |
| CPE334 | 原創 | Unbounded Knapsack | 中等 |
| CPE335 | 原創 | Longest Increasing Path in a Matrix | 中等 |
| CPE336 | 原創 | Merge K Sorted Arrays | 中等 |
| CPE337 | 原創 | Longest Happy Prefix | 中等 |
| CPE338 | 原創 | Round Robin CPU Scheduling | 中等 |
| CPE339 | 原創 | Minimum Swaps to Sort Array | 中等 |
| CPE340 | 原創 | Best Time to Buy and Sell Stock II | 簡單 |
| CPE341 | 原創 | House Robber | 簡單 |
| CPE342 | 原創 | Binary Tree Zigzag Level Order Traversal | 中等 |
| CPE343 | 原創 | Binary Tree Maximum Path Sum | 中等 |
| CPE344 | 原創 | Word Ladder | 中等 |
| CPE345 | 原創 | Sliding Puzzle Solvability Check | 簡單 |
| CPE346 | 原創 | Distinct Subsequences Matching Target | 中等 |
| CPE347 | 原創 | Next Greater Element (Circular Array) | 中等 |
| CPE348 | 原創 | Remove K Digits to Form Smallest Number | 中等 |
| CPE349 | 原創 | Maximum Gap | 簡單 |
| CPE350 | 原創 | Minimum Window Substring | 困難 |
| CPE351 | 原創 | Maximal Square | 中等 |
| CPE352 | 原創 | Perfect Squares | 中等 |
| CPE353 | 原創 | Decode Ways | 中等 |
| CPE354 | 10714 | Ants | 簡單 |
| CPE355 | 10391 | Compound Words | 中等 |
| CPE356 | 10870 | Recurrences | 困難 |
| CPE357 | 11624 | Fire! | 困難 |
| CPE358 | 10917 | A Walk Through the Forest | 困難 |
| CPE359 | 原創 | Bipartite Matching | 困難 |
| CPE360 | 原創 | Maximum Flow (Edmonds-Karp) | 困難 |
| CPE361 | 原創 | Two-Dimensional Knapsack | 中等 |
| CPE362 | 原創 | Median of a Data Stream | 中等 |
| CPE363 | 原創 | Kth Largest Element in a Stream | 中等 |
| CPE364 | 原創 | Validate Binary Search Tree | 中等 |
| CPE365 | 原創 | Count Leaves in Binary Tree | 簡單 |
| CPE366 | 原創 | Symmetric Tree Check | 中等 |
| CPE367 | 原創 | Invert Binary Tree | 簡單 |
| CPE368 | 原創 | Valid Palindrome Ignoring Non-alphanumeric | 簡單 |
| CPE369 | 原創 | Reverse Words in a String | 簡單 |
| CPE370 | 原創 | Set Matrix Zeroes | 中等 |
| CPE371 | 原創 | Minimum Cost Climbing Stairs | 簡單 |
| CPE372 | 原創 | Paint Fence | 中等 |
| CPE373 | 原創 | Assign Cookies | 簡單 |
| CPE374 | 原創 | Non-overlapping Intervals Removal Count | 中等 |
| CPE375 | 原創 | Super Ugly Number | 中等 |
| CPE376 | 原創 | 3Sum Closest | 中等 |
| CPE377 | 原創 | 4Sum | 中等 |
| CPE378 | 原創 | Palindrome Partitioning (List All Ways) | 中等 |
| CPE379 | 原創 | Combinations (Choose K from N) | 簡單 |
| CPE380 | 原創 | Restore IP Addresses | 中等 |
| CPE381 | 原創 | Longest Palindrome by Character Frequency | 簡單 |
| CPE382 | 原創 | Suffix Array Construction | 中等 |
| CPE383 | 原創 | Range Sum Query - Immutable | 簡單 |
| CPE384 | 原創 | Range Sum Query 2D - Immutable | 中等 |
| CPE385 | 原創 | Rotting Oranges | 中等 |
| CPE386 | 原創 | Flood Fill | 簡單 |
| CPE387 | 原創 | Shortest Path in Binary Matrix | 中等 |
| CPE388 | 原創 | Jump Game | 簡單 |
| CPE389 | 原創 | Jump Game II | 中等 |
| CPE390 | 原創 | Partition Labels | 中等 |
| CPE391 | 原創 | Add Two Numbers (Linked List Simulation) | 簡單 |
| CPE392 | 原創 | Trie with Wildcard Search | 中等 |
| CPE393 | 原創 | Design Circular Deque | 中等 |
| CPE394 | 原創 | Top K Frequent Elements | 中等 |
| CPE395 | 原創 | Sort Characters By Frequency | 簡單 |
| CPE396 | 原創 | Find All Anagrams in a String | 中等 |
| CPE397 | 原創 | Binary Search Tree Delete Node | 困難 |
| CPE398 | 原創 | Kth Smallest Element in a BST | 中等 |
| CPE399 | 原創 | Path Sum in Binary Tree | 簡單 |
| CPE400 | 原創 | Count Complete Tree Nodes | 簡單 |
| CPE401 | 原創 | Minimum Depth of Binary Tree | 簡單 |
| CPE402 | 原創 | Diameter of Binary Tree | 中等 |
| CPE403 | 原創 | Count Good Nodes in Binary Tree | 中等 |
| CPE404 | 200 | Rare Order | 中等 |
| CPE405 | 10281 | Average Speed | 中等 |
| CPE406 | 11631 | Dark roads | 簡單 |
| CPE407 | 10930 | A-Sequence | 中等 |
| CPE408 | 340 | Master-Mind Hints | 中等 |
| CPE409 | 386 | Perfect Cubes | 簡單 |
| CPE410 | 原創 | Longest Increasing Subsequence with Reconstruction | 中等 |
| CPE411 | 原創 | Coin Change with Reconstruction | 中等 |
| CPE412 | 原創 | Edit Distance Operation Breakdown | 中等 |
| CPE413 | 原創 | Longest Common Subsequence Reconstruction | 中等 |
| CPE414 | 原創 | Subset Sum Reconstruction | 中等 |
| CPE415 | 原創 | 0/1 Knapsack Item Reconstruction | 中等 |
| CPE416 | 原創 | Tiling a 2xN Board with Dominoes | 簡單 |
| CPE417 | 原創 | Tribonacci Staircase | 簡單 |
| CPE418 | 原創 | Count Univalue Subtrees | 中等 |
| CPE419 | 原創 | Sum of Left Leaves | 簡單 |
| CPE420 | 原創 | Binary Tree Right Side View | 中等 |
| CPE421 | 原創 | Construct Binary Tree from Preorder and Inorder | 中等 |
| CPE422 | 原創 | Rotate Matrix Layers | 中等 |
| CPE423 | 原創 | Pascal's Triangle (Full) | 簡單 |
| CPE424 | 原創 | Cheapest Flights Within K Stops | 中等 |
| CPE425 | 原創 | Insert Interval | 中等 |
| CPE426 | 原創 | Find Minimum in Rotated Sorted Array | 中等 |
| CPE427 | 原創 | Triangle Minimum Path Sum | 中等 |
| CPE428 | 原創 | Best Time to Buy and Sell Stock with Cooldown | 中等 |
| CPE429 | 原創 | Best Time to Buy and Sell Stock III | 中等 |
| CPE430 | 原創 | Subsets II (With Duplicates) | 中等 |
| CPE431 | 原創 | Boats to Save People | 簡單 |
| CPE432 | 原創 | Task Scheduler | 中等 |
| CPE433 | 原創 | Valid Triangle Number Count | 中等 |
| CPE434 | 原創 | Sort Array By Parity | 簡單 |
| CPE435 | 原創 | Hamming Distance | 簡單 |
| CPE436 | 原創 | Power of Two Check | 簡單 |
| CPE437 | 原創 | Integer to English Words | 困難 |
| CPE438 | 原創 | Text Justification | 困難 |
| CPE439 | 原創 | Redundant Connection | 中等 |
| CPE440 | 原創 | Group Shifted Strings | 中等 |
| CPE441 | 原創 | Binary Tree Level Sums | 簡單 |
| CPE442 | 原創 | Two Sum in BST | 中等 |
| CPE443 | 原創 | Maximum Width of Binary Tree | 中等 |
| CPE444 | 原創 | Minimum Height of Balanced BST from Sorted Array | 簡單 |
| CPE445 | 原創 | Non-decreasing Array Check | 中等 |
| CPE446 | 原創 | Third Maximum Number | 簡單 |
| CPE447 | 原創 | Majority Element II | 中等 |
| CPE448 | 原創 | Plus One | 簡單 |
| CPE449 | 原創 | Move All Negatives to One Side | 簡單 |
| CPE450 | 原創 | Isomorphic Strings | 簡單 |
| CPE451 | 原創 | Word Pattern Matching | 簡單 |
| CPE452 | 原創 | Find All Duplicates in an Array | 簡單 |
| CPE453 | 原創 | First Missing Positive | 中等 |
| CPE454 | 615 | 這是一棵樹嗎？ | 中等 |
| CPE455 | 621 | 祕密研究 | 簡單 |
| CPE456 | 10079 | 切披薩 | 簡單 |
| CPE457 | 原創 | Dijkstra 最短路徑還原 | 中等 |
| CPE458 | 原創 | Kruskal 最小生成樹邊列表 | 中等 |
| CPE459 | 原創 | 3xN 骨牌鋪磚 | 中等 |
| CPE460 | 原創 | 最長回文子序列還原 | 中等 |
| CPE461 | 原創 | 加權區間排程 | 中等 |
| CPE462 | 原創 | 等差數列切片計數 | 簡單 |
| CPE463 | 原創 | 最長遞增子序列個數 | 中等 |
| CPE464 | 原創 | 最後 K 個數的乘積 | 簡單 |
| CPE465 | 原創 | 連續子陣列和能被 K 整除 | 中等 |
| CPE466 | 原創 | 含萬用字元的合法括號字串 | 中等 |
| CPE467 | 原創 | 巢狀字串解碼 | 中等 |
| CPE468 | 原創 | 基本計算機 | 中等 |
| CPE469 | 原創 | 最長震盪子陣列 | 中等 |
| CPE470 | 原創 | 字串的最大公因字串 | 簡單 |
| CPE471 | 原創 | N 皇后輸出所有解 | 困難 |
| CPE472 | 原創 | 正規表示式匹配 | 困難 |
| CPE473 | 原創 | 路徑總和 II | 中等 |
| CPE474 | 原創 | 展開二元樹為鏈結串列 | 簡單 |
| CPE475 | 原創 | 填充每個節點的右側指標 | 中等 |
| CPE476 | 原創 | 擺動排序 | 簡單 |
| CPE477 | 原創 | B 進位數字和 | 簡單 |
| CPE478 | 原創 | 關鍵連線（橋） | 困難 |
| CPE479 | 原創 | 螺旋矩陣 III | 中等 |
| CPE480 | 原創 | 三個數的最大乘積 | 簡單 |
| CPE481 | 原創 | 最接近原點的 K 個點 | 簡單 |
| CPE482 | 原創 | 移除最少括號使字串合法 | 中等 |
| CPE483 | 原創 | 乘積小於 K 的子陣列 | 中等 |
| CPE484 | 原創 | 最大連續 1 的個數 III | 中等 |
| CPE485 | 原創 | 最短未排序連續子陣列 | 中等 |
| CPE486 | 原創 | 依身高重建佇列 | 中等 |
| CPE487 | 原創 | 最少射箭引爆氣球 | 中等 |
| CPE488 | 原創 | 退格字串比較 | 簡單 |
| CPE489 | 原創 | 有序陣列的平方 | 簡單 |
| CPE490 | 原創 | 搜尋二維矩陣 | 中等 |
| CPE491 | 原創 | D 天內送達包裹的船運能力 | 中等 |
| CPE492 | 原創 | 可可吃香蕉 | 中等 |
| CPE493 | 原創 | 陣列的樞紐索引 | 簡單 |
| CPE494 | 原創 | 陣列的度 | 簡單 |
| CPE495 | 原創 | 反轉二進位位元 | 簡單 |
| CPE496 | 原創 | 計算 0 到 N 的位元數 | 簡單 |
| CPE497 | 原創 | 比較版本號 | 簡單 |
| CPE498 | 原創 | 合併相似的物品 | 簡單 |
| CPE499 | 原創 | 陣列的排名轉換 | 簡單 |
| CPE500 | 原創 | 相對排序陣列 | 簡單 |
