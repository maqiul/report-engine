@echo off
echo ========================================
echo   ReportEngine Web - 启动中...
echo ========================================
echo.

echo [1/2] 启动后端 API (http://localhost:5000)...
start "ReportEngine WebApi" cmd /k "cd /d D:\report-engine\web\ReportEngine.WebApi && dotnet run --urls http://localhost:5000"

timeout /t 3 /nobreak >nul

echo [2/2] 启动前端 (http://localhost:5173)...
start "ReportEngine Frontend" cmd /k "cd /d D:\report-engine\web\frontend && npm run dev"

timeout /t 5 /nobreak >nul

echo.
echo ========================================
echo   启动完成！
echo   前端: http://localhost:5173
echo   后端: http://localhost:5000/swagger
echo ========================================
echo.

start http://localhost:5173
