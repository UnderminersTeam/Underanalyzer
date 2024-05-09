using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// A struct declaration/instantiation within the AST.
/// </summary>
public class StructNode : IFragmentNode
{
    /// <summary>
    /// The body of the struct (typically a block with assignments).
    /// </summary>
    public IASTNode Body { get; }

    bool IASTNode.Duplicated { get; set; } = false;

    public StructNode(IASTNode body)
    {
        Body = body;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
