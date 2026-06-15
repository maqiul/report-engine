with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

# 范围 2124-2484
start = 2123  # 0-indexed
end = 2484

import re
print('=== Border 卡片 ===')
for i in range(start, min(end, len(lines))):
    if re.search(r'new Border\b', lines[i]):
        print(f'{i+1}: {lines[i].rstrip()[:80]}')

print()
print('=== Color.FromRgb 颜色块 ===')
for i in range(start, min(end, len(lines))):
    if 'Color.FromRgb' in lines[i]:
        print(f'{i+1}: {lines[i].rstrip()[:80]}')

print()
print('=== SolidColorBrush 使用统计 ===')
cnt = 0
for i in range(start, min(end, len(lines))):
    if 'SolidColorBrush' in lines[i]:
        cnt += 1
print(f'共 {cnt} 处使用 SolidColorBrush')