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
/// Represents a nullish coalesce node in the AST.
/// </summary>
internal sealed class NullishCoalesceNode : IASTNode
{
    /// <summary>
    /// Left side of the nullish coalesce node.
    /// </summary>
    public IASTNode Left { get; private set; }

    /// <summary>
    /// Right side of the nullish coalesce node.
    /// </summary>
    public IASTNode Right { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a nullish coalesce node, given the provided token and expressions for the left and right sides.
    /// </summary>
    public NullishCoalesceNode(TokenOperator token, IASTNode left, IASTNode right)
    {
        Left = left;
        Right = right;
        NearbyToken = token;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Left = Left.PostProcess(context);
        Right = Right.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
