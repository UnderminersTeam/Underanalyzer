using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

public class RepeatLoop : Loop
{
    public override List<IControlFlowNode> Children { get; } = [ null, null, null ];

    public IControlFlowNode Head { get => Children[0]; private set => Children[0] = value; }

    public IControlFlowNode Tail { get => Children[1]; private set => Children[1] = value; }

    public IControlFlowNode After { get => Children[2]; private set => Children[2] = value; }

    public RepeatLoop(int startAddress, int endAddress, IControlFlowNode head, IControlFlowNode tail, IControlFlowNode after)
        : base(startAddress, endAddress)
    {
        Head = head;
        Tail = tail;
        After = after;
    }

    public override void UpdateFlowGraph()
    {
        // Get rid of branch (and unneeded logic) from branch into Head
        // The (first) predecessor of Head should always be a Block, as it has logic
        Block headPred = Head.Predecessors[0] as Block;
        headPred.Instructions.RemoveRange(headPred.Instructions.Count - 4, 4);
        IControlFlowNode.DisconnectSuccessor(headPred, 1);

        // Get rid of jumps (and unneeded logic) from Tail
        IControlFlowNode.DisconnectSuccessor(Tail, 1);
        IControlFlowNode.DisconnectSuccessor(Tail, 0);
        Block tailBlock = Tail as Block;
        tailBlock.Instructions.RemoveRange(tailBlock.Instructions.Count - 5, 5);

        // Remove unneeded logic from After (should also always be a Block)
        Block afterBlock = After as Block;
        afterBlock.Instructions.RemoveAt(0);

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
