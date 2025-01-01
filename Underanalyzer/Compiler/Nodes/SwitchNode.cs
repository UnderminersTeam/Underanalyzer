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
/// Represents a "switch" statement in the AST.
/// </summary>
internal sealed class SwitchNode : IASTNode
{
    /// <summary>
    /// Expression to be used for matching in the switch statement node.
    /// </summary>
    public IASTNode Expression { get; }

    /// <summary>
    /// Contents of the switch statement.
    /// </summary>
    public List<IASTNode> Children { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private SwitchNode(TokenKeyword token, IASTNode expression, List<IASTNode> children)
    {
        NearbyToken = token;
        Expression = expression;
        Children = children;
    }

    /// <summary>
    /// Creates a switch statement node, parsing from the given context's current position.
    /// </summary>
    public static SwitchNode? Parse(ParseContext context)
    {
        // Parse "switch" keyword
        if (context.EnsureToken(KeywordKind.Switch) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse expression being matched
        if (Expressions.ParseExpression(context) is not IASTNode expression)
        {
            return null;
        }
        
        // Parse main block
        List<IASTNode> children = new(32);
        context.EnsureToken(SeparatorKind.BlockOpen, KeywordKind.Begin);
        context.SkipSemicolons();
        while (!context.EndOfCode && !context.IsCurrentToken(SeparatorKind.BlockClose, KeywordKind.End))
        {
            // Parse statements, but particularly "case" and "default" labels
            IToken currentToken = context.Tokens[context.Position];
            if (currentToken is TokenKeyword { Kind: KeywordKind.Case } tokenCase)
            {
                // "case" label: parse expression
                context.Position++;
                if (Expressions.ParseExpression(context) is IASTNode caseExpr)
                {
                    SwitchCaseNode caseNode = new(tokenCase, caseExpr);
                    context.EnsureToken(SeparatorKind.Colon);
                    children.Add(caseNode);
                }
            }
            else if (currentToken is TokenKeyword { Kind: KeywordKind.Default } tokenDefault)
            {
                // "default" label: no expression to parse
                context.Position++;
                SwitchCaseNode defaultNode = new(tokenDefault, null);
                context.EnsureToken(SeparatorKind.Colon);
                children.Add(defaultNode);
            }
            else if (Statements.ParseStatement(context) is IASTNode statement)
            {
                // Regular statement
                children.Add(statement);
            }
            else
            {
                // Failed to parse statement; stop parsing this block.
                break;
            }
            context.SkipSemicolons();
        }
        context.EnsureToken(SeparatorKind.BlockClose, KeywordKind.End);

        // Create final statement
        return new SwitchNode(tokenKeyword, expression, children);
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
