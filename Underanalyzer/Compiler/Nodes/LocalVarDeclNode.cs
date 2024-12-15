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
/// Represents a (list of) local variable declarations in the AST.
/// </summary>
internal sealed class LocalVarDeclNode : IASTNode
{
    /// <summary>
    /// List of local variables being declared by this node.
    /// </summary>
    public List<string> DeclaredLocals { get; }

    /// <summary>
    /// List of values being assigned by this node, or null for locals with no assignment.
    /// Corresponds 1-to-1 with <see cref="DeclaredLocals"/> in order.
    /// </summary>
    public List<IASTNode?> AssignedValues { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private LocalVarDeclNode(IToken token, List<string> declaredLocals, List<IASTNode?> assignedValues)
    {
        DeclaredLocals = declaredLocals;
        AssignedValues = assignedValues;
        NearbyToken = token;
    }

    /// <summary>
    /// Creates a local variable declaration node, parsing from the given context's current position.
    /// </summary>
    public static LocalVarDeclNode? Parse(ParseContext context)
    {
        // Parse "var" keyword
        if (context.EnsureToken(KeywordKind.Var) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse list of variables being declared
        List<string> declaredLocals = new(8);
        List<IASTNode?> assignedValues = new(8);
        while (!context.EndOfCode && context.Tokens[context.Position] is TokenVariable tokenVariable)
        {
            context.Position++;

            // Add to declaration's list
            declaredLocals.Add(tokenVariable.Text);

            // Check if overriding a builtin variable
            // TODO: modern gamemaker versions probably allow this, and we can disable this check
            if (tokenVariable.BuiltinVariable is not null)
            {
                context.CompileContext.PushError($"Declaring local variable over builtin '{tokenVariable.Text}'", tokenVariable);
            }

            // Add to this scope's local list
            context.CurrentScope.DeclareLocal(tokenVariable.Text);

            // Check for assignment
            if (context.IsCurrentToken(OperatorKind.Assign) || context.IsCurrentToken(OperatorKind.Assign2))
            {
                context.Position++;
                if (Expressions.ParseExpression(context) is IASTNode value)
                {
                    assignedValues.Add(value);
                }
                else
                {
                    // Failed to parse expression; stop parsing local var declaration
                    assignedValues.Add(null);
                    break;
                }
            }
            else
            {
                // No value getting assigned, just add null to maintain proper alignment
                assignedValues.Add(null);
            }

            // If next token isn't a comma, we're done parsing the list
            if (!context.IsCurrentToken(SeparatorKind.Comma))
            {
                break;
            }

            // Otherwise, move past comma and keep parsing
            context.Position++;
        }

        // Create final local var declaration node
        return new LocalVarDeclNode(tokenKeyword, declaredLocals, assignedValues);
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
