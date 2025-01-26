/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// Interface for patching branch offsets into an arbitrary number of instructions.
/// </summary>
internal interface IMultiBranchPatch
{
    /// <summary>
    /// Adds an instruction to be patched.
    /// </summary>
    public void AddInstruction(IGMInstruction instruction);
}

/// <summary>
/// Helper struct to make a single forward branch.
/// </summary>
internal readonly ref struct SingleForwardBranchPatch(IGMInstruction instruction)
{
    public void Patch(BytecodeContext context)
    {
        context.PatchBranch(instruction, context.Position - instruction.Address);
    }
}