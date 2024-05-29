namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a reference to a single enum value in the AST.
/// </summary>
public class EnumValueNode : IExpressionNode
{
    /// <summary>
    /// The name of the base enum type being referenced.
    /// </summary>
    public string EnumName { get; }

    /// <summary>
    /// The name of the value on the enum being referenced.
    /// </summary>
    public string EnumValueName { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int64;

    public EnumValueNode(string enumName, string enumValueName)
    {
        EnumName = enumName;
        EnumValueName = enumValueName;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }
        printer.Write(EnumName);
        printer.Write('.');
        printer.Write(EnumValueName);
        if (Group)
        {
            printer.Write(')');
        }
    }
}
