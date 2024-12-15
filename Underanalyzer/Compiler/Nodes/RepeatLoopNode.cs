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
/// Represents a "repeat" loop in the AST.
/// </summary>
internal sealed class RepeatLoopNode : IASTNode
{
    /// <summary>
    /// Expression used for the number of times this repeat loop node repeats.
    /// </summary>
    public IASTNode TimesToRepeat { get; }

    /// <summary>
    /// Body of the repeat loop node.
    /// </summary>
    public IASTNode Body { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private RepeatLoopNode(TokenKeyword token, IASTNode timesToRepeat, IASTNode body)
    {
        NearbyToken = token;
        TimesToRepeat = timesToRepeat;
        Body = body;
    }

    /// <summary>
    /// Creates a repeat loop node, parsing from the given context's current position.
    /// </summary>
    public static RepeatLoopNode? Parse(ParseContext context)
    {
        // Parse "repeat" keyword
        if (context.EnsureToken(KeywordKind.Repeat) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse loop repeat count expression
        if (Expressions.ParseExpression(context) is not IASTNode timesToRepeat)
        {
            return null;
        }

        // Parse loop body
        if (Statements.ParseStatement(context) is not IASTNode body)
        {
            return null;
        }

        // Create final statement
        return new RepeatLoopNode(tokenKeyword, timesToRepeat, body);
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
