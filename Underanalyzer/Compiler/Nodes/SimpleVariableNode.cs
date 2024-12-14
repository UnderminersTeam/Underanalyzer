/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a simple variable reference in the AST
/// </summary>
internal sealed class SimpleVariableNode : IAssignableASTNode, IVariableASTNode
{
    /// <inheritdoc/>
    public string VariableName { get; }

    /// <inheritdoc/>
    public IBuiltinVariable? BuiltinVariable { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a simple variable reference node, given the provided variable token.
    /// </summary>
    public SimpleVariableNode(TokenVariable token)
    {
        NearbyToken = token;
        VariableName = token.Text;
        BuiltinVariable = token.BuiltinVariable;
    }

    /// <summary>
    /// Creates a simple variable reference node, given the provided name and builtin variable.
    /// </summary>
    public SimpleVariableNode(string variableName, IBuiltinVariable? builtinVariable)
    {
        VariableName = variableName;
        BuiltinVariable = builtinVariable;
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
