/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// Represents a function entry produced during code generation.
/// </summary>
public sealed class FunctionEntry
{
    /// <summary>
    /// Byte offset of the function entry in the bytecode.
    /// </summary>
    public int BytecodeOffset { get; }

    /// <summary>
    /// Number of local variables declared in the function entry.
    /// </summary>
    public int LocalCount { get; }

    /// <summary>
    /// Number of arguments passed into the function entry.
    /// </summary>
    public int ArgumentCount { get; }

    /// <summary>
    /// Name of the function, if one exists; <see langword="null"/> otherwise.
    /// </summary>
    public string? FunctionName { get; }

    /// <summary>
    /// Kind of function entry. Useful for generating a final code entry name.
    /// </summary>
    public FunctionEntryKind Kind { get; }

    /// <summary>
    /// Function object that this function entry resolves to.
    /// </summary>
    public IGMFunction? ResolvedFunction { get; private set; } = null;

    internal FunctionEntry(int bytecodeOffset, int localCount, int argumentCount, string? functionName, FunctionEntryKind kind)
    {
        BytecodeOffset = bytecodeOffset;
        LocalCount = localCount;
        ArgumentCount = argumentCount;
        FunctionName = functionName;
        Kind = kind;
    }

    /// <summary>
    /// Resolves this function entry to a specific function object, old or new.
    /// </summary>
    public void Resolve(IGMFunction function)
    {
        ResolvedFunction = function;
    }
}

/// <summary>
/// Kinds of function entries that can exist.
/// </summary>
public enum FunctionEntryKind
{
    /// <summary>
    /// Function entry is for a function declaration, either named or anonymous.
    /// </summary>
    FunctionDeclaration,

    /// <summary>
    /// Function entry is for a struct instantiation.
    /// </summary>
    StructInstantiation
}
