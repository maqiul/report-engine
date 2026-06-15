with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

# 删除索引 2830 到 2985（包含 2985 = 156 行）
# 保留前 2830 行（含 line2830 的 '}'），并保留 2986 起的所有内容
# 等价于：lines[0..2829] + wrapper + lines[2986..]

before = lines[:2830]   # line 1..2830
after = lines[2986:]    # line 2987..

wrapper = '''        private void OnShowDataSourceClicked()
        {
            DataSourceDialog.Show(this, _template, () => { PushUndo(); MarkDirty(); });
        }

'''

new_lines = before + [wrapper] + after
with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','w',encoding='utf-8') as f:
    f.writelines(new_lines)

print(f'Before: {len(lines)} lines')
print(f'After:  {len(new_lines)} lines')
print(f'Removed: {len(lines) - len(new_lines)} lines')