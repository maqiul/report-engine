@echo off
REM 一键 bump ReportEngine 三件套版本号（Windows wrapper）
REM 内部调用 Node.js 版（无 PowerShell escape 问题）
REM 用法: scripts\bump-version.cmd 0.6.0

if "%1"=="" (
    echo Usage: scripts\bump-version.cmd ^<version^>
    echo Example: scripts\bump-version.cmd 0.6.0
    exit /b 1
)

where node >nul 2>&1
if errorlevel 1 (
    echo Error: Node.js not found in PATH
    echo Install from https://nodejs.org/
    exit /b 1
)

node "%~dp0bump-version.js" %1
