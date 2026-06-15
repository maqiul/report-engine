using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// SubReportElement / SubReportDataBinding 行为测试：
///   - 默认值（Source="" / ParamMap 空）
///   - HeightMode 默认 "auto"
///   - RepeatPerRow 默认 true
///   - ParamMap 增删改
///   - 多个 ParamMap 项互不影响
///   - SubReport 继承 ReportElement 默认 Id
/// </summary>
public class SubReportDataBindingTests
{
    [Fact]
    public void SubReportElement_Defaults()
    {
        var s = new SubReportElement();
        Assert.Equal("", s.TemplateRef);
        Assert.NotNull(s.DataBinding);
        Assert.Equal("auto", s.HeightMode);
        Assert.True(s.RepeatPerRow);
    }

    [Fact]
    public void SubReportDataBinding_Defaults()
    {
        var b = new SubReportDataBinding();
        Assert.Equal("", b.Source);
        Assert.NotNull(b.ParamMap);
        Assert.Empty(b.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_AddAndRead()
    {
        var b = new SubReportDataBinding();
        b.ParamMap["id"] = "currentRow.id";
        b.ParamMap["name"] = "currentRow.name";
        Assert.Equal(2, b.ParamMap.Count);
        Assert.Equal("currentRow.id", b.ParamMap["id"]);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_RemoveKey()
    {
        var b = new SubReportDataBinding();
        b.ParamMap["k"] = "v";
        b.ParamMap.Remove("k");
        Assert.Empty(b.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_MultipleValues_Independent()
    {
        var b = new SubReportDataBinding();
        b.ParamMap["a"] = "1";
        b.ParamMap["b"] = "2";
        b.ParamMap["c"] = "3";
        Assert.Equal(3, b.ParamMap.Count);
        Assert.Equal("1", b.ParamMap["a"]);
        Assert.Equal("2", b.ParamMap["b"]);
        Assert.Equal("3", b.ParamMap["c"]);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_NullValue_Handled()
    {
        var b = new SubReportDataBinding();
        b.ParamMap["k"] = null!;
        Assert.True(b.ParamMap.ContainsKey("k"));
    }

    [Fact]
    public void SubReportElement_InheritsReportElementDefaults()
    {
        var s = new SubReportElement();
        Assert.NotNull(s.Id);
        Assert.NotEmpty(s.Id);
        Assert.True(s.Visible);
        Assert.Equal(1.0, s.Opacity);
        Assert.False(s.Locked);
    }

    [Fact]
    public void SubReportElement_HeightMode_CanBeChanged()
    {
        var s = new SubReportElement();
        s.HeightMode = "fixed";
        Assert.Equal("fixed", s.HeightMode);
        s.HeightMode = "expand";
        Assert.Equal("expand", s.HeightMode);
    }

    [Fact]
    public void SubReportElement_RepeatPerRow_CanBeChanged()
    {
        var s = new SubReportElement();
        s.RepeatPerRow = false;
        Assert.False(s.RepeatPerRow);
    }

    [Fact]
    public void SubReportElement_DataBinding_Shared_NotCopied()
    {
        // 两次创建 SubReportElement，DataBinding 是各自的新实例
        var s1 = new SubReportElement();
        var s2 = new SubReportElement();
        Assert.NotSame(s1.DataBinding, s2.DataBinding);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_KeyCaseSensitive()
    {
        var b = new SubReportDataBinding();
        b.ParamMap["ID"] = "1";
        b.ParamMap["id"] = "2";
        Assert.Equal(2, b.ParamMap.Count);
        Assert.Equal("1", b.ParamMap["ID"]);
        Assert.Equal("2", b.ParamMap["id"]);
    }
}
