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
/// Represents a unary (!, ~, +, - on left side) expression in the AST.
/// </summary>
internal sealed class UnaryNode : IASTNode
{
    /// <summary>
    /// Expression being pre-incremented/pre-decremented.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Whether this prefix is an increment (++) or a decrement (--).
    /// </summary>
    public UnaryKind Kind { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Kind of unary operation being performed.
    /// </summary>
    public enum UnaryKind
    {
        BooleanNot,     // !
        BitwiseNegate,  // ~
        Positive,       // +
        Negative        // -
    }

    private UnaryNode(IToken token, UnaryKind kind, IASTNode expression)
    {
        NearbyToken = token;
        Kind = kind;
        Expression = expression;
    }

    /// <summary>
    /// Creates a prefix node, parsing from the given context's current position,
    /// and given whether or not the prefix is an increment.
    /// </summary>
    public static UnaryNode? Parse(ParseContext context, IToken token, UnaryKind kind)
    {
        // Parse expression after token
        if (Expressions.ParseChainExpression(context) is not IASTNode expression)
        {
            return null;
        }

        // Create final node
        return new UnaryNode(token, kind, expression);
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
