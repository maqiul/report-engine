#!/usr/bin/env python3
"""Step2.A 收尾（修正版 v3）：
- 用 analyze_methods.py 的花括号配平算法（跳过字符串/verbatim/插值/注释）精确定位方法体边界
- 删 12 个旧方法（10 渲染 + ParseBrush + BandTypeName 定义）
- GetBandBrush 已在 BandStyle.cs，跳过
- 注入 using static BandStyle
- 字段 + 构造器实例化
- 改写调用点：RenderCanvas/DrawRulers/RenderPreview/ParseBrush
- 注入 BuildCanvasRenderContext
"""
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
MW = ROOT / "ReportEngine.Designer.Wpf" / "MainWindow.cs"

sys.path.insert(0, str(ROOT / "scripts"))
from analyze_methods import find_method_body, strip_strings_and_comments


def read():
    return MW.read_text(encoding="utf-8")


def write(text):
    MW.write_text(text, encoding="utf-8")


def remove_method_by_name(text, name):
    """用花括号配平算法精确删除方法体（首尾行包含）。"""
    lines = text.splitlines(keepends=True)
    rng = find_method_body(lines, name)
    if rng is None:
        raise RuntimeError(f"method not found: {name}")
    s, e = rng  # 1-based, e inclusive
    print(f"  removed: {name} (lines {s}-{e}, {e - s + 1} lines)")
    del lines[s - 1 : e]
    return "".join(lines)


def main():
    text = read()

    # ---- Step A: 注入 using static BandStyle ----
    text = text.replace(
        "using static ReportEngine.Designer.Wpf.ElementIcons;",
        "using static ReportEngine.Designer.Wpf.ElementIcons;\nusing static ReportEngine.Designer.Wpf.BandStyle;",
        1,
    )

    # ---- Step B: 字段注入 ----
    text = text.replace(
        "        private readonly Canvas _previewCanvas;\n        private readonly Canvas _vRuler;\n",
        "        private readonly Canvas _previewCanvas;\n        private readonly Canvas _vRuler;\n\n"
        "        // 渲染器（Step2.A 拆出）\n"
        "        private readonly CanvasRenderer _canvasRenderer;\n"
        "        private readonly PreviewRenderer _previewRenderer;\n",
        1,
    )

    # ---- Step C: 构造器内实例化 ----
    text = text.replace(
        "                Content = _previewCanvas,\n"
        "                Visibility = Visibility.Collapsed,\n"
        "            };\n",
        "                Content = _previewCanvas,\n"
        "                Visibility = Visibility.Collapsed,\n"
        "            };\n\n"
        "            // 渲染器（Step2.A 拆出）：构造函数持 canvas/ruler/scroll 引用，渲染状态走 ctx 参数\n"
        "            _canvasRenderer = new CanvasRenderer(_canvas, _hRuler, _vRuler, _scrollViewer);\n"
        "            _previewRenderer = new PreviewRenderer(_previewCanvas);\n",
        1,
    )

    # ---- Step D: 删 12 个旧方法（用配平算法） ----
    # GetBandBrush 已在 BandStyle.cs，跳过
    deletes = [
        "RenderCanvas",
        "RenderPreview",
        "DrawPreviewElement",
        "DrawPreviewBand",
        "DrawDashedLine",
        "DrawHandle",
        "DrawElement",
        "DrawBand",
        "DrawRulers",
        "ResolvePreviewValue",
        "ParseBrush",
        "BandTypeName",
    ]
    for name in deletes:
        text = remove_method_by_name(text, name)

    # ---- Step E: 改写 4 类调用点 ----
    # E1: RenderCanvas()
    n = text.count("RenderCanvas()")
    text = text.replace(
        "RenderCanvas()",
        "_canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand)",
    )
    print(f"  RenderCanvas() replaced: {n}")

    # E2: DrawRulers()
    n = text.count("DrawRulers()")
    text = text.replace(
        "DrawRulers()",
        "_canvasRenderer.RenderRulers(_template!, _zoom)",
    )
    print(f"  DrawRulers() replaced: {n}")

    # E3: RenderPreview()
    n = text.count("RenderPreview()")
    text = text.replace(
        "RenderPreview()",
        "_previewRenderer.Render(_template!, _zoom, _previewData)",
    )
    print(f"  RenderPreview() replaced: {n}")

    # E4: ParseBrush(...) -> BrushParser.Parse(...)
    n = text.count("ParseBrush(")
    text = text.replace("ParseBrush(", "BrushParser.Parse(")
    print(f"  ParseBrush( replaced: {n} (only call sites, def already removed)")

    # ---- Step F: 注入 BuildCanvasRenderContext ----
    anchor = "        private void OnVRulerMouseDown"
    if anchor not in text:
        raise RuntimeError("OnVRulerMouseDown anchor not found")
    text = text.replace(
        anchor,
        '        private CanvasRenderContext BuildCanvasRenderContext()\n'
        '        {\n'
        '            if (_template == null)\n'
        '                return null!; // 调用方均已判 _template 非空，这里不抛\n'
        '            // ShowMargins 原 MainWindow 无 toggle，始终为 true（margin 永远绘制以保留行为）\n'
        '            return new CanvasRenderContext(\n'
        '                _template, _zoom, _gridSpacingMm, _showGrid,\n'
        '                showMargins: true, _gridColor,\n'
        '                _vGuides, _hGuides, _snapLinesX, _snapLinesY);\n'
        '        }\n\n'
        + anchor,
        1,
    )

    write(text)
    print("DONE")


if __name__ == "__main__":
    main()