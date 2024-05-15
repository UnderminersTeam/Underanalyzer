using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.ControlFlow;

internal class WithLoop : Loop
{
    public override List<IControlFlowNode> Children { get; } = [null, null, null, null];

    /// <summary>
    /// The start of the loop body of the with loop.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this is disconnected from its predecessors.
    /// </remarks>
    public IControlFlowNode Head { get => Children[0]; private set => Children[0] = value; }

    /// <summary>
    /// The end of the with loop.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this is disconnected from its successors.
    /// </remarks>
    public IControlFlowNode Tail { get => Children[1]; private set => Children[1] = value; }

    /// <summary>
    /// The node reached after the with loop is completed.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this becomes a new <see cref="EmptyNode"/>, which is then disconnected from the external graph.
    /// </remarks>
    public IControlFlowNode After { get => Children[2]; private set => Children[2] = value; }

    /// <summary>
    /// If not null, this is a special block jumped to from within the with statement for "break" statements.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this node is disconnected from the graph.
    /// </remarks>
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

        // Remove all predecessors of Tail that are before this loop
        for (int i = Tail.Predecessors.Count - 1; i >= 0; i--)
        {
            IControlFlowNode curr = Tail.Predecessors[i];
            if (curr.StartAddress < StartAddress)
            {
                IControlFlowNode.DisconnectPredecessor(Tail, i);
            }
        }

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
        IExpressionNode target = builder.ExpressionStack.Pop();
        if (target is Int16Node i16 && i16.Value == (int)IGMInstruction.InstanceType.StackTop)
        {
            // Pull instance from stacktop, if possible
            if (builder.ExpressionStack.Count != 0 && !builder.ExpressionStack.Peek().Duplicated)
            {
                target = builder.ExpressionStack.Pop();
            }
        }
        BlockNode body = builder.BuildBlock(Head);
        output.Add(new WithLoopNode(target, body));
    }
}
