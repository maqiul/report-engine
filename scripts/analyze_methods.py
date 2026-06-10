#!/usr/bin/env python3
"""扫描 MainWindow.cs 里 13 个方法，输出每个方法体的精确行范围。
配平算法会跳过字符串字面量、verbatim string、字符字面量、注释、$"" 插值。
输出：方法名 -> (起始行, 结束行, 整段内容)
"""
import re
import json

PATH = r"D:\report-engine\ReportEngine.Designer.Wpf\MainWindow.cs"

METHODS = [
    "RenderCanvas", "RenderPreview", "DrawPreviewBand", "DrawPreviewElement",
    "DrawDashedLine", "DrawBand", "DrawElement", "DrawHandle", "DrawRulers",
    "GetBandBrush", "ParseBrush", "BandTypeName", "ResolvePreviewValue",
]


def find_method_body(src_lines, method_name):
    """找到方法定义起始行，返回 (start_idx_inclusive, end_idx_exclusive) 基于 1-based 行号。"""
    # 找包含 "method_name(" 的非注释行
    n = len(src_lines)
    start = -1
    for i, line in enumerate(src_lines):
        if method_name + "(" in line and "private" in line and "//" not in line.split(method_name)[0]:
            start = i
            break
    if start == -1:
        return None

    # 找到本行的 '{' （可能在同一行或后续行）
    j = start
    while j < n and "{" not in src_lines[j]:
        j += 1
    if j >= n:
        return None
    brace_line = j

    # 从 brace_line 开始花括号配平，跟踪字符串状态
    depth = 0
    k = brace_line
    while k < n:
        line = src_lines[k]
        # 简化 tokenizer: 跳过 "..."、@"..."、`$"..."`、//...、/*...*/
        cleaned = strip_strings_and_comments(line)
        for ch in cleaned:
            if ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
        if depth == 0 and k >= brace_line:
            return (start + 1, k + 1)  # 1-based, end 是包含 '}' 的行
        k += 1
    return None


def strip_strings_and_comments(line):
    """移除字符串字面量、verbatim string、$"" 插值、注释。
    简化版：处理常见场景，复杂度足够本任务用。
    """
    out = []
    i = 0
    n = len(line)
    while i < n:
        c = line[i]
        # 行注释
        if c == "/" and i + 1 < n and line[i + 1] == "/":
            break
        # 块注释（单行内）
        if c == "/" and i + 1 < n and line[i + 1] == "*":
            j = line.find("*/", i + 2)
            if j == -1:
                break
            i = j + 2
            continue
        # 字符字面量 'x'
        if c == "'" and (i == 0 or line[i - 1] != "\\"):
            j = line.find("'", i + 1)
            if j == -1: break
            i = j + 1
            continue
        # 普通字符串 "..."
        if c == '"' and not (i > 0 and line[i - 1] == "@") and not (i > 1 and line[i - 2:i] == "$@"):
            # 简化：不处理跨行字符串（这种代码罕见）
            j = line.find('"', i + 1)
            if j == -1: break
            i = j + 1
            continue
        # verbatim string @"..."
        if c == "@" and i + 1 < n and line[i + 1] == '"':
            j = line.find('"', i + 2)
            if j == -1: break
            i = j + 1
            continue
        # interpolated string $"..."
        if c == "$" and i + 1 < n and line[i + 1] == '"':
            j = line.find('"', i + 2)
            if j == -1: break
            i = j + 1
            continue
        out.append(c)
        i += 1
    return "".join(out)


def main():
    with open(PATH, "r", encoding="utf-8") as f:
        src_lines = f.read().split("\n")

    results = {}
    for m in METHODS:
        r = find_method_body(src_lines, m)
        if r:
            start, end = r
            results[m] = {"start": start, "end": end, "lines": end - start + 1}
            print(f"  {m:25s} lines {start:4d}-{end:4d} ({end - start + 1} lines)")
        else:
            print(f"  {m:25s} NOT FOUND")

    # 输出 JSON 备用
    with open("methods.json", "w", encoding="utf-8") as f:
        json.dump(results, f, ensure_ascii=False, indent=2)


if __name__ == "__main__":
    main()