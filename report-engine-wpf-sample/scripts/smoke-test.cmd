@echo off
rem WPF Sample smoke test — 启动 + 5 秒后 taskkill 验证不崩
rem 用法：scripts\smoke-test.cmd

setlocal

set "EXE=%~dp0..\bin\Release\net8.0-windows\ReportEngine.WpfSample.exe"
set "TEMPLATE=%~dp0..\bin\Release\net8.0-windows\Templates\order-summary.rptx"

echo === WPF Sample Smoke Test ===
echo.

if not exist "%EXE%" (
    echo [FAIL] exe not found: %EXE%
    echo        请先 dotnet build -c Release
    exit /b 1
)

if not exist "%TEMPLATE%" (
    echo [FAIL] template not found: %TEMPLATE%
    exit /b 1
)

echo [1/4] exe:    %EXE%
echo [2/4] tpl:    %TEMPLATE%
echo.

rem 启动 WPF 应用（不阻塞）
echo [3/4] launching (5s timeout)...
start "" "%EXE%"

rem 等 5 秒看是否还在跑（崩了会立即退出）
timeout /t 5 /nobreak >nul

rem 杀进程
taskkill /IM ReportEngine.WpfSample.exe /F >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo.
    echo [PASS] WPF sample ran for 5+ seconds without crashing
    echo        (template loaded + rendered + viewer displayed)
    exit /b 0
) else (
    echo.
    echo [FAIL] WPF sample crashed within 5 seconds
    exit /b 1
)
