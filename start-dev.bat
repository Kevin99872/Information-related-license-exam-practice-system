@echo off
REM 智慧資訊類考照學習系統 - 快速啟動腳本 (Windows)

echo ========================================
echo 智慧資訊類考照學習系統 - 開發環境啟動
echo ========================================
echo.

REM 檢查前置條件
echo [1/4] 檢查前置條件...
node --version >nul 2>&1 || (echo ❌ 請先安裝 Node.js && exit /b 1)
echo ✓ Node.js 已安裝

dotnet --version >nul 2>&1 || (echo ❌ 請先安裝 .NET SDK && exit /b 1)
echo ✓ .NET SDK 已安裝

python --version >nul 2>&1 || (echo ❌ 請先安裝 Python && exit /b 1)
echo ✓ Python 已安裝

echo.

REM 啟動 Redis
echo [2/4] 啟動 Redis...
cd server
docker-compose up -d
echo ✓ Redis 已啟動 (port: 6379)
echo.

REM 啟動 Web 端
echo [3/4] 啟動 Web 端 (React + TypeScript)...
cd ..\web
if not exist "node_modules" (
    echo 安裝依賴...
    call npm install
)
start cmd /k "npm start"
echo ✓ Web 端已啟動 (http://localhost:3000)
echo.

REM 顯示接下來的步驟
echo [4/4] 接下來的步驟...
echo.
echo 📌 要啟動 Server 端 (C# API):
echo    cd server\api
echo    dotnet restore
echo    dotnet run
echo.
echo 📌 要啟動 Python 服務:
echo    cd server\python
echo    python -m venv venv
echo    venv\Scripts\activate
echo    pip install -r requirements.txt
echo    python main.py
echo.
echo 📌 要啟動 Desktop 端:
echo    cd desktop
echo    dotnet new avalonia.mvvm -n ExamDesktop
echo    cd ExamDesktop
echo    dotnet restore
echo    dotnet run
echo.
echo ========================================
echo 開發環境已準備好！
echo ========================================
