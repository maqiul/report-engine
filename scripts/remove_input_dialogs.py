with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

# 删除 3055..3110 (56 行)
# 索引 3054..3110
# 保留前 3054 行（含 line3055 之前的空行），保留 3111 起的所有内容
before = lines[:3054]   # line 1..3054
after = lines[3111:]    # line 3112..end (含 MainWindow 收尾和 namespace 收尾)
# 验证 after 头两行
print('after[0]:', repr(after[0]))
print('after[1]:', repr(after[1]))

new_lines = before + after
with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','w',encoding='utf-8') as f:
    f.writelines(new_lines)

print(f'Before: {len(lines)} lines')
print(f'After:  {len(new_lines)} lines')
print(f'Removed: {len(lines) - len(new_lines)} lines')