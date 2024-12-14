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
/// Represents a post (++/-- on right side) expression in the AST.
/// </summary>
internal sealed class PostfixNode : IMaybeStatementASTNode
{
    /// <summary>
    /// Expression being post-incremented/post-decremented.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Whether this postfix is an increment (++) or a decrement (--).
    /// </summary>
    public bool IsIncrement { get; }

    /// <inheritdoc/>
    public bool IsStatement { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a postfix node, given whether or not the postfix is an increment.
    /// </summary>
    public PostfixNode(TokenOperator token, IASTNode expression, bool isIncrement)
    {
        NearbyToken = token;
        Expression = expression;
        IsIncrement = isIncrement;
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
        // TODO
    }
}
