with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

start = 2123  # 0-indexed line 2124
end = 2378    # 0-indexed line 2379

import re
# 找所有 if/else if 分支 (在 UpdatePropertyListCore 范围)
print('=== if/else if branches in UpdatePropertyListCore ===')
for i in range(start, min(end, len(lines))):
    l = lines[i]
    if re.match(r'^\s*(if|else if|else)\s*\(', l):
        print(f'{i+1}: {l.rstrip()[:90]}')
    elif re.match(r'^\s*else\s*$', l):
        print(f'{i+1}: else')

print()
print('=== ctx.Add* 调用次数 ===')
from collections import Counter
calls = Counter()
for i in range(start, min(end, len(lines))):
    l = lines[i]
    m = re.search(r'ctx\.(Add[A-Z]\w*)\(', l)
    if m:
        calls[m.group(1)] += 1
for k, v in calls.most_common():
    print(f'  ctx.{k}(): {v}')