#!/usr/bin/env python3
"""统计 MainWindow.cs 中各方法的实际行数 (花括号深度计数)."""
import re, sys

PATH = r'D:\report-engine\ReportEngine.Designer.Wpf\MainWindow.cs'

with open(PATH, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# 找每个 private 方法签名 (开始行)
methods = []
for i, line in enumerate(lines):
    m = re.match(r'\s+private\s+(?:async\s+)?(?:void|bool|int|ReportElement|List|double|string|Band|TextElement|ReportTemplate|IReadOnlyList|HashSet)\s+(\w+)\s*\(', line)
    if m:
        methods.append((i+1, m.group(1)))  # 1-based

# 算每个方法行数 (深度计数)
results = []
for start, name in methods:
    depth = 0
    started = False
    end = start - 1
    for j in range(start-1, len(lines)):
        for c in lines[j]:
            if c == '{':
                depth += 1
                started = True
            elif c == '}':
                depth -= 1
                if started and depth == 0:
                    end = j + 1
                    break
        if started and depth == 0:
            break
    length = end - start + 1
    if length > 0:
        results.append((length, start, name))

# 按行数降序输出
results.sort(reverse=True)
for length, start, name in results:
    print(f'{length:4d} 行: line {start:4d}  {name}')
