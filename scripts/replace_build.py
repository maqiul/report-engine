with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    content = f.read()

before = content.count('BuildCanvasRenderContext()')
content = content.replace('BuildCanvasRenderContext()', 'Build()')
after = content.count('BuildCanvasRenderContext()')

with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','w',encoding='utf-8') as f:
    f.write(content)

print(f'Replaced: {before} occurrences -> {after} remaining')