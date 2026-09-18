@echo off
rem WinForms Sample smoke test - launch + 5s liveness check
rem usage: scripts\smoke-test.cmd
setlocal

set "BIN=%~dp0..\bin\Release\net8.0-windows"
set "EXE=%BIN%\ReportEngine.WinFormsSample.exe"
set "TPL=%BIN%\Templates\order-summary.rptx"

echo === WinForms Sample Smoke Test ===
echo.

if not exist "%EXE%" (
    echo [FAIL] exe not found: %EXE%
    echo        run 'dotnet build -c Release' first
    exit /b 1
)
if not exist "%TPL%" (
    echo [FAIL] template not found: %TPL%
    exit /b 1
)

echo [1/3] exe: %EXE%
echo [2/3] tpl: %TPL%
echo.
echo [3/3] launching, 5s liveness window...

pushd "%BIN%"
start "" ReportEngine.WinFormsSample.exe
ping -n 6 127.0.0.1 >nul

tasklist /FI "IMAGENAME eq ReportEngine.WinFormsSample.exe" /NH | findstr /I "WinFormsSample" >nul
if %ERRORLEVEL% EQU 0 (
    echo.
    echo [PASS] process alive after 5s - no crash on startup
    taskkill /IM ReportEngine.WinFormsSample.exe /F >nul 2>&1
    popd
    endlocal
    exit /b 0
) else (
    echo.
    echo [FAIL] process gone within 5s - crashed on startup
    popd
    endlocal
    exit /b 1
)
