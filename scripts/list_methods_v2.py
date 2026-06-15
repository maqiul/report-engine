with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

import re
methods = []
for i, line in enumerate(lines):
    # 主类的方法签名: 8空格缩进 + private [static] + 返回类型 + 名字( + 参数 + ) + {
    if re.match(r'^        private (?:static )?(?:void|bool|int|async) \w+\([^)]*\)\s*$', line):
        # 取方法名
        m = re.match(r'^        private (?:static )?(?:void|bool|int|async) (\w+)\(', line)
        if m:
            methods.append((i, m.group(1)))

print(f'Found {len(methods)} methods')
print()

# 对每个方法, 用花括号深度找结束
def find_end(idx):
    """从 idx 找方法的结束 (下一个 8空格缩进的 private 方法 或 类结束)"""
    # 简化: 找下一行 8空格 private 签名 或 } 在 4 缩进
    for j in range(idx+1, len(lines)):
        l = lines[j]
        # 跳过空白行/注释
        if l.strip() == '' or l.strip().startswith('//'):
            continue
        # 下一个 8 空格 private 签名
        if re.match(r'^        private (?:static )?(?:void|bool|int|async) \w+\(', l):
            return j
        # 主类结束 (4空格 } 单独行)
        if re.match(r'^    \}$', l):
            return j + 1
    return len(lines)

# 找出大方法
big = []
for idx, name in methods:
    end = find_end(idx)
    body = end - idx
    if body >= 50:
        big.append((idx+1, name, body))

big.sort(key=lambda x: -x[2])
for line_no, name, body in big[:20]:
    print(f'Line {line_no:4}: {name:40s}  ~{body:3} lines')