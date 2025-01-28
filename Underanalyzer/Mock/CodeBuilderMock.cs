﻿/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using Underanalyzer.Compiler;
using Underanalyzer.Decompiler;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Mock;

/// <summary>
/// A default implementation of <see cref="ICodeBuilder"/>.
/// </summary>
public class CodeBuilderMock(GameContextMock gameContext) : ICodeBuilder
{

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, DataType dataType)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode,
            Type1 = dataType
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, DataType dataType1, DataType dataType2)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode,
            Type1 = dataType1,
            Type2 = dataType2
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, short value, DataType dataType1, DataType dataType2)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode,
            ValueShort = value,
            Type1 = dataType1,
            Type2 = dataType2
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, int value, DataType dataType1, DataType dataType2)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode,
            ValueInt = value,
            Type1 = dataType1,
            Type2 = dataType2
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, long value, DataType dataType1, DataType dataType2)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode,
            ValueLong = value,
            Type1 = dataType1,
            Type2 = dataType2
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, double value, DataType dataType1, DataType dataType2)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode,
            ValueDouble = value,
            Type1 = dataType1,
            Type2 = dataType2
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, Opcode opcode, ComparisonType comparisonType, DataType dataType1, DataType dataType2)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = opcode,
            ComparisonKind = comparisonType,
            Type1 = dataType1,
            Type2 = dataType2
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateInstruction(int address, ExtendedOpcode extendedOpcode, int value)
    {
        if (extendedOpcode == ExtendedOpcode.PushReference)
        {
            return new GMInstruction()
            {
                Address = address,
                Kind = Opcode.Extended,
                ExtKind = extendedOpcode,
                Type1 = DataType.Int32,
                AssetReferenceId = value & 0xFFFFFF,
                AssetReferenceType = (AssetType)(value >> 24)
            };
        }
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.Extended,
            ExtKind = extendedOpcode,
            Type1 = DataType.Int32,
            ValueInt = value
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateWithExitInstruction(int address)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.PopWithContext,
            PopWithContextExit = true
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateCallInstruction(int address, int argumentCount)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.Call,
            Type1 = DataType.Int32,
            ArgumentCount = argumentCount
        };
    }

    /// <inheritdoc/>
    public void PatchInstruction(IGMInstruction instruction, string variableName, InstanceType instanceType, VariableType variableType, bool isBuiltin)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            mockInstruction.Variable = new GMVariable(new GMString(variableName))
            {
                InstanceType = isBuiltin ? InstanceType.Builtin : instanceType,
            };
            mockInstruction.InstType = instanceType;
            mockInstruction.ReferenceVarType = variableType;
        }
    }

    /// <inheritdoc/>
    public void PatchInstruction(IGMInstruction instruction, string functionName, IBuiltinFunction? builtinFunction)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            if (gameContext.Builtins.LookupBuiltinFunction(functionName) is not null)
            {
                mockInstruction.Function = new GMFunction(functionName);
            }
            else if (gameContext.GlobalFunctions.TryGetFunction(functionName, out IGMFunction? function))
            {
                mockInstruction.Function = function;
            }
            else
            {
                throw new Exception($"Failed to look up function \"{functionName}\"");
            }
        }
    }

    /// <inheritdoc/>
    public void PatchInstruction(IGMInstruction instruction, string stringContent)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            mockInstruction.ValueString = new GMString(stringContent);
        }
    }

    /// <inheritdoc/>
    public void PatchInstruction(IGMInstruction instruction, int branchOffset)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            mockInstruction.BranchOffset = branchOffset;
        }
    }

    /// <inheritdoc/>
    public bool IsGlobalFunctionName(string name)
    {
        return gameContext.GlobalFunctions.FunctionNameExists(name);
    }
}
