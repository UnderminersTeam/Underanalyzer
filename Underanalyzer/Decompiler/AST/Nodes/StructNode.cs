using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// A struct declaration/instantiation within the AST.
/// </summary>
public class StructNode : IFragmentNode, IExpressionNode
{
    /// <summary>
    /// The body of the struct (typically a block with assignments).
    /// </summary>
    public BlockNode Body { get; private set; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public ASTFragmentContext FragmentContext { get; }
    public bool SemicolonAfter { get => false; }

    public StructNode(BlockNode body, ASTFragmentContext fragmentContext)
    {
        Body = body;
        FragmentContext = fragmentContext;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Body.Clean(cleaner);
        Body.UseBraces = true;
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        throw new NotImplementedException();
    }

    public void Print(ASTPrinter printer)
    {
        Body.Print(printer);
    }
}
