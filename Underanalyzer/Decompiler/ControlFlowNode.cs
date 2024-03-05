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
    /// If disconnected from the rest of the graph, e.g. at the start of a high-level
    /// control flow structure like a loop, this points to the enveloping structure.
    /// </summary>
    public IControlFlowNode Parent { get; set; }

    /// <summary>
    /// If this is a high-level control flow structure like a loop, this represents
    /// all relevant internal nodes that this structure requires handles for.
    /// </summary>
    public List<IControlFlowNode> Children { get; }

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
    /// Utility function to insert a new node to the control flow graph, which is a 
    /// sole predecessor of "node", and takes on all predecessors of "node" that are
    /// within a range of addresses, ending at "node"'s address.
    /// </summary>
    internal static void InsertPredecessors(IControlFlowNode node, IControlFlowNode newPredecessor, int startAddr)
    {
        // Reroute all earlier predecessors of "node" to "newPredecessor"
        for (int i = 0; i < node.Predecessors.Count; i++)
        {
            IControlFlowNode currPred = node.Predecessors[i];
            if (currPred.StartAddress >= startAddr && currPred.StartAddress < node.StartAddress)
            {
                newPredecessor.Predecessors.Add(currPred);
                ReplaceConnections(currPred.Successors, node, newPredecessor);
                node.Predecessors.RemoveAt(i);
                i--;
            }
        }

        // Route "newPredecessor" into "node"
        newPredecessor.Successors.Add(node);
        node.Predecessors.Insert(0, newPredecessor);
    }

    /// <summary>
    /// Utility function to disconnect a node from one of its predecessors.
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

    /// <summary>
    /// Helper function to replace all instances of "search" with "replace" in a control flow list.
    /// </summary>
    private static void ReplaceConnections(List<IControlFlowNode> list, IControlFlowNode search, IControlFlowNode replace)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == search)
            {
                list[i] = replace;
            }
        }
    }

    /// <summary>
    /// Utility function to reroute an entire section of control flow (from its beginning and end),
    /// to make way for a new high-level control flow structure, such as a loop.
    /// Assumes that "after" should have its first predecessor replaced by this new structure.
    /// </summary>
    /// <remarks>
    /// This assumes that there are no connections between any nodes contained in "newStructure" to any external nodes,
    /// except for "start" and "after" explicitly. If that is not the case, those will need to be manually cleaned up.
    /// </remarks>
    internal static void InsertStructure(IControlFlowNode start, IControlFlowNode after, IControlFlowNode newStructure)
    {
        // If the start node is unreachable, then so is our new structure
        if (start.Unreachable)
        {
            newStructure.Unreachable = true;
            start.Unreachable = false;
        }

        // Reroute all nodes going into "start" to instead go into "newStructure"
        for (int i = 0; i < start.Predecessors.Count; i++)
        {
            newStructure.Predecessors.Add(start.Predecessors[i]);
            ReplaceConnections(start.Predecessors[i].Successors, start, newStructure);
        }
        if (start.Parent is not null)
        {
            ReplaceConnections(start.Parent.Children, start, newStructure);
        }
        // TODO: do we care about "start"'s Children?
        start.Predecessors.Clear();

        // Reroute predecessor at index 0 from "after" to instead come from "newStructure"
        after.Predecessors[0].Successors.RemoveAll(a => a == after);
        after.Predecessors[0] = newStructure;
        newStructure.Successors.Add(after);
    }
}
