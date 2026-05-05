# 智慧資訊類考照學習系統 - 完整建置指南

## 📋 專案概述
這是一個全棧的考照學習平台，提供線上版和本地版兩種使用方式。

### 架構
```
┌─────────────┐         ┌──────────────┐         ┌──────────────┐
│   Web端     │         │  Server端    │         │ Desktop端    │
│ React+TS    │◄───────►│ C#+Python    │◄───────►│ Avalonia     │
│ (線上版)    │         │ (線上版)     │         │ (本地版)     │
└─────────────┘         └──────────────┘         └──────────────┘
                               │
                               ▼
                        ┌──────────────┐
                        │    Redis     │
                        │   (數據庫)   │
                        └──────────────┘
```

---

## 🚀 快速開始

### 前置條件
- Node.js 18+ (用於 web)
- .NET 6+ (用於 server 和 desktop)
- Python 3.8+ (用於 Python 服務)
- Docker & Docker Compose (用於 Redis)
- Git

### 安裝步驟

#### 1️⃣ 初始化 Web 端（React + TypeScript）

```bash
cd web

# 安裝依賴
npm install

# 開發模式運行
npm start

# 構建生產版本
npm run build

# 運行測試
npm test
```

**預期結果**: 應用會在 http://localhost:3000 啟動

#### 2️⃣ 初始化 Server 端（C# + Python + Redis）

##### 步驟 A: 啟動 Redis
```bash
cd server

# 使用 Docker Compose 啟動 Redis
docker-compose up -d

# 驗證 Redis 是否運行
docker ps | grep redis
```

##### 步驟 B: 建立 C# API 項目
```bash
cd server/api

# 建立新的 ASP.NET Core Web API 項目
dotnet new webapi -n ExamAPI -f net6.0

# 安裝必要的 NuGet 包
dotnet add package StackExchange.Redis
dotnet add package Newtonsoft.Json
dotnet add package Serilog.AspNetCore

# 恢復依賴
dotnet restore

# 運行 API
dotnet run

# 預期結果: API 會在 http://localhost:5000 運行
```

##### 步驟 C: 啟動 Python 服務
```bash
cd server/python

# 建立虛擬環境
python -m venv venv

# 激活虛擬環境
# Windows:
venv\Scripts\activate
# Mac/Linux:
source venv/bin/activate

# 安裝 Python 依賴
pip install -r requirements.txt

# 運行 Python 服務
python main.py
```

#### 3️⃣ 初始化 Desktop 端（Avalonia + C# + Python）

##### 步驟 A: 建立 Avalonia 項目
```bash
cd desktop

# 安裝 Avalonia 模板 (如果還沒安裝)
dotnet new install Avalonia.Templates

# 建立新的 Avalonia MVVM 項目
dotnet new avalonia.mvvm -n ExamDesktop -f net6.0

# 進入項目目錄
cd ExamDesktop

# 恢復依賴
dotnet restore

# 運行桌面應用
dotnet run
```

##### 步驟 B: 設置 Python 本地分析模塊
```bash
cd desktop/python

# 建立虛擬環境
python -m venv venv

# 激活虛擬環境
venv\Scripts\activate  # Windows
# 或
source venv/bin/activate  # Mac/Linux

# 安裝依賴
pip install -r requirements.txt
```

---

## 📁 完整項目結構

```
Information-related-license-exam-practice-system/
│
├── README.md                          # 主說明文件
├── .gitignore                         # Git 忽略規則
│
├── web/                               # 🌐 線上版客戶端 (React + TypeScript)
│   ├── public/
│   │   └── index.html
│   ├── src/
│   │   ├── index.tsx                 # 應用入點
│   │   ├── App.tsx                   # 主應用組件
│   │   ├── App.css
│   │   ├── index.css
│   │   ├── components/               # UI 組件
│   │   ├── pages/                    # 頁面
│   │   ├── services/                 # API 服務
│   │   └── types/                    # TypeScript 類型
│   ├── package.json
│   ├── tsconfig.json
│   └── .gitignore
│
├── server/                            # ⚙️ 線上版服務器 (C# + Python + Redis)
│   ├── README.md
│   ├── docker-compose.yml             # Redis 配置
│   ├── .env.example                   # 環境變量示例
│   │
│   ├── api/                           # ASP.NET Core Web API
│   │   ├── README.md
│   │   ├── Controllers/               # API 控制器
│   │   ├── Models/                    # 資料模型
│   │   ├── Services/                  # 業務服務
│   │   ├── appsettings.json
│   │   ├── Dockerfile
│   │   └── Program.cs
│   │
│   └── python/                        # Python 分析和評分模塊
│       ├── main.py                    # Python 服務入點
│       ├── requirements.txt           # Python 依賴
│       ├── analyzer.py                # 分析模塊
│       └── grader.py                  # 評分模塊
│
└── desktop/                           # 🖥️ 本地版客戶端 (Avalonia + C# + Python)
    ├── README.md
    │
    ├── Views/                         # Avalonia UI 視圖
    │   ├── MainWindow.xaml
    │   └── Pages/
    │
    ├── ViewModels/                    # ViewModel 邏輯
    │   └── MainWindowViewModel.cs
    │
    ├── Models/                        # 資料模型
    │   ├── Exam.cs
    │   └── Submission.cs
    │
    ├── Services/                      # 業務服務
    │   ├── ExamService.cs
    │   └── GradingService.cs
    │
    ├── python/                        # Python 本地分析模塊
    │   ├── main.py
    │   ├── analyzer.py
    │   └── requirements.txt
    │
    ├── data/                          # 本地資料存儲 (CSV)
    │   └── .gitkeep
    │
    └── App.xaml
```

---

## 🧪 測試

### Web 端測試
```bash
cd web
npm test                              # 運行單元測試
npm run build                         # 構建並檢查錯誤
```

### Server 端測試
```bash
cd server/api
dotnet test                           # 運行單元測試
dotnet build                          # 構建檢查
```

### Desktop 端測試
```bash
cd desktop/ExamDesktop
dotnet build                          # 構建檢查
```

---

## 🔧 配置

### 環境變量
複製 `server/.env.example` 為 `.env` 並根據需要修改：
```bash
cp server/.env.example server/.env
```

### Redis 配置
```bash
# 啟動 Redis
cd server
docker-compose up -d

# 查看 Redis 狀態
docker-compose ps

# 停止 Redis
docker-compose down
```

---

## 📦 依賴管理

| 項目 | 包管理器 | 命令 |
|------|--------|------|
| web | npm | `npm install` |
| server (C#) | dotnet | `dotnet restore` |
| server (Python) | pip | `pip install -r requirements.txt` |
| desktop (C#) | dotnet | `dotnet restore` |
| desktop (Python) | pip | `pip install -r requirements.txt` |

---

## 🚢 部署

### Docker 部署
```bash
# 構建 Docker 鏡像
docker build -t exam-api ./server/api

# 使用 Docker Compose 全棧部署
cd server
docker-compose up -d
```

### 生產構建
```bash
# Web
cd web
npm run build

# Server
cd server/api
dotnet publish -c Release -o ./publish

# Desktop
cd desktop/ExamDesktop
dotnet publish -c Release -f net6.0-windows
```

---

## 📝 開發工作流

1. **創建分支**: `git checkout -b feature/your-feature`
2. **開發實現**: 在相應的端進行開發
3. **本地測試**: 按照上述測試步驟進行
4. **提交代碼**: `git commit -m "描述"` 和 `git push`
5. **創建 PR**: 在 GitHub 上創建 Pull Request
6. **代碼審查**: 等待代碼審查
7. **合併**: 通過審查後合併到 main

---

## 🐛 常見問題

### Redis 連接失敗
```bash
# 確保 Redis 正在運行
docker ps | grep redis

# 如果未運行，啟動它
docker-compose up -d
```

### Python 虛擬環境問題
```bash
# 重新建立虛擬環境
rm -rf venv
python -m venv venv
source venv/bin/activate  # 或 venv\Scripts\activate
pip install -r requirements.txt
```

### npm 依賴衝突
```bash
cd web
rm -rf node_modules package-lock.json
npm install
```

---

## 📚 技術文檔

- [React 文檔](https://react.dev)
- [TypeScript 文檔](https://www.typescriptlang.org/docs/)
- [ASP.NET Core 文檔](https://docs.microsoft.com/aspnet/core)
- [Avalonia 文檔](https://docs.avaloniaui.net)
- [Redis 文檔](https://redis.io/documentation)
- [Python 文檔](https://docs.python.org/3/)

---

## 🤝 貢獻指南

歡迎貢獻！請遵循以下步驟：
1. Fork 本倉庫
2. 建立你的特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交你的改動 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打開一個 Pull Request

---

## 📄 許可證

此項目採用 MIT 許可證。詳見 [LICENSE](LICENSE) 文件。

---

## 💬 聯繫方式

如有問題或建議，請提交 Issue 或聯繫項目維護者。

---

## ✅ 建置檢查清單

- [ ] 已安裝 Node.js 18+
- [ ] 已安裝 .NET 6+
- [ ] 已安裝 Python 3.8+
- [ ] 已安裝 Docker 和 Docker Compose
- [ ] 已 clone 倉庫
- [ ] 已初始化 web 依賴 (`npm install`)
- [ ] 已啟動 Redis (`docker-compose up -d`)
- [ ] 已初始化 server API
- [ ] 已初始化 desktop 項目
- [ ] 所有服務都能正常啟動

---

**祝你開發愉快！🎉**
