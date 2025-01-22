/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a "return" statement in the AST.
/// </summary>
internal sealed class ReturnNode(TokenKeyword token, IASTNode returnValue) : IASTNode
{
    /// <summary>
    /// Expression being used as a return value for this node.
    /// </summary>
    public IASTNode ReturnValue { get; private set; } = returnValue;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; } = token;

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        ReturnValue = ReturnValue.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Generate return value, and convert to Variable data type
        ReturnValue.GenerateCode(context);
        context.ConvertDataType(DataType.Variable);

        // If necessary, perform data stack cleanup
        if (context.DoAnyControlFlowRequireCleanup())
        {
            // Store return value into temporary local variable, perform stack cleanup, and then re-push return value using local
            VariablePatch tempVariable = new(VMConstants.TempReturnVariable, InstanceType.Local);
            context.Emit(Opcode.Pop, tempVariable, DataType.Variable);
            context.GenerateControlFlowCleanup();
            context.Emit(Opcode.PushLocal, tempVariable, DataType.Variable);
        }

        // Emit actual return
        context.Emit(Opcode.Return, DataType.Variable);
    }
}
