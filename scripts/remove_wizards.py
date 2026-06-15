with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','r',encoding='utf-8') as f:
    lines = f.readlines()

print(f'Total before: {len(lines)} lines')

# 删除 ShowDataBindingWizard: 2837-2927 (line 索引 2836-2926)
# 删除 ShowTemplateParamsDialog + summary: 2928-3024 (line 索引 2927-3023)
# 删除 RefreshDsList: 3025-3034 (line 索引 3024-3033)
# 删除 RefreshFieldList: 3037-3042 (line 索引 3036-3041)
# 注意: 索引是0-based, line 1-based
# 验证锚点
for n, expected in [(2836, 'ShowDataBindingWizard'), (2926, 'btnPanel.Children.Add(btnBind);'),
                    (2927, '/// <summary>模板参数'), (2928, 'ShowTemplateParamsDialog'),
                    (3023, 'dlg.ShowDialog();'), (3024, 'RefreshDsList'),
                    (3033, 'if (list.Items.Count > 0) list.SelectedIndex = 0;'),
                    (3034, ''), (3035, 'private static void RefreshFieldList'),
                    (3041, 'list.Items.Add(f);'), (3042, '')]:
    print(f'Line {n+1}: {lines[n].rstrip()[:80]!r}  [expected: {expected!r}]')

# 从大到小删除(避免索引错位)
# 删除 RefreshFieldList (3037-3042)
del lines[3036:3042]
print(f'After Remove RefreshFieldList: {len(lines)} lines')

# 删除 RefreshDsList (3025-3034)
del lines[3024:3034]
print(f'After Remove RefreshDsList: {len(lines)} lines')

# 删除 ShowTemplateParamsDialog (2928-3024)
del lines[2927:3024]
print(f'After Remove ShowTemplateParamsDialog: {len(lines)} lines')

# 删除 ShowDataBindingWizard (2837-2927)
del lines[2836:2927]
print(f'After Remove ShowDataBindingWizard: {len(lines)} lines')

# 在 line 2836 之前(原 ShowDataBindingWizard位置)插入两个wrapper
# 用 lines.insert(idx, ...)
wrapper_dbw = '''        private void OnShowDataBindingWizardClicked()
        {
            if (_template == null || _selectedElement == null)
            {
                _statusText.Text = "请先选中一个元素再打开数据绑定向导";
                return;
            }
            if (_template.DataSources.Count == 0)
            {
                _statusText.Text = "请先添加数据源（文件→数据源）";
                return;
            }
            DataBindingWizardDialog.Show(this, _template, _selectedElement,
                (dsName, fieldName, propChoice, expression) =>
                {
                    PushUndo();
                    if (propChoice == "文本内容 (Text)" && _selectedElement is TextElement txt)
                    {
                        txt.Text = expression;
                    }
                    else if (propChoice == "可见性表达式 (VisibleExpression)")
                    {
                        _selectedElement.VisibleExpression = expression;
                    }
                    MarkDirty();
                    RefreshUI();
                    _statusText.Text = "已绑定: " + expression;
                });
        }

'''

wrapper_tpd = '''        private void OnShowTemplateParamsClicked()
        {
            if (_template == null) return;
            TemplateParamsDialog.Show(this, _template,
                () => { PushUndo(); MarkDirty(); });
        }

'''

# 在索引 2836 前插入 wrapper_dbw + wrapper_tpd
# 但 ShowDataBindingWizard 在删完后被 ShowTemplateParamsDialog 接续,所以 wrapper_tpd 在 wrapper_dbw 之前/之后取决于原顺序
# 原顺序: ShowDataBindingWizard (2837) -> ShowTemplateParamsDialog (2929)
# 所以插入顺序: wrapper_dbw 在前, wrapper_tpd 在后
lines.insert(2836, wrapper_dbw + wrapper_tpd)

with open(r'ReportEngine.Designer.Wpf\MainWindow.cs','w',encoding='utf-8') as f:
    f.writelines(lines)

print(f'Final: {len(lines)} lines')