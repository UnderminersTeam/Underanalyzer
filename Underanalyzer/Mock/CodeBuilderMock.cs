/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Mock;

/// <summary>
/// A default implementation of <see cref="ICodeBuilder"/>.
/// </summary>
public class CodeBuilderMock : ICodeBuilder
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
    public void PatchInstruction(IGMInstruction instruction, string variableName, InstanceType instanceType, VariableType variableType, bool isBuiltin)
    {
        if (instruction is GMInstruction mockInstruction)
        {
            mockInstruction.Variable = new GMVariable(new GMString(variableName));
            mockInstruction.InstType = instanceType;
            mockInstruction.ReferenceVarType = variableType;
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
}
