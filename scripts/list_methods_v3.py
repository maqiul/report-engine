with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

import re
methods = []
# 任意返回类型
for i, line in enumerate(lines):
    m = re.match(r'^        private\s+(?:static\s+)?(\S+)\s+(\w+)\([^)]*\)\s*$', line)
    if m and m.group(1) not in ('const', 'readonly'):
        methods.append((i, m.group(1), m.group(2)))

print(f'Found {len(methods)} methods')
print()

def find_end(idx):
    for j in range(idx+1, len(lines)):
        l = lines[j]
        if l.strip() == '' or l.strip().startswith('//'):
            continue
        if re.match(r'^        (private|public|protected|internal)\s+', l):
            return j
        if re.match(r'^    \}\s*$', l):
            return j + 1
    return len(lines)

big = []
for idx, ret, name in methods:
    end = find_end(idx)
    body = end - idx
    if body >= 20:
        big.append((idx+1, name, ret, body))

big.sort(key=lambda x: -x[3])
for line_no, name, ret, body in big[:25]:
    print(f'Line {line_no:4}: {ret:18s} {name:35s}  ~{body:3} lines')