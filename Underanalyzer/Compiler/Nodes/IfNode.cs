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
/// Represents an "if" statement in the AST.
/// </summary>
internal sealed class IfNode : IASTNode
{
    /// <summary>
    /// Condition of the if statement node.
    /// </summary>
    public IASTNode? Condition { get; }

    /// <summary>
    /// True statement/block of the if statement node.
    /// </summary>
    public IASTNode? TrueStatement { get; }

    /// <summary>
    /// False/else statement/block of the if statement node.
    /// </summary>
    public IASTNode? FalseStatement { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates an if statement node, parsing from the given context's current position.
    /// </summary>
    public IfNode(ParseContext context)
    {
        NearbyToken = context.EnsureToken(KeywordKind.If);
        Condition = Expressions.ParseExpression(context);
        if (context.IsCurrentToken(KeywordKind.Then))
        {
            context.Position++;
        }
        TrueStatement = Statements.ParseStatement(context);
        context.SkipSemicolons();
        if (context.IsCurrentToken(KeywordKind.Else))
        {
            context.Position++;
            FalseStatement = Statements.ParseStatement(context);
        }
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
