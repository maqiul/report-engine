@echo off
rem Cross-platform consistency check: .NET dump -> Java dump -> frontend dump -> compare
rem usage: tests\cross-consistency\run.cmd
setlocal
set "ROOT=%~dp0..\.."
set "ART=%~dp0artifacts"

echo [1/4] .NET dump (Core.Tests CrossConsistencyDumpTest)
pushd "%ROOT%\tests\ReportEngine.Core.Tests"
set "CC_DUMP=%ART%\dotnet-summary.json"
call dotnet test -c Release --filter "FullyQualifiedName~CrossConsistencyDumpTest" --nologo || (popd & exit /b 1)
popd

echo.
echo [2/4] Java dump (java-lib CrossConsistencyDumpTest)
pushd "%ROOT%\java-lib"
set "CC_DUMP=%ART%\java-summary.json"
call gradlew.bat test --tests "com.reportengine.lib.CrossConsistencyDumpTest" --console=plain || (popd & exit /b 1)
popd

echo.
echo [3/4] frontend dump (report-engine-vue renderCore)
pushd "%ROOT%\report-engine-vue"
call npm run build || (popd & exit /b 1)
popd
call node "%~dp0frontend-dump.mjs" || exit /b 1

echo.
echo [4/4] compare (.NET / Java / frontend)
node "%~dp0check.js" || exit /b 1

echo.
echo ALL DONE
endlocal
