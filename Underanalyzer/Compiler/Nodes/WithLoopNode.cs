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
/// Represents a "with" loop in the AST.
/// </summary>
internal sealed class WithLoopNode : IASTNode
{
    /// <summary>
    /// Expression used as an iterator for this with loop node.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Body of the with loop node.
    /// </summary>
    public IASTNode Body { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private WithLoopNode(TokenKeyword token, IASTNode expression, IASTNode body)
    {
        NearbyToken = token;
        Expression = expression;
        Body = body;
    }

    /// <summary>
    /// Creates a with loop node, parsing from the given context's current position.
    /// </summary>
    public static WithLoopNode? Parse(ParseContext context)
    {
        // Parse "with" keyword
        if (context.EnsureToken(KeywordKind.With) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse loop condition
        if (Expressions.ParseExpression(context) is not IASTNode expression)
        {
            return null;
        }

        // Skip "do" keyword, if present
        if (context.IsCurrentToken(KeywordKind.Do))
        {
            context.Position++;
        }

        // Parse loop body
        if (Statements.ParseStatement(context) is not IASTNode body)
        {
            return null;
        }

        // Create final statement
        return new WithLoopNode(tokenKeyword, expression, body);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Expression = Expression.PostProcess(context);
        Body = Body.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
