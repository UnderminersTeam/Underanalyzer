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
/// Represents a dot (.) variable reference in the AST, as part of a chain reference.
/// </summary>
internal sealed class DotVariableNode : IAssignableASTNode, IVariableASTNode
{
    /// <summary>
    /// Expression on the left side of the dot.
    /// </summary>
    public IASTNode LeftExpression { get; }

    /// <inheritdoc/>
    public string VariableName { get; }

    /// <inheritdoc/>
    public IBuiltinVariable? BuiltinVariable { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a dot variable reference node, given the provided left side expression, and variable token.
    /// </summary>
    public DotVariableNode(IASTNode leftExpression, TokenVariable token)
    {
        LeftExpression = leftExpression;
        NearbyToken = token;
        VariableName = token.Text;
        BuiltinVariable = token.BuiltinVariable;
    }

    /// <summary>
    /// Creates a dot variable reference node, given the provided left side expression, and function token.
    /// </summary>
    public DotVariableNode(IASTNode leftExpression, TokenFunction token)
    {
        LeftExpression = leftExpression;
        NearbyToken = token;
        VariableName = token.Text;
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
