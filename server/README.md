# 服務器端 - Server

## 簡介
線上版服務器端，負責資料運算和存儲。

## 技術棧
- **語言**: C# (ASP.NET Core) + Python
- **資料存儲**: Redis
- **API**: RESTful API

## 項目結構
```
server/
├── api/              # ASP.NET Core API
├── python/           # Python 腳本
├── config/           # 配置文件
└── docker-compose.yml # Redis 容器配置
```

## 安裝和運行

### 前置條件
- .NET 6 或更高版本
- Python 3.8 或更高版本
- Docker (用於 Redis)

### 步驟
1. 啟動 Redis
   ```bash
   docker-compose up -d
   ```

2. 安裝 C# 依賴
   ```bash
   cd api
   dotnet restore
   ```

3. 安裝 Python 依賴
   ```bash
   cd python
   pip install -r requirements.txt
   ```

4. 運行 API 服務
   ```bash
   cd api
   dotnet run
   ```

## API 端點
- `GET /api/health` - 健康檢查
- `POST /api/exam` - 建立考試
- `GET /api/exam/{id}` - 獲取考試詳情
- `POST /api/submit` - 提交答案
- `GET /api/score` - 獲取分數

## 配置
編輯 `config/appsettings.json` 進行配置。
