using System;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base interface for all nodes in the AST.
/// </summary>
public interface IASTNode
{
    /// <summary>
    /// Prints this node using the provided printer.
    /// </summary>
    public void Print(ASTPrinter printer);
}
