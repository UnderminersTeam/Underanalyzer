﻿using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

/// <summary>
/// Represents a "do...until" loop node in a control flow graph.
/// </summary>
public class DoUntilLoop : Loop
{
    public override List<IControlFlowNode> Children { get; } = [null, null, null];

    /// <summary>
    /// The top loop point and start of the loop body, as written in the source code.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this is disconnected from its predecessors.
    /// </remarks>
    public IControlFlowNode Head { get => Children[0]; private set => Children[0] = value; }

    /// <summary>
    /// The bottom loop point of the loop. This is where the loop condition and branch to the loop head is located.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this is disconnected from its successors.
    /// </remarks>
    public IControlFlowNode Tail { get => Children[1]; private set => Children[1] = value; }

    /// <summary>
    /// The "sink" location of the loop. The loop condition being false or "break" statements will lead to this location.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this becomes a new <see cref="EmptyNode"/>, which is then disconnected from the external graph.
    /// </remarks>
    public IControlFlowNode After { get => Children[2]; private set => Children[2] = value; }

    public DoUntilLoop(int startAddress, int endAddress, IControlFlowNode head, IControlFlowNode tail, IControlFlowNode after) 
        : base(startAddress, endAddress)
    {
        Head = head;
        Tail = tail;
        After = after;
    }

    public override void UpdateFlowGraph()
    {
        // Get rid of jumps from tail
        IControlFlowNode.DisconnectSuccessor(Tail, 1);
        IControlFlowNode.DisconnectSuccessor(Tail, 0);
        Block tailBlock = Tail as Block;
        tailBlock.Instructions.RemoveAt(tailBlock.Instructions.Count - 1);

        // Add a new node that is branched to at the end, to keep control flow internal
        var oldAfter = After;
        var newAfter = new EmptyNode(After.StartAddress);
        IControlFlowNode.InsertPredecessors(After, newAfter, Head.EndAddress);
        newAfter.Parent = this;
        After = newAfter;

        // Insert structure into graph
        IControlFlowNode.InsertStructure(Head, oldAfter, this);

        // Update parent status of Head, as well as this loop, for later operation
        Parent = Head.Parent;
        Head.Parent = this;
    }

    public override string ToString()
    {
        return $"{nameof(DoUntilLoop)} (start address {StartAddress}, end address {EndAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
    }
}
