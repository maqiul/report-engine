#!/usr/bin/env python3
"""从 MainWindow.cs 中精确删除指定方法定义（按花括号配平）。
edit_file 反向匹配坑太多，用脚本更稳。
"""
import re
import sys

PATH = r"D:\report-engine\ReportEngine.Designer.Wpf\MainWindow.cs"

# 要删除的方法签名（用正则匹配行首 "private static ..."）
METHODS_TO_STRIP = [
    r"private\s+static\s+Button\s+MakeToolBtn",
    r"private\s+static\s+Border\s+MakeStatusTab",
    r"private\s+static\s+Button\s+MakeFmtBtn",
    r"private\s+static\s+void\s+AddToolboxBtn",
    r"private\s+static\s+Button\s+MakeSmallIconBtn",
    r"private\s+static\s+MenuItem\s+MakeMenuItem",
    r"private\s+static\s+Border\s+MakeElementBorder",
    r"private\s+static\s+BorderDef\s+EnsureBorder",
    r"private\s+static\s+TextElement\s+New(Text|FieldBox|SummaryBox|SysVarBox)",
    r"private\s+static\s+LineElement\s+NewLine",
    r"private\s+static\s+ImageElement\s+NewImage",
    r"private\s+static\s+ShapeElement\s+NewShape",
    r"private\s+static\s+SubReportElement\s+NewSubReport",
    r"private\s+static\s+BarcodeElement\s+NewBarcode",
    r"private\s+static\s+TableElement\s+NewTable",
    r"private\s+static\s+CrossTabElement\s+NewCrossTab",
    r"private\s+static\s+ChartElement\s+NewChart",
    r"private\s+static\s+string\s+BandIcon",
    r"private\s+static\s+string\s+ElementIcon",
]


def strip_one(text: str, sig_regex: str):
    """删除一个匹配 sig_regex 的方法定义，返回 (新文本, 删除位置 1-based 行号, 是否删除)。"""
    lines = text.split("\n")
    out = []
    i = 0
    deleted_line = -1
    n = len(lines)
    pat = re.compile(r"^\s*" + sig_regex + r"\s*\(")
    while i < n:
        line = lines[i]
        if pat.match(line):
            deleted_line = i + 1
            # 找花括号配平（处理跨行）
            j = i
            depth = 0
            opened = False
            while j < n:
                for ch in lines[j]:
                    if ch == "{":
                        depth += 1
                        opened = True
                    elif ch == "}":
                        depth -= 1
                j += 1
                if opened and depth == 0:
                    break
            # 跳过该方法（i..j-1），并吃掉方法前的多余空行
            while out and out[-1].strip() == "":
                out.pop()
            i = j
        else:
            out.append(line)
            i += 1
    return "\n".join(out), deleted_line


def main():
    with open(PATH, "r", encoding="utf-8") as f:
        text = f.read()

    before = len(text.split("\n"))
    for sig in METHODS_TO_STRIP:
        text, ln = strip_one(text, sig)
        label = sig.split("\\s+")[-1] if "\\s+" in sig else sig
        if ln > 0:
            print(f"  [ok ] line {ln}: {label}")
        else:
            print(f"  [skip] {label}")

    # 合并 3+ 连续空行为 2
    text = re.sub(r"\n\n\n+", "\n\n", text)

    with open(PATH, "w", encoding="utf-8") as f:
        f.write(text)

    after = len(text.split("\n"))
    print(f"\n行数: {before} -> {after}  (减 {before - after})")


if __name__ == "__main__":
    main()