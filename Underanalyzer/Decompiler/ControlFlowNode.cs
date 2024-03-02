using System;
using System.Collections.Generic;
using System.Text;

namespace Underanalyzer.Decompiler;

public interface IControlFlowNode
{
    /// <summary>
    /// The address of the first instruction from the original bytecode, where this node begins.
    /// </summary>
    public int StartAddress { get; }

    /// <summary>
    /// The address of the instruction after this node ends (that is, exclusive).
    /// </summary>
    public int EndAddress { get; }

    /// <summary>
    /// All nodes which precede this one in the control flow graph.
    /// </summary>
    public List<IControlFlowNode> Predecessors { get; }

    /// <summary>
    /// All nodes which succeed this one in the control flow graph.
    /// </summary>
    public List<IControlFlowNode> Successors { get; }
}
