"""
Step3.C driver: 抽离 MainWindow 中的 ShowPageSetupDialog 到 PageSetupDialog.cs

策略: 不动 7 个调用点 (MakeToolBtn/MakeMenuItem/MakeStatusTab/属性面板的
muEditBtn/muEnableBtn/右键菜单), 它们都是 ShowPageSetupDialog() 方法组传
Action。把原方法替换为 1 行 wrapper:
    private void ShowPageSetupDialog()
    {
        if (_template == null) return;
        PageSetupDialog.Show(this, _template, () => {
            PushUndo(); MarkDirty(); RefreshUI();
        });
    }
然后删除原方法的 262 行实现, 只留 6 行 wrapper。

注意: 原方法 line 2958 起始, 约 3220 结束 (3220 之后是 // === 字体选择弹窗
===)。
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from analyze_methods import find_method_body  # type: ignore

cs_path = Path('ReportEngine.Designer.Wpf/MainWindow.cs')
text = cs_path.read_text(encoding='utf-8')

lines = text.splitlines(keepends=True)

# 1) 定位原 ShowPageSetupDialog 方法的精确范围
result = find_method_body(lines, 'ShowPageSetupDialog')
assert result is not None, 'ShowPageSetupDialog 定义未找到'
start_1based, end_1based = result
start_idx = start_1based - 1  # 0-based
end_idx = end_1based - 1      # 0-based, 含 }
print(f'原方法: lines {start_1based}..{end_1based} ({end_idx - start_idx + 1} 行)')

# 2) 替换为 1 行 wrapper
wrapper = '''        private void ShowPageSetupDialog()
        {
            if (_template == null) return;
            PageSetupDialog.Show(this, _template, () => {
                PushUndo(); MarkDirty(); RefreshUI();
            });
        }

'''

# 3) 替换 [start_idx, end_idx] 区段（含前后注释行 // =========== 页面设置弹窗 ===========）
# 注释行是 start_idx - 1, 但要保留
replacement = wrapper
new_lines = lines[:start_idx] + [replacement] + lines[end_idx + 1:]

# 4) 清理多余空行
result_lines = []
prev_blank = False
for line in new_lines:
    is_blank = line.strip() == ''
    if is_blank and prev_blank:
        continue
    result_lines.append(line)
    prev_blank = is_blank

cs_path.write_text(''.join(result_lines), encoding='utf-8')
print(f'替换完成: 原 {end_idx - start_idx + 1} 行 -> wrapper 6 行 + 1 注释')
print(f'文件: {sum(len(l) for l in result_lines)} bytes, {len(result_lines)} lines')
