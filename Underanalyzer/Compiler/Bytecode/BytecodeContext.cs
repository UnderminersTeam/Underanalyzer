/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using Underanalyzer.Compiler.Nodes;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// A context for bytecode generation, for a single code entry.
/// </summary>
internal sealed class BytecodeContext : ISubCompileContext
{
    /// <inheritdoc/>
    public CompileContext CompileContext { get; }

    /// <inheritdoc/>
    public FunctionScope CurrentScope { get; set; }

    /// <summary>
    /// Root node used during bytecode generation.
    /// </summary>
    public IASTNode RootNode { get; }

    /// <summary>
    /// Set of global functions defined locally in the code entry being compiled.
    /// </summary>
    public HashSet<string>? LocalGlobalFunctions { get; }

    /// <summary>
    /// Current list of written instructions, in order by address.
    /// </summary>
    public List<IGMInstruction> Instructions { get; } = new(64);

    /// <summary>
    /// Current list of function entries, in order by address.
    /// </summary>
    public List<FunctionEntry> FunctionEntries { get; } = new(4);

    /// <summary>
    /// List of instruction patches generated during code generation.
    /// </summary>
    public InstructionPatches Patches { get; } = InstructionPatches.Create();

    /// <summary>
    /// Current instruction writing position.
    /// </summary>
    public int Position { get; private set; } = 0;

    // Stack used for storing data types as on the VM data stack.
    private readonly Stack<DataType> _dataTypeStack = new(16);

    // Code builder used for creating instructions, modifying them, and creating code entries.
    private readonly ICodeBuilder _codeBuilder;

    // Reference to the game context for quick access.
    private readonly IGameContext _gameContext;

    public BytecodeContext(CompileContext context, IASTNode rootNode, FunctionScope rootScope, HashSet<string>? localGlobalFunctions)
    {
        CompileContext = context;
        RootNode = rootNode;
        CurrentScope = rootScope;
        LocalGlobalFunctions = localGlobalFunctions;

        rootScope.ControlFlowContexts = new(8);
        _gameContext = context.GameContext;
        _codeBuilder = _gameContext.CodeBuilder;
    }

    /// <summary>
    /// Performs bytecode generation for a full code entry, from the root.
    /// </summary>
    public void GenerateCode()
    {
        RootNode.GenerateCode(this);

#if DEBUG
        if (_dataTypeStack.Count > 0)
        {
            throw new Exception("Data type stack not cleared by end of code generation");
        }
#endif
    }

    /// <summary>
    /// Performs post-processing on generated code, resolving references and patches.
    /// </summary>
    public static void PatchInstructions(CompileContext context, InstructionPatches patches)
    {
        ICodeBuilder codeBuilder = context.GameContext.CodeBuilder;

        // Resolve variable patches
        foreach (VariablePatch variablePatch in patches.VariablePatches!)
        {
            codeBuilder.PatchInstruction(variablePatch.Instruction!, variablePatch.Name, variablePatch.InstanceType, variablePatch.VariableType, variablePatch.IsBuiltin);
        }

        // Resolve function patches
        foreach (FunctionPatch functionPatch in patches.FunctionPatches!)
        {
            codeBuilder.PatchInstruction(functionPatch.Instruction!, functionPatch.Name, functionPatch.BuiltinFunction);
        }

        // Resolve string patches
        foreach (StringPatch stringPatch in patches.StringPatches!)
        {
            codeBuilder.PatchInstruction(stringPatch.Instruction!, stringPatch.Content);
        }
    }

    /// <summary>
    /// Emits an instruction with the given opcode, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits a single-type instruction with the given opcode and data type, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, DataType dataType)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, dataType);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits a double-type instruction with the given opcode and data types, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, DataType dataType1, DataType dataType2)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 16-bit integer, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, short value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 32-bit integer, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, int value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 8;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 64-bit integer, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, long value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 12;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and 64-bit floating point number, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, double value, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, value, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 12;
        return instr;
    }

    /// <summary>
    /// Emits a double-type instruction with the given opcode, comparison type, and data types, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, ComparisonType comparisonType, DataType dataType1, DataType dataType2)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, comparisonType, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given extended opcode, at the current position.
    /// </summary>
    public IGMInstruction Emit(ExtendedOpcode extendedOpcode)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, extendedOpcode);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given extended opcode and integer value, at the current position.
    /// </summary>
    public IGMInstruction Emit(ExtendedOpcode extendedOpcode, int extendedValue)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, extendedOpcode, extendedValue);
        Instructions.Add(instr);
        Position += 8;
        return instr;
    }

    /// <summary>
    /// Emits a dulication instruction with the given data type and duplication size, at the current position.
    /// </summary>
    public IGMInstruction EmitDuplicate(DataType dataType, byte duplicationSize)
    {
        IGMInstruction instr = _codeBuilder.CreateDuplicateInstruction(Position, dataType, duplicationSize);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits a dulication instruction with the given data type and duplication sizes, at the current position.
    /// </summary>
    public IGMInstruction EmitDupSwap(DataType dataType, byte duplicationSize, byte duplicationSize2)
    {
        IGMInstruction instr = _codeBuilder.CreateDupSwapInstruction(Position, dataType, duplicationSize, duplicationSize2);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits a pop swap instruction with the given swap size, at the current position.
    /// </summary>
    public IGMInstruction EmitPopSwap(byte swapSize)
    {
        IGMInstruction instr = _codeBuilder.CreatePopSwapInstruction(Position, swapSize);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with opcode <see cref="Opcode.PopWithContext"/>, and <see cref="IGMInstruction.PopWithContextExit"/> as <see langword="true"/>.
    /// </summary>
    public IGMInstruction EmitPopWithExit()
    {
        IGMInstruction instr = _codeBuilder.CreateWithExitInstruction(Position);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and given variable, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, VariablePatch variable, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 8;

        variable.Instruction = instr;
        Patches.VariablePatches!.Add(variable);

        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and given function, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, FunctionPatch function, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 8;

        function.Instruction = instr;
        Patches.FunctionPatches!.Add(function);

        return instr;
    }

    /// <summary>
    /// Emits a <see cref="Opcode.Call"/> instruction with the given argument count, and given function, at the current position.
    /// </summary>
    public IGMInstruction EmitCall(FunctionPatch function, int argumentCount)
    {
        IGMInstruction instr = _codeBuilder.CreateCallInstruction(Position, argumentCount);
        Instructions.Add(instr);
        Position += 8;

        function.Instruction = instr;
        Patches.FunctionPatches!.Add(function);

        return instr;
    }

    /// <summary>
    /// Emits a <see cref="Opcode.CallVariable"/> instruction with the given argument count, at the current position.
    /// </summary>
    public IGMInstruction EmitCallVariable(int argumentCount)
    {
        IGMInstruction instr = _codeBuilder.CreateCallVariableInstruction(Position, argumentCount);
        Instructions.Add(instr);
        Position += 4;
        return instr;
    }

    /// <summary>
    /// Emits an instruction with the given opcode, data types, and given string, at the current position.
    /// </summary>
    public IGMInstruction Emit(Opcode opcode, StringPatch stringPatch, DataType dataType1, DataType dataType2 = DataType.Double)
    {
        IGMInstruction instr = _codeBuilder.CreateInstruction(Position, opcode, dataType1, dataType2);
        Instructions.Add(instr);
        Position += 8;

        stringPatch.Instruction = instr;
        Patches.StringPatches!.Add(stringPatch);

        return instr;
    }

    /// <summary>
    /// Patches a single instruction with the given branch offset.
    /// </summary>
    public void PatchBranch(IGMInstruction instruction, int branchOffset)
    {
        _codeBuilder.PatchInstruction(instruction, branchOffset);
    }

    /// <summary>
    /// Pushes a data type to the data type stack.
    /// </summary>
    public void PushDataType(DataType dataType)
    {
        _dataTypeStack.Push(dataType);
    }

    /// <summary>
    /// Peeks the top data type from the data type stack.
    /// </summary>
    public DataType PeekDataType()
    {
        return _dataTypeStack.Peek();
    }

    /// <summary>
    /// Pops a data type from the data type stack.
    /// </summary>
    public DataType PopDataType()
    {
        return _dataTypeStack.Pop();
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
    /// Possible instance conversion types. See <see cref="ConvertToInstanceId"/>.
    /// </summary>
    public enum InstanceConversionType
    {
        /// <summary>
        /// No conversion performed.
        /// </summary>
        None,

        /// <summary>
        /// Conversion directly to int32 performed.
        /// </summary>
        Int32,

        /// <summary>
        /// Magic stacktop ID used (<see cref="InstanceType.StackTop"/>).
        /// </summary>
        StacktopId
    }

    /// <summary>
    /// Converts the data type on the top of the stack to an instance ID, depending on GameMaker version.
    /// </summary>
    /// <remarks>
    /// Pops the top data type from the stack.
    /// </remarks>
    /// <returns>
    /// <see cref="InstanceConversionType"/> enumeration representing what conversion was performed.
    /// </returns>
    public InstanceConversionType ConvertToInstanceId()
    {
        // If data type isn't an integer, convert to one
        DataType dataType = PopDataType();
        if (dataType != DataType.Int32)
        {
            if (dataType == DataType.Variable && CompileContext.GameContext.UsingGMLv2)
            {
                // In GMLv2, use magic stacktop integer to reference variable types
                Emit(Opcode.PushImmediate, (short)InstanceType.StackTop, DataType.Int16);
                return InstanceConversionType.StacktopId;
            }

            // Otherwise, if either not GMLv2, or type is not a variable type, perform direct conversion
            Emit(Opcode.Convert, dataType, DataType.Int32);
            return InstanceConversionType.Int32;
        }

        // No conversion was performed
        return InstanceConversionType.None;
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

    /// <summary>
    /// Returns whether the given name is a global function name of any kind.
    /// </summary>
    public bool IsGlobalFunctionName(string name)
    {
        // Check if it's a locally-declared global function
        if (LocalGlobalFunctions?.Contains(name) ?? false)
        {
            return true;
        }

        // Check builtin functions
        if (_gameContext.Builtins.LookupBuiltinFunction(name) is not null)
        {
            return true;
        }

        // Check script assets
        if (_gameContext.GetScriptId(name, out int _))
        {
            return true;
        }

        // Do a general global function lookup (depending on ICodeBuilder's implementation)
        return _codeBuilder.IsGlobalFunctionName(name);
    }

    /// <summary>
    /// Pushes a control flow context onto the control flow context stack.
    /// </summary>
    public void PushControlFlowContext(IControlFlowContext context)
    {
        CurrentScope.ControlFlowContexts!.Push(context);
    }

    /// <summary>
    /// Pops a control flow context from the control flow context stack.
    /// </summary>
    public void PopControlFlowContext()
    {
        CurrentScope.ControlFlowContexts!.Pop();
    }

    /// <summary>
    /// Returns whether there are any control flow contexts on the control flow context stack.
    /// </summary>
    public bool AnyControlFlowContexts()
    {
        return CurrentScope.ControlFlowContexts!.Count > 0;
    }

    /// <summary>
    /// Returns whether there are any loop contexts on the control flow context stack.
    /// </summary>
    public bool AnyLoopContexts()
    {
        foreach (IControlFlowContext context in CurrentScope.ControlFlowContexts!)
        {
            if (context.IsLoop)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the top control flow context from the control flow context stack.
    /// </summary>
    public IControlFlowContext GetTopControlFlowContext()
    {
        return CurrentScope.ControlFlowContexts!.Peek();
    }
}
