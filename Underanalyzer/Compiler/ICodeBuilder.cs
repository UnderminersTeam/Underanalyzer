/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Diagnostics.CodeAnalysis;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler;

/// <summary>
/// Represents an implementation for building code entries and emitting instructions, during compilation.
/// </summary>
public interface ICodeBuilder
{
    /// <summary>
    /// Creates an instruction with an address and an opcode.
    /// </summary>
    public IGMInstruction CreateInstruction(int address, Opcode opcode);

    /// <summary>
    /// Creates an instruction with an address, an opcode, and a single data type.
    /// </summary>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, DataType dataType);

    /// <summary>
    /// Creates an instruction with an address, an opcode, and two data types.
    /// </summary>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, DataType dataType1, DataType dataType2);

    /// <summary>
    /// Creates an instruction with an address, an opcode, two data types, and a 16-bit integer value.
    /// </summary>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, short value, DataType dataType1, DataType dataType2);

    /// <summary>
    /// Creates an instruction with an address, an opcode, two data types, and a 32-bit integer value.
    /// </summary>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, int value, DataType dataType1, DataType dataType2);

    /// <summary>
    /// Creates an instruction with an address, an opcode, two data types, and a 64-bit integer value.
    /// </summary>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, long value, DataType dataType1, DataType dataType2);

    /// <summary>
    /// Creates an instruction with an address, an opcode, two data types, and a 64-bit floating point value.
    /// </summary>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, double value, DataType dataType1, DataType dataType2);

    /// <summary>
    /// Creates an instruction with an address and an argument count.
    /// </summary>
    /// <remarks>
    /// The instruction will be opcode <see cref="Opcode.Call"/>, with data type 1 being <see cref="DataType.Int32"/>.
    /// </remarks>
    public IGMInstruction CreateCallInstruction(int address, int argumentCount);

    /// <summary>
    /// Patches an existing instruction with a variable reference.
    /// </summary>
    public void PatchInstruction(IGMInstruction instruction, string variableName, InstanceType instanceType, VariableType variableType, bool isBuiltin);

    /// <summary>
    /// Patches an existing instruction with a function reference.
    /// </summary>
    public void PatchInstruction(IGMInstruction instruction, string functionName, IBuiltinFunction? builtinFunction);

    /// <summary>
    /// Patches an existing instruction with a string reference.
    /// </summary>
    public void PatchInstruction(IGMInstruction instruction, string stringContent);

    /// <summary>
    /// Returns whether a global function name of any kind exists with the given name.
    /// </summary>
    /// <remarks>
    /// This should return true if the function either already exists, or can be created during (or before) instruction patching.
    /// </remarks>
    public bool IsGlobalFunctionName(string name);
}
