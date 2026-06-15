with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    content = f.read()

# 改 _canvasRenderer.Render(Build(), ...) -> _canvasRenderer.Render(CanvasRenderContextFactory.Build(...))
before = content.count('Build()')
content = content.replace('Build()', 'CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY)')

with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','w',encoding='utf-8') as f:
    f.write(content)

print(f'Replaced: {before} occurrences')
print(f'Any remaining Build() without prefix: {content.count("Build()")}')