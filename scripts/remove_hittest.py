with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

# HitTest: line 1365-1399 (含末尾空行) - 下个内容是 // ============================== 文件操作 ============================== (line 1401)
print(f'Total before: {len(lines)}')
print(f'Line 1365: {lines[1364].rstrip()!r}')
print(f'Line 1399: {lines[1398].rstrip()!r}')
print(f'Line 1400: {lines[1399].rstrip()!r}')
print(f'Line 1401: {lines[1400].rstrip()!r}')

# del 索引 1364..1398
del lines[1364:1399]

with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','w',encoding='utf-8') as f:
    f.writelines(lines)

print(f'After: {len(lines)}')
print(f'Removed: {1399-1364} lines')