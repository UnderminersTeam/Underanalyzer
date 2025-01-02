/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a "throw" statement in the AST.
/// </summary>
internal sealed class ThrowNode : IASTNode
{
    /// <summary>
    /// The expression being thrown.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; private set; }

    private ThrowNode(TokenKeyword nearbyToken, IASTNode expression)
    {
        Expression = expression;
        NearbyToken = nearbyToken;
    }

    /// <summary>
    /// Creates a throw statement node, parsing from the given context's current position.
    /// </summary>
    public static ThrowNode? Parse(ParseContext context)
    {
        // Parse "throw" keyword
        if (context.EnsureToken(KeywordKind.Throw) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse expression being thrown
        if (Expressions.ParseExpression(context) is not IASTNode expression)
        {
            return null;
        }

        // Create final statement
        return new ThrowNode(tokenKeyword, expression);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Expression = Expression.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        throw new System.NotImplementedException();
    }
}