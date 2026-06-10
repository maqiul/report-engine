"""
Step3.B driver: 抽离 MainWindow 中的 ShowColorPicker 到 ColorPickerDialog.cs

执行步骤:
  1) 改 MainWindow 调用点: ShowColorPicker(value ?? "") -> ColorPickerDialog.Show(this, value ?? "")
  2) 删除 ShowColorPicker 方法定义（用 find_method_body 定位）
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from analyze_methods import find_method_body  # type: ignore

cs_path = Path('ReportEngine.Designer.Wpf/MainWindow.cs')
text = cs_path.read_text(encoding='utf-8')

# 1) 改调用点
old_call = 'var picked = ShowColorPicker(value ?? "");'
new_call = 'var picked = ColorPickerDialog.Show(this, value ?? "");'
assert old_call in text, f'调用点未找到: {old_call!r}'
text = text.replace(old_call, new_call, 1)
print(f'调用点已改: {old_call!r} -> {new_call!r}')

# 2) 定位并删除 ShowColorPicker 方法
lines = text.splitlines(keepends=True)
result_tuple = find_method_body(lines, 'ShowColorPicker')
assert result_tuple is not None, 'ShowColorPicker 定义未找到'
start_line_1based, end_line_1based = result_tuple
start_idx = start_line_1based - 1  # 0-based
end_idx = end_line_1based - 1      # 0-based, 含 }

# 删除 [start_idx, end_idx]（含闭合 }）
new_lines = lines[:start_idx] + lines[end_idx + 1:]

# 清理紧接其后的空行
result = []
prev_blank = False
for line in new_lines:
    is_blank = line.strip() == ''
    if is_blank and prev_blank:
        continue
    result.append(line)
    prev_blank = is_blank

cs_path.write_text(''.join(result), encoding='utf-8')
print(f'删除 ShowColorPicker: lines {start_line_1based}..{end_line_1based} ({end_idx - start_idx + 1} 行)')
print(f'文件: {sum(len(l) for l in result)} bytes, {len(result)} lines')
