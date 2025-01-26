﻿/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

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
    /// Creates a binary chain node, given the provided token and expressions for the arguments and corresponding operations.
    /// </summary>
    public BinaryChainNode(IToken? token, List<IASTNode> arguments, List<BinaryOperation> operations)
    {
        Arguments = arguments;
        Operations = operations;
        NearbyToken = token;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].PostProcess(context);
        }

        // Attempt to optimize while constants are present (at the start, according to official behavior)
        while (Arguments is [IConstantASTNode left, IConstantASTNode right, ..])
        {
            if (PerformConstantOperation(context, Operations[0], left, right) is IConstantASTNode replacement)
            {
                // Replace left and right with the single constant, and remove operation
                Arguments.RemoveAt(0);
                Arguments[0] = replacement;
                Operations.RemoveAt(0);
            }
            else
            {
                // Failed to perform operation; stop here
                break;
            }
        }

        // If there's only one argument left, return only that
        if (Arguments.Count == 1)
        {
            return Arguments[0];
        }

        return this;
    }

    /// <summary>
    /// Attempts to perform a compile-time binary operation between two constants, or returns null if not possible.
    /// </summary>
    private static IConstantASTNode? PerformConstantOperation(ParseContext context, BinaryOperation operation, IConstantASTNode left, IConstantASTNode right)
    {
        // Coerce booleans into numbers
        if (left is BooleanNode leftBool)
        {
            left = new NumberNode(leftBool.Value ? 1 : 0, leftBool.NearbyToken);
        }
        if (right is BooleanNode rightBool)
        {
            right = new NumberNode(rightBool.Value ? 1 : 0, rightBool.NearbyToken);
        }

        // Perform operation
        return (operation, left, right) switch
        {
            (BinaryOperation.Add, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(leftNumber.Value + rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.Add, NumberNode leftNumber, Int64Node rightInt64) =>
                new NumberNode(leftNumber.Value + rightInt64.Value, leftNumber.NearbyToken),
            (BinaryOperation.Add, Int64Node leftInt64, NumberNode rightNumber) =>
                new NumberNode(leftInt64.Value + rightNumber.Value, leftInt64.NearbyToken),
            (BinaryOperation.Add, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value + rightInt64.Value, leftInt64.NearbyToken),
            (BinaryOperation.Add, StringNode leftString, Int64Node rightString) =>
                new StringNode(leftString.Value + rightString.Value, leftString.NearbyToken),

            (BinaryOperation.Subtract, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(leftNumber.Value - rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.Subtract, NumberNode leftNumber, Int64Node rightInt64) =>
                new NumberNode(leftNumber.Value - rightInt64.Value, leftNumber.NearbyToken),
            (BinaryOperation.Subtract, Int64Node leftInt64, NumberNode rightNumber) =>
                new NumberNode(leftInt64.Value - rightNumber.Value, leftInt64.NearbyToken),
            (BinaryOperation.Subtract, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value - rightInt64.Value, leftInt64.NearbyToken),

            (BinaryOperation.Multiply, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(leftNumber.Value * rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.Multiply, NumberNode leftNumber, Int64Node rightInt64) =>
                new NumberNode(leftNumber.Value * rightInt64.Value, leftNumber.NearbyToken),
            (BinaryOperation.Multiply, NumberNode leftNumber, StringNode rightString) =>
                new StringNode(
                    new StringBuilder(rightString.Value.Length * (int)leftNumber.Value)
                        .Insert(0, rightString.Value, (int)leftNumber.Value)
                        .ToString(),
                    leftNumber.NearbyToken),
            (BinaryOperation.Multiply, Int64Node leftInt64, NumberNode rightNumber) =>
                new NumberNode(leftInt64.Value * rightNumber.Value, leftInt64.NearbyToken),
            (BinaryOperation.Multiply, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value * rightInt64.Value, leftInt64.NearbyToken),

            (BinaryOperation.Divide, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(leftNumber.Value / rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.Divide, NumberNode leftNumber, Int64Node rightInt64) =>
                new NumberNode(leftNumber.Value / rightInt64.Value, leftNumber.NearbyToken),
            (BinaryOperation.Divide, Int64Node leftInt64, NumberNode rightNumber) =>
                new NumberNode(leftInt64.Value / rightNumber.Value, leftInt64.NearbyToken),
            (BinaryOperation.Divide, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value / CheckDivisionByZero(context, rightInt64, rightInt64.Value), leftInt64.NearbyToken),

            (BinaryOperation.GMLDivRemainder, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode((long)leftNumber.Value / CheckDivisionByZero(context, rightNumber, (long)rightNumber.Value), leftNumber.NearbyToken),
            (BinaryOperation.GMLDivRemainder, NumberNode leftNumber, Int64Node rightInt64) =>
                new Int64Node((long)leftNumber.Value / CheckDivisionByZero(context, rightInt64, rightInt64.Value), leftNumber.NearbyToken),
            (BinaryOperation.GMLDivRemainder, Int64Node leftInt64, NumberNode rightNumber) =>
                new Int64Node(leftInt64.Value / CheckDivisionByZero(context, rightNumber, (long)rightNumber.Value), leftInt64.NearbyToken),
            (BinaryOperation.GMLDivRemainder, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value / CheckDivisionByZero(context, rightInt64, rightInt64.Value), leftInt64.NearbyToken),

            (BinaryOperation.GMLModulo, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(leftNumber.Value % rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.GMLModulo, NumberNode leftNumber, Int64Node rightInt64) =>
                new Int64Node((long)leftNumber.Value % CheckDivisionByZero(context, rightInt64, rightInt64.Value), leftNumber.NearbyToken),
            (BinaryOperation.GMLModulo, Int64Node leftInt64, NumberNode rightNumber) =>
                new Int64Node(leftInt64.Value % CheckDivisionByZero(context, rightNumber, (long)rightNumber.Value), leftInt64.NearbyToken),
            (BinaryOperation.GMLModulo, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value % CheckDivisionByZero(context, rightInt64, rightInt64.Value), leftInt64.NearbyToken),

            (BinaryOperation.CompareEqual or BinaryOperation.CompareNotEqual or
             BinaryOperation.CompareLesser or BinaryOperation.CompareLesserEqual or
             BinaryOperation.CompareGreater or BinaryOperation.CompareGreaterEqual,
             _, _) =>
                CompareConstants(operation, left, right),

            // Small note for &&, ||, and ^^: the official compiler does not used typed booleans here for whatever reason
            (BinaryOperation.LogicalAnd, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(((leftNumber.Value > 0.5) && (rightNumber.Value > 0.5)) ? 1 : 0, leftNumber.NearbyToken),
            (BinaryOperation.LogicalAnd, NumberNode leftNumber, Int64Node rightInt64) =>
                new NumberNode(((leftNumber.Value > 0.5) && (rightInt64.Value > 0.5)) ? 1 : 0, leftNumber.NearbyToken),
            (BinaryOperation.LogicalAnd, Int64Node leftInt64, NumberNode rightNumber) =>
                new NumberNode(((leftInt64.Value > 0.5) && (rightNumber.Value > 0.5)) ? 1 : 0, leftInt64.NearbyToken),
            (BinaryOperation.LogicalAnd, Int64Node leftInt64, Int64Node rightInt64) =>
                new NumberNode(((leftInt64.Value > 0.5) && (rightInt64.Value > 0.5)) ? 1 : 0, leftInt64.NearbyToken),

            (BinaryOperation.LogicalOr, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(((leftNumber.Value > 0.5) || (rightNumber.Value > 0.5)) ? 1 : 0, leftNumber.NearbyToken),
            (BinaryOperation.LogicalOr, NumberNode leftNumber, Int64Node rightInt64) =>
                new NumberNode(((leftNumber.Value > 0.5) || (rightInt64.Value > 0.5)) ? 1 : 0, leftNumber.NearbyToken),
            (BinaryOperation.LogicalOr, Int64Node leftInt64, NumberNode rightNumber) =>
                new NumberNode(((leftInt64.Value > 0.5) || (rightNumber.Value > 0.5)) ? 1 : 0, leftInt64.NearbyToken),
            (BinaryOperation.LogicalOr, Int64Node leftInt64, Int64Node rightInt64) =>
                new NumberNode(((leftInt64.Value > 0.5) || (rightInt64.Value > 0.5)) ? 1 : 0, leftInt64.NearbyToken),

            (BinaryOperation.LogicalXor, NumberNode leftNumber, NumberNode rightNumber) =>
                new NumberNode(((leftNumber.Value > 0.5) ^ (rightNumber.Value > 0.5)) ? 1 : 0, leftNumber.NearbyToken),
            (BinaryOperation.LogicalXor, NumberNode leftNumber, Int64Node rightInt64) =>
                new NumberNode(((leftNumber.Value > 0.5) ^ (rightInt64.Value > 0.5)) ? 1 : 0, leftNumber.NearbyToken),
            (BinaryOperation.LogicalXor, Int64Node leftInt64, NumberNode rightNumber) =>
                new NumberNode(((leftInt64.Value > 0.5) ^ (rightNumber.Value > 0.5)) ? 1 : 0, leftInt64.NearbyToken),
            (BinaryOperation.LogicalXor, Int64Node leftInt64, Int64Node rightInt64) =>
                new NumberNode(((leftInt64.Value > 0.5) ^ (rightInt64.Value > 0.5)) ? 1 : 0, leftInt64.NearbyToken),

            (BinaryOperation.BitwiseAnd, NumberNode leftNumber, NumberNode rightNumber) =>
                GetBitwiseNumberResult((long)leftNumber.Value & (long)rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.BitwiseAnd, NumberNode leftInt64, Int64Node rightInt64) =>
                new Int64Node((long)leftInt64.Value & rightInt64.Value, leftInt64.NearbyToken),
            (BinaryOperation.BitwiseAnd, Int64Node leftInt64, NumberNode rightNumber) =>
                new Int64Node(leftInt64.Value & (long)rightNumber.Value, leftInt64.NearbyToken),
            (BinaryOperation.BitwiseAnd, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value & rightInt64.Value, leftInt64.NearbyToken),

            (BinaryOperation.BitwiseOr, NumberNode leftNumber, NumberNode rightNumber) =>
                GetBitwiseNumberResult((long)leftNumber.Value | (long)rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.BitwiseOr, NumberNode leftInt64, Int64Node rightInt64) =>
                new Int64Node((long)leftInt64.Value | rightInt64.Value, leftInt64.NearbyToken),
            (BinaryOperation.BitwiseOr, Int64Node leftInt64, NumberNode rightNumber) =>
                new Int64Node(leftInt64.Value | (long)rightNumber.Value, leftInt64.NearbyToken),
            (BinaryOperation.BitwiseOr, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value | rightInt64.Value, leftInt64.NearbyToken),

            (BinaryOperation.BitwiseXor, NumberNode leftNumber, NumberNode rightNumber) =>
                GetBitwiseNumberResult((long)leftNumber.Value ^ (long)rightNumber.Value, leftNumber.NearbyToken),
            (BinaryOperation.BitwiseXor, NumberNode leftInt64, Int64Node rightInt64) =>
                new Int64Node((long)leftInt64.Value ^ rightInt64.Value, leftInt64.NearbyToken),
            (BinaryOperation.BitwiseXor, Int64Node leftInt64, NumberNode rightNumber) =>
                new Int64Node(leftInt64.Value ^ (long)rightNumber.Value, leftInt64.NearbyToken),
            (BinaryOperation.BitwiseXor, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(leftInt64.Value ^ rightInt64.Value, leftInt64.NearbyToken),

            // Small note for << and >>: there is an upper bound on these, but not a lower bound
            (BinaryOperation.BitwiseShiftLeft, NumberNode leftNumber, NumberNode rightNumber) =>
                GetBitwiseNumberResult(((int)rightNumber.Value >= 64) ? 0 : ((long)leftNumber.Value << (int)rightNumber.Value), leftNumber.NearbyToken),
            (BinaryOperation.BitwiseShiftLeft, NumberNode leftNumber, Int64Node rightInt64) =>
                GetBitwiseNumberResult(((int)rightInt64.Value >= 64) ? 0 : ((long)leftNumber.Value << (int)rightInt64.Value), leftNumber.NearbyToken),
            (BinaryOperation.BitwiseShiftLeft, Int64Node leftInt64, NumberNode rightNumber) =>
                new Int64Node(((int)rightNumber.Value >= 64) ? 0 : (leftInt64.Value << (int)rightNumber.Value), leftInt64.NearbyToken),
            (BinaryOperation.BitwiseShiftLeft, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(((int)rightInt64.Value >= 64) ? 0 : (leftInt64.Value << (int)rightInt64.Value), leftInt64.NearbyToken),

            (BinaryOperation.BitwiseShiftRight, NumberNode leftNumber, NumberNode rightNumber) =>
                GetBitwiseNumberResult(((int)rightNumber.Value >= 64) ? 0 : ((long)leftNumber.Value >> (int)rightNumber.Value), leftNumber.NearbyToken),
            (BinaryOperation.BitwiseShiftRight, NumberNode leftNumber, Int64Node rightInt64) =>
                GetBitwiseNumberResult(((int)rightInt64.Value >= 64) ? 0 : ((long)leftNumber.Value >> (int)rightInt64.Value), leftNumber.NearbyToken),
            (BinaryOperation.BitwiseShiftRight, Int64Node leftInt64, NumberNode rightNumber) =>
                new Int64Node(((int)rightNumber.Value >= 64) ? 0 : (leftInt64.Value >> (int)rightNumber.Value), leftInt64.NearbyToken),
            (BinaryOperation.BitwiseShiftRight, Int64Node leftInt64, Int64Node rightInt64) =>
                new Int64Node(((int)rightInt64.Value >= 64) ? 0 : (leftInt64.Value >> (int)rightInt64.Value), leftInt64.NearbyToken),

            _ => null
        };
    }

    /// <summary>
    /// Checks for division by zero with a 64-bit integer and returns the divisor.
    /// </summary>
    private static long CheckDivisionByZero(ParseContext context, IConstantASTNode node, long number)
    {
        if (number == 0)
        {
            context.CompileContext.PushError("Division by zero", node.NearbyToken);
            return 1;
        }
        return number;
    }

    /// <summary>
    /// Compares two constants and returns either true or false as a <see cref="BooleanNode"/>.
    /// </summary>
    private static BooleanNode? CompareConstants(BinaryOperation operation, IConstantASTNode left, IConstantASTNode right)
    {
        // Calculate a difference between the left/right constants
        (bool success, double difference) = (left, right) switch
        {
            (NumberNode { Value: double leftNumber }, NumberNode { Value: double rightNumber }) =>
                (true, leftNumber - rightNumber),
            (NumberNode { Value: double leftNumber }, Int64Node { Value: long rightInt64 }) =>
                (true, leftNumber - rightInt64),
            (Int64Node { Value: long leftInt64 }, NumberNode { Value: double rightNumber }) =>
                (true, leftInt64 - rightNumber),
            (Int64Node { Value: long leftInt64 }, Int64Node { Value: long rightInt64 }) =>
                (true, leftInt64 - rightInt64),
            (StringNode { Value: string leftString }, StringNode { Value: string rightString }) =>
                (true, string.Compare(leftString, rightString)),
            _ => (false, 0)
        };

        if (!success)
        {
            return null;
        }

        // If a difference could be calculated, perform actual operation
        return operation switch
        {
            BinaryOperation.CompareEqual => new BooleanNode(difference == 0, left.NearbyToken),
            BinaryOperation.CompareNotEqual => new BooleanNode(difference != 0, left.NearbyToken),
            BinaryOperation.CompareLesser => new BooleanNode(difference < 0, left.NearbyToken),
            BinaryOperation.CompareLesserEqual => new BooleanNode(difference <= 0, left.NearbyToken),
            BinaryOperation.CompareGreater => new BooleanNode(difference > 0, left.NearbyToken),
            BinaryOperation.CompareGreaterEqual => new BooleanNode(difference >= 0, left.NearbyToken),
            _ => null
        };
    }

    /// <summary>
    /// Returns either an integer or floating point result, depending on the integer value provided.
    /// </summary>
    private static IConstantASTNode GetBitwiseNumberResult(long number, IToken? nearbyToken)
    {
        // Logic according to official compiler: output integer if highest bit of 32-bit integer is set
        if ((number & 0x80000000) != 0)
        {
            return new Int64Node(number, nearbyToken);
        }
        return new NumberNode(number, nearbyToken);
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Generate leftmost argument, and coerce depending on operation
        Arguments[0].GenerateCode(context);
        CoerceBinaryDataType(context, Operations[0]);

        // Generate short-circuit logic if relevant
        if (context.CompileContext.GameContext.UsingLogicalShortCircuit)
        {
            foreach (BinaryOperation operation in Operations)
            {
                if (operation is BinaryOperation.LogicalAnd or BinaryOperation.LogicalOr)
                {
                    GenerateShortCircuitCode(context);
                    return;
                }
            }
        }

        // Otherwise, generate generic binary operations
        for (int i = 1; i < Arguments.Count; i++)
        {
            // Generate current argument, and coerce depending on operation
            BinaryOperation currentOperation = Operations[i - 1];
            Arguments[i].GenerateCode(context);
            CoerceBinaryDataType(context, currentOperation);

            // Get biased data type to push to type stack
            DataType rightType = context.PopDataType();
            DataType leftType = context.PopDataType();
            DataType biasedType = leftType.BiasWith(rightType);

            // Generate instruction
            switch (currentOperation)
            {
                case BinaryOperation.Add:
                    context.Emit(Opcode.Add, rightType, leftType);
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.Subtract:
                    context.Emit(Opcode.Subtract, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.Multiply:
                    context.Emit(Opcode.Multiply, rightType, leftType);
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.Divide:
                    context.Emit(Opcode.Divide, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.GMLModulo:
                    context.Emit(Opcode.GMLModulo, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.GMLDivRemainder:
                    context.Emit(Opcode.GMLDivRemainder, rightType, leftType);
                    if ((rightType == DataType.String && leftType != DataType.Variable) || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.CompareLesser:
                    context.Emit(Opcode.Compare, ComparisonType.LesserThan, rightType, leftType);
                    context.PushDataType(DataType.Boolean);
                    break;
                case BinaryOperation.CompareLesserEqual:
                    context.Emit(Opcode.Compare, ComparisonType.LesserEqualThan, rightType, leftType);
                    context.PushDataType(DataType.Boolean);
                    break;
                case BinaryOperation.CompareEqual:
                    context.Emit(Opcode.Compare, ComparisonType.EqualTo, rightType, leftType);
                    context.PushDataType(DataType.Boolean);
                    break;
                case BinaryOperation.CompareNotEqual:
                    context.Emit(Opcode.Compare, ComparisonType.NotEqualTo, rightType, leftType);
                    context.PushDataType(DataType.Boolean);
                    break;
                case BinaryOperation.CompareGreater:
                    context.Emit(Opcode.Compare, ComparisonType.GreaterThan, rightType, leftType);
                    context.PushDataType(DataType.Boolean);
                    break;
                case BinaryOperation.CompareGreaterEqual:
                    context.Emit(Opcode.Compare, ComparisonType.GreaterEqualThan, rightType, leftType);
                    context.PushDataType(DataType.Boolean);
                    break;
                case BinaryOperation.LogicalAnd:
                case BinaryOperation.BitwiseAnd:
                    context.Emit(Opcode.And, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.LogicalOr:
                case BinaryOperation.BitwiseOr:
                    context.Emit(Opcode.Or, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.LogicalXor:
                case BinaryOperation.BitwiseXor:
                    context.Emit(Opcode.Xor, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.BitwiseShiftLeft:
                    context.Emit(Opcode.ShiftLeft, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
                case BinaryOperation.BitwiseShiftRight:
                    context.Emit(Opcode.ShiftRight, rightType, leftType);
                    if (rightType == DataType.String || leftType == DataType.String)
                    {
                        biasedType = DataType.Double;
                    }
                    context.PushDataType(biasedType);
                    break;
            }
        }
    }

    /// <summary>
    /// Generates short-circuit logic for logical AND/OR operations.
    /// </summary>
    private void GenerateShortCircuitCode(BytecodeContext context)
    {
        // Branches for false short-circuit, true short-circuit, and no short-circuit
        MultiForwardBranchPatch falseShortCircuitPatch = new();
        MultiForwardBranchPatch trueShortCircuitPatch = new();
        MultiForwardBranchPatch noShortCircuitPatch = new();

        // Generate short-circuit chain
        for (int i = 1; i < Arguments.Count; i++)
        {
            // Convert previous type in chain to a boolean
            context.ConvertDataType(DataType.Boolean);

            // Branch differently depending on operation with current argument and previous argument
            if (Operations[i - 1] == BinaryOperation.LogicalAnd)
            {
                falseShortCircuitPatch.AddInstruction(context, context.Emit(Opcode.BranchFalse));
            }
            else
            {
                trueShortCircuitPatch.AddInstruction(context, context.Emit(Opcode.BranchTrue));
            }

            // Generate current argument
            Arguments[i].GenerateCode(context);
        }

        // Convert final type in chain to boolean
        context.ConvertDataType(DataType.Boolean);
        context.PushDataType(DataType.Boolean);

        // If false branch was used, generate short-circuit block for it
        if (falseShortCircuitPatch.Used)
        {
            noShortCircuitPatch.AddInstruction(context, context.Emit(Opcode.Branch));
            falseShortCircuitPatch.Patch(context);
            context.Emit(Opcode.Push, (short)0, DataType.Int16);
        }

        // If true branch was used, generate short-circuit block for it
        if (trueShortCircuitPatch.Used)
        {
            noShortCircuitPatch.AddInstruction(context, context.Emit(Opcode.Branch));
            trueShortCircuitPatch.Patch(context);
            context.Emit(Opcode.Push, (short)1, DataType.Int16);
        }

        // Branch destination if no short-circuit was performed
        noShortCircuitPatch.Patch(context);
    }

    /// <summary>
    /// Coerces the data type of a binary operation argument, depending on operation being performed.
    /// </summary>
    private static void CoerceBinaryDataType(BytecodeContext context, BinaryOperation operation)
    {
        DataType sourceType = context.PeekDataType();
        switch ((operation, sourceType))
        {
            case (BinaryOperation.Add, DataType.Boolean):
            case (BinaryOperation.Subtract, DataType.Boolean):
            case (BinaryOperation.Multiply, DataType.Boolean):
            case (BinaryOperation.GMLDivRemainder, DataType.Boolean):
            case (BinaryOperation.GMLModulo, DataType.Boolean):
                context.Emit(Opcode.Convert, DataType.Boolean, DataType.Int32);
                context.PopDataType();
                context.PushDataType(DataType.Int32);
                break;

            case (BinaryOperation.Divide, not (DataType.Double or DataType.Variable)):
                context.Emit(Opcode.Convert, sourceType, DataType.Double);
                context.PopDataType();
                context.PushDataType(DataType.Double);
                break;

            case (BinaryOperation.LogicalAnd, not DataType.Boolean):
            case (BinaryOperation.LogicalOr, not DataType.Boolean):
            case (BinaryOperation.LogicalXor, not DataType.Boolean):
                context.Emit(Opcode.Convert, sourceType, DataType.Boolean);
                context.PopDataType();
                context.PushDataType(DataType.Boolean);
                break;

            case (BinaryOperation.BitwiseShiftLeft, not DataType.Int64):
            case (BinaryOperation.BitwiseShiftRight, not DataType.Int64):
                context.Emit(Opcode.Convert, sourceType, DataType.Int64);
                context.PopDataType();
                context.PushDataType(DataType.Int64);
                break;

            case (BinaryOperation.BitwiseAnd, DataType.Variable or DataType.Double):
            case (BinaryOperation.BitwiseOr, DataType.Variable or DataType.Double):
            case (BinaryOperation.BitwiseXor, DataType.Variable or DataType.Double):
                context.Emit(Opcode.Convert, sourceType, DataType.Int64);
                context.PopDataType();
                context.PushDataType(DataType.Int64);
                break;

            case (BinaryOperation.BitwiseAnd, DataType.Boolean or DataType.String):
            case (BinaryOperation.BitwiseOr, DataType.Boolean or DataType.String):
            case (BinaryOperation.BitwiseXor, DataType.Boolean or DataType.String):
                context.Emit(Opcode.Convert, sourceType, DataType.Int32);
                context.PopDataType();
                context.PushDataType(DataType.Int32);
                break;
        }
    }
}
