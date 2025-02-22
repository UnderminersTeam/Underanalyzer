/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents an assignment statement in the AST.
/// </summary>
internal sealed class AssignNode : IASTNode
{
    /// <summary>
    /// Expression being assigned to.
    /// </summary>
    public IAssignableASTNode Destination { get; private set; }

    /// <summary>
    /// The expression being evaluted and assigned to the destination.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Kind of assignment being performed.
    /// </summary>
    public AssignKind Kind { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken => Destination.NearbyToken;

    public enum AssignKind
    {
        Normal,
        CompoundPlus,
        CompoundMinus,
        CompoundTimes,
        CompoundDivide,
        CompoundMod,
        CompoundBitwiseAnd,
        CompoundBitwiseOr,
        CompoundBitwiseXor,
        CompoundNullishCoalesce
    }

    /// <summary>
    /// Creates an assignment node from the given destination and expression.
    /// </summary>
    public AssignNode(AssignKind kind, IAssignableASTNode destination, IASTNode expression)
    {
        Kind = kind;
        Destination = destination;
        Expression = expression;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        // TODO: if destination is an accessor that requires rewriting (e.g. function calls or non-basic array accessors),
        // rewrite it. also, if we are a compound operation, we have to recursively clone the destination without pre/post expressions
        // (so that the pre/post doesn't occur twice)

        Destination = Destination.PostProcess(context) as IAssignableASTNode ?? throw new Exception("Destination no longer assignable");
        Expression = Expression.PostProcess(context);

        // Remove variable assignments to themselves
        if (Destination is SimpleVariableNode { VariableName: string destName } && 
            Expression is SimpleVariableNode { VariableName: string exprName } &&
            destName == exprName)
        {
            return EmptyNode.Create();
        }

        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        switch (Kind)
        {
            case AssignKind.Normal:
                Expression.GenerateCode(context);
                Destination.GenerateAssignCode(context);
                break;
            case AssignKind.CompoundPlus:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.Add);
                break;
            case AssignKind.CompoundMinus:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.Subtract);
                break;
            case AssignKind.CompoundTimes:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.Multiply);
                break;
            case AssignKind.CompoundDivide:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.Divide);
                break;
            case AssignKind.CompoundMod:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.GMLModulo);
                break;
            case AssignKind.CompoundBitwiseAnd:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.And);
                break;
            case AssignKind.CompoundBitwiseOr:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.Or);
                break;
            case AssignKind.CompoundBitwiseXor:
                Destination.GenerateCompoundAssignCode(context, Expression, Opcode.Xor);
                break;
            case AssignKind.CompoundNullishCoalesce:
                // Push destination value first
                Destination.GenerateCode(context);
                context.ConvertDataType(DataType.Variable);

                // Check if nullish; branch around right side (and assignment) if not
                context.Emit(ExtendedOpcode.IsNullishValue);
                SingleForwardBranchPatch skipRightSidePatch = new(context.Emit(Opcode.BranchFalse));

                // Right side (but remove nullish result from left side first)
                context.Emit(Opcode.PopDelete, DataType.Variable);
                Expression.GenerateCode(context);
                context.ConvertDataType(DataType.Variable);

                // Assign right side, then branch around removal of non-nullish destination value
                context.PushDataType(DataType.Variable);
                Destination.GenerateAssignCode(context);
                SingleForwardBranchPatch skipDestinationPopPatch = new(context.Emit(Opcode.Branch));

                // Remove non-nullish destination value from stack
                skipRightSidePatch.Patch(context);
                context.Emit(Opcode.PopDelete, DataType.Variable);
                skipDestinationPopPatch.Patch(context);
                break;
        }
    }

    /// <summary>
    /// Performs the inner operation for a compound assignment, given its opcode.
    /// </summary>
    public static void PerformCompoundOperation(BytecodeContext context, Opcode operationOpcode)
    {
        // Pull expression type from stack
        DataType operationDataType = context.PeekDataType();
        if (operationOpcode is Opcode.And or Opcode.Or or Opcode.Xor)
        {
            // Bitwise operations convert to 64-bit integers
            context.ConvertDataType(DataType.Int64);
            operationDataType = DataType.Int64;
        }
        else if (operationDataType == DataType.Boolean)
        {
            // Booleans convert to integers
            context.Emit(Opcode.Convert, DataType.Boolean, DataType.Int32);
            context.PopDataType();
            operationDataType = DataType.Int32;
        }
        else
        {
            // Don't want operation type on stack anymore, so just remove it
            context.PopDataType();
        }

        // Perform the operation
        context.Emit(operationOpcode, operationDataType, DataType.Variable);
    }
}
