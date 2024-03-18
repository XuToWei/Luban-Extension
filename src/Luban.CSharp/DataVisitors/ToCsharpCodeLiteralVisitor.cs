using System.Text;
using Luban.CodeFormat;
using Luban.CSharp.TypeVisitors;
using Luban.DataLoader;
using Luban.Datas;
using Luban.DataVisitors;
using Luban.Defs;
using Luban.Utils;

namespace Luban.CSharp.DataVisitors;

public class ToCsharpCodeLiteralVisitor : IDataFuncVisitor<string>
{
    public static ToCsharpCodeLiteralVisitor Ins { get; } = new();

    public ICodeStyle CodeStyle = CodeFormatManager.Ins.CsharpDefaultCodeStyle;

    public string Accept(DBool type)
    {
        return type.Value ? "true" : "false";
    }

    public string Accept(DByte type)
    {
        return type.Value.ToString();
    }

    public string Accept(DShort type)
    {
        return type.Value.ToString();
    }

    public string Accept(DInt type)
    {
        return type.Value.ToString();
    }

    public string Accept(DLong type)
    {
        return type.Value.ToString();
    }

    public string Accept(DFloat type)
    {
        return $"{type.Value}f";
    }

    public string Accept(DDouble type)
    {
        return type.Value.ToString();
    }

    public string Accept(DDateTime type)
    {
        return type.UnixTimeOfCurrentContext().ToString();
    }

    public string Accept(DString type)
    {
        return $"@\"{type.Value}\"";
    }

    public string Accept(DBean type)
    {
        var x = new StringBuilder();
        x.Append($"new {type.ImplType.FullNameWithTopModule} (");
        for (int i = 0; i < type.Fields.Count; i++)
        {
            var defField = (DefField)type.ImplType.HierarchyFields[i];
            if (!defField.NeedExport())
            {
                continue;
            }
            var f = type.Fields[i];
            if (f == null)
            {
                x.Append("default");
            }
            else
            {
                x.Append($"{f.Apply(this)}");
            }
            if (i < type.Fields.Count - 1)
            {
                x.Append(", ");
            }
        }
        x.Append(")");
        return x.ToString();
    }

    private void Append(List<DType> datas, StringBuilder x)
    {
        x.Append("{ ");
        foreach (var e in datas)
        {
            x.Append(e.Apply(this));
            x.Append(", ");
        }
        x.Append('}');
    }

    public string Accept(DArray type)
    {
        var x = new StringBuilder();
        x.Append($"new {type.Type.Apply(UnderlyingDeclaringTypeNameVisitor.Ins)} ");
        Append(type.Datas, x);
        return x.ToString();
    }

    public string Accept(DList type)
    {
        var x = new StringBuilder();
        x.Append($"new {type.Type.Apply(UnderlyingDeclaringTypeNameVisitor.Ins)} ");
        Append(type.Datas, x);
        return x.ToString();
    }

    public string Accept(DSet type)
    {
        var x = new StringBuilder();
        x.Append($"new {type.Type.Apply(UnderlyingDeclaringTypeNameVisitor.Ins)} ");
        Append(type.Datas, x);
        return x.ToString();
    }

    public string Accept(DMap type)
    {
        var x = new StringBuilder();
        x.Append($"new {type.Type.Apply(UnderlyingDeclaringTypeNameVisitor.Ins)} {{ ");
        foreach (var e in type.Datas)
        {
            x.Append($"[ {e.Key.Apply(this)} ] = {e.Value.Apply(this)}, ");
        }
        x.Append('}');
        return x.ToString();
    }

    public string Accept(DEnum type)
    {
        if (string.IsNullOrEmpty(type.StrValue) || string.Equals(type.StrValue, "0"))
            return "default";
        var x = new StringBuilder();
        string[] enums = type.StrValue.Split('|');
        for (int i = 0; i < enums.Length; i++)
        {
            foreach (var enumItem in type.Type.DefEnum.Items)
            {
                if (enumItem.Name == enums[i] || enumItem.Value == enums[i] || enumItem.Alias == enums[i])
                {
                    x.Append($"{type.Type.Apply(UnderlyingDeclaringTypeNameVisitor.Ins)}.{enumItem.Name}");
                    break;
                }
            }
            if (i != enums.Length - 1)
            {
                x.Append(" | ");
            }
        }
        return x.ToString();
    }
}
