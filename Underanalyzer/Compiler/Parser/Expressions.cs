/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Nodes;

namespace Underanalyzer.Compiler.Parser;

/// <summary>
/// Helper to parse expressions.
/// </summary>
internal static class Expressions
{
    /// <summary>
    /// Attempts to parse an expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseExpression(ParseContext context)
    {
        IASTNode? lhs = ParseChainExpression(context);
        return lhs;

        // TODO: the rest of the parsing here (and handling lhs)
    }

    /// <summary>
    /// Attempts to parse a chain expression from the current parse position of the context.
    /// A chain expression is any leftmost expression followed by ".", a variable/function/accessor, and repeating.
    /// </summary>
    public static IASTNode? ParseChainExpression(ParseContext context)
    {
        IASTNode? lhs = ParseLeftmostExpression(context);
        return lhs;

        // TODO: the rest of the parsing here (and handling lhs)
    }

    /// <summary>
    /// Attempts to parse a leftmost expression, such as a single token, or a grouped full expression.
    /// </summary>
    public static IASTNode? ParseLeftmostExpression(ParseContext context)
    {
        if (context.EndOfCode)
        {
            context.CompileContext.PushError("Unexpected end of code");
            return null;
        }

        // Check type of expression based on first token
        IToken token = context.Tokens[context.Position];
        switch (token)
        {
            case TokenNumber tokenNumber:
                context.Position++;
                return new NumberNode(tokenNumber);
            case TokenInt64 tokenInt64:
                context.Position++;
                return new Int64Node(tokenInt64);
            case TokenString tokenString:
                context.Position++;
                return new StringNode(tokenString);
            case TokenBoolean tokenBoolean:
                context.Position++;
                return new BooleanNode(tokenBoolean);
            case TokenAssetReference tokenAssetReference:
                context.Position++;
                return new AssetReferenceNode(tokenAssetReference);
            case TokenFunction tokenFunction:
                context.Position++;
                return new SimpleFunctionCallNode(context, tokenFunction);
            case TokenVariable tokenVariable:
                context.Position++;
                return new SimpleVariableNode(tokenVariable);
            case TokenSeparator { Kind: SeparatorKind.GroupOpen }:
                {
                    context.Position++;
                    IASTNode? groupedExpression = ParseExpression(context);
                    context.EnsureToken(SeparatorKind.GroupClose);
                    return groupedExpression;
                }
            case TokenSeparator { Kind: SeparatorKind.ArrayOpen }:
                {
                    context.Position++;
                    if (context.CompileContext.GameContext.UsingGMS2OrLater)
                    {
                        return SimpleFunctionCallNode.ParseArrayLiteral(context);
                    }

                    context.CompileContext.PushError("Cannot use array literals before GMS2", token);
                    return null;
                }
            case TokenOperator { Kind: OperatorKind.Increment or OperatorKind.Decrement } tokenPrefix:
                context.Position++;
                return new PrefixNode(context, tokenPrefix, tokenPrefix.Kind == OperatorKind.Increment);
            case TokenOperator { Kind: OperatorKind.Not or OperatorKind.BitwiseNegate or 
                                       OperatorKind.Plus or OperatorKind.Minus } tokenUnary:
                context.Position++;
                return new UnaryNode(context, tokenUnary, tokenUnary.Kind switch
                {
                    OperatorKind.Not => UnaryNode.UnaryKind.BooleanNot,
                    OperatorKind.BitwiseNegate => UnaryNode.UnaryKind.BitwiseNegate,
                    OperatorKind.Plus => UnaryNode.UnaryKind.Positive,
                    OperatorKind.Minus => UnaryNode.UnaryKind.Negative,
                    _ => throw new Exception("Unknown operator kind for unary operation")
                });
            case TokenKeyword { Kind: KeywordKind.Not } tokenUnaryNotKeyword:
                context.Position++;
                return new UnaryNode(context, tokenUnaryNotKeyword, UnaryNode.UnaryKind.BooleanNot);
            // TODO: structs, function declarations, new, delete
        }
                
        context.Position++;
        context.CompileContext.PushError("Failed to find a valid expression", token);
        return null;
    }
}
