using ReportEngine.Core.Data;
using ReportEngine.Core.SubReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportEngine.Core.Rendering;

/// <summary>
/// 报表渲染引擎
/// 核心职责：
///   1. 解析模板 → 计算布局 → 分页
///   2. 处理子报表嵌套（递归渲染）
///   3. 生成页面模型（供 PDF/打印 使用）
///
/// 注意：此类是纯布局引擎，不依赖任何 PDF 库
/// PDF 导出、打印等由下游的 PdfRenderer / PrintService 实现
/// </summary>
public class ReportRenderer
{
    private readonly ExpressionEngine _expressionEngine = new();
    private readonly ITemplateResolver _templateResolver;

    public ReportRenderer(ITemplateResolver templateResolver)
    {
        _templateResolver = templateResolver;
    }

    /// <summary>
    /// 渲染模板为页面列表
    /// </summary>
    public async Task<RenderedReport> RenderAsync(
        ReportTemplate template,
        Dictionary<string, List<Dictionary<string, object>>> dataSources)
    {
        // 多联打印：将数据按每联份数分批，每批独立渲染后拼版到一张物理页
        if (template.Page.MultiUp != null && template.Page.MultiUp.Count > 1)
        {
            return await RenderMultiUpAsync(template, dataSources);
        }

        var context = new RenderContext
        {
            PageWidth = template.Page.Width,
            PageHeight = template.Page.Height,
        };

        // 加载数据源
        foreach (var kvp in dataSources)
        {
            context.DataSources[kvp.Key] = kvp.Value;
        }

        // 计算总页数（需两次遍历：第一次计算布局，第二次生成页面）
        var layoutResult = await CalculateLayoutAsync(template, context);

        return layoutResult;
    }

    /// <summary>
    /// 多联打印渲染：
    /// Page.Width/Height = 整张纸尺寸
    /// 等分为 Rows×Columns 份，每份 = cellW×cellH
    /// 每条记录按单联尺寸渲染，然后平铺到物理页上
    /// </summary>
    private async Task<RenderedReport> RenderMultiUpAsync(
        ReportTemplate template,
        Dictionary<string, List<Dictionary<string, object>>> dataSources)
    {
        var mu = template.Page.MultiUp!;
        int rows = Math.Max(1, mu.Rows);
        int cols = Math.Max(1, mu.Columns);
        int perPage = rows * cols;

        // 物理纸张尺寸 = Page.Width/Height
        double physW = template.Page.Width;
        double physH = template.Page.Height;

        // 单联尺寸 = 纸张等分（扣除间距）
        double cellW = (physW - mu.HSpacing * (cols - 1)) / cols;
        double cellH = (physH - mu.VSpacing * (rows - 1)) / rows;
        if (cellW <= 0 || cellH <= 0) return new RenderedReport { Template = template, PageWidth = physW, PageHeight = physH };

        // 找到主数据源（第一个 Detail Band 的数据源）
        var detailBand = template.Bands.FirstOrDefault(b => b.Type == BandType.Detail);
        string? mainDsName = detailBand?.DataSource;
        var mainRows = new List<Dictionary<string, object>>();
        if (mainDsName != null && dataSources.TryGetValue(mainDsName, out var mr))
            mainRows = mr;

        var result = new RenderedReport
        {
            Template = template,
            PageWidth = physW,
            PageHeight = physH,
        };

        // 按 perPage 条记录分批
        int totalRecords = Math.Max(1, mainRows.Count);
        for (int batchStart = 0; batchStart < totalRecords; batchStart += perPage)
        {
            var physPage = new RenderedPage();

            for (int slot = 0; slot < perPage; slot++)
            {
                int recordIndex = batchStart + slot;
                if (recordIndex >= mainRows.Count) break;

                // 为当前记录构造单条数据源
                var singleRowDs = new Dictionary<string, List<Dictionary<string, object>>>();
                foreach (var kvp in dataSources)
                {
                    if (kvp.Key == mainDsName)
                        singleRowDs[kvp.Key] = new List<Dictionary<string, object>> { mainRows[recordIndex] };
                    else
                        singleRowDs[kvp.Key] = kvp.Value;
                }

                // 以单联尺寸渲染这条记录
                var ctx = new RenderContext
                {
                    PageWidth = cellW,
                    PageHeight = cellH,
                };
                foreach (var kvp in singleRowDs)
                    ctx.DataSources[kvp.Key] = kvp.Value;

                // 临时构造单联模板（页面尺寸=单联大小，无边距）
                var cellTemplate = new ReportTemplate
                {
                    Page = new PageInfo { Width = cellW, Height = cellH, Margin = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 } },
                    Bands = template.Bands,
                    DataSources = template.DataSources,
                };
                var singleResult = await CalculateLayoutAsync(cellTemplate, ctx);

                // 取第一页的元素
                if (singleResult.Pages.Count == 0) continue;
                var logicalPage = singleResult.Pages[0];

                // 计算该 slot 在物理页上的位置偏移
                int slotRow, slotCol;
                if (mu.Direction == "Vertical")
                {
                    slotCol = slot / rows;
                    slotRow = slot % rows;
                }
                else // Horizontal (Z字形)
                {
                    slotRow = slot / cols;
                    slotCol = slot % cols;
                }

                double offsetX = slotCol * (cellW + mu.HSpacing);
                double offsetY = slotRow * (cellH + mu.VSpacing);

                // 直接平移元素到对应槽位（不缩放）
                foreach (var el in logicalPage.Elements)
                {
                    physPage.Elements.Add(OffsetElement(el, offsetX, offsetY));
                }
            }

            result.Pages.Add(physPage);
        }

        // 回填页码
        for (int i = 0; i < result.Pages.Count; i++)
        {
            result.Pages[i].PageNumber = i + 1;
            result.Pages[i].TotalPages = result.Pages.Count;
        }

        return result;
    }

    /// <summary>
    /// 核心布局计算：
    ///   1. 渲染 ReportHeader（只第一页一次）
    ///   2. 每页顶部渲染 Header
    ///   3. 遍历明细行，渲染 Detail Band（带 Y 偏移）
    ///   4. 子报表内容嵌入父页（带相对偏移）
    ///   5. 分页：当前 Y 超出可用高度时换页
    ///   6. 末页渲染 ReportFooter；每页底部渲染 Footer
    /// </summary>
    private async Task<RenderedReport> CalculateLayoutAsync(
        ReportTemplate template, RenderContext context)
    {
        var renderedReport = new RenderedReport
        {
            Template = template,
            PageWidth = context.PageWidth,
            PageHeight = context.PageHeight,
        };

        var availableHeight = context.PageHeight
            - template.Page.Margin.Top
            - template.Page.Margin.Bottom;

        var headerBand     = template.Bands.FirstOrDefault(b => b.Type == BandType.Header);
        var footerBand     = template.Bands.FirstOrDefault(b => b.Type == BandType.Footer);
        var reportHeader   = template.Bands.FirstOrDefault(b => b.Type == BandType.ReportHeader);
        var reportFooter   = template.Bands.FirstOrDefault(b => b.Type == BandType.ReportFooter);

        var currentPage = new RenderedPage();
        double currentY = 0;

        // --- 1. 第一页：ReportHeader ---
        if (reportHeader != null)
        {
            currentPage.Elements.AddRange(RenderBandElementsAt(reportHeader, context, currentY));
            currentY += CalculateBandHeight(reportHeader);
        }

        // --- 2. 第一页：Header ---
        if (headerBand != null)
        {
            currentPage.Elements.AddRange(RenderBandElementsAt(headerBand, context, currentY));
            currentY += CalculateBandHeight(headerBand);
        }

        // --- 3. Detail 主循环 ---
        var detailBands = template.Bands.Where(b => b.Type == BandType.Detail).ToList();

        foreach (var detailBand in detailBands)
        {
            if (string.IsNullOrEmpty(detailBand.DataSource))
                continue;

            var dataSourceName = detailBand.DataSource;
            context.DataSourceName = dataSourceName;

            var rows = context.DataSources.TryGetValue(dataSourceName, out var r) ? r : new();

            // 多栏打印路径
            if (detailBand.MultiColumn != null && detailBand.MultiColumn.ColumnCount > 1)
            {
                var mc = detailBand.MultiColumn;
                int colCount = mc.ColumnCount;
                double pageContentWidth = template.Page.Width - template.Page.Margin.Left - template.Page.Margin.Right;
                double colWidth = (pageContentWidth - mc.ColumnSpacing * (colCount - 1)) / colCount;
                double bandH = CalculateBandHeight(detailBand);
                int colIndex = 0;
                double mcStartY = currentY;

                for (int i = 0; i < rows.Count; i++)
                {
                    context.CurrentRow = rows[i];
                    context.CurrentRowNumber = i + 1;

                    // 计算当前栏的 X 偏移
                    double colX = colIndex * (colWidth + mc.ColumnSpacing);

                    // 分页判断
                    var footerH2 = footerBand != null ? CalculateBandHeight(footerBand) : 0;
                    if (currentY + bandH > availableHeight - footerH2)
                    {
                        if (footerBand != null)
                        {
                            var fy = availableHeight - footerH2;
                            currentPage.Elements.AddRange(RenderBandElementsAt(footerBand, context, fy));
                        }
                        renderedReport.Pages.Add(currentPage);
                        currentPage = new RenderedPage();
                        currentY = 0;
                        mcStartY = 0;
                        colIndex = 0;
                        if (headerBand != null)
                        {
                            currentPage.Elements.AddRange(RenderBandElementsAt(headerBand, context, currentY));
                            currentY += CalculateBandHeight(headerBand);
                            mcStartY = currentY;
                        }
                        colX = 0;
                    }

                    // 渲染元素并偏移到对应栏
                    var elems = RenderBandElementsAt(detailBand, context, currentY);
                    foreach (var el in elems)
                    {
                        currentPage.Elements.Add(OffsetElement(el, colX, 0));
                    }

                    colIndex++;
                    if (colIndex >= colCount)
                    {
                        colIndex = 0;
                        currentY += bandH;
                    }
                }
                // 最后一行未填满也要推进 Y
                if (colIndex > 0) currentY += bandH;
                continue;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                context.CurrentRow = rows[i];
                context.CurrentRowNumber = i + 1;

                var detailBandHeight = CalculateBandHeight(detailBand);

                // 预渲染子报表，拿到元素列表 + 实际高度
                var subReportBatches = new List<(SubReportElement sub, List<RenderedElement> elements, double height)>();
                foreach (var subElement in detailBand.Elements.OfType<SubReportElement>())
                {
                    if (!subElement.RepeatPerRow) continue;
                    var (subElems, subH) = await RenderSubReportContentAsync(subElement, context);
                    subReportBatches.Add((subElement, subElems, subH));
                }
                var totalSubHeight = subReportBatches.Sum(b => b.height);
                var rowTotalHeight = detailBandHeight + totalSubHeight;

                // --- 分页判断 ---
                var footerHeight = footerBand != null ? CalculateBandHeight(footerBand) : 0;
                if (currentY + rowTotalHeight > availableHeight - footerHeight)
                {
                    // 渲染当前页 Footer（贴底）
                    if (footerBand != null)
                    {
                        var footerY = availableHeight - footerHeight;
                        currentPage.Elements.AddRange(RenderBandElementsAt(footerBand, context, footerY));
                    }

                    renderedReport.Pages.Add(currentPage);
                    currentPage = new RenderedPage();
                    currentY = 0;

                    // 新页 Header
                    if (headerBand != null)
                    {
                        currentPage.Elements.AddRange(RenderBandElementsAt(headerBand, context, currentY));
                        currentY += CalculateBandHeight(headerBand);
                    }
                }

                // 渲染当前行 Detail（带 Y 偏移，跳过子报表元素）
                currentPage.Elements.AddRange(RenderBandElementsAt(detailBand, context, currentY));
                var afterDetailY = currentY + detailBandHeight;

                // 嵌入子报表元素：以子报表元素左上角(sub.X, afterDetailY)为基准偏移
                double subY = afterDetailY;
                foreach (var (sub, elems, height) in subReportBatches)
                {
                    foreach (var e in elems)
                    {
                        currentPage.Elements.Add(OffsetElement(e, sub.X, subY));
                    }
                    subY += height;
                }
                currentY = subY;
            }
        }

        // --- 4. 末页：ReportFooter ---
        if (reportFooter != null)
        {
            currentPage.Elements.AddRange(RenderBandElementsAt(reportFooter, context, currentY));
            currentY += CalculateBandHeight(reportFooter);
        }

        // --- 5. 末页：Footer（贴底）---
        if (footerBand != null)
        {
            var footerH = CalculateBandHeight(footerBand);
            var footerY = availableHeight - footerH;
            currentPage.Elements.AddRange(RenderBandElementsAt(footerBand, context, footerY));
        }

        renderedReport.Pages.Add(currentPage);

        // --- 6. 回填总页数 ---
        context.TotalPages = renderedReport.Pages.Count;
        for (int i = 0; i < renderedReport.Pages.Count; i++)
        {
            renderedReport.Pages[i].PageNumber = i + 1;
            renderedReport.Pages[i].TotalPages = renderedReport.Pages.Count;
        }

        return renderedReport;
    }

    /// <summary>
    /// 渲染 band 元素并整体平移指定 Y 偏移
    /// </summary>
    private List<RenderedElement> RenderBandElementsAt(Band band, RenderContext context, double offsetY)
    {
        var raw = RenderBandElements(band, context);
        var result = new List<RenderedElement>(raw.Count);
        foreach (var e in raw)
            result.Add(OffsetElement(e, 0, offsetY));

        // 多层表头：递归渲染 SubBands
        if (band.SubBands != null)
        {
            double subY = offsetY + band.Height;
            foreach (var sub in band.SubBands)
            {
                result.AddRange(RenderBandElementsAt(sub, context, subY));
                subY += CalculateBandHeight(sub);
            }
        }

        return result;
    }

    /// <summary>
    /// 平移已渲染元素（class 版，手动复制属性）
    /// </summary>
    private static RenderedElement OffsetElement(RenderedElement el, double dx, double dy)
    {
        switch (el)
        {
            case RenderedTextElement t:
                return new RenderedTextElement
                {
                    Id = t.Id, X = t.X + dx, Y = t.Y + dy, Width = t.Width, Height = t.Height,
                    BackgroundColor = t.BackgroundColor, Border = t.Border,
                    Text = t.Text, Font = t.Font, Alignment = t.Alignment, Hyperlink = t.Hyperlink,
                };
            case RenderedImageElement i:
                return new RenderedImageElement
                {
                    Id = i.Id, X = i.X + dx, Y = i.Y + dy, Width = i.Width, Height = i.Height,
                    BackgroundColor = i.BackgroundColor, Border = i.Border, Source = i.Source,
                };
            case RenderedLineElement l:
                return new RenderedLineElement
                {
                    Id = l.Id, X = l.X + dx, Y = l.Y + dy, Width = l.Width, Height = l.Height,
                    BackgroundColor = l.BackgroundColor, Border = l.Border,
                    Direction = l.Direction, LineWidth = l.LineWidth, LineColor = l.LineColor,
                };
            case RenderedShapeElement s:
                return new RenderedShapeElement
                {
                    Id = s.Id, X = s.X + dx, Y = s.Y + dy, Width = s.Width, Height = s.Height,
                    BackgroundColor = s.BackgroundColor, Border = s.Border,
                    Shape = s.Shape, FillColor = s.FillColor, BorderRadius = s.BorderRadius,
                };
            case RenderedBarcodeElement bc:
                return new RenderedBarcodeElement
                {
                    Id = bc.Id, X = bc.X + dx, Y = bc.Y + dy, Width = bc.Width, Height = bc.Height,
                    BackgroundColor = bc.BackgroundColor, Border = bc.Border,
                    Value = bc.Value, Format = bc.Format, ForeColor = bc.ForeColor, BackColor = bc.BackColor, ShowText = bc.ShowText,
                };
            case RenderedTableElement tbl:
                return new RenderedTableElement
                {
                    Id = tbl.Id, X = tbl.X + dx, Y = tbl.Y + dy, Width = tbl.Width, Height = tbl.Height,
                    BackgroundColor = tbl.BackgroundColor, Border = tbl.Border,
                    RowCount = tbl.RowCount, ColCount = tbl.ColCount,
                    ColumnWidths = tbl.ColumnWidths, RowHeights = tbl.RowHeights,
                    Cells = tbl.Cells, BorderWidth = tbl.BorderWidth, BorderColor = tbl.BorderColor,
                };
            case RenderedCrossTabElement ctab:
                return new RenderedCrossTabElement
                {
                    Id = ctab.Id, X = ctab.X + dx, Y = ctab.Y + dy, Width = ctab.Width, Height = ctab.Height,
                    BackgroundColor = ctab.BackgroundColor, Border = ctab.Border,
                    RowCount = ctab.RowCount, ColCount = ctab.ColCount,
                    ColumnWidths = ctab.ColumnWidths, RowHeights = ctab.RowHeights,
                    Cells = ctab.Cells, BorderWidth = ctab.BorderWidth, BorderColor = ctab.BorderColor,
                };
            default:
                return el;
        }
    }

    /// <summary>
    /// 递归渲染子报表并返回（元素列表, 总高度）
    /// 与父报表不同，子报表采用流式不分页布局（供父报表嵌入）
    /// </summary>
    private async Task<(List<RenderedElement> elements, double height)> RenderSubReportContentAsync(
        SubReportElement subElement, RenderContext parentContext)
    {
        // 防无限递归
        if (parentContext.NestingDepth >= RenderContext.MaxNestingDepth)
        {
            return (new List<RenderedElement>(), subElement.Height);
        }

        // 1. 加载子模板
        var childTemplate = await _templateResolver.ResolveAsync(subElement.TemplateRef);

        // 2. 构建子报表上下文
        var childContext = new RenderContext
        {
            PageWidth = subElement.Width > 0 ? subElement.Width : childTemplate.Page.Width,
            PageHeight = childTemplate.Page.Height,
            NestingDepth = parentContext.NestingDepth + 1,
        };

        // 3. 解析 ParamMap（表达式在父上下文计算）
        var paramMap = new Dictionary<string, object>();
        foreach (var kvp in subElement.DataBinding.ParamMap)
        {
            var resolved = _expressionEngine.Evaluate(kvp.Value, parentContext);
            paramMap[kvp.Key] = resolved;
        }

        // 4. 准备子报表数据源（按参数过滤或继承父数据源）
        foreach (var ds in childTemplate.DataSources)
        {
            if (paramMap.Count > 0 && parentContext.DataSources.TryGetValue(ds.Name, out var childRows))
            {
                childContext.DataSources[ds.Name] = FilterRows(childRows, paramMap);
            }
            else if (parentContext.DataSources.TryGetValue(ds.Name, out var existingRows))
            {
                childContext.DataSources[ds.Name] = existingRows;
            }
            else
            {
                childContext.DataSources[ds.Name] = new List<Dictionary<string, object>>();
            }
        }

        // 5. 以“不分页”模式递归渲染
        return await RenderEmbeddedAsync(childTemplate, childContext);
    }

    /// <summary>
    /// 不分页的流式渲染（仅供子报表内嵌使用）
    /// 按 ReportHeader → Header → Detail×N → Footer → ReportFooter 顺序堆叠
    /// </summary>
    private async Task<(List<RenderedElement> elements, double height)> RenderEmbeddedAsync(
        ReportTemplate template, RenderContext context)
    {
        var elements = new List<RenderedElement>();
        double y = 0;

        var rh = template.Bands.FirstOrDefault(b => b.Type == BandType.ReportHeader);
        if (rh != null)
        {
            elements.AddRange(RenderBandElementsAt(rh, context, y));
            y += CalculateBandHeight(rh);
        }

        var h = template.Bands.FirstOrDefault(b => b.Type == BandType.Header);
        if (h != null)
        {
            elements.AddRange(RenderBandElementsAt(h, context, y));
            y += CalculateBandHeight(h);
        }

        foreach (var detail in template.Bands.Where(b => b.Type == BandType.Detail))
        {
            if (string.IsNullOrEmpty(detail.DataSource)) continue;

            context.DataSourceName = detail.DataSource;
            var rows = context.DataSources.TryGetValue(detail.DataSource, out var r) ? r : new();

            for (int i = 0; i < rows.Count; i++)
            {
                context.CurrentRow = rows[i];
                context.CurrentRowNumber = i + 1;

                elements.AddRange(RenderBandElementsAt(detail, context, y));
                var detailH = CalculateBandHeight(detail);

                // 嵌套子报表（递归支持）
                double subY = y + detailH;
                foreach (var subElement in detail.Elements.OfType<SubReportElement>())
                {
                    if (!subElement.RepeatPerRow) continue;
                    var (subElems, subH) = await RenderSubReportContentAsync(subElement, context);
                    foreach (var e in subElems)
                        elements.Add(OffsetElement(e, subElement.X, subY));
                    subY += subH;
                }

                y = Math.Max(y + detailH, subY);
            }
        }

        var f = template.Bands.FirstOrDefault(b => b.Type == BandType.Footer);
        if (f != null)
        {
            elements.AddRange(RenderBandElementsAt(f, context, y));
            y += CalculateBandHeight(f);
        }

        var rf = template.Bands.FirstOrDefault(b => b.Type == BandType.ReportFooter);
        if (rf != null)
        {
            elements.AddRange(RenderBandElementsAt(rf, context, y));
            y += CalculateBandHeight(rf);
        }

        return (elements, y);
    }

    /// <summary>
    /// 简单行过滤
    /// </summary>
    private List<Dictionary<string, object>> FilterRows(
        List<Dictionary<string, object>> rows,
        Dictionary<string, object> paramMap)
    {
        if (paramMap.Count == 0) return rows;

        var firstParam = paramMap.First();
        return rows.Where(row =>
        {
            if (row.TryGetValue(firstParam.Key, out var val))
                return Equals(val, firstParam.Value);
            return false;
        }).ToList();
    }

    /// <summary>
    /// 渲染 band 中的所有元素（替换表达式）
    /// </summary>
    private List<RenderedElement> RenderBandElements(Band band, RenderContext context)
    {
        var rendered = new List<RenderedElement>();

        foreach (var element in band.Elements)
        {
            // 跳过子报表（已单独处理）
            if (element is SubReportElement) continue;

            var renderedElement = RenderElement(element, context);
            if (renderedElement != null)
                rendered.Add(renderedElement);
        }

        return rendered;
    }

    /// <summary>
    /// 渲染单个元素
    /// </summary>
    private RenderedElement? RenderElement(ReportElement element, RenderContext context)
    {
        // 检查可见性条件
        if (!string.IsNullOrEmpty(element.VisibleExpression))
        {
            var isVisible = _expressionEngine.Evaluate(element.VisibleExpression, context);
            if (!Convert.ToBoolean(isVisible))
                return null;
        }

        return element switch
        {
            TextElement text => RenderTextElement(text, context),
            ImageElement img => new RenderedImageElement
            {
                Id = img.Id, X = img.X, Y = img.Y,
                Width = img.Width, Height = img.Height,
                Source = img.Source,
                BackgroundColor = img.BackgroundColor,
                Border = img.Border,
            },
            LineElement line => new RenderedLineElement
            {
                Id = line.Id, X = line.X, Y = line.Y,
                Width = line.Width, Height = line.Height,
                Direction = line.Direction,
                LineWidth = line.LineWidth,
                LineColor = line.LineColor,
            },
            ShapeElement shape => new RenderedShapeElement
            {
                Id = shape.Id, X = shape.X, Y = shape.Y,
                Width = shape.Width, Height = shape.Height,
                Shape = shape.Shape,
                FillColor = shape.FillColor,
                BorderRadius = shape.BorderRadius,
            },
            BarcodeElement barcode => new RenderedBarcodeElement
            {
                Id = barcode.Id, X = barcode.X, Y = barcode.Y,
                Width = barcode.Width, Height = barcode.Height,
                Value = _expressionEngine.Evaluate(barcode.Value, context),
                Format = barcode.Format,
                ForeColor = barcode.ForeColor,
                BackColor = barcode.BackColor,
                ShowText = barcode.ShowText,
            },
            TableElement table => RenderTableElement(table, context),
            CrossTabElement ct => RenderCrossTabElement(ct, context),
            _ => null
        };
    }

    private RenderedTableElement RenderTableElement(TableElement table, RenderContext context)
    {
        var rendered = new RenderedTableElement
        {
            Id = table.Id, X = table.X, Y = table.Y,
            Width = table.Width, Height = table.Height,
            RowCount = table.RowCount, ColCount = table.ColCount,
            ColumnWidths = table.ColumnWidths.Count > 0 ? table.ColumnWidths : EqualSplit(table.Width, table.ColCount),
            RowHeights = table.RowHeights.Count > 0 ? table.RowHeights : EqualSplit(table.Height, table.RowCount),
            BorderWidth = table.BorderWidth,
            BorderColor = table.BorderColor,
        };
        foreach (var cell in table.Cells)
        {
            rendered.Cells.Add(new RenderedTableCell
            {
                Row = cell.Row, Col = cell.Col,
                RowSpan = cell.RowSpan, ColSpan = cell.ColSpan,
                Text = _expressionEngine.Evaluate(cell.Text, context),
                Font = cell.Font,
                Alignment = cell.Alignment,
                BackgroundColor = cell.BackgroundColor,
            });
        }
        return rendered;
    }

    private static List<double> EqualSplit(double total, int count)
    {
        var list = new List<double>();
        double each = count > 0 ? total / count : total;
        for (int i = 0; i < count; i++) list.Add(each);
        return list;
    }

    private RenderedCrossTabElement RenderCrossTabElement(CrossTabElement ct, RenderContext context)
    {
        // 1. 获取数据源
        var rows = new List<Dictionary<string, object>>();
        if (context.DataSources.ContainsKey(ct.DataSource))
        {
            var src = context.DataSources[ct.DataSource];
            if (src is IEnumerable<Dictionary<string, object>> enumerable)
            {
                rows = new List<Dictionary<string, object>>(enumerable);
            }
        }

        // 2. 提取唯一行/列值
        var rowValues = new List<string>();
        var colValues = new List<string>();
        foreach (var row in rows)
        {
            var rv = GetFieldValue(row, ct.RowFields);
            if (!rowValues.Contains(rv)) rowValues.Add(rv);
            var cv = GetFieldValue(row, ct.ColumnFields);
            if (!colValues.Contains(cv)) colValues.Add(cv);
        }

        // 3. 计算聊合矩阵
        int measureCount = ct.Measures.Count > 0 ? ct.Measures.Count : 1;
        int dataColCount = colValues.Count * measureCount;
        int totalColCount = ct.RowFields.Count + dataColCount + (ct.ShowColumnTotal ? measureCount : 0);
        int headerRows = 1; // 列头
        int totalRowCount = headerRows + rowValues.Count + (ct.ShowRowTotal ? 1 : 0);

        // 4. 生成单元格
        var cells = new List<RenderedTableCell>();

        // 表头行
        for (int c = 0; c < ct.RowFields.Count; c++)
        {
            cells.Add(new RenderedTableCell { Row = 0, Col = c, Text = ct.RowFields[c], Font = ct.HeaderFont, Alignment = TextAlignment.Center });
        }
        for (int ci = 0; ci < colValues.Count; ci++)
        {
            for (int mi = 0; mi < measureCount; mi++)
            {
                int col = ct.RowFields.Count + ci * measureCount + mi;
                var label = measureCount > 1 ? colValues[ci] + "/" + (ct.Measures[mi].Label ?? ct.Measures[mi].Field) : colValues[ci];
                cells.Add(new RenderedTableCell { Row = 0, Col = col, Text = label, Font = ct.HeaderFont, Alignment = TextAlignment.Center });
            }
        }
        if (ct.ShowColumnTotal)
        {
            for (int mi = 0; mi < measureCount; mi++)
            {
                int col = ct.RowFields.Count + dataColCount + mi;
                cells.Add(new RenderedTableCell { Row = 0, Col = col, Text = "合计", Font = ct.HeaderFont, Alignment = TextAlignment.Center });
            }
        }

        // 数据行
        for (int ri = 0; ri < rowValues.Count; ri++)
        {
            int row = headerRows + ri;
            // 行头
            for (int c = 0; c < ct.RowFields.Count; c++)
            {
                var parts = rowValues[ri].Split('|');
                cells.Add(new RenderedTableCell { Row = row, Col = c, Text = c < parts.Length ? parts[c] : "", Font = ct.CellFont, Alignment = TextAlignment.Left });
            }
            // 数据单元格
            double[] rowTotals = new double[measureCount];
            for (int ci = 0; ci < colValues.Count; ci++)
            {
                for (int mi = 0; mi < measureCount; mi++)
                {
                    var measure = ct.Measures.Count > mi ? ct.Measures[mi] : null;
                    double val = Aggregate(rows, ct.RowFields, rowValues[ri], ct.ColumnFields, colValues[ci], measure);
                    rowTotals[mi] += val;
                    int col = ct.RowFields.Count + ci * measureCount + mi;
                    cells.Add(new RenderedTableCell { Row = row, Col = col, Text = FormatValue(val, measure?.Format), Font = ct.CellFont, Alignment = TextAlignment.Right });
                }
            }
            // 行合计
            if (ct.ShowColumnTotal)
            {
                for (int mi = 0; mi < measureCount; mi++)
                {
                    int col = ct.RowFields.Count + dataColCount + mi;
                    cells.Add(new RenderedTableCell { Row = row, Col = col, Text = FormatValue(rowTotals[mi], ct.Measures.Count > mi ? ct.Measures[mi].Format : null), Font = ct.CellFont, Alignment = TextAlignment.Right });
                }
            }
        }

        // 列合计行
        if (ct.ShowRowTotal)
        {
            int row = headerRows + rowValues.Count;
            for (int c = 0; c < ct.RowFields.Count; c++)
            {
                cells.Add(new RenderedTableCell { Row = row, Col = c, Text = c == 0 ? "合计" : "", Font = ct.HeaderFont, Alignment = TextAlignment.Left });
            }
            for (int ci = 0; ci < colValues.Count; ci++)
            {
                for (int mi = 0; mi < measureCount; mi++)
                {
                    var measure = ct.Measures.Count > mi ? ct.Measures[mi] : null;
                    double val = AggregateColumn(rows, ct.ColumnFields, colValues[ci], measure);
                    int col = ct.RowFields.Count + ci * measureCount + mi;
                    cells.Add(new RenderedTableCell { Row = row, Col = col, Text = FormatValue(val, measure?.Format), Font = ct.HeaderFont, Alignment = TextAlignment.Right });
                }
            }
            if (ct.ShowColumnTotal)
            {
                for (int mi = 0; mi < measureCount; mi++)
                {
                    var measure = ct.Measures.Count > mi ? ct.Measures[mi] : null;
                    double val = AggregateAll(rows, measure);
                    int col = ct.RowFields.Count + dataColCount + mi;
                    cells.Add(new RenderedTableCell { Row = row, Col = col, Text = FormatValue(val, measure?.Format), Font = ct.HeaderFont, Alignment = TextAlignment.Right });
                }
            }
        }

        // 5. 计算列宽/行高
        double cellW = totalColCount > 0 ? ct.Width / totalColCount : 20;
        double cellH = totalRowCount > 0 ? ct.Height / totalRowCount : 6;
        var colWidths = new List<double>();
        for (int i = 0; i < totalColCount; i++) colWidths.Add(cellW);
        var rowHeights = new List<double>();
        for (int i = 0; i < totalRowCount; i++) rowHeights.Add(cellH);

        return new RenderedCrossTabElement
        {
            Id = ct.Id, X = ct.X, Y = ct.Y, Width = ct.Width, Height = ct.Height,
            BackgroundColor = ct.BackgroundColor, Border = ct.Border,
            RowCount = totalRowCount, ColCount = totalColCount,
            ColumnWidths = colWidths, RowHeights = rowHeights,
            Cells = cells, BorderWidth = ct.BorderWidth, BorderColor = ct.BorderColor,
        };
    }

    private static string GetFieldValue(Dictionary<string, object> row, List<string> fields)
    {
        var parts = new List<string>();
        foreach (var f in fields)
        {
            parts.Add(row.ContainsKey(f) ? Convert.ToString(row[f]) ?? "" : "");
        }
        return string.Join("|", parts);
    }

    private static double Aggregate(List<Dictionary<string, object>> rows, List<string> rowFields, string rowKey, List<string> colFields, string colKey, CrossTabMeasure? measure)
    {
        double sum = 0; int count = 0;
        foreach (var r in rows)
        {
            if (GetFieldValue(r, rowFields) == rowKey && GetFieldValue(r, colFields) == colKey)
            {
                double v = GetNumericValue(r, measure?.Field ?? "");
                sum += v; count++;
            }
        }
        return ApplyAggregate(sum, count, measure?.Aggregate ?? "Sum");
    }

    private static double AggregateColumn(List<Dictionary<string, object>> rows, List<string> colFields, string colKey, CrossTabMeasure? measure)
    {
        double sum = 0; int count = 0;
        foreach (var r in rows)
        {
            if (GetFieldValue(r, colFields) == colKey)
            {
                sum += GetNumericValue(r, measure?.Field ?? ""); count++;
            }
        }
        return ApplyAggregate(sum, count, measure?.Aggregate ?? "Sum");
    }

    private static double AggregateAll(List<Dictionary<string, object>> rows, CrossTabMeasure? measure)
    {
        double sum = 0; int count = 0;
        foreach (var r in rows)
        {
            sum += GetNumericValue(r, measure?.Field ?? ""); count++;
        }
        return ApplyAggregate(sum, count, measure?.Aggregate ?? "Sum");
    }

    private static double ApplyAggregate(double sum, int count, string agg)
    {
        switch (agg)
        {
            case "Count": return count;
            case "Avg": return count > 0 ? sum / count : 0;
            case "Min": return sum; // 简化处理
            case "Max": return sum;
            default: return sum; // Sum
        }
    }

    private static double GetNumericValue(Dictionary<string, object> row, string field)
    {
        if (string.IsNullOrEmpty(field) || !row.ContainsKey(field)) return 0;
        var val = row[field];
        if (val is double d) return d;
        if (val is int i) return i;
        if (val is long l) return l;
        if (val is decimal dc) return (double)dc;
        double.TryParse(Convert.ToString(val), out double result);
        return result;
    }

    private static string FormatValue(double val, string? format)
    {
        if (string.IsNullOrEmpty(format)) return val.ToString("N2");
        try { return val.ToString(format); }
        catch { return val.ToString("N2"); }
    }

    private RenderedTextElement RenderTextElement(TextElement text, RenderContext context)
    {
        context.FieldFormat = text.Format;
        var renderedText = _expressionEngine.Evaluate(text.Text, context);
        context.FieldFormat = null;

        return new RenderedTextElement
        {
            Id = text.Id,
            X = text.X,
            Y = text.Y,
            Width = text.Width,
            Height = text.Height,
            Text = renderedText,
            Font = text.Font,
            Alignment = text.Alignment,
            BackgroundColor = text.BackgroundColor,
            Border = text.Border,
            Hyperlink = text.Hyperlink,
        };
    }

    /// <summary>
    /// 计算 band 的渲染高度
    /// </summary>
    private double CalculateBandHeight(Band band)
    {
        double maxHeight = band.Height;

        foreach (var element in band.Elements)
        {
            var bottom = element.Y + element.Height;
            if (bottom > maxHeight)
                maxHeight = bottom;
        }

        // 多层表头：加上 SubBands 的高度
        if (band.SubBands != null)
        {
            foreach (var sub in band.SubBands)
            {
                maxHeight += CalculateBandHeight(sub);
            }
        }

        return maxHeight;
    }

    /// <summary>
    /// 计算页面内容总高度
    /// </summary>
    private double CalculatePageContentHeight(RenderedPage page)
    {
        return page.Elements.Max(e => e.Y + e.Height);
    }
}

// 渲染输出模型（RenderedReport / RenderedPage / RenderedElement / 具体元素类）
// 已拆分到 RenderedReport.cs 和 RenderedElements.cs，本文件仅保留 ReportRenderer 主类。
