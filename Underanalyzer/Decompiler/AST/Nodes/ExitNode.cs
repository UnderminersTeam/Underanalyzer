using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an "exit"/"empty return" statement in the AST.
/// </summary>
public class ExitNode : IASTNode
{
    bool IASTNode.Duplicated { get; set; } = false;

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
