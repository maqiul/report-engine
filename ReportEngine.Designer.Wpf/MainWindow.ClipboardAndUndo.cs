using System;
using System.Linq;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
private void DeleteSelected()
{
    if (_template == null) return;
    var targets = NudgeRunner.ResolveTargets(_selectedElements, _selectedElement);
    if (targets.Count == 0) return;
    PushUndo();
    ElementDeleter.DeleteFromBands(_template.Bands, targets);
    _selectedElements.Clear();
    _selectedElement = null;
    MarkDirty();
    RefreshUI();
}

private void CutSelected()
{
    if (_selectedElement == null) return;
    CopySelected();
    DeleteSelected();
}

private void CopySelected()
{
    if (_selectedElement == null) return;
    _clipboardJson = ClipboardHelper.SerializeElement(_selectedElement, _parser);
    _statusText.Text = "已复制元素";
}

private void PasteElement()
{
    if (_clipboardJson == null || _template == null) return;
    var el = ClipboardPasteHelper.ParseAndOffset(_clipboardJson, _parser);
    if (el == null) return;
    InsertElement(el);
    _statusText.Text = "已粘贴元素";
}

private void DuplicateSelected()
{
    if (_selectedElement == null || _template == null) return;
    var band = _selectedBand ?? _template.Bands.FirstOrDefault();
    if (band == null) return;
    var newEl = ElementDuplicator.Duplicate(_selectedElement, _parser);
    if (newEl == null) return;
    PushUndo();
    band.Elements.Add(newEl);
    _selectedElement = newEl;
    _selectedElements.Clear();
    _selectedElements.Add(newEl);
    MarkDirty();
    RefreshUI();
    _statusText.Text = "已复制元素 (Ctrl+D)";
}

private void SelectAll()
{
    if (_template == null) return;
    _selectedElements.Clear();
    var band = _selectedBand ?? _template.Bands.FirstOrDefault();
    if (band != null)
    {
        foreach (var el in band.Elements)
            _selectedElements.Add(el);
        if (_selectedElements.Count > 0)
            _selectedElement = _selectedElements[0];
        _selectedBand = band;
    }
    RefreshUI();
}

internal void PushUndo()
{
    if (_template == null) return;
    try
    {
        var json = _parser.Serialize(_template);
        _undoStack.Add(json);
        if (_undoStack.Count > MaxUndo) _undoStack.RemoveAt(0);
        _redoStack.Clear();
    }
    catch { }
}

private void Undo()
{
    if (_undoStack.Count == 0 || _template == null) return;
    try
    {
        _redoStack.Add(_parser.Serialize(_template));
        var json = _undoStack[_undoStack.Count - 1];
        _undoStack.RemoveAt(_undoStack.Count - 1);
        _template = _parser.Parse(json);
        _selectedElement = null;
        _selectedBand = null;
        _dirty = true;
        RefreshUI();
        _statusText.Text = "已撤销";
    }
    catch { }
}

private void Redo()
{
    if (_redoStack.Count == 0 || _template == null) return;
    try
    {
        _undoStack.Add(_parser.Serialize(_template));
        var json = _redoStack[_redoStack.Count - 1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
        _template = _parser.Parse(json);
        _selectedElement = null;
        _selectedBand = null;
        _dirty = true;
        RefreshUI();
        _statusText.Text = "已重做";
    }
    catch { }
}

private void UpdateUndoRedoButtons()
{
    if (_undoBtn != null)
        _undoBtn.IsEnabled = _undoStack.Count > 0;
    if (_redoBtn != null)
        _redoBtn.IsEnabled = _redoStack.Count > 0;

    // 剪切/复制/删除：需要选中元素
    bool hasSelection = _selectedElement != null;
    if (_cutBtn != null)
        _cutBtn.IsEnabled = hasSelection;
    if (_copyBtn != null)
        _copyBtn.IsEnabled = hasSelection;
    if (_deleteBtn != null)
        _deleteBtn.IsEnabled = hasSelection;

    // 粘贴：需要剪贴板有内容
    if (_pasteBtn != null)
        _pasteBtn.IsEnabled = _clipboardJson != null;
}

    }
}
