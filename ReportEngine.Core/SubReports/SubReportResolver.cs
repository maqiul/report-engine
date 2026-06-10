using ReportEngine.Core.Parsing;

namespace ReportEngine.Core.SubReports;

/// <summary>
/// 子模板解析器
/// 负责加载、缓存、解析嵌套的子模板
/// 支持文件系统和内存两种模板来源
/// </summary>
public interface ITemplateResolver
{
    /// <summary>
    /// 根据模板引用加载模板对象
    /// </summary>
    Task<ReportTemplate> ResolveAsync(string templateRef);

    /// <summary>
    /// 检查模板是否存在
    /// </summary>
    bool Exists(string templateRef);
}

/// <summary>
/// 文件系统模板解析器 - 桌面端默认使用
/// 从本地目录扫描 .rptx 文件
/// </summary>
public class FileSystemTemplateResolver : ITemplateResolver
{
    private readonly string _templatesDir;
    private readonly TemplateParser _parser;
    private readonly Dictionary<string, ReportTemplate> _cache = new();

    public FileSystemTemplateResolver(string templatesDir)
    {
        _templatesDir = templatesDir;
        _parser = new TemplateParser();

        if (!Directory.Exists(_templatesDir))
            Directory.CreateDirectory(_templatesDir);
    }

    public async Task<ReportTemplate> ResolveAsync(string templateRef)
    {
        // 先查缓存
        if (_cache.TryGetValue(templateRef, out var cached))
            return cached;

        // 支持相对路径和纯文件名
        var filePath = Path.IsPathRooted(templateRef)
            ? templateRef
            : Path.Combine(_templatesDir, templateRef);

        if (!File.Exists(filePath))
            throw new TemplateNotFoundException(templateRef, _templatesDir);

        // 防递归：检测循环引用
        var template = _parser.ParseFile(filePath);
        _cache[templateRef] = template;

        return template;
    }

    public bool Exists(string templateRef)
    {
        var filePath = Path.IsPathRooted(templateRef)
            ? templateRef
            : Path.Combine(_templatesDir, templateRef);

        return File.Exists(filePath);
    }

    /// <summary>
    /// 保存模板到文件系统
    /// </summary>
    public void Save(ReportTemplate template, string fileName)
    {
        var json = _parser.Serialize(template);
        var filePath = Path.Combine(_templatesDir, fileName);
        File.WriteAllText(filePath, json);

        // 更新缓存
        _cache[fileName] = template;
    }

    /// <summary>
    /// 列出所有可用模板
    /// </summary>
    public IReadOnlyList<string> ListTemplates()
    {
        return Directory.GetFiles(_templatesDir, "*.rptx")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .ToList()!;
    }

    /// <summary>
    /// 清空缓存
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}

/// <summary>
/// 复合模板解析器 - 支持多来源查找
/// 例如：先查内存缓存 → 再查文件系统 → 最后查远程API
/// </summary>
public class CompositeTemplateResolver : ITemplateResolver
{
    private readonly List<ITemplateResolver> _resolvers = new();

    public CompositeTemplateResolver AddResolver(ITemplateResolver resolver)
    {
        _resolvers.Add(resolver);
        return this;
    }

    public async Task<ReportTemplate> ResolveAsync(string templateRef)
    {
        foreach (var resolver in _resolvers)
        {
            if (resolver.Exists(templateRef))
                return await resolver.ResolveAsync(templateRef);
        }
        throw new TemplateNotFoundException(templateRef, "(composite)");
    }

    public bool Exists(string templateRef)
    {
        return _resolvers.Any(r => r.Exists(templateRef));
    }
}

public class TemplateNotFoundException : Exception
{
    public TemplateNotFoundException(string templateRef, string searchPath)
        : base($"Template '{templateRef}' not found in '{searchPath}'.")
    {
        TemplateRef = templateRef;
        SearchPath = searchPath;
    }

    public string TemplateRef { get; }
    public string SearchPath { get; }
}
