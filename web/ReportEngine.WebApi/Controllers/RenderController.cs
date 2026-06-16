using Microsoft.AspNetCore.Mvc;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.WebApi.Models;
using System.Text.Json;

namespace ReportEngine.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RenderController : ControllerBase
{
    private readonly TemplateParser _parser = new();
    private readonly InMemoryTemplateResolver _resolver = new();

    [HttpPost("preview")]
    public async Task<ActionResult<RenderResponse>> Preview([FromBody] RenderRequest request)
    {
        try
        {
            // 1. 解析模板
            var template = _parser.Parse(request.TemplateJson);

            // 2. 转换数据类型（object? -> object）
            var dataSources = ConvertData(request.Data);

            // 3. 渲染
            var renderer = new ReportRenderer(_resolver);
            var rendered = await renderer.RenderAsync(template, dataSources);

            // 4. 转换为前端格式
            var response = new RenderResponse
            {
                Success = true,
                Pages = rendered.Pages.Select((page, idx) => new Models.PageInfo
                {
                    PageNumber = idx + 1,
                    Width = rendered.PageWidth,
                    Height = rendered.PageHeight,
                    Elements = page.Elements.Select(ConvertElement).ToList()
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return Ok(new RenderResponse
            {
                Success = false,
                Error = ex.Message
            });
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

    private RenderedElementInfo ConvertElement(RenderedElement element)
    {
        var info = new RenderedElementInfo
        {
            X = element.X,
            Y = element.Y,
            Width = element.Width,
            Height = element.Height,
            BackgroundColor = element.BackgroundColor
        };

        switch (element)
        {
            case RenderedTextElement text:
                info.Type = "text";
                info.Text = text.Text;
                info.Alignment = text.Alignment.ToString().ToLower();
                info.Font = new FontInfo
                {
                    Family = text.Font.Family,
                    Size = text.Font.Size,
                    Bold = text.Font.Bold,
                    Italic = text.Font.Italic,
                    Underline = text.Font.Underline,
                    Color = text.Font.Color ?? "#000000"
                };
                break;

            case RenderedLineElement line:
                info.Type = "line";
                info.BorderColor = line.LineColor;
                info.BorderWidth = line.LineWidth;
                break;

            case RenderedShapeElement shape:
                info.Type = "shape";
                info.BackgroundColor = shape.FillColor;
                break;

            case RenderedImageElement image:
                info.Type = "image";
                info.Text = image.Source; // Base64 or URL
                break;

            case RenderedBarcodeElement barcode:
                info.Type = "barcode";
                info.Text = barcode.Value;
                break;

            case RenderedTableElement table:
                info.Type = "table";
                // 表格比较复杂，先简化处理
                break;

            default:
                info.Type = "unknown";
                break;
        }

        return info;
    }
}

/// <summary>
/// 内存模板解析器（用于 Web API，支持子报表）
/// </summary>
public class InMemoryTemplateResolver : ITemplateResolver
{
    private readonly Dictionary<string, ReportTemplate> _templates = new();
    private readonly TemplateParser _parser = new();

    public void Add(string name, string json)
    {
        _templates[name] = _parser.Parse(json);
    }

    public void Add(string name, ReportTemplate template)
    {
        _templates[name] = template;
    }

    public Task<ReportTemplate> ResolveAsync(string templateRef)
    {
        if (_templates.TryGetValue(templateRef, out var template))
            return Task.FromResult(template);
        throw new Exception($"Template '{templateRef}' not found");
    }

    public bool Exists(string templateRef) => _templates.ContainsKey(templateRef);
}
