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
/// Represents a conditional (ternary) node in the AST.
/// </summary>
internal sealed class ConditionalNode : IASTNode
{
    /// <summary>
    /// Condition of the conditional node.
    /// </summary>
    public IASTNode Condition { get; }

    /// <summary>
    /// True expression of the conditional node.
    /// </summary>
    public IASTNode TrueExpression { get; }

    /// <summary>
    /// False expression of the conditional node.
    /// </summary>
    public IASTNode FalseExpression { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a conditional node, given the provided token and expressions for condition, true, and false.
    /// </summary>
    public ConditionalNode(TokenOperator token, IASTNode condition, IASTNode trueExpression, IASTNode falseExpression)
    {
        Condition = condition;
        TrueExpression = trueExpression;
        FalseExpression = falseExpression;
        NearbyToken = token;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        // TODO
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
