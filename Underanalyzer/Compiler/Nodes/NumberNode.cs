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
/// Represents a constant 64-bit floating point (or integer) number in the AST.
/// </summary>
internal sealed class NumberNode : IConstantASTNode
{
    /// <summary>
    /// Number being used as a value for this node.
    /// </summary>
    public double Value { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    public NumberNode(TokenNumber token)
    {
        Value = token.Value;
        NearbyToken = token;
    }

    public NumberNode(double value, IToken? nearbyToken)
    {
        Value = value;
        NearbyToken = nearbyToken;
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
