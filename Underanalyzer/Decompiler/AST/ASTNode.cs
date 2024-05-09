using System;
namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base interface for all nodes in the AST.
/// </summary>
public interface IASTNode
{
    /// <summary>
    /// If true, this node was duplicated during simulation.
    /// </summary>
    public bool Duplicated { get; internal set; }

    /// <summary>
    /// Prints this node using the provided printer.
    /// </summary>
    public void Print(ASTPrinter printer);
}
