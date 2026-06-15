with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

# 删除 SnapPosition: line 1357-1424 (含末尾空行)
# 索引 1356-1423
print(f'Total before: {len(lines)} lines')
print(f'Line 1357: {lines[1356].rstrip()!r}')
print(f'Line 1424: {lines[1423].rstrip()!r}')
print(f'Line 1425: {lines[1424].rstrip()!r}')

# del 索引 1356..1423 (含)
del lines[1356:1424]

with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','w',encoding='utf-8') as f:
    f.writelines(lines)

print(f'After: {len(lines)} lines')
print(f'Removed: {1424-1356} lines')