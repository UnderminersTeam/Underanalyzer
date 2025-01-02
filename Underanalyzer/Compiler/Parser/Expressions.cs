/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Nodes;
using static Underanalyzer.Compiler.Nodes.BinaryChainNode;

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
        return ParseConditional(context);
    }

    /// <summary>
    /// Attempts to parse a conditional (ternary) expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseConditional(ParseContext context)
    {
        IASTNode? lhs = ParseNullishExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "?"
        if (!context.EndOfCode && 
            context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.Conditional } tokenConditional)
        {
            context.Position++;

            // Parse true expression
            if (ParseOrExpression(context) is IASTNode trueExpr)
            {
                // Check for ":"
                context.EnsureToken(SeparatorKind.Colon);

                // Parse false expression
                if (ParseOrExpression(context) is IASTNode falseExpr)
                {
                    // Replace left side with new node
                    lhs = new ConditionalNode(tokenConditional, lhs, trueExpr, falseExpr);
                }
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a nullish (??) expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseNullishExpression(ParseContext context)
    {
        IASTNode? lhs = ParseOrExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "??"
        if (!context.EndOfCode && 
            context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.NullishCoalesce } tokenNullishCoalesce)
        {
            context.Position++;

            // Parse right expression
            if (ParseOrExpression(context) is IASTNode rightExpr)
            {
                // Replace left side with new node
                lhs = new NullishCoalesceNode(tokenNullishCoalesce, lhs, rightExpr);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a logical OR (||) expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseOrExpression(ParseContext context)
    {
        IASTNode? lhs = ParseAndExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "||"
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.LogicalOr } tokenOperator)
        {
            context.Position++;

            // Parse right side of binary operation
            if (ParseAndExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "||" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.LogicalOr } tokenNextOperator)
                {
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseAndExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a logical AND (&&) expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseAndExpression(ParseContext context)
    {
        IASTNode? lhs = ParseXorExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "&&"
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.LogicalAnd } tokenOperator)
        {
            context.Position++;

            // Parse right side of binary operation
            if (ParseXorExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "&&" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.LogicalAnd } tokenNextOperator)
                {
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseXorExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a logical XOR (^^) expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseXorExpression(ParseContext context)
    {
        IASTNode? lhs = ParseCompareExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "^^"
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.LogicalXor } tokenOperator)
        {
            context.Position++;

            // Parse right side of binary operation
            if (ParseCompareExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "&&" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is TokenOperator { Kind: OperatorKind.LogicalXor } tokenNextOperator)
                {
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseCompareExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a comparison expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseCompareExpression(ParseContext context)
    {
        IASTNode? lhs = ParseBitwiseExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "==", "=", ":=", "!=", "<>", ">", ">=", "<", "<="
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is TokenOperator
            {
                Kind: OperatorKind.CompareEqual or OperatorKind.Assign or OperatorKind.Assign2 or
                      OperatorKind.CompareNotEqual or OperatorKind.CompareNotEqual2 or
                      OperatorKind.CompareGreater or OperatorKind.CompareGreaterEqual or
                      OperatorKind.CompareLesser or OperatorKind.CompareLesserEqual
            }
            tokenOperator)
        {
            context.Position++;

            // Parse right side of binary operation
            if (ParseBitwiseExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "==", "=", ":=", "!=", "<>", ">", ">=", "<", "<=" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is TokenOperator
                       {
                           Kind: OperatorKind.CompareEqual or OperatorKind.Assign or OperatorKind.Assign2 or
                                 OperatorKind.CompareNotEqual or OperatorKind.CompareNotEqual2 or
                                 OperatorKind.CompareGreater or OperatorKind.CompareGreaterEqual or
                                 OperatorKind.CompareLesser or OperatorKind.CompareLesserEqual
                       } 
                       tokenNextOperator)
                {
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseBitwiseExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a bitwise expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseBitwiseExpression(ParseContext context)
    {
        IASTNode? lhs = ParseBitwiseShiftExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "&", "|", "^"
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is TokenOperator
            {
                Kind: OperatorKind.BitwiseAnd or OperatorKind.BitwiseOr or OperatorKind.BitwiseXor
            }
            tokenOperator)
        {
            context.Position++;

            // Parse right side of binary operation
            if (ParseBitwiseShiftExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "&", "|", "^" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is TokenOperator
                       {
                           Kind: OperatorKind.BitwiseAnd or OperatorKind.BitwiseOr or OperatorKind.BitwiseXor
                       }
                       tokenNextOperator)
                {
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseBitwiseShiftExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a bitwise shift expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseBitwiseShiftExpression(ParseContext context)
    {
        IASTNode? lhs = ParseAddSubtractExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "<<", ">>"
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is TokenOperator
            {
                Kind: OperatorKind.BitwiseShiftLeft or OperatorKind.BitwiseShiftRight
            }
            tokenOperator)
        {
            context.Position++;

            // Parse right side of binary operation
            if (ParseAddSubtractExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "<<", ">>" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is TokenOperator
                       {
                           Kind: OperatorKind.BitwiseShiftLeft or OperatorKind.BitwiseShiftRight
                       }
                       tokenNextOperator)
                {
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseAddSubtractExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a add/subtract expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseAddSubtractExpression(ParseContext context)
    {
        IASTNode? lhs = ParseMultiplyDivideExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "+", "-"
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is TokenOperator
            {
                Kind: OperatorKind.Plus or OperatorKind.Minus
            }
            tokenOperator)
        {
            context.Position++;

            // Parse right side of binary operation
            if (ParseMultiplyDivideExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "+", "-" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is TokenOperator
                       {
                           Kind: OperatorKind.Plus or OperatorKind.Minus
                       }
                       tokenNextOperator)
                {
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseMultiplyDivideExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a multiply/divide/mod/remainder expression from the current parse position of the context.
    /// </summary>
    public static IASTNode? ParseMultiplyDivideExpression(ParseContext context)
    {
        IASTNode? lhs = ParseChainExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Check for "*", "/", "%", "mod", "div"
        if (!context.EndOfCode &&
            context.Tokens[context.Position] is 
            TokenOperator { Kind: OperatorKind.Times or OperatorKind.Divide or OperatorKind.Mod } or
            TokenKeyword { Kind: KeywordKind.Mod or KeywordKind.Div }) 
        {
            IToken tokenOperator = context.Tokens[context.Position];
            context.Position++;

            // Parse right side of binary operation
            if (ParseChainExpression(context) is IASTNode rightExpr)
            {
                // Start accumulating additional arguments in binary operation chain
                List<IASTNode> arguments = new(4) { lhs, rightExpr };
                List<BinaryOperation> operations = new(4) { OperationKindFromToken(tokenOperator) };

                // Check for "*", "/", "%", "mod", "div" to continue finding arguments
                while (!context.EndOfCode &&
                       context.Tokens[context.Position] is
                       TokenOperator { Kind: OperatorKind.Times or OperatorKind.Divide or OperatorKind.Mod } or
                       TokenKeyword { Kind: KeywordKind.Mod or KeywordKind.Div })
                {
                    IToken tokenNextOperator = context.Tokens[context.Position];
                    context.Position++;

                    // Parse right side of current binary operation
                    if (ParseChainExpression(context) is IASTNode nextRightExpr)
                    {
                        arguments.Add(nextRightExpr);
                        operations.Add(OperationKindFromToken(tokenNextOperator));
                    }
                    else
                    {
                        break;
                    }
                }

                // Replace left side with this new binary chain
                lhs = new BinaryChainNode(tokenOperator, arguments, operations);
            }
        }

        return lhs;
    }

    /// <summary>
    /// Attempts to parse a chain expression from the current parse position of the context.
    /// A chain expression is any leftmost expression followed by ".", a variable/function/accessor, and repeating.
    /// </summary>
    public static IASTNode? ParseChainExpression(ParseContext context, bool stopAtFunctionCall = false)
    {
        IASTNode? lhs = ParseLeftmostExpression(context);
        if (lhs is null)
        {
            return null;
        }

        // Parse chain until nothing else matches
        while (!context.EndOfCode)
        {
            IToken currentToken = context.Tokens[context.Position];

            // Check for an accessor
            if (currentToken is TokenSeparator
                {
                    Kind: SeparatorKind.ArrayOpen       or SeparatorKind.ArrayOpenList  or
                          SeparatorKind.ArrayOpenMap    or SeparatorKind.ArrayOpenGrid  or
                          SeparatorKind.ArrayOpenDirect or SeparatorKind.ArrayOpenStruct
                }
                tokenArrayOpen)
            {
                context.Position++;

                // Parse accessor
                AccessorNode? accessor = AccessorNode.Parse(context, tokenArrayOpen, lhs, tokenArrayOpen.Kind switch
                {
                    SeparatorKind.ArrayOpen =>          AccessorNode.AccessorKind.Array,
                    SeparatorKind.ArrayOpenDirect =>    AccessorNode.AccessorKind.ArrayDirect,
                    SeparatorKind.ArrayOpenList =>      AccessorNode.AccessorKind.List,
                    SeparatorKind.ArrayOpenMap =>       AccessorNode.AccessorKind.Map,
                    SeparatorKind.ArrayOpenGrid =>      AccessorNode.AccessorKind.Grid,
                    SeparatorKind.ArrayOpenStruct =>    AccessorNode.AccessorKind.Struct,
                    _ => throw new Exception("Unknown accessor kind")
                });
                if (accessor is null)
                {
                    // Failed to parse accessor; stop parsing chain
                    break;
                }

                // If in GMLv2, rewrite old 2D array accesses as modern ones
                if (context.CompileContext.GameContext.UsingGMLv2)
                {
                    accessor = accessor.Convert2DArrayToTwoAccessors();
                }

                // This accessor is now the left side of the chain
                lhs = accessor;
                continue;
            }

            // Check for a function call not assigned to any specific variable name
            if (!stopAtFunctionCall &&
                currentToken is TokenSeparator { Kind: SeparatorKind.GroupOpen } tokenOpen)
            {
                context.Position++;

                // Parse function call, which becomes left side of chain
                lhs = new FunctionCallNode(context, tokenOpen, lhs);
                continue;
            }

            // Check for dot access
            if (currentToken is TokenSeparator { Kind: SeparatorKind.Dot } tokenDot)
            {
                context.Position++;

                if (context.EndOfCode)
                {
                    context.CompileContext.PushError("Unexpected end of code", tokenDot);
                    break;
                }

                // Create dot variable node, combining left side and following token
                IToken nextToken = context.Tokens[context.Position];
                if (nextToken is TokenVariable tokenVariable)
                {
                    context.Position++;
                    lhs = new DotVariableNode(lhs, tokenVariable);
                }
                else if (nextToken is TokenFunction tokenFunction)
                {
                    context.Position++;
                    lhs = new DotVariableNode(lhs, tokenFunction);

                    if (!stopAtFunctionCall)
                    {
                        // Parse function call here as well
                        if (!context.EndOfCode && context.Tokens[context.Position] is TokenSeparator { Kind: SeparatorKind.GroupOpen } tokenDotOpen)
                        {
                            lhs = new FunctionCallNode(context, tokenDotOpen, lhs);
                        }
                        else
                        {
                            // Throw error (this should really never happen, though)
                            context.EnsureToken(SeparatorKind.GroupOpen);
                        }
                    }
                }
                else
                {
                    context.CompileContext.PushError("Expected variable or function call after dot", tokenDot);
                    break;
                }
                continue;
            }

            // Nothing matched; chain is over
            break;
        }

        // Check for and parse postfix
        if (!context.EndOfCode && context.Tokens[context.Position] is TokenOperator 
            { 
                Kind: OperatorKind.Increment or OperatorKind.Decrement 
            } 
            tokenPostfix)
        {
            context.Position++;
            lhs = new PostfixNode(tokenPostfix, lhs, tokenPostfix.Kind == OperatorKind.Increment);
        }

        return lhs;
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
                context.Position++;
                if (context.CompileContext.GameContext.UsingGMS2OrLater)
                {
                    return SimpleFunctionCallNode.ParseArrayLiteral(context);
                }
                context.CompileContext.PushError("Cannot use array literals before GMS2", token);
                return null;
            case TokenOperator { Kind: OperatorKind.Increment or OperatorKind.Decrement } tokenPrefix:
                context.Position++;
                return PrefixNode.Parse(context, tokenPrefix, tokenPrefix.Kind == OperatorKind.Increment);
            case TokenOperator { Kind: OperatorKind.Not or OperatorKind.BitwiseNegate or 
                                       OperatorKind.Plus or OperatorKind.Minus } tokenUnary:
                context.Position++;
                return UnaryNode.Parse(context, tokenUnary, tokenUnary.Kind switch
                {
                    OperatorKind.Not => UnaryNode.UnaryKind.BooleanNot,
                    OperatorKind.BitwiseNegate => UnaryNode.UnaryKind.BitwiseNegate,
                    OperatorKind.Plus => UnaryNode.UnaryKind.Positive,
                    OperatorKind.Minus => UnaryNode.UnaryKind.Negative,
                    _ => throw new Exception("Unknown operator kind for unary operation")
                });
            case TokenKeyword { Kind: KeywordKind.Not } tokenUnaryNotKeyword:
                context.Position++;
                return UnaryNode.Parse(context, tokenUnaryNotKeyword, UnaryNode.UnaryKind.BooleanNot);
            case TokenKeyword { Kind: KeywordKind.Function } tokenFunction:
                context.Position++;
                if (context.CompileContext.GameContext.UsingGMLv2)
                {
                    return FunctionDeclNode.Parse(context, tokenFunction);
                }
                context.CompileContext.PushError("Cannot declare functions before GMLv2 (GameMaker 2.3+)", token);
                return null;
            case TokenSeparator { Kind: SeparatorKind.BlockOpen } tokenBlockOpen:
                context.Position++;
                if (context.CompileContext.GameContext.UsingGMLv2)
                {
                    return FunctionDeclNode.ParseStruct(context, tokenBlockOpen);
                }
                context.CompileContext.PushError("Cannot use structs before GMLv2 (GameMaker 2.3+)", token);
                return null;
            case TokenKeyword { Kind: KeywordKind.Begin } tokenBegin:
                context.Position++;
                if (context.CompileContext.GameContext.UsingGMLv2)
                {
                    return FunctionDeclNode.ParseStruct(context, tokenBegin);
                }
                context.CompileContext.PushError("Cannot use structs before GMLv2 (GameMaker 2.3+)", token);
                return null;
            case TokenKeyword { Kind: KeywordKind.New }:
                if (context.CompileContext.GameContext.UsingGMLv2)
                {
                    return NewObjectNode.Parse(context);
                }
                context.CompileContext.PushError("Cannot use new before GMLv2 (GameMaker 2.3+)", token);
                return null;
        }
                
        context.Position++;
        context.CompileContext.PushError("Failed to find a valid expression", token);
        return null;
    }
}
