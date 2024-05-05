using System.Collections.Generic;

namespace Underanalyzer.Decompiler.ControlFlow;

internal class Nullish : IControlFlowNode
{
    public enum NullishType
    {
        Expression,
        Assignment
    }

    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = [null];

    public bool Unreachable { get; set; } = false;

    public NullishType NullishKind { get; }

    /// <summary>
    /// The node that gets executed if the predecessor has a nullish value on the top of the stack after it.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this has its predecessors disconnected. 
    /// All paths exiting from it are also isolated from the external graph.
    /// </remarks>
    public IControlFlowNode IfNullish { get => Children[0]; private set => Children[0] = value; }

    public Nullish(int startAddress, int endAddress, NullishType nullishKind, IControlFlowNode ifNullishNode)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
        NullishKind = nullishKind;
        IfNullish = ifNullishNode;
    }

    /// <summary>
    /// Finds all nullish operations present in a list of blocks, and updates the control flow graph accordingly.
    /// </summary>
    public static List<Nullish> FindNullish(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks;

        List<Nullish> res = new();

        foreach (var block in blocks)
        {
            // Search for pattern
            if (block.Instructions is
                [..,
                { Kind: IGMInstruction.Opcode.Extended, ExtKind: IGMInstruction.ExtendedOpcode.IsNullishValue },
                { Kind: IGMInstruction.Opcode.BranchFalse }
                ])
            {
                Block ifNullishBlock = block.Successors[0] as Block;
                Block afterBlock = block.Successors[1] as Block;

                // Determine nullish type by using the block "after"
                NullishType nullishKind = NullishType.Expression;
                if (afterBlock.Instructions is [{ Kind: IGMInstruction.Opcode.PopDelete }, ..])
                    nullishKind = NullishType.Assignment;

                Nullish n = new(block.EndAddress, afterBlock.StartAddress, nullishKind, ifNullishBlock);
                res.Add(n);

                // Remove instructions from this block
                block.Instructions.RemoveRange(block.Instructions.Count - 2, 2);

                // Remove pop instruction from "if nullish" block
                ifNullishBlock.Instructions.RemoveAt(0);

                Block endOfNullishBlock = null;
                if (nullishKind == NullishType.Assignment)
                {
                    // Remove pop instruction from "after" block
                    afterBlock.Instructions.RemoveAt(0);

                    // Our "end of nullish" block is always before the "after" block.
                    // Remove its branch instruction.
                    endOfNullishBlock = blocks[afterBlock.BlockIndex - 1];
                    endOfNullishBlock.Instructions.RemoveAt(endOfNullishBlock.Instructions.Count - 1);
                }

                // Disconnect sections of graph
                IControlFlowNode.DisconnectPredecessor(ifNullishBlock, 0);
                if (nullishKind == NullishType.Expression)
                    IControlFlowNode.DisconnectPredecessor(afterBlock, 1);
                IControlFlowNode.DisconnectPredecessor(afterBlock, 0);
                if (endOfNullishBlock is not null)
                    IControlFlowNode.DisconnectSuccessor(endOfNullishBlock, 0);

                // Insert new node into graph
                block.Successors.Add(n);
                n.Predecessors.Add(block);
                n.Successors.Add(afterBlock);
                afterBlock.Predecessors.Add(n);
            }
        }

        ctx.NullishNodes = res;
        return res;
    }
}
