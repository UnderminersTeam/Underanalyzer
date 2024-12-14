/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a chain of binary operations in the AST.
/// </summary>
internal sealed class BinaryChainNode : IASTNode
{
    /// <summary>
    /// Arguments for operations.
    /// </summary>
    public List<IASTNode> Arguments { get; }

    /// <summary>
    /// Order of operations being performed in this chain.
    /// </summary>
    public List<BinaryOperation> Operations { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Kinds of binary operations.
    /// </summary>
    public enum BinaryOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        GMLDivRemainder,
        GMLModulo,
        LogicalAnd,
        LogicalOr,
        LogicalXor,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseShiftLeft,
        BitwiseShiftRight,
        CompareEqual,
        CompareNotEqual,
        CompareGreater,
        CompareGreaterEqual,
        CompareLesser,
        CompareLesserEqual
    }

    /// <summary>
    /// Gets the binary operation kind from a given token.
    /// </summary>
    public static BinaryOperation OperationKindFromToken(IToken token)
    {
        if (token is TokenOperator tokenOperator)
        {
            return tokenOperator.Kind switch
            {
                OperatorKind.Plus => BinaryOperation.Add,
                OperatorKind.Minus => BinaryOperation.Subtract,
                OperatorKind.Times => BinaryOperation.Multiply,
                OperatorKind.Divide => BinaryOperation.Divide,
                OperatorKind.Mod => BinaryOperation.GMLModulo,
                OperatorKind.LogicalAnd => BinaryOperation.LogicalAnd,
                OperatorKind.LogicalOr => BinaryOperation.LogicalOr,
                OperatorKind.LogicalXor => BinaryOperation.LogicalXor,
                OperatorKind.BitwiseAnd => BinaryOperation.BitwiseAnd,
                OperatorKind.BitwiseOr => BinaryOperation.BitwiseOr,
                OperatorKind.BitwiseXor => BinaryOperation.BitwiseXor,
                OperatorKind.BitwiseShiftLeft => BinaryOperation.BitwiseShiftLeft,
                OperatorKind.BitwiseShiftRight => BinaryOperation.BitwiseShiftRight,
                OperatorKind.CompareEqual or OperatorKind.Assign or OperatorKind.Assign2 => BinaryOperation.CompareEqual,
                OperatorKind.CompareNotEqual or OperatorKind.CompareNotEqual2 => BinaryOperation.CompareNotEqual,
                OperatorKind.CompareGreater => BinaryOperation.CompareGreater,
                OperatorKind.CompareGreaterEqual => BinaryOperation.CompareGreaterEqual,
                OperatorKind.CompareLesser => BinaryOperation.CompareLesser,
                OperatorKind.CompareLesserEqual => BinaryOperation.CompareLesserEqual,
                _ => throw new Exception("Unknown operator")
            };
        }
        else if (token is TokenKeyword tokenKeyword)
        {
            return tokenKeyword.Kind switch
            {
                KeywordKind.Div => BinaryOperation.GMLDivRemainder,
                KeywordKind.Mod => BinaryOperation.GMLModulo,
                _ => throw new Exception("Unknown operator")
            };
        }
        throw new Exception("Unknown operator");
    }

    /// <summary>
    /// Creates a nullish coalesce node, given the provided token and expressions for the left and right sides.
    /// </summary>
    public BinaryChainNode(IToken token, List<IASTNode> arguments, List<BinaryOperation> operations)
    {
        Arguments = arguments;
        Operations = operations;
        NearbyToken = token;
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
