/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a dot (.) variable reference in the AST, as part of a chain reference.
/// </summary>
internal sealed class DotVariableNode : IAssignableASTNode, IVariableASTNode
{
    /// <summary>
    /// Expression on the left side of the dot.
    /// </summary>
    public IASTNode LeftExpression { get; private set; }

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
        // Combine numbers into instance type (before processing left side!)
        if (LeftExpression is NumberNode { Value: double numberValue } && (int)numberValue == numberValue)
        {
            SimpleVariableNode combined = new(VariableName, BuiltinVariable);
            combined.SetExplicitInstanceType((InstanceType)(int)numberValue);
            return combined;
        }

        // Process left side
        LeftExpression = LeftExpression.PostProcess(context);

        // Resolve enum values to a constant, if possible
        if (LeftExpression is SimpleVariableNode { VariableName: string enumName })
        {
            // Check parse enums for a constant value first
            if (context.ParseEnums.TryGetValue(enumName, out EnumDeclaration? parseDecl) &&
                parseDecl.IntegerValues.TryGetValue(VariableName, out long parseValue))
            {
                return new Int64Node(parseValue, NearbyToken);
            }

            // Check fully-resolved enums as well (and enforce error checking here)
            if (context.CompileContext.Enums.TryGetValue(enumName, out GMEnum? decl))
            {
                if (decl.TryGetValue(VariableName, out long value))
                {
                    return new Int64Node(value, NearbyToken);
                }
                context.CompileContext.PushError($"Failed to find enum value for '{enumName}.{VariableName}'", NearbyToken);
            }
        }

        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
