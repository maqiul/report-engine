@echo off
REM 一键 bump ReportEngine 三件套版本号
REM 用法: scripts\bump-version.cmd 0.6.0
REM 然后: git add . && git commit -m "chore: bump to 0.6.0" && git tag v0.6.0 && git push origin main --tags

if "%1"=="" (
    echo Usage: scripts\bump-version.cmd ^<version^>
    echo Example: scripts\bump-version.cmd 0.6.0
    exit /b 1
)

set VERSION=%1
set REPO_ROOT=%~dp0..

echo === Bumping version to %VERSION% ===
echo.

REM 1. .NET Directory.Build.props
echo [1/4] .NET: Directory.Build.props
if exist "%REPO_ROOT%\Directory.Build.props" (
    powershell -ExecutionPolicy Bypass -Command "$c = [System.IO.File]::ReadAllText('%REPO_ROOT%\Directory.Build.props'); $c = [regex]::Replace($c, '<Version>[^<]+</Version>', '<Version>%VERSION%</Version>'); [System.IO.File]::WriteAllText('%REPO_ROOT%\Directory.Build.props', $c, [System.Text.UTF8Encoding]::new($false)); Write-Host '  Updated'"
)

REM 2. Java java-lib
echo [2/4] Java: java-lib/build.gradle
if exist "%REPO_ROOT%\java-lib\build.gradle" (
    powershell -ExecutionPolicy Bypass -Command "$c = [System.IO.File]::ReadAllText('%REPO_ROOT%\java-lib\build.gradle'); $c = [regex]::Replace($c, \"version\s*=\s*'[^']+'\", \"version = '%VERSION%'\"); [System.IO.File]::WriteAllText('%REPO_ROOT%\java-lib\build.gradle', $c, [System.Text.UTF8Encoding]::new($false)); Write-Host '  Updated'"
)

REM 3. Java starter
echo [3/4] Java: report-engine-starter/build.gradle
if exist "%REPO_ROOT%\report-engine-starter\build.gradle" (
    powershell -ExecutionPolicy Bypass -Command "$c = [System.IO.File]::ReadAllText('%REPO_ROOT%\report-engine-starter\build.gradle'); $c = [regex]::Replace($c, \"version\s*=\s*'[^']+'\", \"version = '%VERSION%'\"); [System.IO.File]::WriteAllText('%REPO_ROOT%\report-engine-starter\build.gradle', $c, [System.Text.UTF8Encoding]::new($false)); Write-Host '  Updated'"
)

REM 4. npm @reportengine/vue
echo [4/4] npm: report-engine-vue/package.json
if exist "%REPO_ROOT%\report-engine-vue\package.json" (
    powershell -ExecutionPolicy Bypass -Command "$c = [System.IO.File]::ReadAllText('%REPO_ROOT%\report-engine-vue\package.json'); $old = ([regex]::Match($c, '\"version\":\s*\"[0-9][^\"]*\"')).Value; $c = $c.Replace($old, '\"version\": \"%VERSION%\"'); [System.IO.File]::WriteAllText('%REPO_ROOT%\report-engine-vue\package.json', $c, [System.Text.UTF8Encoding]::new($false)); Write-Host '  Updated'"
)

echo.
echo === Bump complete ===
echo.
echo Next steps:
echo   cd %REPO_ROOT%
echo   git add .
echo   git commit -m "chore: bump version to %VERSION%"
echo   git tag v%VERSION%
echo   git push origin main --tags
echo.
echo This will trigger 3 publish jobs:
echo   - ci.yml =^> NuGet (needs NUGET_API_KEY secret)
echo   - java-ci.yml =^> Maven Central (needs 4 secrets + 1-3 day review)
echo   - frontend-ci.yml =^> npm public (needs NPM_TOKEN secret)
echo.
echo See docs\RELEASE.md for secrets setup.
