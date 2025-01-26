/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// Interface for control flow contexts during bytecode generation.
/// </summary>
internal interface IControlFlowContext
{
    /// <summary>
    /// Whether this control flow context requires cleanup upon exiting early.
    /// </summary>
    public bool RequiresCleanup { get; }

    /// <summary>
    /// Whether this control flow context is for a loop.
    /// </summary>
    public bool IsLoop { get; }

    /// <summary>
    /// Whether a break statement has been used on this control flow context.
    /// </summary>
    public bool BreakUsed { get; }

    /// <summary>
    /// Whether a continue statement has been used on this control flow context.
    /// </summary>
    public bool ContinueUsed { get; }

    /// <summary>
    /// Generates cleanup code for the control flow context, using the given bytecode context.
    /// </summary>
    public void GenerateCleanupCode(BytecodeContext context);

    /// <summary>
    /// Uses a break statement branch on this control flow context, for the provided instruction.
    /// </summary>
    public void UseBreak(BytecodeContext context, IGMInstruction instruction);

    /// <summary>
    /// Uses a continue statement branch on this control flow context, for the provided instruction.
    /// </summary>
    public void UseContinue(BytecodeContext context, IGMInstruction instruction);
}

/// <summary>
/// Control flow context for basic loops, such as while/for loops (with no cleanup, etc.).
/// </summary>
internal class BasicLoopContext(IMultiBranchPatch breakPatch, IMultiBranchPatch continuePatch) : IControlFlowContext
{
    /// <inheritdoc/>
    public bool RequiresCleanup => false;

    /// <inheritdoc/>
    public bool IsLoop => true;

    /// <inheritdoc/>
    public bool BreakUsed { get; private set; } = false;

    /// <inheritdoc/>
    public bool ContinueUsed { get; private set; } = false;

    /// <inheritdoc/>
    public void GenerateCleanupCode(BytecodeContext context)
    {
    }

    /// <inheritdoc/>
    public void UseBreak(BytecodeContext context, IGMInstruction instruction)
    {
        BreakUsed = true;
        breakPatch.AddInstruction(context, instruction);
    }

    /// <inheritdoc/>
    public void UseContinue(BytecodeContext context, IGMInstruction instruction)
    {
        ContinueUsed = true;
        continuePatch.AddInstruction(context, instruction);
    }
}
