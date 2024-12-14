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
/// Represents a prefix (++/-- on left side) expression or statement in the AST.
/// </summary>
internal sealed class PrefixNode : IMaybeStatementASTNode
{
    /// <summary>
    /// Expression being pre-incremented/pre-decremented.
    /// </summary>
    public IASTNode? Expression { get; private set; }

    /// <summary>
    /// Whether this prefix is an increment (++) or a decrement (--).
    /// </summary>
    public bool IsIncrement { get; }

    /// <inheritdoc/>
    public bool IsStatement { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a prefix node, parsing from the given context's current position,
    /// and given whether or not the prefix is an increment.
    /// </summary>
    public PrefixNode(ParseContext context, TokenOperator token, bool isIncrement)
    {
        NearbyToken = token;
        Expression = Expressions.ParseChainExpression(context);
        IsIncrement = isIncrement;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Expression = Expression?.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
