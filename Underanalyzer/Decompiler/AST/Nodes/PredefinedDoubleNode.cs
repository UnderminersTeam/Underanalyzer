using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a predefined double constant in the AST, with one single part.
/// </summary>
public class PredefinedDoubleSingleNode : IExpressionNode, IConditionalValueNode
{
    public string Value { get; }
    public double OriginalValue { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Double;

    public string ConditionalTypeName => "PredefinedDouble";
    public string ConditionalValue => Value;

    public PredefinedDoubleSingleNode(string value, double originalValue)
    {
        Value = value;
        OriginalValue = originalValue;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public virtual void Print(ASTPrinter printer)
    {
        printer.Write(Value);
    }

    public bool RequiresMultipleLines(ASTPrinter printer)
    {
        return false;
    }

    public IExpressionNode ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        if (type is IMacroTypeConditional conditional)
        {
            return conditional.Resolve(cleaner, this);
        }
        return null;
    }
}

/// <summary>
/// Represents a predefined double constant in the AST, with multiple parts.
/// </summary>
public class PredefinedDoubleMultiNode : PredefinedDoubleSingleNode, IMultiExpressionNode
{
    public PredefinedDoubleMultiNode(string value, double originalValue) : base(value, originalValue)
    {
    }

    public override void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }
        base.Print(printer);
        if (Group)
        {
            printer.Write(')');
        }
    }
}
