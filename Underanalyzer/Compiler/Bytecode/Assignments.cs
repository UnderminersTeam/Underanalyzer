/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using Underanalyzer.Compiler.Nodes;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Bytecode;

/// <summary>
/// Helper for generating code for assigning values to variables.
/// </summary>
internal static class Assignments
{
    /// <summary>
    /// Generates code for a simple assignment operation, given the destination being assigned to.
    /// </summary>
    public static void GenerateAssignCode(BytecodeContext context, IAssignableASTNode destination)
    {
        // Get data type that's being assigned
        DataType expressionType = context.PopDataType();

        // Emit different code depending on type of assignment being performed
        switch (destination)
        {
            case SimpleVariableNode simpleVarNode:
                VariablePatch varPatch = new(simpleVarNode.VariableName, simpleVarNode.ExplicitInstanceType, 
                                             VariableType.Normal, simpleVarNode.BuiltinVariable is not null);
                context.Emit(Opcode.Pop, varPatch, DataType.Variable, expressionType);
                break;
            case DotVariableNode dotVarNode:
                // TODO
                throw new NotImplementedException();
            case AccessorNode accessorNode:
                // TODO
                throw new NotImplementedException();
        }
    }
}
