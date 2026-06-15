with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

# 找 2205-2378 之间每个 } 出现
import re
for i in range(2204, 2380):
    l = lines[i]
    if re.match(r'^\s*\}\s*$', l) or '// ===' in l or '// ==' in l:
        print(f'{i+1}: {l.rstrip()[:80]}')