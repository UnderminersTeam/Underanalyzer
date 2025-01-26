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
/// Represents a "continue" statement in the AST.
/// </summary>
internal sealed class ContinueNode(TokenKeyword token) : IASTNode
{
    /// <inheritdoc/>
    public IToken? NearbyToken { get; } = token;

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Need at least one loop outside of this statement
        if (!context.AnyLoopContexts())
        {
            context.CompileContext.PushError($"Continue used outside of any loop", NearbyToken);
            return;
        }

        // Use control flow context's continue branch
        context.GetTopControlFlowContext().UseContinue(context, context.Emit(Opcode.Branch));
    }
}
