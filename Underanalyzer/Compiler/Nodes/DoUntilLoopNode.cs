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
/// Represents a "do...until" loop in the AST.
/// </summary>
internal sealed class DoUntilLoopNode : IASTNode
{
    /// <summary>
    /// Body of the do...until loop node.
    /// </summary>
    public IASTNode Body { get; }

    /// <summary>
    /// Condition of the do...until loop node.
    /// </summary>
    public IASTNode Condition { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private DoUntilLoopNode(TokenKeyword token, IASTNode body, IASTNode condition)
    {
        NearbyToken = token;
        Body = body;
        Condition = condition;
    }

    /// <summary>
    /// Creates a do...until loop node, parsing from the given context's current position.
    /// </summary>
    public static DoUntilLoopNode? Parse(ParseContext context)
    {
        // Parse "do" keyword
        if (context.EnsureToken(KeywordKind.Do) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse loop body
        if (Statements.ParseStatement(context) is not IASTNode body)
        {
            return null;
        }

        // Parse "until" keyword
        context.SkipSemicolons();
        if (context.EnsureToken(KeywordKind.Until) is not TokenKeyword tokenKeyword2)
        {
            return null;
        }

        // Parse loop condition
        if (Expressions.ParseExpression(context) is not IASTNode condition)
        {
            return null;
        }

        // Create final statement
        return new DoUntilLoopNode(tokenKeyword, body, condition);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
