"""
Step3.A driver: 抽离 MainWindow 中的中英文映射 + GetBandBrush 到 EnumCnMap.cs

执行步骤:
  1) 在 MainWindow.cs 顶部加 using static EnumCnMap;（如有）
  2) 用 analyze_methods.find_method_body 定位要删的方法块
  3) 删除这些方法块
  4) 保留 surrounding 区段 (空行 + 注释)
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from analyze_methods import find_method_body  # type: ignore

cs_path = Path('ReportEngine.Designer.Wpf/MainWindow.cs')
text = cs_path.read_text(encoding='utf-8')
lines = text.splitlines(keepends=True)

# 已知：line 3015 = GetBandBrush 开始，line 3081 = 最后一个 CNToChartType 后的空行
# 我们用 find_method_body 找精确结束
fn_names = [
    'GetBandBrush',          # 3015 - 未使用，删
    'ElementTypeName',       # 3034
    'AlignToCN',             # 3051
    'CNToAlign',             # 3052
    'DirToCN',               # 3054
    'CNToDir',               # 3055
    'ShapeToCN',             # 3057
    'CNToShape',             # 3058
    'SizingToCN',            # 3060
    'CNToSizing',            # 3061
    'BoxTypeToCN',           # 3063
    'GetTextElLabel',        # 3065
    'BcFmtToCN',             # 3076
    'CNToBcFmt',             # 3077
    'ChartTypeCN',           # 3079
    'CNToChartType',         # 3080
]

# 找每个方法的 1-based 起始行
starts = {}  # name -> 1-based start line
for i, line in enumerate(lines, 1):
    for fn in fn_names:
        if fn in starts:
            continue
        # 匹配：私有静态 + 返回类型 + 方法名 + (
        if (f'private static' in line and f'{fn}(' in line):
            starts[fn] = i

# 按起始行排序（保证按文件顺序处理）
sorted_fns = sorted(starts.items(), key=lambda x: x[1])
print('Found method starts:')
for fn, ln in sorted_fns:
    print(f'  {fn}: line {ln}')

# 找每个方法 body 结束（不删除后续注释 / 下一个方法或空行分隔）
def find_block_end(start_idx_0based):
    """从 0-based start_idx 开始找花括号匹配的结束（包含闭合 } 那一行）"""
    i = start_idx_0based
    # 找第一个 {
    while i < len(lines) and '{' not in lines[i]:
        i += 1
    if i >= len(lines):
        return start_idx_0based
    # 用深度计数
    depth = 0
    in_str = False
    in_verbatim = False
    str_char = ''
    j = i
    while j < len(lines):
        line = lines[j]
        k = 0
        while k < len(line):
            ch = line[k]
            if in_verbatim:
                if ch == '"' and k + 1 < len(line) and line[k+1] == '"':
                    k += 2
                    continue
                k += 1
                continue
            if in_str:
                if ch == '\\':
                    k += 2
                    continue
                if ch == str_char:
                    in_str = False
                k += 1
                continue
            if ch == '@' and k + 1 < len(line) and line[k+1] == '"':
                in_verbatim = True
                k += 2
                continue
            if ch == '"' or ch == "'":
                in_str = True
                str_char = ch
                k += 1
                continue
            if ch == '/' and k + 1 < len(line) and line[k+1] == '/':
                break  # 行注释
            if ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth == 0:
                    return j  # 0-based 结束行（含 }）
            k += 1
        j += 1
    return j - 1

# 计算每个方法要删的 [start, end] 范围（0-based，含闭合行）
blocks = []
for fn, ln1 in sorted_fns:
    start0 = ln1 - 1
    end0 = find_block_end(start0)
    blocks.append((fn, start0, end0))
    print(f'  {fn}: lines {ln1}..{end0+1} (body {end0+1 - ln1 + 1} lines)')

# 反向删除（从下往上）
# 同时把方法之间的空行保留 1 行
to_delete = set()
for fn, s, e in blocks:
    for i in range(s, e + 1):
        to_delete.add(i)

new_lines = []
i = 0
while i < len(lines):
    if i in to_delete:
        # 跳过这个块；保留紧贴的上一个空行（仅 1 个）
        i += 1
        continue
    new_lines.append(lines[i])
    i += 1

# 清理连续空行
result = []
prev_blank = False
for line in new_lines:
    is_blank = line.strip() == ''
    if is_blank and prev_blank:
        continue
    result.append(line)
    prev_blank = is_blank

# 在文件顶部加 using static（如还没有）
if 'using static EnumCnMap;' not in ''.join(result):
    # 找第一个 using ReportEngine.Core.Designer.Wpf; 或类似的位置
    insert_idx = 0
    for i, line in enumerate(result):
        if line.startswith('using '):
            insert_idx = i + 1
    result.insert(insert_idx, 'using static ReportEngine.Designer.Wpf.EnumCnMap;\n')

cs_path.write_text(''.join(result), encoding='utf-8')
print(f'\nDeleted {len(to_delete)} lines, added using static EnumCnMap')
print(f'File size: {sum(len(l) for l in result)} bytes, {len(result)} lines')
