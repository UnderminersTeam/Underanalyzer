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
/// Helper to parse statements.
/// </summary>
internal static class Statements
{
    /// <summary>
    /// Attempts to parse a statement from the current parse position of the 
    /// context, returning the root node of the statement.
    /// </summary>
    public static IASTNode? ParseStatement(ParseContext context)
    {
        if (context.EndOfCode)
        {
            context.CompileContext.PushError("Unexpected end of code");
            return null;
        }

        // Check type of statement based on first token
        IToken token = context.Tokens[context.Position];
        switch (token)
        {
            case TokenSeparator { Kind: SeparatorKind.BlockOpen }:
            case TokenKeyword { Kind: KeywordKind.Begin }:
                return BlockNode.ParseRegular(context);
            case TokenKeyword { Kind: KeywordKind.If }:
                return IfNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.Switch }:
                return SwitchNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.Try }:
                return TryCatchNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.While }:
                return WhileLoopNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.For }:
                return ForLoopNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.Repeat }:
                return RepeatLoopNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.Do }:
                return DoUntilLoopNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.With }:
                return WithLoopNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.Var }:
                return LocalVarDeclNode.Parse(context);
            case TokenKeyword { Kind: KeywordKind.Static } tokenStatic:
                StaticDeclarations.Parse(context);
                return BlockNode.CreateEmpty(tokenStatic);
            case TokenKeyword { Kind: KeywordKind.Exit } tokenExit:
                context.Position++;
                return new ExitNode(tokenExit);
            case TokenKeyword { Kind: KeywordKind.Return } tokenReturn:
                {
                    context.Position++;
                    if (!context.EndOfCode)
                    {
                        IToken nextToken = context.Tokens[context.Position];
                        if (nextToken is not TokenSeparator { Kind: SeparatorKind.Semicolon } and
                                         not TokenKeyword { Kind: not KeywordKind.Function })
                        {
                            if (Expressions.ParseExpression(context) is IASTNode returnValue)
                            {
                                return new ReturnNode(tokenReturn, returnValue);
                            }
                        }
                    }
                    return new ExitNode(tokenReturn);
                }
            case TokenKeyword { Kind: KeywordKind.Break } tokenBreak:
                context.Position++;
                return new BreakNode(tokenBreak);
            case TokenKeyword { Kind: KeywordKind.Continue } tokenContinue:
                context.Position++;
                return new ContinueNode(tokenContinue);
            // TODO: enums
            default:
                if (ParseAssignmentOrExpressionStatement(context) is IASTNode stmt)
                {
                    return stmt;
                }
                break;
        }

        context.CompileContext.PushError("Failed to find a valid statement", token);
        return null;
    }

    private static IASTNode? ParseAssignmentOrExpressionStatement(ParseContext context)
    {
        // Parse expression (destination of assignment, or chain function call)
        if (Expressions.ParseChainExpression(context) is not IASTNode expr)
        {
            return null;
        }

        // Check for assignment
        if (expr is IAssignableASTNode assignable && !context.EndOfCode && 
            context.Tokens[context.Position] is TokenOperator
            {
                Kind: OperatorKind.Assign                   or OperatorKind.Assign2             or
                      OperatorKind.CompoundPlus             or OperatorKind.CompoundMinus       or
                      OperatorKind.CompoundTimes            or OperatorKind.CompoundDivide      or
                      OperatorKind.CompoundMod              or OperatorKind.CompoundBitwiseAnd  or
                      OperatorKind.CompoundBitwiseOr        or OperatorKind.CompoundBitwiseXor  or
                      OperatorKind.CompoundNullishCoalesce
            } 
            tokenOperator)
        {
            context.Position++;

            // If left side is assigning to a read-only builtin variable, push an error
            if (assignable is IVariableASTNode variable && !(variable.BuiltinVariable?.CanSet ?? true))
            {
                context.CompileContext.PushError($"Attempting to assign read-only variable '{variable.VariableName}'", variable.NearbyToken);
            }

            // Parse expression on right side and make assignment if possible
            if (Expressions.ParseExpression(context) is IASTNode rhs)
            {
                return new AssignNode(tokenOperator.Kind switch
                {
                    OperatorKind.Assign or OperatorKind.Assign2 =>  AssignNode.AssignKind.Normal,
                    OperatorKind.CompoundPlus =>                    AssignNode.AssignKind.CompoundPlus,
                    OperatorKind.CompoundMinus =>                   AssignNode.AssignKind.CompoundMinus,
                    OperatorKind.CompoundTimes =>                   AssignNode.AssignKind.CompoundTimes,
                    OperatorKind.CompoundDivide =>                  AssignNode.AssignKind.CompoundDivide,
                    OperatorKind.CompoundMod =>                     AssignNode.AssignKind.CompoundMod,
                    OperatorKind.CompoundBitwiseAnd =>              AssignNode.AssignKind.CompoundBitwiseAnd,
                    OperatorKind.CompoundBitwiseOr =>               AssignNode.AssignKind.CompoundBitwiseOr,
                    OperatorKind.CompoundBitwiseXor =>              AssignNode.AssignKind.CompoundBitwiseXor,
                    OperatorKind.CompoundNullishCoalesce =>         AssignNode.AssignKind.CompoundNullishCoalesce,
                    _ => throw new Exception("Unknown operator kind in assignment")
                }, assignable, rhs);
            }
            return null;
        }
        
        if (expr is IMaybeStatementASTNode statement)
        {
            // This is an expression that can also be a standalone statement, so mark it as such and return it
            statement.IsStatement = true;
            return statement;
        }

        // Unknown floating expression; can't do anything with it
        context.CompileContext.PushError("Expression floating outside of any statement", expr.NearbyToken);
        return null;
    }
}
