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

    /// <summary>
    /// If true, this node's predecessors do not truly exist. That is,
    /// the predecessors are only directly before this node's instructions.
    /// </summary>
    public bool Unreachable { get; set; }

    /// <summary>
    /// Utility function to insert a new control flow node into the graph,
    /// as a successor to an existing node, in the place of an existing successor.
    /// </summary>
    internal static void InsertSuccessor(IControlFlowNode node, int successorIndex, IControlFlowNode newSuccessor)
    {
        IControlFlowNode oldSuccessor = node.Successors[successorIndex];

        // Reroute successor
        node.Successors[successorIndex] = newSuccessor;

        // Find predecessor of old successor and reroute that as well
        int predIndex = oldSuccessor.Predecessors.FindIndex(p => p == node);
        oldSuccessor.Predecessors[predIndex] = newSuccessor;

        // Add predecessor and successor to the newly-inserted node
        newSuccessor.Predecessors.Add(node);
        newSuccessor.Successors.Add(oldSuccessor);
    }

    /// <summary>
    /// Utility function to disconnect a node from one of its successors.
    /// </summary>
    internal static void DisconnectSuccessor(IControlFlowNode node, int successorIndex)
    {
        IControlFlowNode oldSuccessor = node.Successors[successorIndex];

        // Remove successor
        node.Successors.RemoveAt(successorIndex);

        // Remove predecessor from old successor
        int predIndex = oldSuccessor.Predecessors.FindIndex(p => p == node);
        oldSuccessor.Predecessors.RemoveAt(predIndex);
    }

    /// <summary>
    /// Utility function to disconnect a node from one of its predecessor.
    /// </summary>
    internal static void DisconnectPredecessor(IControlFlowNode node, int predecessorIndex)
    {
        IControlFlowNode oldPredecessor = node.Predecessors[predecessorIndex];

        // Remove predecessor
        node.Predecessors.RemoveAt(predecessorIndex);

        // Remove successor from old predecessor
        int succIndex = oldPredecessor.Successors.FindIndex(p => p == node);
        oldPredecessor.Successors.RemoveAt(succIndex);
    }
}
