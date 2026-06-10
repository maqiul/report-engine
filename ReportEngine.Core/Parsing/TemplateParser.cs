using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ReportEngine.Core.Parsing;

/// <summary>
/// 模板解析器：将 .rptx JSON 字符串解析为 ReportTemplate 对象模型
/// 使用 Newtonsoft.Json 以确保 net462 / netstandard2.0 / net8 全 TFM 兼容
/// </summary>
public class TemplateParser
{
    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new List<JsonConverter>
        {
            new ReportElementConverter(),
            new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() }
        }
    };

    /// <summary>
    /// 从 JSON 字符串解析模板
    /// </summary>
    public ReportTemplate Parse(string json)
    {
        ReportTemplate? template;
        try
        {
            template = JsonConvert.DeserializeObject<ReportTemplate>(json, Settings);
        }
        catch (JsonException ex)
        {
            // 将底层 JSON 解析异常包装为业务异常，避免调用方依赖 Newtonsoft 类型
            throw new TemplateParseException("Failed to parse template JSON: " + ex.Message, ex);
        }

        if (template == null)
            throw new TemplateParseException("Failed to parse template: result is null.");
        ValidateTemplate(template);
        return template;
    }

    /// <summary>
    /// 从 .rptx 文件解析模板
    /// </summary>
    public ReportTemplate ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Template file not found: " + filePath);

        var json = File.ReadAllText(filePath);
        return Parse(json);
    }

    /// <summary>
    /// 将模板序列化为 JSON 字符串（用于保存/导出）
    /// </summary>
    public string Serialize(ReportTemplate template)
    {
        return JsonConvert.SerializeObject(template, Settings);
    }

    /// <summary>
    /// 模板合法性校验
    /// </summary>
    private void ValidateTemplate(ReportTemplate template)
    {
        if (template.Bands.Count == 0)
            throw new TemplateParseException("Template must have at least one band.");

        var dsNames = new HashSet<string>(template.DataSources.Select(ds => ds.Name));

        foreach (var band in template.Bands)
        {
            if (!string.IsNullOrEmpty(band.DataSource) && !dsNames.Contains(band.DataSource!))
            {
                throw new TemplateParseException(
                    "Band of type '" + band.Type + "' references unknown datasource '" + band.DataSource + "'.");
            }

            foreach (var element in band.Elements)
            {
                if (element is SubReportElement sub)
                {
                    if (string.IsNullOrEmpty(sub.TemplateRef))
                        throw new TemplateParseException("SubReport element has empty TemplateRef.");

                    if (!string.IsNullOrEmpty(sub.DataBinding.Source)
                        && !dsNames.Contains(sub.DataBinding.Source))
                    {
                        throw new TemplateParseException(
                            "SubReport references unknown datasource '" + sub.DataBinding.Source + "'.");
                    }
                }
            }
        }
    }
}

/// <summary>
/// 自定义 JSON 转换器：根据 "type" 字段反序列化为具体元素类型
/// </summary>
public class ReportElementConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(ReportElement).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObj = JObject.Load(reader);
        var typeProp = jObj["type"]?.ToString();

        if (string.IsNullOrEmpty(typeProp))
            throw new TemplateParseException("Report element must have a 'type' property.");

        ReportElement element = typeProp switch
        {
            "text"      => new TextElement(),
            "image"     => new ImageElement(),
            "line"      => new LineElement(),
            "shape"     => new ShapeElement(),
            "subreport" => new SubReportElement(),
            "chart"     => new ChartElement(),
            "barcode"   => new BarcodeElement(),
            "table"     => new TableElement(),
            "crosstab"  => new CrossTabElement(),
            _ => throw new TemplateParseException("Unknown element type: '" + typeProp + "'")
        };

        // 移除 type 字段后填充其余属性
        jObj.Remove("type");
        serializer.Populate(jObj.CreateReader(), element);
        return element;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null) { writer.WriteNull(); return; }

        var typeStr = value switch
        {
            TextElement      => "text",
            ImageElement     => "image",
            LineElement      => "line",
            ShapeElement     => "shape",
            SubReportElement => "subreport",
            ChartElement     => "chart",
            BarcodeElement   => "barcode",
            TableElement     => "table",
            CrossTabElement  => "crosstab",
            _ => throw new TemplateParseException("Unknown element type: " + value.GetType().Name)
        };

        // 用一个移除自身 converter 的子 serializer 序列化，避免递归
        var settings = new JsonSerializerSettings
        {
            ContractResolver = serializer.ContractResolver,
            NullValueHandling = NullValueHandling.Ignore,
        };
        foreach (var c in serializer.Converters)
        {
            if (c is ReportElementConverter) continue;
            settings.Converters.Add(c);
        }

        var jo = JObject.FromObject(value, JsonSerializer.Create(settings));
        jo.AddFirst(new JProperty("type", typeStr));
        jo.WriteTo(writer);
    }
}

public class TemplateParseException : Exception
{
    public TemplateParseException(string message) : base(message) { }
    public TemplateParseException(string message, Exception innerException) : base(message, innerException) { }
}
