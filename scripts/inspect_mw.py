import io
with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()
print('Total lines:', len(lines))
for n in [2830, 2831, 2984, 2985, 2986, 2987, 2988]:
    print(f'Line {n+1}: {repr(lines[n])}')