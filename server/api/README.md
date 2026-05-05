# C# ASP.NET Core API

## 建立新項目

```bash
dotnet new webapi -n ExamPracticeAPI
cd ExamPracticeAPI
```

## 安裝必要包

```bash
dotnet add package StackExchange.Redis
dotnet add package Newtonsoft.Json
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
```

## 項目結構

```
api/
├── Controllers/      # API 控制器
├── Models/          # 資料模型
├── Services/        # 業務邏輯服務
├── Configuration/   # 配置類
├── appsettings.json # 應用配置
├── Dockerfile       # Docker 配置
└── Program.cs       # 應用入點
```

## 開發

1. 建立 ASP.NET Core Web API 項目
2. 配置 Redis 連接
3. 實現考試、提交、評分等 API 端點
4. 與 Python 服務進行通信
