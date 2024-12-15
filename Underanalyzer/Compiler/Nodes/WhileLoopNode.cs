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
/// Represents a "while" loop in the AST.
/// </summary>
internal sealed class WhileLoopNode : IASTNode
{
    /// <summary>
    /// Condition of the while loop node.
    /// </summary>
    public IASTNode Condition { get; }

    /// <summary>
    /// Body of the while loop node.
    /// </summary>
    public IASTNode Body { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private WhileLoopNode(TokenKeyword token, IASTNode condition, IASTNode body)
    {
        NearbyToken = token;
        Condition = condition;
        Body = body;
    }

    /// <summary>
    /// Creates a while loop node, parsing from the given context's current position.
    /// </summary>
    public static WhileLoopNode? Parse(ParseContext context)
    {
        // Parse "while" keyword
        if (context.EnsureToken(KeywordKind.While) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse loop condition
        if (Expressions.ParseExpression(context) is not IASTNode condition)
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
        return new WhileLoopNode(tokenKeyword, condition, body);
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
