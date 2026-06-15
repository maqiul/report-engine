using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// SubReportElement / SubReportDataBinding 完整字段测试：
///   - SubReportElement 完整字段（TemplateRef/DataBinding/HeightMode/RepeatPerRow）
///   - SubReportDataBinding 完整字段（Source/ParamMap）
///   - 字段组合行为
/// </summary>
public class SubReportElementCompleteTests
{
    // ============== SubReportElement ==============

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
    public void SubReportElement_AllSetters()
    {
        var s = new SubReportElement
        {
            TemplateRef = "sub_report.rpt",
            HeightMode = "fixed",
            RepeatPerRow = false,
        };
        s.DataBinding.Source = "detail_data";
        s.DataBinding.ParamMap.Add("parentId", "{{id}}");

        Assert.Equal("sub_report.rpt", s.TemplateRef);
        Assert.Equal("fixed", s.HeightMode);
        Assert.False(s.RepeatPerRow);
        Assert.Equal("detail_data", s.DataBinding.Source);
        Assert.Single(s.DataBinding.ParamMap);
    }

    [Fact]
    public void SubReportElement_TemplateRef_CanBeEmpty()
    {
        var s = new SubReportElement { TemplateRef = "" };
        Assert.Equal("", s.TemplateRef);
    }

    [Fact]
    public void SubReportElement_TemplateRef_CanBePath()
    {
        var s = new SubReportElement { TemplateRef = "templates/sub.rpt" };
        Assert.Equal("templates/sub.rpt", s.TemplateRef);
    }

    [Fact]
    public void SubReportElement_HeightMode_DefaultAuto()
    {
        var s = new SubReportElement();
        Assert.Equal("auto", s.HeightMode);
    }

    [Fact]
    public void SubReportElement_HeightMode_CanBeFixed()
    {
        var s = new SubReportElement { HeightMode = "fixed" };
        Assert.Equal("fixed", s.HeightMode);
    }

    [Fact]
    public void SubReportElement_HeightMode_CanBeAnyString()
    {
        var s = new SubReportElement { HeightMode = "custom" };
        Assert.Equal("custom", s.HeightMode);
    }

    [Fact]
    public void SubReportElement_RepeatPerRow_DefaultTrue()
    {
        var s = new SubReportElement();
        Assert.True(s.RepeatPerRow);
    }

    [Fact]
    public void SubReportElement_RepeatPerRow_CanBeFalse()
    {
        var s = new SubReportElement { RepeatPerRow = false };
        Assert.False(s.RepeatPerRow);
    }

    [Fact]
    public void SubReportElement_DataBinding_DefaultNotNull()
    {
        var s = new SubReportElement();
        Assert.NotNull(s.DataBinding);
    }

    // ============== SubReportDataBinding ==============

    [Fact]
    public void SubReportDataBinding_Defaults()
    {
        var d = new SubReportDataBinding();
        Assert.Equal("", d.Source);
        Assert.NotNull(d.ParamMap);
        Assert.Empty(d.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_AllSetters()
    {
        var d = new SubReportDataBinding
        {
            Source = "orders",
        };
        d.ParamMap.Add("customerId", "{{id}}");
        d.ParamMap.Add("region", "{{region}}");

        Assert.Equal("orders", d.Source);
        Assert.Equal(2, d.ParamMap.Count);
    }

    [Fact]
    public void SubReportDataBinding_Source_CanBeEmpty()
    {
        var d = new SubReportDataBinding { Source = "" };
        Assert.Equal("", d.Source);
    }

    [Fact]
    public void SubReportDataBinding_Source_CanBeSet()
    {
        var d = new SubReportDataBinding { Source = "details" };
        Assert.Equal("details", d.Source);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_CanBeEmpty()
    {
        var d = new SubReportDataBinding();
        Assert.Empty(d.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_CanAddOne()
    {
        var d = new SubReportDataBinding();
        d.ParamMap.Add("key", "value");
        Assert.Single(d.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_CanAddMultiple()
    {
        var d = new SubReportDataBinding();
        d.ParamMap.Add("p1", "{{v1}}");
        d.ParamMap.Add("p2", "{{v2}}");
        d.ParamMap.Add("p3", "{{v3}}");
        Assert.Equal(3, d.ParamMap.Count);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_CanBeCleared()
    {
        var d = new SubReportDataBinding();
        d.ParamMap.Add("key", "value");
        d.ParamMap.Clear();
        Assert.Empty(d.ParamMap);
    }

    [Fact]
    public void SubReportElement_FullCombination()
    {
        var s = new SubReportElement
        {
            TemplateRef = "invoice_detail.rpt",
            HeightMode = "auto",
            RepeatPerRow = true,
        };
        s.DataBinding.Source = "invoice_items";
        s.DataBinding.ParamMap.Add("invoiceId", "{{invoice.id}}");
        s.DataBinding.ParamMap.Add("customerId", "{{customer.id}}");

        Assert.Equal("invoice_detail.rpt", s.TemplateRef);
        Assert.Equal("auto", s.HeightMode);
        Assert.True(s.RepeatPerRow);
        Assert.Equal("invoice_items", s.DataBinding.Source);
        Assert.Equal(2, s.DataBinding.ParamMap.Count);
    }
}
