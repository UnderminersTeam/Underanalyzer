using System;
using System.Collections.Generic;
using System.Text;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a single block of code in the AST.
/// Blocks can have an arbitrary number of child nodes.
/// </summary>
public class BlockNode : IFragmentNode
{
    /// <summary>
    /// Whether or not curly braces are required for this block.
    /// </summary>
    public bool UseBraces { get; set; } = true;

    /// <summary>
    /// All children contained within this block.
    /// </summary>
    public List<IASTNode> Children { get; internal set; } = new();

    bool IASTNode.Duplicated { get; set; } = false;

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
