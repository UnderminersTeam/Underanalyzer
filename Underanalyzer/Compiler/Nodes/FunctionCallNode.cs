/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a function call in the AST, not tied to any specific variable name.
/// </summary>
internal sealed class FunctionCallNode : IMaybeStatementASTNode
{
    /// <summary>
    /// Expression being called.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Arguments being used for this function call, in order.
    /// </summary>
    public List<IASTNode> Arguments { get; }

    /// <inheritdoc/>
    public bool IsStatement { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a function call node, parsing from the given context's current position,
    /// and given the provided expression being called.
    /// </summary>
    public FunctionCallNode(ParseContext context, TokenSeparator token, IASTNode expression)
    {
        NearbyToken = token;
        Expression = expression;
        Arguments = Functions.ParseCallArguments(context, 2047 /* TODO: change based on gamemaker version? */);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Expression = Expression.PostProcess(context);
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].PostProcess(context);
        }
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
