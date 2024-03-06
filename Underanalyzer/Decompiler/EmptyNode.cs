using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

/// <summary>
/// Represents an empty node in the control flow graph.
/// This is generally used for reshaping control flow to make later analysis easier.
/// </summary>
public class EmptyNode(int address) : IControlFlowNode
{
    public int StartAddress { get; set; } = address;

    public int EndAddress { get; set; } = address;

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = new();

    public bool Unreachable { get; set; } = false;

    public override string ToString()
    {
        return $"{nameof(EmptyNode)} (address {StartAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
    }
}
