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
    /// Generates cleanup code for the control flow context, using the given bytecode context.
    /// </summary>
    public void GenerateCleanupCode(BytecodeContext context);
}
