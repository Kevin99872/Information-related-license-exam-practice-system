# 本地版 - Desktop

## 簡介
使用 Avalonia 開發的跨平台桌面應用，支援 Windows、macOS 和 Linux。

## 技術棧
- **前端框架**: Avalonia (XAML)
- **後端**: C# + Python
- **資料存儲**: CSV + Redis
- **支援**: 離線/在線模式

## 項目結構
```
desktop/
├── Views/           # UI 視圖 (XAML)
├── ViewModels/      # ViewModel 邏輯
├── Models/          # 資料模型
├── Services/        # 業務邏輯服務
├── python/          # Python 腳本
├── data/            # CSV 資料文件
└── App.xaml         # 應用主文件
```

## 安裝和運行

### 前置條件
- .NET 6 或更高版本
- Python 3.8 或更高版本

### 步驟
1. 安裝 Avalonia 模板
   ```bash
   dotnet new install Avalonia.Templates
   ```

2. 建立新的 Avalonia 項目
   ```bash
   dotnet new avalonia.mvvm -n ExamPracticeDesktop
   ```

3. 安裝依賴
   ```bash
   cd ExamPracticeDesktop
   dotnet restore
   ```

4. 安裝 Python 依賴
   ```bash
   cd python
   pip install -r requirements.txt
   ```

5. 運行應用
   ```bash
   dotnet run
   ```

## 功能
- 離線考試練習
- 本地代碼評分
- CSV 資料存儲
- Redis 同步 (可選)
- 實時分析和反饋

## 構建
```bash
dotnet publish -c Release -o ./publish
```
