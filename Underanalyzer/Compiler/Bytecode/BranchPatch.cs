/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// Interface for patching branch offsets into an arbitrary number of instructions.
/// </summary>
internal interface IMultiBranchPatch
{
    /// <summary>
    /// Adds an instruction to be patched.
    /// </summary>
    public void AddInstruction(BytecodeContext context, IGMInstruction instruction);
}

/// <summary>
/// Branch patch for an arbitrary number of forward branches.
/// </summary>
internal readonly struct MultiForwardBranchPatch() : IMultiBranchPatch
{
    // Address that will be branched to by all patched instructions
    private readonly List<IGMInstruction> _instructions = new(4);

    /// <summary>
    /// Whether this branch patch has been used by any instructions.
    /// </summary>
    public bool Used => _instructions.Count > 0;

    /// <summary>
    /// Number of instructions that have used this patch.
    /// </summary>
    public int NumberUsed => _instructions.Count;

    /// <inheritdoc/>
    public void AddInstruction(BytecodeContext context, IGMInstruction instruction)
    {
        _instructions.Add(instruction);
    }

    /// <summary>
    /// Patches all added instructions, based on the current bytecode position.
    /// </summary>
    public void Patch(BytecodeContext context)
    {
        int destAddress = context.Position;
        foreach (IGMInstruction instruction in _instructions)
        {
            context.PatchBranch(instruction, destAddress - instruction.Address);
        }
    }
}

/// <summary>
/// Branch patch for an arbitrary number of backward branches.
/// </summary>
internal readonly struct MultiBackwardBranchPatch(BytecodeContext context) : IMultiBranchPatch
{
    // Address that will be branched to by all patched instructions
    private readonly int _destAddress = context.Position;

    /// <inheritdoc/>
    public void AddInstruction(BytecodeContext context, IGMInstruction instruction)
    {
        context.PatchBranch(instruction, _destAddress - instruction.Address);
    }
}

/// <summary>
/// Branch patch for an arbitrary number of backward branches, but also tracked.
/// </summary>
internal class MultiBackwardBranchPatchTracked(BytecodeContext context) : IMultiBranchPatch
{
    // Address that will be branched to by all patched instructions
    private readonly int _destAddress = context.Position;

    /// <summary>
    /// Whether this branch patch has been used by any instructions.
    /// </summary>
    public bool Used => NumberUsed > 0;

    /// <summary>
    /// Number of instructions that have used this patch.
    /// </summary>
    public int NumberUsed { get; private set; } = 0;

    /// <inheritdoc/>
    public void AddInstruction(BytecodeContext context, IGMInstruction instruction)
    {
        NumberUsed++;
        context.PatchBranch(instruction, _destAddress - instruction.Address);
    }
}

/// <summary>
/// Helper struct to make a single forward branch.
/// </summary>
internal readonly ref struct SingleForwardBranchPatch(IGMInstruction instruction)
{
    /// <summary>
    /// Patches the single instruction, based on the current bytecode position.
    /// </summary>
    public void Patch(BytecodeContext context)
    {
        context.PatchBranch(instruction, context.Position - instruction.Address);
    }
}
