/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// A variable patch used during bytecode generation, to assign variables to instructions.
/// </summary>
internal record struct VariablePatch(string Name, InstanceType InstanceType, VariableType VariableType = VariableType.Normal, bool IsBuiltin = false)
{
    /// <summary>
    /// Associated instruction to patch, or <see langword="null"/> if none is yet assigned.
    /// </summary>
    public IGMInstruction? Instruction { get; set; }
}
