#!/usr/bin/env python3
"""Step2.B 收尾：抽 PreviewJsonParser + ExportDataBuilder。
- 删 MainWindow.ParseSimpleJson / BuildExportData 方法定义
- 注入 'using static' 让 Parse / Build 自动解析
- 改写调用点：ParseSimpleJson(text) -> Parse(text), BuildExportData() -> Build(_previewData)
"""
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
MW = ROOT / "ReportEngine.Designer.Wpf" / "MainWindow.cs"

sys.path.insert(0, str(ROOT / "scripts"))
from analyze_methods import find_method_body


def main():
    text = MW.read_text(encoding="utf-8")
    lines = text.splitlines(keepends=True)

    # ---- 删 ParseSimpleJson / BuildExportData 方法体 ----
    for name in ("ParseSimpleJson", "BuildExportData"):
        rng = find_method_body(lines, name)
        if rng is None:
            raise RuntimeError(f"method not found: {name}")
        s, e = rng  # 1-based inclusive
        print(f"  removed: {name} (lines {s}-{e}, {e - s + 1} lines)")
        del lines[s - 1 : e]
    text = "".join(lines)

    # ---- 注入两个 using static ----
    text = text.replace(
        "using static ReportEngine.Designer.Wpf.BandStyle;",
        "using static ReportEngine.Designer.Wpf.BandStyle;\n"
        "using static ReportEngine.Designer.Wpf.PreviewJsonParser;\n"
        "using static ReportEngine.Designer.Wpf.ExportDataBuilder;",
        1,
    )

    # ---- 改写调用点 ----
    # ParseSimpleJson(text) -> Parse(text)
    n = text.count("ParseSimpleJson(")
    text = text.replace("ParseSimpleJson(", "Parse(")
    print(f"  ParseSimpleJson( -> Parse(: {n}")

    # BuildExportData() -> Build(_previewData)
    n = text.count("BuildExportData()")
    text = text.replace("BuildExportData()", "Build(_previewData)")
    print(f"  BuildExportData() -> Build(_previewData): {n}")

    MW.write_text(text, encoding="utf-8")
    print("DONE")


if __name__ == "__main__":
    main()