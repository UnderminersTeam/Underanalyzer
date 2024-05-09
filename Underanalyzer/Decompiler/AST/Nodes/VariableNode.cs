using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a variable reference in the AST.
/// </summary>
public class VariableNode : IASTNode
{
    bool IASTNode.Duplicated { get; set; } = false;

    // TODO

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
