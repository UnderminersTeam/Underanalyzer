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
    public IASTNode Body { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public ASTFragmentContext FragmentContext { get; }

    public StructNode(IASTNode body, ASTFragmentContext fragmentContext)
    {
        Body = body;
        FragmentContext = fragmentContext;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
