using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler;

/// <summary>
/// Represents a single VM code fragment, used for single function contexts.
/// </summary>
public class Fragment : IControlFlowNode
{
    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IGMCode CodeEntry { get; }

    public List<Block> Blocks { get; }

    public Fragment(int startAddr, int endAddr, IGMCode codeEntry, List<Block> blocks)
    {
        StartAddress = startAddr;
        EndAddress = endAddr;
        CodeEntry = codeEntry;
        Blocks = blocks;
    }

    /// <summary>
    /// Finds code fragments from a code entry and its list of blocks.
    /// Note that this will modify the control flow and instructions of the existing blocks.
    /// </summary>
    public static List<Fragment> FindFragments(IGMCode code, List<Block> blocks)
    {
        if (code.Parent != null)
            throw new ArgumentException("Expected code entry to be root level.", nameof(code));

        // Map code entry addresses to code entries
        Dictionary<int, IGMCode> codeEntries = new();
        for (int i = 0; i < code.ChildCount; i++)
        {
            IGMCode child = code.GetChild(i);
            codeEntries.Add(child.StartOffset, child);
        }

        // Build fragments, using a stack to track hierarchy
        List<Fragment> fragments = new();
        Stack<Fragment> stack = new();
        Fragment current = new(code.StartOffset, code.Length, code, []);
        fragments.Add(current);
        for (int i = 0; i < blocks.Count; i++)
        {
            Block block = blocks[i];

            // Check if our current fragment is ending at this block
            if (block.StartAddress == current.EndAddress)
            {
                if (stack.Count > 0)
                {
                    // If we're an inner fragment, remove "exit" instruction
                    var lastBlockInstructions = current.Blocks[^1].Instructions;
                    if (lastBlockInstructions[^1].Kind != IGMInstruction.Opcode.Exit)
                        throw new Exception("Expected exit at end of fragment.");
                    lastBlockInstructions.RemoveAt(lastBlockInstructions.Count - 1);

                    // This last block guarantees a single exit node, so remove any of its successors
                    IControlFlowNode.DisconnectSuccessor(current.Blocks[^1], 0);

                    // Go to the fragment the next level up
                    current = stack.Pop();
                }
                else
                {
                    // We're done processing now. Add last block and exit loop.
                    current.Blocks.Add(block);

                    if (block.StartAddress != code.Length)
                        throw new Exception("Code length mismatches final block address.");

                    break;
                }
            }

            // Check for new fragment starting at this block
            if (codeEntries.TryGetValue(block.StartAddress, out IGMCode newCode))
            {
                // Our "current" is now the next level up
                stack.Push(current);

                // Compute the end address of this fragment, by looking at previous block
                Block previous = blocks[i - 1];
                if (previous.Instructions[^1].Kind != IGMInstruction.Opcode.Branch)
                    throw new Exception("Expected branch before fragment start.");
                int endAddr = previous.Successors[0].StartAddress;

                // Make our new "current" be this new fragment
                current = new Fragment(block.StartAddress, endAddr, newCode, []);
                fragments.Add(current);

                // Rewire previous block to jump to this fragment, and this fragment
                // to jump to the successor of the previous block.
                IControlFlowNode.InsertSuccessor(previous, 0, current);
            }

            // Add this block to our current fragment
            current.Blocks.Add(block);
        }

        if (stack.Count > 0)
            throw new Exception("Failed to close all fragments.");

        return fragments;
    }
}
