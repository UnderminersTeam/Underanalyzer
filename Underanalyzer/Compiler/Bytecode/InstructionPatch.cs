/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// Struct containing references to lists of instruction patches, to be easily copied around.
/// </summary>
internal readonly struct InstructionPatches
{
    /// <summary>
    /// List of variable patches generated during code generation.
    /// </summary>
    public List<VariablePatch>? VariablePatches { get; init; }

    /// <summary>
    /// List of function patches generated during code generation.
    /// </summary>
    public List<FunctionPatch>? FunctionPatches { get; init; }

    /// <summary>
    /// List of string patches generated during code generation.
    /// </summary>
    public List<StringPatch>? StringPatches { get; init; }

    /// <summary>
    /// Creates an instruction patch struct, initialized with list capacity for patches.
    /// </summary>
    public static InstructionPatches Create()
    {
        return new InstructionPatches()
        {
            VariablePatches = new(32),
            FunctionPatches = new(32),
            StringPatches = new(16)
        };
    }
}

/// <summary>
/// Instruction patch base interface.
/// </summary>
internal interface IInstructionPatch
{
    /// <summary>
    /// Associated instruction to patch, or <see langword="null"/> if none is yet assigned.
    /// </summary>
    public IGMInstruction? Instruction { get; set; }
}

/// <summary>
/// A variable patch used during bytecode generation, to assign variables to instructions.
/// </summary>
internal record struct VariablePatch(string Name, InstanceType InstanceType, VariableType VariableType = VariableType.Normal, bool IsBuiltin = false) : IInstructionPatch
{
    /// <inheritdoc/>
    public IGMInstruction? Instruction { get; set; }

    /// <summary>
    /// Instance type to use for instruction. Sometimes differs from <see cref="InstanceType"/>, due to compiler quirks.
    /// </summary>
    public InstanceType InstructionInstanceType { get; set; } = InstanceType;
}

/// <summary>
/// A function patch used during bytecode generation, to assign functions to instructions.
/// </summary>
internal record struct FunctionPatch(string Name, IBuiltinFunction? BuiltinFunction = null) : IInstructionPatch
{
    /// <inheritdoc/>
    public IGMInstruction? Instruction { get; set; }
}

/// <summary>
/// A string patch used during bytecode generation, to link to strings.
/// </summary>
internal record struct StringPatch(string Content) : IInstructionPatch
{
    /// <inheritdoc/>
    public IGMInstruction? Instruction { get; set; }
}

