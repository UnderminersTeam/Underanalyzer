using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

public class WhileLoop : Loop
{
    public override List<IControlFlowNode> Children { get; } = [null, null, null, null];

    public IControlFlowNode Head { get => Children[0]; private set => Children[0] = value; }

    public IControlFlowNode Tail { get => Children[1]; private set => Children[1] = value; }

    public IControlFlowNode After { get => Children[2]; private set => Children[2] = value; }

    public IControlFlowNode Body { get => Children[3]; private set => Children[3] = value; }

    /// <summary>
    /// If true, then it was detected that this while loop must be written as a for loop.
    /// This can occur when "continue" statements are used within the loop, which otherwise
    /// could not be written using normal if/else statements.
    /// </summary>
    public bool IsForLoopNecessary { get; set; }

    public WhileLoop(int startAddress, int endAddress, IControlFlowNode head, IControlFlowNode tail, IControlFlowNode after)
        : base(startAddress, endAddress)
    {
        Head = head;
        Tail = tail;
        After = after;
    }

    public override void UpdateFlowGraph()
    {
        // Get rid of jump from tail
        IControlFlowNode.DisconnectSuccessor(Tail, 0);
        Block tailBlock = Tail as Block;
        tailBlock.Instructions.RemoveAt(tailBlock.Instructions.Count - 1);

        // Find branch location after head
        Block branchBlock = After.Predecessors[0] as Block;
        if (branchBlock.Instructions[^1].Kind != IGMInstruction.Opcode.BranchFalse)
            throw new Exception("Expected BranchFalse in branch block - misidentified");

        // Identify body node by using branch location's first target (the one that doesn't jump)
        Body = branchBlock.Successors[0];
        Body.Parent = this;

        // Get rid of jumps from branch location
        IControlFlowNode.DisconnectSuccessor(branchBlock, 1);
        IControlFlowNode.DisconnectSuccessor(branchBlock, 0);
        branchBlock.Instructions.RemoveAt(branchBlock.Instructions.Count - 1);

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
}
