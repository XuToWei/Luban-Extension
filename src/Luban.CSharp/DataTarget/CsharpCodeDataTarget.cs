using System.Reflection;
using System.Text;
using Luban.CodeFormat;
using Luban.CodeFormat.CodeStyles;
using Luban.CodeTarget;
using Luban.CSharp.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Luban.DataTarget;
using Luban.Defs;
using Luban.TemplateExtensions;
using Luban.Tmpl;
using Scriban;
using Scriban.Runtime;

namespace Luban.CSharp.DataTarget;

[DataTarget("cs-code-data")]
public class CsharpCodeDataTarget : DataTargetBase
{
    protected virtual ICodeStyle CodeStyle => _codeStyle ??= CreateConfigurableCodeStyle();

    private ICodeStyle _codeStyle;

    private ICodeStyle CreateConfigurableCodeStyle()
    {
        var baseStyle = GenerationContext.Current.GetCodeStyle(Name) ?? CodeFormatManager.Ins.CsharpDefaultCodeStyle;

        var env = EnvManager.Current;
        string namingKey = BuiltinOptionNames.NamingConvention;
        return new OverlayCodeStyle(baseStyle,
            env.GetOptionOrDefault($"{namingKey}.{Name}", "namespace", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{Name}", "type", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{Name}", "method", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{Name}", "property", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{Name}", "field", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{Name}", "enumItem", true, "")
        );
    }

    protected TemplateContext CreateTemplateContext(Template template)
    {
        var ctx = new TemplateContext()
        {
            LoopLimit = 0,
            NewLine = "\n",
        };
        ctx.PushGlobal(new ContextTemplateExtension());
        ctx.PushGlobal(new TypeTemplateExtension());
        OnCreateTemplateContext(ctx);
        return ctx;
    }

    protected void OnCreateTemplateContext(TemplateContext ctx)
    {
        ctx.PushGlobal(new CsharpCodeTemplateExtension());
    }
    
    public void GenerateCodeData(DefTable table, List<Record> records, StringBuilder result)
    {
        var template = GetTemplate();
        var tplCtx = CreateTemplateContext(template);
        var extraEnvs = new ScriptObject
        {
            { "__name", table.Name },
            { "__namespace", table.Namespace },
            { "__namespace_with_top_module", table.NamespaceWithTopModule },
            { "__full_name_with_top_module", table.FullNameWithTopModule },
            { "__table", table },
            { "__this", table },
            { "__key_type", table.KeyTType},
            { "__value_type", table.ValueTType},
            { "__code_style", CodeStyle},
            { "__records", records},
        };
        tplCtx.PushGlobal(extraEnvs);
        result.Append(template.Render(tplCtx));
    }
    
    protected virtual Template GetTemplate()
    {
        if (TemplateManager.Ins.TryGetTemplate($"{typeof(CsharpCodeCodeTarget).GetCustomAttribute<CodeTargetAttribute>().Name}/tabledata", out var template))
        {
            return template;
        }
        throw new Exception("template:tabledata not found");
    }
    
    protected override string DefaultOutputFileExt => "cs";
    public override OutputFile ExportTable(DefTable table, List<Record> records)
    {
        var ss = new StringBuilder();
        GenerateCodeData(table, records, ss);
        return new OutputFile()
        {
            File = $"{table.OutputDataFile}.{DefaultOutputFileExt}",
            Content = ss.ToString(),
        };
    }
}
