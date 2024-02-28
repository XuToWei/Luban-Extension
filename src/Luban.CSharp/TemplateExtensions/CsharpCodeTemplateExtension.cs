using Luban.CSharp.DataVisitors;
using Luban.Defs;

namespace Luban.CSharp.TemplateExtensions;

public class CsharpCodeTemplateExtension : CsharpTemplateExtension
{
    public static string ApplyValue(Record record)
    {
        return record.Data.Apply(ToCsharpCodeLiteralVisitor.Ins);
    }

    public static string ApplyKey(DefTable table, Record record)
    {
        return record.Data.GetField(table.IndexField.Name).Apply(ToCsharpCodeLiteralVisitor.Ins);
    }
}
