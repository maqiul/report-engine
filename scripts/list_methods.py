with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

import re
methods = []
for i, line in enumerate(lines):
    m = re.match(r'        private (?:static )?(?:void|bool|int|async) (\w+)\(\)', line)
    if m:
        methods.append((i, m.group(1)))

# 估算每个方法的行数: 找下一个 private 方法的起点
# 但 MainWindow.cs 也有 private class nested, 这里只粗排
for idx, (line_no, name) in enumerate(methods):
    # 估算结束: 找下一个 private 方法(同缩进)
    next_line = methods[idx+1][0] if idx+1 < len(methods) else len(lines)
    body = next_line - line_no
    if body >= 30:
        print(f'Line {line_no+1:4}: {name:40s}  ~{body:3} lines')

print(f'\nTotal methods >= 30 lines: {sum(1 for _, n in methods if True)}')