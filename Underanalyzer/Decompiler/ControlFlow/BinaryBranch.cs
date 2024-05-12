using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.ControlFlow;

/// <summary>
/// Represents a general binary branch operation. Specifically, only either an if statement or a ternary/conditional operator.
/// </summary>
internal class BinaryBranch : IControlFlowNode
{
    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = [];

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = [null, null, null, null];

    public bool Unreachable { get; set; } = false;

    /// <summary>
    /// The "condition" block of the if statement.
    /// </summary>
    public IControlFlowNode Condition { get => Children[0]; private set => Children[0] = value; }

    /// <summary>
    /// The "true" block of the if statement.
    /// </summary>
    public IControlFlowNode True { get => Children[1]; private set => Children[1] = value; }

    /// <summary>
    /// The "false" block of the if statement.
    /// </summary>
    public IControlFlowNode False { get => Children[2]; private set => Children[2] = value; }

    /// <summary>
    /// The "else" block of the if statement, or null if none exists.
    /// </summary>
    public IControlFlowNode Else { get => Children[3]; private set => Children[3] = value; }

    public BinaryBranch(int startAddress, int endAddress)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
    }

    /// <summary>
    /// Creates a mapping of all blocks to the innermost loop they are contained within.
    /// </summary>
    private static Dictionary<Block, Loop> FindSurroundingLoops(
        List<Block> blocks, Dictionary<int, Block> blockByAddress, List<Loop> loops)
    {
        // Assign blocks to loops.
        // We assume that loops are sorted so that nested loops come after outer loops.
        Dictionary<Block, Loop> surroundingLoops = new();
        foreach (Loop l in loops)
        {
            Block startBlock = blockByAddress[l.StartAddress];
            Block endBlock = blockByAddress[l.EndAddress];
            for (int blockIndex = startBlock.BlockIndex; blockIndex < endBlock.BlockIndex; blockIndex++)
            {
                surroundingLoops[blocks[blockIndex]] = l;
            }
        }

        return surroundingLoops;
    }

    /// <summary>
    /// Visits all nodes that are candidates for the meeting point of the if statement branch, along one path.
    /// Marks off in "visited" all such nodes.
    /// </summary>
    private static void VisitAll(IControlFlowNode start, HashSet<IControlFlowNode> visited, List<Block> blocks)
    {
        Stack<IControlFlowNode> work = new();
        work.Push(start);

        while (work.Count > 0)
        {
            IControlFlowNode node = work.Pop();
            visited.Add(node);

            if (node is Block block &&
                (block.Instructions is [.., { Kind: IGMInstruction.Opcode.Exit }] ||
                 block.Instructions is [.., { Kind: IGMInstruction.Opcode.Return }]))
            {
                // Exit/return statement: flow to following block if one exists, and isn't already a successor
                if (block.BlockIndex + 1 < blocks.Count)
                {
                    IControlFlowNode following = blocks[block.BlockIndex + 1];
                    while (following.Parent is not null)
                    {
                        following = following.Parent;
                    }
                    if (!node.Successors.Contains(following))
                    {
                        work.Push(following);
                    }
                }
            }

            foreach (IControlFlowNode successor in node.Successors)
            {
                if (successor.StartAddress < node.StartAddress || successor == node)
                {
                    throw new DecompilerException("Unresolved loop when following binary branches");
                }
                if (!visited.Contains(successor))
                {
                    work.Push(successor);
                }
            }
        }
    }


    /// <summary>
    /// Visits all nodes that are candidates for the meeting point of the if statement branch, along a second path.
    /// Upon finding a node that was visited along the first path (through VisitAll), returns that node.
    /// </summary>
    private static IControlFlowNode FindMeetpoint(IControlFlowNode start, IControlFlowNode mustBeAfter, HashSet<IControlFlowNode> visited, List<Block> blocks)
    {
        Stack<IControlFlowNode> work = new();
        work.Push(start);

        while (work.Count > 0)
        {
            IControlFlowNode node = work.Pop();

            if (!visited.Add(node) && node.StartAddress >= mustBeAfter.StartAddress)
            {
                // We found our meetpoint!
                return node;
            }

            if (node is Block block &&
                (block.Instructions is [.., { Kind: IGMInstruction.Opcode.Exit }] ||
                 block.Instructions is [.., { Kind: IGMInstruction.Opcode.Return }]))
            {
                // Exit/return statement: flow to following block if one exists, and isn't already a successor
                if (block.BlockIndex + 1 < blocks.Count)
                {
                    IControlFlowNode following = blocks[block.BlockIndex + 1];
                    while (following.Parent is not null)
                    {
                        following = following.Parent;
                    }
                    if (!node.Successors.Contains(following))
                    {
                        work.Push(following);
                    }
                }
            }

            foreach (IControlFlowNode successor in node.Successors)
            {
                if (successor.StartAddress < node.StartAddress || successor == node)
                {
                    throw new DecompilerException("Unresolved loop when following binary branches");
                }
                work.Push(successor);
            }
        }

        throw new DecompilerException("Failed to find binary branch meetpoint");
    }

    /// <summary>
    /// Helper function to insert a continue/break node into the graph.
    /// </summary>
    private static void InsertContinueOrBreak(IControlFlowNode node, Block block, List<Block> blocks)
    {
        // Remove branch instruction
        block.Instructions.RemoveAt(block.Instructions.Count - 1);

        // Reroute into break/continue node
        IControlFlowNode.DisconnectSuccessor(block, 0);
        if (block.Successors.Count == 0)
        {
            block.Successors.Add(node);
            node.Predecessors.Add(block);

            // Now, we want to connect to the following block.
            // However, we may have some other structure there, so we need to follow the parent(s) of the block.
            if (block.BlockIndex + 1 >= blocks.Count)
            {
                throw new DecompilerException("Expected following block after break/continue");
            }
            IControlFlowNode following = blocks[block.BlockIndex + 1];
            while (following.Parent is not null)
            {
                following = following.Parent;
            }
            node.Successors.Add(following);
            following.Predecessors.Add(node);
        }
        else
        {
            // We already have a node after us - it's an unreachable node.
            // Just insert this break/continue statement between this block and that node.
            if (block.Successors.Count != 1 || !block.Successors[0].Unreachable)
            {
                throw new DecompilerException("Expected unreachable block after break/continue");
            }
            IControlFlowNode.InsertSuccessor(block, 0, node);
        }
    }

    /// <summary>
    /// Resolves continue statements and break statements.
    /// These are relatively trivial to find on a linear pass, especially with "after limits."
    /// </summary>
    private static void ResolveExternalJumps(DecompileContext ctx, Dictionary<Block, Loop> surroundingLoops, Dictionary<Block, int> blockAfterLimits)
    {
        foreach (Block block in ctx.Blocks)
        {
            if (block.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }] && block.Successors.Count >= 1)
            {
                IControlFlowNode node = null;

                // Check that we're not supposed to be ignored
                if (ctx.SwitchIgnoreJumpBlocks.Contains(block))
                {
                    continue;
                }

                // Look for a trivial branch to top or end of surrounding loop
                if (surroundingLoops.TryGetValue(block, out Loop loop))
                {
                    if (block.Successors[0] == loop)
                    {
                        // Detected trivial continue
                        node = new ContinueNode(block.EndAddress);

                        // If enclosing loop is a while loop, it must definitively be a while loop,
                        // as we branch to the very top condition (and not to a for loop incrementor).
                        if (loop is WhileLoop whileLoop)
                        {
                            whileLoop.MustBeWhileLoop = true;
                        }
                    }
                    else if (block.Successors[0].StartAddress >= loop.EndAddress)
                    {
                        // Detected trivial break
                        node = new BreakNode(block.EndAddress);
                    }
                }

                if (node is null)
                {
                    // Check if we're breaking/continuing from inside of a switch statement.
                    if (block.Successors[0] is Block succBlock)
                    {
                        if (ctx.SwitchEndBlocks.Contains(succBlock))
                        {
                            // This is a break from inside of a switch
                            node = new BreakNode(block.EndAddress);
                        }
                        else if (ctx.SwitchContinueBlocks.Contains(succBlock))
                        {
                            // This is a continue from inside of a switch
                            node = new ContinueNode(block.EndAddress);
                        }
                    }
                }

                if (node is null && loop is not null)
                {
                    // Look at after limits and see if we can deduce anything there.
                    int afterLimit = blockAfterLimits[block];
                    if (block.Successors[0].StartAddress > afterLimit)
                    {
                        // Detected continue
                        node = new ContinueNode(block.EndAddress);

                        // If enclosing loop is a while loop, it must actually be a for loop - otherwise we would
                        // be branching to the top of the loop, which would have been detected by now.
                        if (loop is WhileLoop whileLoop)
                        {
                            whileLoop.ForLoopIncrementor = block.Successors[0];
                        }
                    }
                }

                if (node is null)
                {
                    // Didn't find anything.
                    continue;
                }

                // Update control flow graph
                InsertContinueOrBreak(node, block, ctx.Blocks);
            }
        }
    }


    /// <summary>
    /// Removes any branches coming from inside the given BinaryBranch, and exiting into "after".
    /// </summary>
    private static void CleanupAfterPredecessors(BinaryBranch bb, IControlFlowNode after)
    {
        // All branches going into "after" from this branch should come from this branch *only*
        for (int j = after.Predecessors.Count - 1; j >= 0; j--)
        {
            IControlFlowNode curr = after.Predecessors[j];

            // Don't accidentally remove this BinaryBranch going into "after" itself.
            if (curr == bb)
            {
                continue;
            }

            // Check that we're within the bounds of this BinaryBranch.
            if (curr.StartAddress >= bb.StartAddress && curr.EndAddress <= bb.EndAddress)
            {
                // Here, we will additionally remove any "else" branch instructions.
                if (bb.Else is not null && curr.EndAddress == bb.Else.StartAddress && curr is Block b)
                {
                    if (b.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
                    {
                        throw new DecompilerException("Expected branch to skip past else block");
                    }
                    b.Instructions.RemoveAt(b.Instructions.Count - 1);
                }

                // Get rid of this connection to "after" from this internal node.
                curr.Successors.RemoveAll(a => a == after);
                after.Predecessors.RemoveAt(j);
            }
        }
    }

    private readonly struct LimitEntry(int limit, bool fromBinary)
    {
        public int Limit { get; } = limit;
        public bool FromBinary { get; } = fromBinary;
    }

    /// <summary>
    /// Computes the largest possible address any given if statement can have its "after" node, or successor,
    /// by constraining it based on previous branches and loops. Computes for each branch block.
    /// </summary>
    private static Dictionary<Block, int> ComputeBlockAfterLimits(List<Block> blocks, Dictionary<Block, Loop> surroundingLoops)
    {
        Dictionary<Block, int> blockToAfterLimit = new();

        List<LimitEntry> limitStack = [new(blocks[^1].EndAddress, false)];

        foreach (Block b in blocks)
        {
            // If we've passed the address of our smallest limit, remove it.
            while (b.StartAddress >= limitStack[^1].Limit)
            {
                limitStack.RemoveAt(limitStack.Count - 1);
                if (limitStack.Count == 0)
                {
                    break;
                }
            }
            if (limitStack.Count == 0)
            {
                break;
            }

            // Find the limit for this block
            int thisLimit;
            if (b.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }])
            {
                if (limitStack[^1].FromBinary)
                {
                    // Most recent limit is from a binary branch - look one further
                    thisLimit = limitStack[^2].Limit;
                }
                else
                {
                    // Most recent limit is from a direct jump - use that directly
                    thisLimit = limitStack[^1].Limit;
                }
            }
            else
            {
                // If we're not Branch, nor BranchFalse/True, we don't really care about determining the limit
                if (b.Instructions is not [.., { Kind: IGMInstruction.Opcode.BranchFalse }] &&
                    b.Instructions is not [.., { Kind: IGMInstruction.Opcode.BranchTrue }]) // TODO: not sure if necessary, but may be for switch statements
                {
                    continue;
                }

                thisLimit = limitStack[^1].Limit;
            }

            // If we have a loop surrounding this block, we can also use that
            if (surroundingLoops.TryGetValue(b, out Loop loop))
            {
                if (loop.EndAddress < thisLimit)
                {
                    thisLimit = loop.EndAddress;
                }
            }

            // Set resulting limit
            blockToAfterLimit[b] = thisLimit;

            // Update limit stack based on this block
            if (b.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }])
            {
                // We're in a Branch block.
                // If we have a smaller limit based on our jump destination address, push that to the stack.
                int newLimit = b.Successors[0].StartAddress;
                if (newLimit <= limitStack[^1].Limit)
                {
                    limitStack.Add(new(newLimit, false));
                }
                else
                {
                    // If our limit is larger, but we have an identical limit already on the stack,
                    // from a BranchTrue/False, we mark it as no longer from a binary block.
                    // If there's a slot between two other limits available, use that as well.
                    for (int i = limitStack.Count - 2; i >= 0; i--)
                    {
                        if (limitStack[i].Limit == newLimit)
                        {
                            limitStack[i] = new(newLimit, false);
                            break;
                        }
                        else if (limitStack[i].Limit > newLimit)
                        {
                            limitStack.Insert(i + 1, new(newLimit, false));
                            break;
                        }
                    }
                }
            }
            else
            {
                // We're in a BranchFalse/BranchTrue block.
                // If we have a smaller limit based on our jump destination address, push that to the stack.
                int newLimit = b.Successors[1].StartAddress;
                if (newLimit <= limitStack[^1].Limit)
                {
                    limitStack.Add(new(newLimit, true));
                }
            }
        }

        return blockToAfterLimit;
    }

    public static List<BinaryBranch> FindBinaryBranches(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks;
        List<Loop> loops = ctx.LoopNodes;

        List<BinaryBranch> res = new();

        Dictionary<Block, Loop> surroundingLoops = FindSurroundingLoops(blocks, ctx.BlocksByAddress, loops);
        Dictionary<Block, int> blockAfterLimits = ComputeBlockAfterLimits(blocks, surroundingLoops);
        HashSet<IControlFlowNode> visited = new();

        // Resolve all continue/break statements
        ResolveExternalJumps(ctx, surroundingLoops, blockAfterLimits);

        // Iterate over blocks in reverse, as the compiler generates them in the order we want
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            Block block = blocks[i];
            if (block.Instructions is [.., { Kind: IGMInstruction.Opcode.BranchFalse }])
            {
                // Follow "jump" path first, marking off all visited blocks
                VisitAll(block.Successors[1], visited, blocks);

                // Locate meetpoint, by following the non-jump path
                IControlFlowNode after = FindMeetpoint(block.Successors[0], block.Successors[1], visited, blocks);

                // Insert new node!
                BinaryBranch bb = new(block.StartAddress, after.StartAddress);
                bb.Condition = block;
                bb.True = block.Successors[0];
                bb.False = block.Successors[1];
                res.Add(bb);

                // Assign else block if we can immediately detect it
                if (bb.False != after)
                {
                    bb.Else = bb.False;
                }

                // Rewire graph
                if (bb.True == after)
                {
                    // If our true block is the same as the after node, then we have an empty if statement
                    bb.True = new EmptyNode(bb.True.StartAddress);
                }
                else
                {
                    // Disconnect start of "true" block from the condition
                    IControlFlowNode.DisconnectPredecessor(bb.True, 0);
                }
                if (bb.Else is not null)
                {
                    // Disconnect start of "else" block from the condition
                    IControlFlowNode.DisconnectPredecessor(bb.Else, 0);
                }
                else
                {
                    // Check if we have an empty else block
                    for (int j = 0; j < after.Predecessors.Count; j++)
                    {
                        IControlFlowNode curr = after.Predecessors[j];
                        if (curr.StartAddress >= bb.StartAddress && curr.EndAddress <= bb.EndAddress &&
                            curr is Block currBlock && currBlock.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }])
                        {
                            // We've found the leftovers of an empty else block...
                            bb.Else = new EmptyNode(after.StartAddress);
                        }
                    }
                }

                // If the condition block is unreachable, then so is the branch
                if (block.Unreachable)
                {
                    bb.Unreachable = true;
                    block.Unreachable = false;
                }

                // Reroute all nodes going into condition to instead go into the branch
                for (int j = 0; j < block.Predecessors.Count; j++)
                {
                    bb.Predecessors.Add(block.Predecessors[j]);
                    IControlFlowNode.ReplaceConnections(block.Predecessors[j].Successors, block, bb);
                }
                if (block.Parent is not null)
                {
                    IControlFlowNode.ReplaceConnections(block.Parent.Children, block, bb);
                    bb.Parent = block.Parent;
                }
                block.Predecessors.Clear();
                for (int j = 0; j < block.Successors.Count; j++)
                {
                    IControlFlowNode.DisconnectSuccessor(block, j);
                }
                bb.Successors.Add(after);

                // Reroute all predecessors to "after" to come from this branch
                CleanupAfterPredecessors(bb, after);
                after.Predecessors.Add(bb);

                // Update parent status of nodes
                block.Parent = bb;
                bb.True.Parent = bb;
                if (bb.Else is not null)
                {
                    bb.Else.Parent = bb;
                }
            }
        }

        // Sort in order from start to finish
        res.Reverse();

        ctx.BinaryBranchNodes = res;
        return res;
    }

    public void BuildAST(ASTBuilder builder, List<IStatementNode> output)
    {
        // Evaluate condition block
        BlockNode conditionBlock = builder.BuildBlock(Condition);
        conditionBlock.UseBraces = false;
        output.Add(conditionBlock);

        IExpressionNode condition = builder.ExpressionStack.Pop();

        // Evaluate true block
        int initialStackCount = builder.ExpressionStack.Count;
        BlockNode trueBlock = builder.BuildBlock(True);
        int postTrueStackCount = builder.ExpressionStack.Count;

        if (Else is not null)
        {
            // Evaluate else block
            BlockNode elseBlock = builder.BuildBlock(Else);
            int postElseStackCount = builder.ExpressionStack.Count;

            if (postTrueStackCount == initialStackCount + 1 && postElseStackCount == postTrueStackCount + 1)
            {
                // We're actually a conditional (ternary) expression
                IExpressionNode falseExpr = builder.ExpressionStack.Pop();
                IExpressionNode trueExpr = builder.ExpressionStack.Pop();
                builder.ExpressionStack.Push(new ConditionalNode(condition, trueExpr, falseExpr));
            }
            else
            {
                // We're an if statement with an else block attached
                output.Add(new IfNode(condition, trueBlock, elseBlock));
            }
        }
        else
        {
            // We're just a simple if statement
            output.Add(new IfNode(condition, trueBlock));
        }
    }
}
