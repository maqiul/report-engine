"""
Step4.A driver: 抽离 MainWindow 9 个 AddProp* 工厂方法到 PropertyRowFactory.cs

执行步骤:
  1) UpdatePropertyListCore 起头: 替换 _propertyStack.Children.Clear() + _currentExpander=null + _propRowIndex=0
     -> 改用 using var ctx = new PropertyRowContext(_propertyStack); 包裹 (但只能改函数体, 不便加 using,
        改为在初始化行后加 _ctx = new ...; 末尾 try/finally Dispose())
  2) 全文替换 9 个 AddProp* 调用名:
     AddPropSection -> ctx.AddSection
     AddPropLabel -> ctx.AddLabel
     AddPropEditor -> ctx.AddEditor
     AddPropExpr -> ctx.AddExpr
     AddPropColor -> ctx.AddColor
     AddPropBool -> ctx.AddBool
     AddPropCombo -> ctx.AddCombo
     AddPropFontRow -> ctx.AddFontRow
     AddPropToCurrentSection -> ctx.AddToCurrentSection
  3) 删除 9 个 AddProp* 方法 (line 2673-2850 + line 2965-2991)
  4) 删除 3 个 state 字段声明:
     private int _propRowIndex;          (line 2245)
     private Expander? _currentExpander;  (line 2673)

注意:
  - AddPropEditor / AddPropExpr / AddPropColor / AddPropFontRow 内部使用了 'this' (Window owner),
    抽到 ctx.AddXxx 后, 'this' 变为入参 'owner'。调用点要把 'this' 加到第一个参数位置
    (AddPropColor 原本是 (label, value, onCommit), 新签名是 (owner, label, value, onCommit))
"""
import re
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from analyze_methods import find_method_body  # type: ignore

cs_path = Path('ReportEngine.Designer.Wpf/MainWindow.cs')
text = cs_path.read_text(encoding='utf-8')

# === 步骤 1: 改 UpdatePropertyListCore 起头 ===
old_init = '''            _propertyStack.Children.Clear();
            _currentExpander = null;
            _propRowIndex = 0;
            if (_template == null) return;'''
new_init = '''            _propertyStack.Children.Clear();
            using var ctx = new PropertyRowContext(_propertyStack);
            if (_template == null) return;'''
assert text.count(old_init) == 1, f'init 块数量不对: {text.count(old_init)}'
text = text.replace(old_init, new_init, 1)
print('步骤 1: UpdatePropertyListCore 起头改为 using var ctx = ...')

# === 步骤 2: 全文替换调用名 ===
# AddProp* -> ctx.Add* (注意 AddPropToCurrentSection 也是)
# 用 'AddProp' 前缀整词匹配, 避免误伤其他 (如 'AddProperty' 这种 IDE 自动生成代码)
renames = [
    ('AddPropSection', 'ctx.AddSection'),
    ('AddPropLabel', 'ctx.AddLabel'),
    ('AddPropEditor', 'ctx.AddEditor'),
    ('AddPropExpr', 'ctx.AddExpr'),
    ('AddPropColor', 'ctx.AddColor'),
    ('AddPropBool', 'ctx.AddBool'),
    ('AddPropCombo', 'ctx.AddCombo'),
    ('AddPropFontRow', 'ctx.AddFontRow'),
    ('AddPropToCurrentSection', 'ctx.AddToCurrentSection'),
]
for old, new in renames:
    n = text.count(old)
    # 仅在调用点改 (用 'Word boundary' 模拟: 'AddProp' 后面跟非字母数字)
    pattern = re.compile(r'\b' + old + r'\b')
    matches = pattern.findall(text)
    text = pattern.sub(new, text)
    print(f'  替换 {old} -> {new}: {len(matches)} 处')

# === 步骤 3: AddPropColor / AddPropExpr / AddPropEditor / AddPropFontRow 内部 'this' -> owner ===
# 找到 'ctx.AddColor(this, ...' 'ctx.AddExpr(this, ...' 'ctx.AddEditor(this, ...' 'ctx.AddFontRow(this, ...'
# 但这些函数可能本来没传 this (只声明 3/4 个参数)
# 实际:
#   AddPropColor(string label, string value, Action<string> onCommit)  // 3 参数
#   AddPropExpr(string label, string value, Action<string> onCommit)   // 3 参数
#   AddPropEditor(string label, string value, Action<string> onCommit) // 3 参数
#   AddPropFontRow(TextElement t)                                       // 1 参数
# 抽离后签名:
#   AddColor(Window owner, string label, string value, Action<string> onCommit)  // 4 参数
#   AddExpr(Window owner, string label, string value, Action<string> onCommit)   // 4 参数
#   AddEditor(Window owner, string label, string value, Action<string> onCommit) // 4 参数
#   AddFontRow(Window owner, TextElement t)                                       // 2 参数
# 所以 ctx.AddColor( -> ctx.AddColor(this,   (在第一个参数位置插入 this,)
# 用正则匹配调用点 'ctx.AddColor(' 后面紧跟非 'this,' 的情况

# Pattern: ctx.AddColor( -> ctx.AddColor(this,
# 注意 AddColor/AddExpr/AddEditor 第一个参数原本是 string label
# ctx.AddColor("xxx", ...)  -> ctx.AddColor(this, "xxx", ...)
# ctx.AddColor(value, ...)  -> ctx.AddColor(this, value, ...)
# ctx.AddColor(t, ...)      -> ctx.AddColor(this, t, ...)  // AddFontRow 唯一 1 参数
insert_patterns = [
    (r'ctx\.AddColor\((?!this,)', 'ctx.AddColor(this, '),
    (r'ctx\.AddExpr\((?!this,)', 'ctx.AddExpr(this, '),
    (r'ctx\.AddEditor\((?!this,)', 'ctx.AddEditor(this, '),
    (r'ctx\.AddFontRow\((?!this,)', 'ctx.AddFontRow(this, '),
]
for pat, rep in insert_patterns:
    p = re.compile(pat)
    matches = p.findall(text)
    text = p.sub(rep, text)
    print(f'  插入 owner 参数: {pat} -> {len(matches)} 处')

# === 步骤 4: 删除 9 个 AddProp* 方法定义 ===
# 已用步骤 2 替换所有 'AddPropXxx' 为 'ctx.AddXxx', 方法定义处的 'private void AddPropXxx('
# 已变成 'private void ctx.AddXxx('. 这是 broken C# (ctx 是变量名) - 必须删除
methods_to_delete = [
    'AddSection', 'AddLabel', 'AddEditor', 'AddExpr', 'AddColor', 'AddBool', 'AddCombo', 'AddFontRow', 'AddToCurrentSection',
]
lines = text.splitlines(keepends=True)

# 找到所有方法定义行, 收集起来一次性删除 (按倒序删以避免行号偏移)
to_delete_ranges = []  # (start_idx_inclusive, end_idx_inclusive) in lines[]
for method in methods_to_delete:
    # 在 'private void ctx.AddXxx(' 形式中找 (步骤 2 替换后是这个形式)
    # 或 'private void AddXxx(' 形式 (如果步骤 2 没匹配上)
    sig1 = f'private void ctx.{method}('
    sig2 = f'private void {method}('
    found = None
    for i, line in enumerate(lines):
        if sig1 in line or sig2 in line:
            found = i
            break
    if found is None:
        print(f'  警告: 方法 {method} 定义未找到 (可能已删除)')
        continue
    # 找到 'private void ctx.AddXxx(' 之后, 向上找前一行 (注释或空行) 一起删
    # 向下找花括号深度配对的方法结束
    # 简化: 用 find_method_body 风格的深度计数
    # 但 sig 已变, 需用 lines 找原始名字的边界
    # 由于 'ctx.AddXxx' 是 sig1 形式, 我们需要回溯原始 'AddPropXxx' 已被步骤 2 替换
    # 所以找 'private void AddXxx(' 或 'private void ctx.AddXxx(' 都能定位
    # 起点是 found (行号 1-based: found+1)
    # 找花括号配对: 从 found 行开始找 '{' 然后深度计数
    brace_depth = 0
    started = False
    end_idx = found
    for j in range(found, len(lines)):
        for ch in lines[j]:
            if ch == '{':
                brace_depth += 1
                started = True
            elif ch == '}':
                brace_depth -= 1
        if started and brace_depth == 0:
            end_idx = j
            break
    # 向上扫描 3 行找注释/空行边界, 一起删 (干净)
    start_idx = found
    for k in range(found - 1, max(-1, found - 4) - 1, -1):
        if k < 0:
            break
        s = lines[k].strip()
        if s.startswith('//') or s == '':
            start_idx = k
        else:
            break
    to_delete_ranges.append((start_idx, end_idx, method))
    print(f'  标记删除: {method} (lines {start_idx + 1}..{end_idx + 1}, {end_idx - start_idx + 1} 行)')

# 倒序删除避免行号偏移
to_delete_ranges.sort(key=lambda x: x[0], reverse=True)
for start_idx, end_idx, method in to_delete_ranges:
    del lines[start_idx:end_idx + 1]

# === 步骤 5: 删除 3 个 state 字段声明 ===
# private int _propRowIndex;          (line 2245)
# private Expander? _currentExpander;  (line 2673, 已被删除)
# _propertyStack 保留 (readonly, 还要用)
field_to_delete = '        private int _propRowIndex;\n'
text2 = ''.join(lines)
if field_to_delete in text2:
    text2 = text2.replace(field_to_delete, '', 1)
    print('  删除字段: private int _propRowIndex;')
else:
    print('  警告: _propRowIndex 字段未找到')

# 清理连续空行
result_lines = []
prev_blank = False
for line in text2.splitlines(keepends=True):
    is_blank = line.strip() == ''
    if is_blank and prev_blank:
        continue
    result_lines.append(line)
    prev_blank = is_blank

cs_path.write_text(''.join(result_lines), encoding='utf-8')
print(f'文件: {sum(len(l) for l in result_lines)} bytes, {len(result_lines)} lines')
