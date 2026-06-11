"""
Step3.D driver: 抽离 MainWindow 中的 ShowFontDialog 到 FontDialog.cs

执行步骤:
  1) 改 MainWindow 调用点: ShowFontDialog(t) -> FontDialog.Show(this, t)
  2) 删除 ShowFontDialog 方法定义（用 find_method_body 定位）

仅 1 个调用方 (AddPropFontRow 内的 btn.Click lambda) + 1 个定义, 比
PageSetupDialog 简单得多, 不需要 wrapper。
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from analyze_methods import find_method_body  # type: ignore

cs_path = Path('ReportEngine.Designer.Wpf/MainWindow.cs')
text = cs_path.read_text(encoding='utf-8')

# 1) 改调用点
old_call = 'ShowFontDialog(t)'
new_call = 'FontDialog.Show(this, t)'
n = text.count(old_call)
assert n == 1, f'调用点数量不对: {n} (期望 1) -- 可能已被替换'
text = text.replace(old_call, new_call, 1)
print(f'调用点已改: {old_call!r} -> {new_call!r}')

# 2) 定位并删除 ShowFontDialog 方法
lines = text.splitlines(keepends=True)
result_tuple = find_method_body(lines, 'ShowFontDialog')
assert result_tuple is not None, 'ShowFontDialog 定义未找到'
start_line_1based, end_line_1based = result_tuple
start_idx = start_line_1based - 1
end_idx = end_line_1based - 1

# 删除 [start_idx, end_idx]
new_lines = lines[:start_idx] + lines[end_idx + 1:]

# 清理连续空行
result_lines = []
prev_blank = False
for line in new_lines:
    is_blank = line.strip() == ''
    if is_blank and prev_blank:
        continue
    result_lines.append(line)
    prev_blank = is_blank

cs_path.write_text(''.join(result_lines), encoding='utf-8')
print(f'删除 ShowFontDialog: lines {start_line_1based}..{end_line_1based} ({end_idx - start_idx + 1} 行)')
print(f'文件: {sum(len(l) for l in result_lines)} bytes, {len(result_lines)} lines')
