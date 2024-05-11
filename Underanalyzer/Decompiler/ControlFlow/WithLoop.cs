using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.ControlFlow;

internal class WithLoop : Loop
{
    public override List<IControlFlowNode> Children { get; } = [null, null, null, null];

    public IControlFlowNode Head { get => Children[0]; private set => Children[0] = value; }

    public IControlFlowNode Tail { get => Children[1]; private set => Children[1] = value; }

    public IControlFlowNode After { get => Children[2]; private set => Children[2] = value; }

    public IControlFlowNode BreakBlock { get => Children[3]; private set => Children[3] = value; }

    public WithLoop(int startAddress, int endAddress, IControlFlowNode head, IControlFlowNode tail,
                    IControlFlowNode after, IControlFlowNode breakBlock)
        : base(startAddress, endAddress)
    {
        Head = head;
        Tail = tail;
        After = after;
        BreakBlock = breakBlock;
    }

    public override void UpdateFlowGraph()
    {
        // Add a new node that is branched to at the end, to keep control flow internal
        var oldAfter = After;
        var newAfter = new EmptyNode(After.StartAddress);
        IControlFlowNode.InsertPredecessors(After, newAfter, Head.EndAddress);
        newAfter.Parent = this;
        After = newAfter;

        // Get rid of jumps from tail
        IControlFlowNode.DisconnectSuccessor(Tail, 1);
        IControlFlowNode.DisconnectSuccessor(Tail, 0);

        IControlFlowNode nodeToEndAt = oldAfter;
        if (BreakBlock != null)
        {
            // Reroute everything going into BreakBlock to instead go into newAfter
            for (int i = 0; i < BreakBlock.Predecessors.Count; i++)
            {
                IControlFlowNode pred = BreakBlock.Predecessors[i];
                newAfter.Predecessors.Add(pred);
                pred.Successors.Add(newAfter);
                IControlFlowNode.DisconnectPredecessor(BreakBlock, i);
                i--;
            }

            // Disconnect BreakBlock completely (and use the node after it as our new end location)
            nodeToEndAt = BreakBlock.Successors[0];
            IControlFlowNode.DisconnectSuccessor(BreakBlock, 0);

            // Get rid of branch instruction from oldAfter
            Block oldAfterBlock = oldAfter as Block;
            oldAfterBlock.Instructions.RemoveAt(oldAfterBlock.Instructions.Count - 1);

            // Disonnect successor of After, now, as it is no longer desired
            IControlFlowNode.DisconnectSuccessor(After, 0);
        }

        // Insert structure into graph
        IControlFlowNode.InsertStructure(Head, nodeToEndAt, this);

        // Update parent status of Head, as well as this loop, for later operation
        Parent = Head.Parent;
        Head.Parent = this;
    }

    public override string ToString()
    {
        return $"{nameof(WithLoop)} (start address {StartAddress}, end address {EndAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
    }

    public override void BuildAST(ASTBuilder builder, List<IStatementNode> output)
    {
        throw new NotImplementedException();
    }
}
