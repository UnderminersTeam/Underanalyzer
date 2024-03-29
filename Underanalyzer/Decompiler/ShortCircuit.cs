﻿using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

public class ShortCircuit : IControlFlowNode
{
    public enum LogicType
    {
        And,
        Or
    }

    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = new();

    public bool Unreachable { get; set; } = false;

    public LogicType LogicKind { get; }

    public ShortCircuit(int startAddress, int endAddress, LogicType logicKind, List<IControlFlowNode> children)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
        LogicKind = logicKind;
        Children = children;
    }

    /// <summary>
    /// Finds all short-circuit operations contained within a list of blocks, and updates the control flow graph accordingly.
    /// </summary>
    public static List<ShortCircuit> FindShortCircuits(List<Block> blocks, bool oldBytecodeVersion = false)
    {
        List<ShortCircuit> shortCircuits = new();

        // Identify and restructure short circuits
        foreach (var block in blocks)
        {
            // Match push.e (or on old versions, pushi.e) instruction, standalone in a block
            if (( oldBytecodeVersion &&  
                    block is { Instructions: [{ Kind: IGMInstruction.Opcode.PushImmediate, 
                                                Type1: IGMInstruction.DataType.Int16 }] })
                    ||
                (!oldBytecodeVersion && 
                    block is { Instructions: [{ Kind: IGMInstruction.Opcode.Push,
                                                Type1: IGMInstruction.DataType.Int16 }] }))
            {
                // Add child nodes
                List<IControlFlowNode> children = [block.Predecessors[0]];
                for (int i = 0; i < block.Predecessors.Count; i++)
                {
                    // Connect to the next condition (the non-branch path from the previous condition)
                    children.Add(block.Predecessors[i].Successors[0]);
                }

                // Create actual node
                LogicType logicKind = (block.Instructions[0].ValueShort == 0) ? LogicType.And : LogicType.Or;
                ShortCircuit sc = new(children[0].StartAddress, block.EndAddress, logicKind, children);
                shortCircuits.Add(sc);

                // Remove branches and connections from previous blocks (not necessarily children!)
                for (int i = block.Predecessors.Count - 1; i >= 0; i--)
                {
                    Block pred = block.Predecessors[i] as Block;
                    pred.Instructions.RemoveAt(pred.Instructions.Count - 1);
                    IControlFlowNode.DisconnectSuccessor(pred, 1);
                    IControlFlowNode.DisconnectSuccessor(pred, 0);
                }
                Block finalBlock = blocks[block.BlockIndex - 1];
                finalBlock.Instructions.RemoveAt(finalBlock.Instructions.Count - 1);
                IControlFlowNode.DisconnectSuccessor(finalBlock, 0);

                // Remove original push instruction that was detected
                block.Instructions.RemoveAt(0);

                // Update overarching control flow
                IControlFlowNode.InsertStructure(children[0], block.Successors[0], sc);

                // Update parent status of the first child, as well as this loop, for later operation
                sc.Parent = children[0].Parent;
                children[0].Parent = sc;

                // Update parent status of remaining children, so they can be later updated if necessary
                for (int i = 1; i < children.Count; i++)
                    children[i].Parent = sc;
            }
        }

        return shortCircuits;
    }

    public override string ToString()
    {
        return $"{nameof(ShortCircuit)} (start address {StartAddress}, end address {EndAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
    }
}
