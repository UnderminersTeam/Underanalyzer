using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

public abstract class Loop : IControlFlowNode
{
    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public abstract List<IControlFlowNode> Children { get; }

    public bool Unreachable { get; set; } = false;

    public Loop(int startAddress, int endAddress)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
    }

    public abstract void UpdateFlowGraph();

    public static List<Loop> FindLoops(List<Block> blocks)
    {
        List<Loop> loops = new();
        HashSet<int> whileLoopsFound = new();

        // Build look-up from block addresses to blocks
        Dictionary<int, Block> blockLookup = new();
        foreach (Block b in blocks)
            blockLookup[b.StartAddress] = b;

        // Search for different loop types based on instruction patterns
        // Do this in reverse order, because we want to find the ends of loops first
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            Block block = blocks[i];

            // If empty, we don't care about the block
            if (block.Instructions.Count == 0)
                continue;

            // Check last instruction (where branches are located)
            IGMInstruction instr = block.Instructions[^1];
            switch (instr.Kind)
            {
                case IGMInstruction.Opcode.Branch:
                    if (instr.BranchOffset < 0)
                    {
                        // While loop detected - only add if this is the first time we see it (in reverse)
                        // If not done only once, then this misfires on "continue" statements
                        int conditionAddr = instr.Address + instr.BranchOffset;
                        if (whileLoopsFound.Add(conditionAddr))
                        {
                            loops.Add(new WhileLoop(conditionAddr, block.EndAddress, 
                                blockLookup[conditionAddr], block, blockLookup[block.EndAddress]));
                        }
                    }
                    break;
                case IGMInstruction.Opcode.BranchFalse:
                    if (instr.BranchOffset < 0)
                    {
                        // Do...until loop detected
                        loops.Add(new DoUntilLoop(block.Successors[1].StartAddress, block.EndAddress,
                            block.Successors[1], block, block.Successors[0]));
                    }
                    break;
                case IGMInstruction.Opcode.BranchTrue:
                    if (instr.BranchOffset < 0)
                    {
                        // Repeat loop detected
                        loops.Add(new RepeatLoop(block.Successors[1].StartAddress, block.EndAddress,
                            block.Successors[1], block, block.Successors[0]));
                    }
                    break;
                case IGMInstruction.Opcode.PushWithContext:
                    {
                        // With loop detected - need to additionally check for break block
                        Block afterBlock = block.Successors[1].Successors[0] as Block;
                        Block breakBlock = null;
                        if (afterBlock.Instructions is [{ Kind: IGMInstruction.Opcode.Branch }])
                        {
                            Block potentialBreakBlock = blockLookup[afterBlock.EndAddress];
                            if (potentialBreakBlock.EndAddress == afterBlock.Successors[0].StartAddress &&
                                potentialBreakBlock.Instructions is 
                                    [{ Kind: IGMInstruction.Opcode.PopWithContext, PopWithContextExit: true }])
                            {
                                breakBlock = potentialBreakBlock;
                            }
                        }
                        loops.Add(new WithLoop(block.EndAddress, block.Successors[1].StartAddress,
                            block.Successors[0], block.Successors[1], afterBlock, breakBlock));
                    }
                    break;
            }
        }

        // Update control flow graph to include the new loops
        loops.Sort((a, b) =>
        {
            if (a.StartAddress < b.StartAddress)
                return -1;
            if (a.StartAddress > b.StartAddress) 
                return 1;
            return b.EndAddress - a.EndAddress;
        });
        foreach (var loop in loops)
            loop.UpdateFlowGraph();

        return loops;
    }
}
