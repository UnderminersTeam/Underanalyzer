using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Underanalyzer.Decompiler.ControlFlow;

internal class Switch : IControlFlowNode
{
    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = new();

    public bool Unreachable { get; set; } = false;

    public Switch(int startAddress, int endAddress)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
    }

    /// <summary>
    /// Initial detection data for a switch statement, used to prevent calculations being done twice.
    /// </summary>
    public class SwitchDetectionData
    {
        public Block EndBlock { get; set; } = null;
        public Block ContinueBlock { get; set; } = null;
        public Block ContinueSkipBlock { get; set; } = null;
        public Block EndOfCaseBlock { get; set; } = null;
        public Block DefaultBranchBlock { get; set; } = null;
    }

    private static void DetectionPass(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks;
        Stack<int> switchTops = new();

        // Go backwards; first good candidate block must be the end of a switch statement
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            Block block = blocks[i];

            // Leave switch statements that we're already past
            if (switchTops.Count > 0 && block.StartAddress <= switchTops.Peek())
            {
                switchTops.Pop();
            }

            // Ensure PopDelete is at start of block
            if (block.Instructions is not [{ Kind: IGMInstruction.Opcode.PopDelete }, ..])
            {
                continue;
            }

            // Ensure our earliest predecessor is a block ending with Branch (excluding blocks ending in BranchTrue, as those are cases)
            if (block.Predecessors.Count == 0)
            {
                continue;
            }
            IControlFlowNode earliestPredecessor = block.Predecessors[0];
            for (int j = 1; j < block.Predecessors.Count; j++)
            {
                if (block.Predecessors[j] is Block predCaseBlock &&
                    predCaseBlock.Instructions is [.., { Kind: IGMInstruction.Opcode.BranchTrue }])
                {
                    continue;
                }
                if (block.Predecessors[j].StartAddress < earliestPredecessor.StartAddress)
                {
                    earliestPredecessor = block.Predecessors[j];
                }
            }
            if (earliestPredecessor is not Block predBlock ||
                predBlock.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
            {
                continue;
            }

            // If this block ends with return/exit, we need to check that this isn't an early
            // return/exit from within a switch statement.
            if (block.Instructions[^1].Kind is IGMInstruction.Opcode.Exit or IGMInstruction.Opcode.Return)
            {
                // Count how many PopDelete instructions we have in this block.
                int numPopDeletes = 1;
                for (int j = 1; j < block.Instructions.Count; j++)
                {
                    if (block.Instructions[j].Kind == IGMInstruction.Opcode.PopDelete)
                    {
                        numPopDeletes++;
                    }
                }

                // If this number isn't equal to the number of switches we're inside of + 1,
                // then this is an early return/exit.
                if (numPopDeletes != switchTops.Count + 1)
                {
                    continue;
                }
            }

            // We've found the end of a switch statement
            ctx.SwitchEndBlocks.Add(block);
            switchTops.Push(earliestPredecessor.StartAddress);

            // Create detection data
            SwitchDetectionData data = new()
            {
                EndBlock = block
            };
            ctx.SwitchData.Add(data);

            // Check if we have a continue block immediately preceding this end block
            if (block.BlockIndex == 0)
            {
                continue;
            }
            Block previousBlock = blocks[block.BlockIndex - 1];
            if (previousBlock.Instructions is not
                [{ Kind: IGMInstruction.Opcode.PopDelete }, { Kind: IGMInstruction.Opcode.Branch }])
            {
                continue;
            }

            // This block should be a continue block, but additionally check we have a branch around it
            if (previousBlock.BlockIndex == 0)
            {
                continue;
            }
            Block previousPreviousBlock = blocks[previousBlock.BlockIndex - 1];
            if (previousPreviousBlock.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
            {
                continue;
            }
            if (previousPreviousBlock.Successors.Count != 1 || previousPreviousBlock.Successors[0] != block)
            {
                continue;
            }

            // This is definitely a switch continue block
            ctx.SwitchContinueBlocks.Add(previousBlock);
            ctx.SwitchIgnoreJumpBlocks.Add(previousPreviousBlock);
            data.ContinueBlock = previousBlock;
            data.ContinueSkipBlock = previousPreviousBlock;

            // Prevent this block from processing during the next iteration
            i--;
        }
    }

    private static void DetailPass(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks;

        foreach (SwitchDetectionData data in ctx.SwitchData)
        {
            // Find first predecessor that ends in Branch (should be the first one that *doesn't* end in BranchTrue)
            Block firstBranchPredecessor = null;
            foreach (IControlFlowNode pred in data.EndBlock.Predecessors)
            {
                if (pred is Block predBlock && predBlock.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }])
                {
                    firstBranchPredecessor = predBlock;
                    break;
                }
            }
            if (firstBranchPredecessor == null)
                throw new Exception("Failed to find end of switch cases");

            // Need to detect whether or not we have a default case in this switch.
            // If previous block ends with Branch, then:
            //  - If it branches beyond the end of the switch (or backwards), then it can't be the default branch itself.
            //      -> Fall into case where previous block doesn't end with Branch
            //  - If it branches into the switch, then it's clearly the default branch
            // If the previous block doesn't end with Branch, then:
            //  - If the next block is Unreachable, and only contains Branch, then firstBranchPredecessor is the default branch
            //  - Otherwise, there's no default branch
            data.EndOfCaseBlock = firstBranchPredecessor;
            bool prevBlockIsDefaultBranch;
            if (firstBranchPredecessor.BlockIndex >= 1)
            {
                Block prevBlock = blocks[firstBranchPredecessor.BlockIndex - 1];
                if (prevBlock.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
                {
                    prevBlockIsDefaultBranch = false;
                }
                else if (prevBlock.Successors[0].StartAddress > data.EndBlock.StartAddress ||
                         prevBlock.Successors[0].StartAddress <= prevBlock.StartAddress)
                {
                    prevBlockIsDefaultBranch = false;
                }
                else
                {
                    prevBlockIsDefaultBranch = true;
                    data.DefaultBranchBlock = prevBlock;
                }
            }
            else
            {
                prevBlockIsDefaultBranch = false;
            }
            if (!prevBlockIsDefaultBranch)
            {
                Block nextBlock = blocks[firstBranchPredecessor.BlockIndex + 1];
                if (nextBlock.Unreachable && nextBlock.Instructions is [{ Kind: IGMInstruction.Opcode.Branch }])
                {
                    data.DefaultBranchBlock = firstBranchPredecessor;
                    data.EndOfCaseBlock = nextBlock;
                }
            }

            // Update list of blocks that we should ignore
            ctx.SwitchIgnoreJumpBlocks.Add(data.EndOfCaseBlock);
            if (data.DefaultBranchBlock is not null)
            {
                ctx.SwitchIgnoreJumpBlocks.Add(data.DefaultBranchBlock);
            }
        }
    }

    /// <summary>
    /// Scans for all blocks representing the end of a switch statement, the "continue" block of a switch statement,
    /// as well as other important branch blocks that should not be touched by binary branch break/continue detection.
    /// Stores this data for later use when creating/inserting actual switch statements into the graph.
    /// </summary>
    public static void FindSwitchStatements(DecompileContext ctx)
    {
        ctx.SwitchEndBlocks = new();
        ctx.SwitchData = new();
        ctx.SwitchContinueBlocks = new();
        ctx.SwitchIgnoreJumpBlocks = new();

        // First pass: simply detect the end blocks of switch statements, as well as continue blocks.
        // We do this first as this requires a special algorithm to prevent false positives/negatives.
        DetectionPass(ctx);

        // Second pass: find details about the remaining important blocks.
        // We do this second as this sometimes requires knowledge of surrounding switch statements.
        DetailPass(ctx);
    }
}
