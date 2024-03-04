using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

public class DoUntilLoop : Loop
{
    public override List<IControlFlowNode> Children { get; } = [null, null, null];

    public IControlFlowNode Head { get => Children[0]; private set => Children[0] = value; }

    public IControlFlowNode Tail { get => Children[1]; private set => Children[1] = value; }

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
}
