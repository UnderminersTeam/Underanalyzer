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

    private static void VisitAll(IControlFlowNode start, HashSet<IControlFlowNode> visited)
    {
        Stack<IControlFlowNode> work = new();
        work.Push(start);

        while (work.Count > 0)
        {
            IControlFlowNode node = work.Pop();
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

    private static IControlFlowNode FindMeetpoint(IControlFlowNode start, IControlFlowNode mustBeAfter, HashSet<IControlFlowNode> visited)
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

            foreach (IControlFlowNode successor in node.Successors)
            {
                if (successor.StartAddress < node.StartAddress || successor == node)
                    throw new Exception("Unresolved loop");
                work.Push(successor);
            }
        }

        throw new Exception("Failed to find meetpoint!");
    }

    /// <summary>
    /// Resolves continue statements that jump backwards, and break statements that exit the loop.
    /// These are trivial to find on a linear pass, without considering branches.
    /// </summary>
    private static void ResolveBasicBreakContinue(List<Block> blocks, Dictionary<Block, Loop> surroundingLoops)
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
        }
    }

    public static List<BinaryBranch> FindBinaryBranches(List<Block> blocks, List<Loop> loops)
    {
        List<BinaryBranch> res = new();

        Dictionary<Block, Loop> surroundingLoops = FindSurroundingLoops(blocks, loops);
        HashSet<IControlFlowNode> visited = new();

        // Resolve continue statements that jump backwards
        ResolveBasicBreakContinue(blocks, surroundingLoops);

        // Iterate over blocks in reverse, as the compiler generates them in the order we want
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            Block block = blocks[i];
            if (block.Instructions is [.., { Kind: IGMInstruction.Opcode.BranchFalse }])
            {
                // Follow "jump" path first, marking off all visited blocks
                VisitAll(block.Successors[1], visited);

                // Locate meetpoint, by following the non-jump path
                IControlFlowNode after = FindMeetpoint(block.Successors[0], block.Successors[1], visited);

                // Insert new node!
                BinaryBranch bb = new(block.StartAddress, after.StartAddress);
                bb.Condition = block;
                bb.True = block.Successors[0];
                bb.False = block.Successors[1];
                if (bb.False != after)
                    bb.Else = bb.False;
                res.Add(bb);

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
                if (bb.Else != null)
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

                // All branches going into "after" from this branch should come from this branch
                for (int j = after.Predecessors.Count - 1; j >= 0; j--)
                {
                    IControlFlowNode curr = after.Predecessors[j];
                    if (curr.StartAddress >= bb.StartAddress && curr.EndAddress <= bb.EndAddress)
                    {
                        curr.Successors.RemoveAll(a => a == after);
                        after.Predecessors.RemoveAt(j);
                    }
                }
                after.Predecessors.Add(bb);

                // Update parent status of nodes
                block.Parent = bb;
                bb.True.Parent = bb;
                if (bb.Else != null)
                    bb.Else.Parent = bb;

                // TODO: Remove branch instructions from else branches
                // Also will want to add test assertions to existing tests for this, and this will likely affect loops too
            }
        }

        // Sort in order from start to finish
        res.Reverse();

        return res;
    }
}
