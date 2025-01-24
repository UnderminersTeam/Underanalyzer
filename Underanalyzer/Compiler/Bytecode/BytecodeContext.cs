/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Nodes;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// A context for bytecode generation, for a single code entry.
/// </summary>
internal sealed class BytecodeContext
{
    /// <summary>
    /// The compile context for the overarching code entry.
    /// </summary>
    public CompileContext CompileContext { get; }

    /// <summary>
    /// Root node used during bytecode generation.
    /// </summary>
    public IASTNode RootNode { get; }

    /// <summary>
    /// Current function scope being used during bytecode generation.
    /// </summary>
    public FunctionScope CurrentScope { get; set; }

    /// <summary>
    /// Current list of written instructions, in order by address.
    /// </summary>
    public List<IGMInstruction> Instructions { get; } = new(64);

    /// <summary>
    /// Current list of function entries, in order by address.
    /// </summary>
    public List<FunctionEntry> FunctionEntries { get; } = new(4);

    /// <summary>
    /// List of variable patches generated during code generation.
    /// </summary>
    internal List<VariablePatch> VariablePatches { get; } = new(32);

    /// <summary>
    /// List of string patches generated during code generation.
    /// </summary>
    internal List<StringPatch> StringPatches { get; } = new(16);

    // Current instruction writing position.
    private int _position = 0;

    // Stack used for storing data types as on the VM data stack.
    private readonly Stack<DataType> _dataTypeStack = new(16);

    // Code builder used for creating instructions, modifying them, and creating code entries.
    private readonly ICodeBuilder _codeBuilder;

    public BytecodeContext(CompileContext context, IASTNode rootNode, FunctionScope rootScope)
    {
        CompileContext = context;
        RootNode = rootNode;
        CurrentScope = rootScope;

        rootScope.ControlFlowContexts = new(8);
        _codeBuilder = context.GameContext.CodeBuilder;
    }

    /// <summary>
    /// Performs bytecode generation for a full code entry, from the root.
    /// </summary>
    public void GenerateCode()
    {
        RootNode.GenerateCode(this);
    }

    /// <summary>
    /// Performs post-processing on generated code, resolving references and patches.
    /// </summary>
    public static void PatchInstructions(CompileContext context, List<VariablePatch> variablePatches, List<StringPatch> stringPatches)
    {
        ICodeBuilder codeBuilder = context.GameContext.CodeBuilder;

        // Resolve variable patches
        foreach (VariablePatch variablePatch in variablePatches)
        {
            codeBuilder.PatchInstruction(variablePatch.Instruction!, variablePatch.Name, variablePatch.InstanceType, variablePatch.VariableType, variablePatch.IsBuiltin);
        }

        // Resolve string patches
        foreach (StringPatch stringPatch in stringPatches)
        {
            codeBuilder.PatchInstruction(stringPatch.Instruction!, stringPatch.Content);
        }
    }

    /// <summary>
    /// Emits an instruction with the given opcode, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode);
        Instructions.Add(instr);
        _position += 4;
        return instr;
    }

    /// <summary>
    /// Emits a single-type instruction with the given opcode and data type, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, DataType dataType)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, dataType);
        Instructions.Add(instr);
        _position += 4;
        return instr;
    }

    /// <summary>
    /// Emits a double-type instruction with the given opcode and data types, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, DataType dataType1, DataType dataType2)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, dataType1, dataType2);
        Instructions.Add(instr);
        _position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 16-bit integer, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, short value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        _position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 32-bit integer, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, int value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        _position += 8;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 64-bit integer, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, long value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        _position += 12;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 64-bit floating point number, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, double value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        _position += 12;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and given variable, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, VariablePatch variable, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, dataType1, dataType2);
        Instructions.Add(instr);
        _position += 8;

        variable.Instruction = instr;
        VariablePatches.Add(variable);

        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and given string, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, StringPatch stringPatch, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(_position, opcode, dataType1, dataType2);
        Instructions.Add(instr);
        _position += 8;

        stringPatch.Instruction = instr;
        StringPatches.Add(stringPatch);

        return instr;
    }

    /// <summary>
    /// Pushes a data type to the data type stack.
    /// </summary>
    public void PushDataType(DataType dataType)
    {
        _dataTypeStack.Push(dataType);
    }

    /// <summary>
    /// Emits a <see cref="Opcode.Convert"/> instruction from the current type at the 
    /// top of the data type stack, to the destination data type.
    /// </summary>
    /// <remarks>Pops the data type at the top of the stack, and does not push anything back.</remarks>
    /// <returns><see langword="true"/> if a conversion was emitted; <see langword="false"/> otherwise.</returns>
    public bool ConvertDataType(DataType destDataType)
    {
        // Pop data type from top of stack
        DataType srcDataType = _dataTypeStack.Pop();

        // Emit convert instruction if data type is different; otherwise do nothing
        if (srcDataType != destDataType)
        {
            Emit(Opcode.Convert, srcDataType, destDataType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns whether any control flow contexts require cleanup currently, for early exits.
    /// </summary>
    public bool DoAnyControlFlowRequireCleanup()
    {
        foreach (IControlFlowContext context in CurrentScope.ControlFlowContexts!)
        {
            if (context.RequiresCleanup)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Generates control flow cleanup code.
    /// </summary>
    public void GenerateControlFlowCleanup()
    {
        foreach (IControlFlowContext context in CurrentScope.ControlFlowContexts!)
        {
            if (context.RequiresCleanup)
            {
                context.GenerateCleanupCode(this);
            }
        }
    }
}
