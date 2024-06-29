using System;
using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// A struct declaration/instantiation within the AST.
/// </summary>
public class StructNode : IFragmentNode, IExpressionNode, IConditionalValueNode
{
    /// <summary>
    /// The body of the struct (typically a block with assignments).
    /// </summary>
    public BlockNode Body { get; private set; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public bool SemicolonAfter => false;
    public bool EmptyLineBefore => false;
    public bool EmptyLineAfter => false;
    public ASTFragmentContext FragmentContext { get; }

    public string ConditionalTypeName => "Struct";
    public string ConditionalValue => "";

    public StructNode(BlockNode body, ASTFragmentContext fragmentContext)
    {
        Body = body;
        FragmentContext = fragmentContext;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Body.Clean(cleaner);
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        throw new NotImplementedException();
    }

    public void Print(ASTPrinter printer)
    {
        if (Body.Children.Count == 0)
        {
            // Don't print a normal block in this case; condense down
            printer.Write("{}");
        }
        else
        {
            Body.Print(printer);
        }
    }

    public bool RequiresMultipleLines(ASTPrinter printer)
    {
        return Body.Children.Count != 0;
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
