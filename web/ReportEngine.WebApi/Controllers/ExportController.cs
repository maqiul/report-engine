using Microsoft.AspNetCore.Mvc;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Pdf;
using ReportEngine.Export.Excel;

namespace ReportEngine.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly TemplateParser _parser = new();
    private readonly InMemoryTemplateResolver _resolver = new();
    private readonly PdfSharpExporter _pdfExporter = new();
    private readonly ClosedXmlExporter _excelExporter = new();

    /// <summary>
    /// 导出 PDF
    /// </summary>
    [HttpPost("pdf")]
    public async Task<IActionResult> ExportPdf([FromBody] Models.RenderRequest request)
    {
        try
        {
            var template = _parser.Parse(request.TemplateJson);
            var dataSources = ConvertData(request.Data);

            var renderer = new ReportRenderer(_resolver);
            var rendered = await renderer.RenderAsync(template, dataSources);

            var pdfBytes = _pdfExporter.Export(rendered);

            return File(pdfBytes, "application/pdf", "report.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 导出 Excel
    /// </summary>
    [HttpPost("excel")]
    public async Task<IActionResult> ExportExcel([FromBody] Models.RenderRequest request)
    {
        try
        {
            var template = _parser.Parse(request.TemplateJson);
            var dataSources = ConvertData(request.Data);

            var renderer = new ReportRenderer(_resolver);
            var rendered = await renderer.RenderAsync(template, dataSources);

            var excelBytes = _excelExporter.Export(rendered);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Dictionary<string, List<Dictionary<string, object>>> ConvertData(
        Dictionary<string, List<Dictionary<string, object?>>> data)
    {
        var result = new Dictionary<string, List<Dictionary<string, object>>>();
        foreach (var kvp in data)
        {
            var rows = kvp.Value.Select(row =>
                row.ToDictionary(
                    k => k.Key,
                    v => v.Value ?? (object)""
                )).ToList();
            result[kvp.Key] = rows;
        }
        return result;
    }
}
