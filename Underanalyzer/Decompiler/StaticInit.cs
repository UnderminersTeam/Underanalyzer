using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

/// <summary>
/// Represents a static initialization block for a function.
/// </summary>
public class StaticInit : IControlFlowNode
{
    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = [null];

    public bool Unreachable { get; set; } = false;

    /// <summary>
    /// The top of the static initialization block.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this has its predecessors disconnected.
    /// </remarks>
    public IControlFlowNode Head { get => Children[0]; private set => Children[0] = value; }

    public StaticInit(int startAddress, int endAddress, IControlFlowNode head)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
        Head = head;
    }

    /// <summary>
    /// Finds all static initialization blocks present in a list of blocks, and updates the control flow graph accordingly.
    /// </summary>
    public static List<StaticInit> FindStaticInits(List<Block> blocks)
    {
        List<StaticInit> res = new();

        foreach (var block in blocks)
        {
            // Check for pattern
            if (block.Instructions is [.., 
                { Kind: IGMInstruction.Opcode.Extended, ExtKind: IGMInstruction.ExtendedOpcode.HasStaticInitialized }, 
                { Kind: IGMInstruction.Opcode.BranchTrue }])
            {
                StaticInit si = new(block.EndAddress, block.Successors[1].StartAddress, block.Successors[0]);
                res.Add(si);

                // Remove instructions from this block
                block.Instructions.RemoveRange(block.Instructions.Count - 2, 2);

                // Remove instruction from ending block
                Block afterBlock = block.Successors[1] as Block;
                afterBlock.Instructions.RemoveAt(0);

                // Disconnect predecessors of the head and our after block
                IControlFlowNode.DisconnectPredecessor(si.Head, 0);
                IControlFlowNode.DisconnectPredecessor(afterBlock, 1);
                IControlFlowNode.DisconnectPredecessor(afterBlock, 0);

                // Insert into control flow graph (done manually, here)
                block.Successors.Add(si);
                si.Predecessors.Add(block);
                si.Successors.Add(afterBlock);
                afterBlock.Predecessors.Add(si);

                // Update parent status of head and this structure
                si.Parent = si.Head.Parent;
                si.Head.Parent = si;
            }
        }

        return res;
    }
}
