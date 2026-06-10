"""Rename BandTypeName( to Name( in MainWindow.cs (BandStyle.Name replaces it)."""
from pathlib import Path

p = Path("ReportEngine.Designer.Wpf/MainWindow.cs")
t = p.read_text(encoding="utf-8")
n = t.count("BandTypeName(")
t = t.replace("BandTypeName(", "Name(")
p.write_text(t, encoding="utf-8")
print(f"BandTypeName( -> Name(: {n}")