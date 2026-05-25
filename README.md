# 智慧考試系統
## 啟發原因
因應時代的快速發展,軟體工程師不僅僅侷限於大學時學習的技能,為了讓企業更好的對個人評估其實力標準會參考Leetcode,CPE證照,TQC.....等相關證照的證明,但其困境在於每個人的時間有限,平日除了應付工作之外也要做各種繁忙事務,更是需要支付大把金錢供那些考照機構進行報名及學習,為了節省這些費用及活用時間,我們做了這個Program.
## 技術要點
- UI操作介面
    - 開始畫面
        - 伸縮式菜單
        - 考照選項
    - 考試設定控制項
        - 時間設定控制項(分鐘)
        - 題目數量設定控制項
        - 編譯語言控制項
    - 考時畫面
        - 計時器(Timer)
        - 題目隨機抽選顯示
        - 提交驗證按鈕
        - 實際可運行簡易IDE
        
    - 設定
        - 設定Agent API_KEY或本地AI model選擇
        - 設定介面色彩
        - 設定字型大小
    - 結束評測畫面
        - 成績評測顯示介面
        - AI問答畫面
        - 一鍵分析程式碼漏洞與寫法問題
- 後端
    - 本地語言AI model
        - 考照分析prompt
        - 問答role
    - 程式碼驗證
        - 分析輸入輸出答案是否一致
        - 隨機動態題目輸入
    - 隨機題目抽選
    - API_KEY線上Agent
        - API_KEY串接
        - prompt工程
## 軟體特點
- 無網路環境可執行
    
    此專案所有題庫皆為本地儲存,使用SQLite進行規劃及管理
- 多系統泛用

    不管你是linux,macOS,windows,因avalonia框架所帶來的便利性,發出release時都可以全部環境執行
- 多種編譯方式通用

    在這個方案中我們提供了C/C++,Pyhton,C#等路徑提供編譯及測試環境,可以根據輸入的路徑進行編譯

## 快速開始
- install

    點選旁邊release->找到自己的系統版本->點選下載->解包至資料夾->Enjoy!
- 編譯路徑

    設定>編譯路徑>依照自己的系統版本進行路徑設置


## 技術框架
- 題庫資料庫
    - SQLite
- 程式主體框架
    - Python
    - C#
    - NET.Core 10.0.101
    - Avalonia 

## 題庫匯入格式
匯入頁面支援兩種方式：
- 單題新增：直接輸入題目代碼、考照種類、題目敘述、解法程式碼，以及多筆測試資料與驗證資料。
- 表單匯入：支援 `.csv`、`.xls`、`.xlsx`，每一列代表一筆測試資料；相同 `ProblemCode` 的列會合併成同一題。

### 欄位規格
匯入表單請依下列欄位順序建立：
`ProblemCode`, `ExamType`, `Title`, `Description`, `Difficulty`, `Status`, `SolutionLanguage`, `SolutionCode`, `OrderIndex`, `IsExample`, `TestInput`, `ExpectedOutput`

### 範例值
- `ProblemCode`：`PYD101`
- `ExamType`：`TQC`
- `Difficulty`：`1 / 2 / 3` 或 `簡單 / 中等 / 困難`
- `Status`：`Draft` 或 `Active`
- `IsExample`：`True / False`、`是 / 否`

### 模板下載
在應用程式的「匯入題庫」頁面可以直接下載 CSV 模板，下載後可用 Excel、LibreOffice 或其他試算表工具編輯，再匯入系統。

### Windows 安裝包
Windows 安裝包已輸出到 `desktop/B3/bin/Release/B3-Windows-Install.zip`。

安裝步驟：
- 解壓縮 zip
- 雙擊 `Install-B3.cmd`
- 預設會安裝到 `%LOCALAPPDATA%\Programs\B3`

解除安裝：執行 `Uninstall-B3.ps1`

### 純打包 DLL 版本
如果你只需要 macOS / Linux / Windows 三平台的純打包版本，請執行 `installer/windows/Package-B3-MultiRid.ps1`。

輸出會放在 `desktop/B3/bin/Release/`，包含：
- `B3-win-x64-dll.zip`
- `B3-linux-x64-dll.zip`
- `B3-osx-x64-dll.zip`

每個 zip 解壓後都會看到對應平台的 DLL 與依賴檔，不是單檔 exe，也不是安裝器。

## 參考
- EZTest英文考照
- Tronclass大學學習網站
- 巨X電腦