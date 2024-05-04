using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Underanalyzer.Decompiler;

/// <summary>
/// Represents a general binary branch operation. Specifically, only either an if statement or a ternary/conditional operator.
/// </summary>
public class BinaryBranch : IControlFlowNode
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
    private static Dictionary<Block, Loop> FindSurroundingLoops(List<Block> blocks, List<Loop> loops)
    {
        // Make lookup of address -> block
        Dictionary<int, Block> blockByAddress = new();
        foreach (Block b in blocks)
        {
            blockByAddress[b.StartAddress] = b;
        }

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
    private static void VisitAll(
        Block ifStart, IControlFlowNode start, 
        HashSet<IControlFlowNode> visited, Dictionary<Block, int> blockAfterLimits)
    {
        Stack<IControlFlowNode> work = new();
        work.Push(start);

        int afterLimit = blockAfterLimits[ifStart];

        while (work.Count > 0)
        {
            IControlFlowNode node = work.Pop();

            // Check if we're after our block's after limit. If so, don't consider this node.
            if (node.StartAddress > afterLimit)
            {
                continue;
            }

            // Check if we have any non-direct branch predecessors coming from before this if statement.
            // If so, don't consider this node.
            bool doNotConsider = false;
            foreach (var predecessor in node.Predecessors)
            {
                if (predecessor.StartAddress < ifStart.StartAddress &&
                    predecessor is Block block && block.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
                {
                    doNotConsider = true;
                    break;
                }
            }
            if (doNotConsider)
            {
                continue;
            }

            visited.Add(node);

            foreach (IControlFlowNode successor in node.Successors)
            {
                if (successor.StartAddress < node.StartAddress || successor == node)
                    throw new Exception("Unresolved loop");
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
    /// If no node is found, mustBeAfter is returned instead as the best meeting point.
    /// </summary>
    private static IControlFlowNode FindMeetpoint(
        Block ifStart, IControlFlowNode start, IControlFlowNode mustBeAfter, 
        HashSet<IControlFlowNode> visited, Dictionary<Block, int> blockAfterLimits)
    {
        Stack<IControlFlowNode> work = new();
        work.Push(start);

        int afterLimit = blockAfterLimits[ifStart];

        while (work.Count > 0)
        {
            IControlFlowNode node = work.Pop();

            // Check if we're after our block's after limit. If so, don't consider this node.
            if (node.StartAddress > afterLimit)
            {
                continue;
            }

            // Check if we have any non-direct branch predecessors coming from before this if statement.
            // If so, don't consider this node.
            bool doNotConsider = false;
            foreach (var predecessor in node.Predecessors)
            {
                if (predecessor.StartAddress < ifStart.StartAddress && 
                    predecessor is Block block && block.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
                {
                    doNotConsider = true;
                    break;
                }
            }
            if (doNotConsider)
            {
                continue;
            }

            if (!visited.Add(node) && node.StartAddress >= mustBeAfter.StartAddress)
            {
                // We found our meetpoint!
                return node;
            }

            foreach (IControlFlowNode successor in node.Successors)
            {
                if (successor.StartAddress < node.StartAddress || successor == node)
                    throw new Exception("Unresolved loop");
                work.Push(successor);
            }
        }

        return mustBeAfter;
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
                throw new Exception("Expected following block after break/continue");
            IControlFlowNode following = blocks[block.BlockIndex + 1];
            while (following.Parent is not null)
                following = following.Parent;
            node.Successors.Add(following);
            following.Predecessors.Add(node);
        }
        else
        {
            // We already have a node after us - it's an unreachable node.
            // Just insert this break/continue statement between this block and that node.
            if (block.Successors.Count != 1 || !block.Successors[0].Unreachable)
                throw new Exception("Expected unreachable block after break/continue");
            IControlFlowNode.InsertSuccessor(block, 0, node);
        }
    }

    /// <summary>
    /// Resolves continue statements that jump backwards, and break statements that exit the loop.
    /// These are trivial to find on a linear pass, without considering branches.
    /// </summary>
    private static void ResolveBasicBreakContinue(List<Block> blocks, Dictionary<Block, Loop> surroundingLoops, List<IControlFlowNode> output)
    {
        foreach (Block block in blocks)
        {
            if (block.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }] &&
                block.Successors.Count >= 1 && surroundingLoops.TryGetValue(block, out Loop loop))
            {
                IControlFlowNode node;
                if (block.Successors[0] == loop)
                {
                    // Detected trivial continue
                    node = new ContinueNode(block.EndAddress);
                }
                else if (block.Successors[0].StartAddress >= loop.EndAddress)
                {
                    // Detected trivial break
                    node = new BreakNode(block.EndAddress);
                }
                else
                {
                    // Didn't detect either a trivial break or continue
                    continue;
                }
                output?.Add(node);

                // Update control flow graph
                InsertContinueOrBreak(node, block, blocks);
            }
        }
    }

    /// <summary>
    /// Resolves any remaining break/continue statements; ones that require checking for 
    /// branches going out of bounds of the if statement they're contained within.
    /// </summary>
    private static void ResolveOtherBreakContinue(BinaryBranch bb, List<Block> blocks, Dictionary<Block, Loop> surroundingLoops, List<IControlFlowNode> output)
    {
        int startIndex = (bb.Condition as Block).BlockIndex;
        int endAddress = bb.Successors[0].StartAddress;
        for (int i = startIndex + 1; i < blocks.Count && blocks[i].StartAddress < endAddress; i++)
        {
            Block block = blocks[i];
            if (block.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }] &&
                block.Successors.Count >= 1 && block.Successors[0].StartAddress >= endAddress)
            {
                surroundingLoops.TryGetValue(block, out Loop loop);

                IControlFlowNode node;
                if (loop is not null &&
                    (block.Successors[0] is not Block ||
                    (block.Successors[0] as Block).Instructions is not [{ Kind: IGMInstruction.Opcode.PopDelete }, ..]))
                {
                    // Detected continue (we're definitely *not* breaking out of a switch)
                    node = new ContinueNode(block.EndAddress);

                    // If enclosing loop is a while loop, it must actually be a for loop - otherwise we would
                    // be branching to the top of the loop, which would have been detected by now.
                    // TODO: In switch statement processing, while loop -> for loop conversion needs to happen too
                    if (loop is WhileLoop whileLoop)
                    {
                        whileLoop.ForLoopIncrementor = block.Successors[0];
                    }
                }
                else
                {
                    // Detected break (in switch statements specifically - we detected a PopDelete at destination).
                    // Technically, this can still be a continue in certain situations, so we set a
                    // flag for switch statements to verify this later.
                    //
                    // To be specific, this can be a continue if the last part of a for loop involves a return/exit,
                    // and the for loop is contained inside of a repeat loop, OR if this is a continue inside of
                    // a switch statement, which branches to a special block generated at the end of the switch.
                    //
                    // When processing switch statements, we check if this "break" branches to the end of the
                    // enclosing switch statement. Otherwise, it must be a continue statement.
                    node = new BreakNode(block.EndAddress, true);
                    node.Children.AddRange(block.Successors);
                }
                output?.Add(node);

                // Update control flow graph
                InsertContinueOrBreak(node, block, blocks);
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
                        throw new Exception("Expected branch to skip past else block");
                    b.Instructions.RemoveAt(b.Instructions.Count - 1);
                }

                // Get rid of this connection to "after" from this internal node.
                curr.Successors.RemoveAll(a => a == after);
                after.Predecessors.RemoveAt(j);
            }
        }
    }

    /// <summary>
    /// Computes the largest possible address any given if statement can have its "after" node, or successor,
    /// by constraining it based on previous branches and loops. Computes for each binary branch block.
    /// </summary>
    private static Dictionary<Block, int> ComputeBlockAfterLimits(List<Block> blocks, Dictionary<Block, Loop> surroundingLoops)
    {
        Dictionary<Block, int> blockToAfterLimit = new();

        Stack<int> limitStack = new();
        limitStack.Push(blocks[^1].EndAddress);

        foreach (Block b in blocks)
        {
            // If we've passed the address of our smallest limit, remove it.
            if (b.StartAddress >= limitStack.Peek())
            {
                limitStack.Pop();
            }

            // We only care about processing binary branches (e.g. if statements, switch cases).
            if (b.Instructions is not [.., { Kind: IGMInstruction.Opcode.BranchFalse }] &&
                b.Instructions is not [.., { Kind: IGMInstruction.Opcode.BranchTrue }]) // TODO: not sure if necessary, but may be for switch statements
            {
                continue;
            }

            // Compute the limit for this specific branch block.
            // If the surrounding loop ends earlier, use that instead.
            int thisLimit = limitStack.Peek();
            if (surroundingLoops.TryGetValue(b, out Loop loop))
            {
                if (loop.EndAddress < thisLimit)
                {
                    thisLimit = loop.EndAddress;
                }
            }
            blockToAfterLimit[b] = thisLimit;

            // If we have a smaller limit in our jump, push that to the stack.
            int newLimit = b.Successors[1].StartAddress;
            if (newLimit < limitStack.Peek())
            {
                limitStack.Push(newLimit);
            }
        }

        return blockToAfterLimit;
    }

    public static List<BinaryBranch> FindBinaryBranches(List<Block> blocks, List<Loop> loops, List<IControlFlowNode> breakContinueOutput = null)
    {
        List<BinaryBranch> res = new();

        Dictionary<Block, Loop> surroundingLoops = FindSurroundingLoops(blocks, loops);
        Dictionary<Block, int> blockAfterLimits = ComputeBlockAfterLimits(blocks, surroundingLoops);
        HashSet<IControlFlowNode> visited = new();

        // Resolve continue statements that jump backwards
        ResolveBasicBreakContinue(blocks, surroundingLoops, breakContinueOutput);

        // Iterate over blocks in reverse, as the compiler generates them in the order we want
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            Block block = blocks[i];
            if (block.Instructions is [.., { Kind: IGMInstruction.Opcode.BranchFalse }])
            {
                // Follow "jump" path first, marking off all visited blocks
                VisitAll(block, block.Successors[1], visited, blockAfterLimits);

                // Locate meetpoint, by following the non-jump path
                IControlFlowNode after = FindMeetpoint(block, block.Successors[0], block.Successors[1], visited, blockAfterLimits);

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

                // Resolve any remaining break/continue statements inside of this loop.
                // This is primarily a problem in "for" loops, "do...until" loops, and "repeat" loops,
                // where continue jumps to the bottom of the loop rather than the top.
                // It's also a problem with using break/continue inside of switch statements.
                ResolveOtherBreakContinue(bb, blocks, surroundingLoops, breakContinueOutput);
                CleanupAfterPredecessors(bb, after);
            }
        }

        // Sort in order from start to finish
        res.Reverse();

        return res;
    }
}
