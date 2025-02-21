/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using Underanalyzer.Compiler;
using Underanalyzer.Compiler.Bytecode;
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
    public IGMInstruction CreateInstruction(int address, ExtendedOpcode extendedOpcode)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.Extended,
            ExtKind = extendedOpcode,
            Type1 = DataType.Int16
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
    public IGMInstruction CreateDuplicateInstruction(int address, DataType dataType, byte duplicationSize)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.Duplicate,
            Type1 = dataType,
            DuplicationSize = duplicationSize
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreateDupSwapInstruction(int address, DataType dataType, byte duplicationSize, byte duplicationSize2)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.Duplicate,
            Type1 = dataType,
            DuplicationSize = duplicationSize,
            DuplicationSize2 = duplicationSize2
        };
    }

    /// <inheritdoc/>
    public IGMInstruction CreatePopSwapInstruction(int address, byte swapSize)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.Pop,
            Type1 = DataType.Int16,
            Type2 = DataType.Variable,
            PopSwapSize = swapSize
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
    public IGMInstruction CreateCallVariableInstruction(int address, int argumentCount)
    {
        return new GMInstruction()
        {
            Address = address,
            Kind = Opcode.CallVariable,
            Type1 = DataType.Variable,
            ArgumentCount = argumentCount
        };
    }

    /// <inheritdoc/>
    public void PatchInstruction(IGMInstruction instruction, string variableName, InstanceType variableInstanceType, InstanceType instructionInstanceType, VariableType variableType, bool isBuiltin)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            // Transform instance type into Self in GMLv2 when not using simple variables
            if (gameContext.UsingGMLv2 && variableType != VariableType.Normal)
            {
                variableInstanceType = InstanceType.Self;
                instructionInstanceType = InstanceType.Self;
            }

            // If the variable is builtin, use builtin instance type
            if (isBuiltin)
            {
                variableInstanceType = InstanceType.Builtin;
                instructionInstanceType = InstanceType.Builtin;
            }

            if (gameContext.MockVariables.TryGetValue((variableName, variableInstanceType), out GMVariable? existingVariable))
            {
                mockInstruction.Variable = existingVariable;
            }
            else
            {
                GMVariable newVariable = new(new GMString(variableName))
                {
                    InstanceType = variableInstanceType
                };
                mockInstruction.Variable = newVariable;
                gameContext.MockVariables.Add((variableName, variableInstanceType), newVariable);
            }

            mockInstruction.InstType = instructionInstanceType;
            mockInstruction.ReferenceVarType = variableType;
        }
    }

    /// <inheritdoc/>
    public void PatchInstruction(IGMInstruction instruction, FunctionScope scope, string functionName, IBuiltinFunction? builtinFunction)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            if (scope.TryGetDeclaredFunction(functionName, out FunctionEntry? entry))
            {
                mockInstruction.Function = entry.Function ?? throw new InvalidOperationException("Function not resolved for function entry");
            }    
            else if (gameContext.Builtins.LookupBuiltinFunction(functionName) is not null)
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
    public void PatchInstruction(IGMInstruction instruction, FunctionEntry functionEntry)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            if (mockInstruction is { Kind: Opcode.Extended, ExtKind: ExtendedOpcode.PushReference })
            {
                if (gameContext.GetScriptId(
                    functionEntry.Function?.Name.Content ?? 
                        throw new InvalidOperationException("Function not resolved for function entry"), 
                    out int assetIndex))
                {
                    mockInstruction.AssetReferenceId = assetIndex & 0xFFFFFF;
                    mockInstruction.AssetReferenceType = (AssetType)(assetIndex >> 24);
                }
                else
                {
                    throw new Exception($"Failed to look up script asset for function \"{functionEntry.Function?.Name.Content}\"");
                }
            }
            else
            {
                mockInstruction.Function = functionEntry.Function ?? throw new InvalidOperationException("Function not resolved for function entry");
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
