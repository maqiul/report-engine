#!/usr/bin/env bash
# Cross-platform consistency check: .NET dump -> Java dump -> frontend dump -> compare
# usage: tests/cross-consistency/run.sh
set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$DIR/../.." && pwd)"
ART="$DIR/artifacts"
mkdir -p "$ART"

echo "[1/4] .NET dump (Core.Tests CrossConsistencyDumpTest)"
(
  cd "$ROOT/tests/ReportEngine.Core.Tests"
  CC_DUMP="$ART/dotnet-summary.json" \
    dotnet test -c Release --filter "FullyQualifiedName~CrossConsistencyDumpTest" --nologo
)

echo ""
echo "[2/4] Java dump (java-lib CrossConsistencyDumpTest)"
(
  cd "$ROOT/java-lib"
  chmod +x ./gradlew
  CC_DUMP="$ART/java-summary.json" \
    ./gradlew test --tests "com.reportengine.lib.CrossConsistencyDumpTest" --console=plain
)

echo ""
echo "[3/4] frontend dump (report-engine-vue renderCore)"
(
  cd "$ROOT/report-engine-vue"
  npm ci
  npm run build
)
node "$DIR/frontend-dump.mjs"

echo ""
echo "[4/4] compare (.NET / Java / frontend)"
node "$DIR/check.js"

echo ""
echo "ALL DONE"
