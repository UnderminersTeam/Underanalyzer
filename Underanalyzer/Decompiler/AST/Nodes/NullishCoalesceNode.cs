using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a nullish coalescing operator (??) in the AST.
/// </summary>
public class NullishCoalesceNode : IMultiExpressionNode, IConditionalValueNode
{
    /// <summary>
    /// The left side of the operator.
    /// </summary>
    public IExpressionNode Left { get; private set; }

    /// <summary>
    /// The right side of the operator.
    /// </summary>
    public IExpressionNode Right { get; private set; }

    public bool Duplicated { get; set; }
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public string ConditionalTypeName => "NullishCoalesce";
    public string ConditionalValue => ""; // TODO?

    public NullishCoalesceNode(IExpressionNode left, IExpressionNode right)
    {
        Left = left;
        Right = right;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Left = Left.Clean(cleaner);
        Right = Right.Clean(cleaner);

        if (Left is IMultiExpressionNode)
        {
            Left.Group = true;
        }
        if (Right is IMultiExpressionNode)
        {
            Right.Group = true;
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }

        Left.Print(printer);
        printer.Write(" ?? ");
        Right.Print(printer);

        if (Group)
        {
            printer.Write(')');
        }
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
