"""
Step3.E driver: 抽离 MainWindow 中的 ShowExpressionEditor + AddExprBtn 到 ExpressionEditorDialog.cs

执行步骤:
  1) 改 MainWindow 调用点: ShowExpressionEditor(tb.Text, v => ...) -> ExpressionEditorDialog.Show(this, tb.Text, v => ...)
  2) 删除 ShowExpressionEditor 方法定义 (line 2990-?)
  3) 删除 AddExprBtn 方法定义 (line ?-?)

仅 1 个调用方 (AddPropExpr 内的 exprBtn.Click lambda), 走 step3b 模式.
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from analyze_methods import find_method_body  # type: ignore

cs_path = Path('ReportEngine.Designer.Wpf/MainWindow.cs')
text = cs_path.read_text(encoding='utf-8')

# 1) 改调用点
old_call = 'ShowExpressionEditor(tb.Text,'
new_call = 'ExpressionEditorDialog.Show(this, tb.Text,'
n = text.count(old_call)
assert n == 1, f'调用点数量不对: {n} (期望 1) -- 可能已被替换'
text = text.replace(old_call, new_call, 1)
print(f'调用点已改: ShowExpressionEditor(...) -> ExpressionEditorDialog.Show(this, ...)')

# 2) 定位并删除 ShowExpressionEditor 方法
lines = text.splitlines(keepends=True)
result = find_method_body(lines, 'ShowExpressionEditor')
assert result is not None, 'ShowExpressionEditor 定义未找到'
show_start_1, show_end_1 = result
show_start_idx = show_start_1 - 1
show_end_idx = show_end_1 - 1

# 3) 定位并删除 AddExprBtn 方法
result2 = find_method_body(lines, 'AddExprBtn')
assert result2 is not None, 'AddExprBtn 定义未找到'
add_start_1, add_end_1 = result2
add_start_idx = add_start_1 - 1
add_end_idx = add_end_1 - 1

# 取合并范围: [min(start), max(end)]
delete_start = min(show_start_idx, add_start_idx)
delete_end = max(show_end_idx, add_end_idx)

# 同时删除 ShowExpressionEditor 上面的标题注释 (// ============================== 表达式编辑器 ==============================)
# 向上扫描直到遇到 '}' 或空行+其它注释块边界. 这里简化: 直接在 show_start 向上多删 1 行(注释行)
extra_header = 0
for k in range(show_start_idx - 1, max(0, show_start_idx - 5) - 1, -1):
    line = lines[k].strip()
    if line.startswith('// =============================='):
        extra_header = show_start_idx - k
        break
    if line and not line.startswith('//'):
        break

delete_start -= extra_header
print(f'删除 ShowExpressionEditor: lines {show_start_1 - extra_header}..{show_end_1} ({show_end_1 - show_start_1 + 1 + extra_header} 行)')
print(f'删除 AddExprBtn: lines {add_start_1}..{add_end_1} ({add_end_1 - add_start_1 + 1} 行)')

new_lines = lines[:delete_start] + lines[delete_end + 1:]

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
print(f'文件: {sum(len(l) for l in result_lines)} bytes, {len(result_lines)} lines')
