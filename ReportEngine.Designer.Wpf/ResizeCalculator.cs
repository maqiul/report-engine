namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 调整手柄坐标计算 - 从 OnCanvasMouseMove ResizeElement 分支抽出。
/// 等价抽离自 MainWindow.OnCanvasMouseMove() ResizeElement switch (12 行)。
///
/// 8 个手柄位置 (0-7):
///   0=NW  1=N  2=NE  3=E  4=SE  5=S  6=SW  7=W
/// 根据 dx/dy 调整 (newX, newY, newW, newH)。
/// </summary>
internal static class ResizeCalculator
{
    public static (double x, double y, double w, double h) Compute(
        int handle, double dx, double dy,
        double startX, double startY, double startW, double startH)
    {
        double newX = startX, newY = startY, newW = startW, newH = startH;
        switch (handle)
        {
            case 0: newX += dx; newY += dy; newW -= dx; newH -= dy; break; // NW
            case 1: newY += dy; newH -= dy; break;                            // N
            case 2: newY += dy; newW += dx; newH -= dy; break;                // NE
            case 3: newW += dx; break;                                        // E
            case 4: newW += dx; newH += dy; break;                            // SE
            case 5: newH += dy; break;                                        // S
            case 6: newX += dx; newW -= dx; newH += dy; break;                // SW
            case 7: newX += dx; newW -= dx; break;                            // W
        }
        return (newX, newY, newW, newH);
    }
}
