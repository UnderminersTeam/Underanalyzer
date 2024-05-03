using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

/// <summary>
/// Represents a break statement in the control flow graph.
/// </summary>
public class BreakNode(int address) : IControlFlowNode
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
        return $"{nameof(BreakNode)} (address {StartAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
    }
}
