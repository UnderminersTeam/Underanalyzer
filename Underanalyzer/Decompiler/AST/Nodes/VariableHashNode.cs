using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a variable hash in the AST, generated at compile-time in more recent GMLv2 versions.
/// </summary>
public class VariableHashNode : IExpressionNode, IStatementNode, IConditionalValueNode
{
    /// <summary>
    /// The variable being referenced.
    /// </summary>
    public IGMVariable Variable;

    public bool Duplicated { get; set; }
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int32;
    public bool SemicolonAfter => true;
    public bool EmptyLineBefore => false;
    public bool EmptyLineAfter => false;

    public string ConditionalTypeName => "VariableHash";
    public string ConditionalValue => Variable.Name.Content; // TODO?

    public VariableHashNode(IGMVariable variable)
    {
        Variable = variable;
    }

    IExpressionNode IASTNode<IExpressionNode>.Clean(ASTCleaner cleaner)
    {
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write("(");
        }

        printer.Write("variable_get_hash(\"");
        printer.Write(Variable.Name.Content);
        printer.Write("\")");

        if (Group)
        {
            printer.Write(")");
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

