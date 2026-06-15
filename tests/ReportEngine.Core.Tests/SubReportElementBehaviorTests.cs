using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// SubReportElement 行为测试：
///   - 默认值
///   - 模板引用
///   - 数据绑定
///   - 高度模式
///   - 逐行重复
/// </summary>
public class SubReportElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new SubReportElement();

        Assert.Equal("", el.TemplateRef);
        Assert.NotNull(el.DataBinding);
        Assert.Equal("auto", el.HeightMode);
        Assert.True(el.RepeatPerRow);
    }

    // ============== TemplateRef ==============

    [Fact]
    public void TemplateRef_EmptyByDefault()
    {
        var el = new SubReportElement();
        Assert.Equal("", el.TemplateRef);
    }

    [Fact]
    public void TemplateRef_SetSimple_Works()
    {
        var el = new SubReportElement { TemplateRef = "invoice-detail" };
        Assert.Equal("invoice-detail", el.TemplateRef);
    }

    [Fact]
    public void TemplateRef_SetWithPath_Works()
    {
        var el = new SubReportElement { TemplateRef = "templates/sub/invoice-detail.rpt" };
        Assert.Contains("templates", el.TemplateRef);
    }

    [Fact]
    public void TemplateRef_SetWithExpression_Works()
    {
        var el = new SubReportElement { TemplateRef = "{{currentRow.templateName}}" };
        Assert.Contains("{{", el.TemplateRef);
    }

    [Fact]
    public void TemplateRef_CanBeChanged()
    {
        var el = new SubReportElement { TemplateRef = "old.rpt" };
        el.TemplateRef = "new.rpt";
        Assert.Equal("new.rpt", el.TemplateRef);
    }

    // ============== DataBinding ==============

    [Fact]
    public void DataBinding_NotNull_ByDefault()
    {
        var el = new SubReportElement();
        Assert.NotNull(el.DataBinding);
    }

    [Fact]
    public void DataBinding_DefaultSource_IsEmpty()
    {
        var el = new SubReportElement();
        Assert.Equal("", el.DataBinding.Source);
    }

    [Fact]
    public void DataBinding_Source_Set_Works()
    {
        var el = new SubReportElement();
        el.DataBinding.Source = "orderDetails";
        Assert.Equal("orderDetails", el.DataBinding.Source);
    }

    [Fact]
    public void DataBinding_ParamMap_NotNull_ByDefault()
    {
        var el = new SubReportElement();
        Assert.NotNull(el.DataBinding.ParamMap);
        Assert.Empty(el.DataBinding.ParamMap);
    }

    [Fact]
    public void DataBinding_ParamMap_Add_Works()
    {
        var el = new SubReportElement();
        el.DataBinding.ParamMap.Add("orderId", "{{currentRow.id}}");
        Assert.Single(el.DataBinding.ParamMap);
    }

    [Fact]
    public void DataBinding_ParamMap_AddMultiple_Works()
    {
        var el = new SubReportElement();
        el.DataBinding.ParamMap.Add("orderId", "{{currentRow.id}}");
        el.DataBinding.ParamMap.Add("customerId", "{{currentRow.customerId}}");
        Assert.Equal(2, el.DataBinding.ParamMap.Count);
    }

    [Fact]
    public void DataBinding_CanBeReplaced()
    {
        var el = new SubReportElement();
        var newBinding = new SubReportDataBinding { Source = "newSource" };
        el.DataBinding = newBinding;
        Assert.Same(newBinding, el.DataBinding);
    }

    // ============== HeightMode ==============

    [Fact]
    public void HeightMode_DefaultIsAuto()
    {
        var el = new SubReportElement();
        Assert.Equal("auto", el.HeightMode);
    }

    [Fact]
    public void HeightMode_SetFixed_Works()
    {
        var el = new SubReportElement { HeightMode = "fixed" };
        Assert.Equal("fixed", el.HeightMode);
    }

    [Fact]
    public void HeightMode_SetAuto_Works()
    {
        var el = new SubReportElement { HeightMode = "fixed" };
        el.HeightMode = "auto";
        Assert.Equal("auto", el.HeightMode);
    }

    [Fact]
    public void HeightMode_AnyString_Accepted()
    {
        var el = new SubReportElement { HeightMode = "custom" };
        Assert.Equal("custom", el.HeightMode);
    }

    // ============== RepeatPerRow ==============

    [Fact]
    public void RepeatPerRow_TrueByDefault()
    {
        var el = new SubReportElement();
        Assert.True(el.RepeatPerRow);
    }

    [Fact]
    public void RepeatPerRow_SetFalse_Works()
    {
        var el = new SubReportElement { RepeatPerRow = false };
        Assert.False(el.RepeatPerRow);
    }

    [Fact]
    public void RepeatPerRow_CanBeToggled()
    {
        var el = new SubReportElement { RepeatPerRow = true };
        el.RepeatPerRow = false;
        Assert.False(el.RepeatPerRow);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void SubReportElement_InvoiceDetail_Works()
    {
        var el = new SubReportElement
        {
            TemplateRef = "invoice-detail.rpt",
            HeightMode = "auto",
            RepeatPerRow = true,
            X = 10,
            Y = 50,
            Width = 190,
            Height = 100
        };
        el.DataBinding.Source = "orderDetails";
        el.DataBinding.ParamMap.Add("orderId", "{{currentRow.orderId}}");

        Assert.Equal("invoice-detail.rpt", el.TemplateRef);
        Assert.Equal("orderDetails", el.DataBinding.Source);
        Assert.Single(el.DataBinding.ParamMap);
    }

    [Fact]
    public void SubReportElement_FixedHeight_Works()
    {
        var el = new SubReportElement
        {
            TemplateRef = "fixed-sub.rpt",
            HeightMode = "fixed",
            X = 10,
            Y = 10,
            Width = 100,
            Height = 50
        };

        Assert.Equal("fixed", el.HeightMode);
        Assert.Equal(50, el.Height);
    }

    [Fact]
    public void SubReportElement_NoRepeat_Works()
    {
        var el = new SubReportElement
        {
            TemplateRef = "summary.rpt",
            RepeatPerRow = false
        };

        Assert.False(el.RepeatPerRow);
    }

    [Fact]
    public void SubReportElement_WithMultipleParams_Works()
    {
        var el = new SubReportElement
        {
            TemplateRef = "detail.rpt"
        };
        el.DataBinding.Source = "items";
        el.DataBinding.ParamMap.Add("parentId", "{{currentRow.id}}");
        el.DataBinding.ParamMap.Add("parentType", "{{currentRow.type}}");
        el.DataBinding.ParamMap.Add("parentName", "{{currentRow.name}}");

        Assert.Equal(3, el.DataBinding.ParamMap.Count);
    }

    [Fact]
    public void SubReportElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Detail, Height = 100 };
        band.Elements.Add(new SubReportElement
        {
            TemplateRef = "sub-detail.rpt",
            X = 10,
            Y = 5,
            Width = 180,
            Height = 90
        });

        Assert.Single(band.Elements);
        var sub = Assert.IsType<SubReportElement>(band.Elements[0]);
        Assert.Equal("sub-detail.rpt", sub.TemplateRef);
    }

    [Fact]
    public void SubReportElement_CanBeModified()
    {
        var el = new SubReportElement
        {
            TemplateRef = "old.rpt",
            HeightMode = "auto",
            RepeatPerRow = true
        };
        
        el.TemplateRef = "new.rpt";
        el.HeightMode = "fixed";
        el.RepeatPerRow = false;
        
        Assert.Equal("new.rpt", el.TemplateRef);
        Assert.Equal("fixed", el.HeightMode);
        Assert.False(el.RepeatPerRow);
    }
}

/// <summary>
/// SubReportDataBinding 行为测试
/// </summary>
public class SubReportDataBindingBehaviorTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var binding = new SubReportDataBinding();

        Assert.Equal("", binding.Source);
        Assert.NotNull(binding.ParamMap);
        Assert.Empty(binding.ParamMap);
    }

    [Fact]
    public void Source_SetAndGet_Works()
    {
        var binding = new SubReportDataBinding { Source = "orders" };
        Assert.Equal("orders", binding.Source);
    }

    [Fact]
    public void ParamMap_Add_Works()
    {
        var binding = new SubReportDataBinding();
        binding.ParamMap.Add("key", "value");
        Assert.Single(binding.ParamMap);
        Assert.Equal("value", binding.ParamMap["key"]);
    }

    [Fact]
    public void ParamMap_AddMultiple_Works()
    {
        var binding = new SubReportDataBinding();
        binding.ParamMap.Add("a", "1");
        binding.ParamMap.Add("b", "2");
        binding.ParamMap.Add("c", "3");
        Assert.Equal(3, binding.ParamMap.Count);
    }

    [Fact]
    public void ParamMap_ContainsKey_Works()
    {
        var binding = new SubReportDataBinding();
        binding.ParamMap.Add("orderId", "{{id}}");
        Assert.True(binding.ParamMap.ContainsKey("orderId"));
        Assert.False(binding.ParamMap.ContainsKey("other"));
    }

    [Fact]
    public void ParamMap_Remove_Works()
    {
        var binding = new SubReportDataBinding();
        binding.ParamMap.Add("key", "value");
        binding.ParamMap.Remove("key");
        Assert.Empty(binding.ParamMap);
    }

    [Fact]
    public void ParamMap_Clear_Works()
    {
        var binding = new SubReportDataBinding();
        binding.ParamMap.Add("a", "1");
        binding.ParamMap.Add("b", "2");
        binding.ParamMap.Clear();
        Assert.Empty(binding.ParamMap);
    }
}
