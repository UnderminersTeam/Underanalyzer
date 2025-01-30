﻿/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.Compiler.Bytecode.BytecodeContext;
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
        // Generate push for left side, and convert it to an instance ID
        LeftExpression.GenerateCode(context);
        context.ConvertToInstanceId();

        // Emit instruction to push (and push data type)
        VariablePatch varPatch = new(VariableName, InstanceType.Self, VariableType.StackTop, BuiltinVariable is not null);
        context.Emit(Opcode.Push, varPatch, DataType.Variable);
        context.PushDataType(DataType.Variable);
    }

    /// <inheritdoc/>
    public void GenerateAssignCode(BytecodeContext context)
    {
        // Generate push for left side, and convert it to an instance ID
        LeftExpression.GenerateCode(context);
        context.ConvertToInstanceId();

        // Simple variable store
        VariablePatch varPatch = new(VariableName, InstanceType.Self, VariableType.StackTop, BuiltinVariable is not null);
        context.Emit(Opcode.Pop, varPatch, DataType.Variable, context.PopDataType());
    }

    /// <inheritdoc/>
    public void GenerateCompoundAssignCode(BytecodeContext context, IASTNode expression, Opcode operationOpcode)
    {
        // Generate push for left side, and convert it to an instance ID
        LeftExpression.GenerateCode(context);
        InstanceConversionType conversionType = context.ConvertToInstanceId();

        // Duplicate instance ID so it can be stored back to later
        if (conversionType == InstanceConversionType.StacktopId)
        {
            // 32-bit integer AND actual instance (as variable type).
            // 4 bytes for int32, plus 16 bytes for RValue.
            context.EmitDuplicate(DataType.Int32, 4);
        }
        else
        {
            // Just a 32-bit integer to duplicate
            context.EmitDuplicate(DataType.Int32, 0);
        }

        // Emit instruction to push
        VariablePatch varPatch = new(VariableName, InstanceType.Self, VariableType.StackTop, BuiltinVariable is not null);
        context.Emit(Opcode.Push, varPatch, DataType.Variable);

        // Push the expression
        expression.GenerateCode(context);

        // Perform operation
        AssignNode.PerformCompoundOperation(context, operationOpcode);

        // Simple variable store, but denote pop order using data types
        context.Emit(Opcode.Pop, varPatch, DataType.Int32, DataType.Variable);
    }

    /// <summary>
    /// Helper function to duplicate a pre/post-increment/decrement value, and swap around the stack.
    /// </summary>
    private static void PrePostDuplicateAndSwap(BytecodeContext context, InstanceConversionType conversionType)
    {
        // Duplicate value
        context.EmitDuplicate(DataType.Variable, 0);
        context.PushDataType(DataType.Variable);

        // Swap around stack to prepare for pop
        if (context.CompileContext.GameContext.UsingGMLv2)
        {
            if (conversionType == InstanceConversionType.StacktopId)
            {
                // Extra 16 bytes for RValue being referenced
                context.EmitDupSwap(DataType.Int32, 4, 9);
            }
            else
            {
                // No stacktop RValue (just an instance ID)
                context.EmitDupSwap(DataType.Int32, 4, 5);
            }
        }
        else
        {
            // Pre-GMLv2 swap operation
            context.EmitPopSwap(5);
        }
    }

    /// <inheritdoc/>
    public void GeneratePrePostAssignCode(BytecodeContext context, bool isIncrement, bool isPre, bool isStatement)
    {
        // Generate push for left side, and convert it to an instance ID
        LeftExpression.GenerateCode(context);
        InstanceConversionType conversionType = context.ConvertToInstanceId();

        // Duplicate instance ID so it can be stored back to later
        if (conversionType == InstanceConversionType.StacktopId)
        {
            // 32-bit integer AND actual instance (as variable type).
            // 4 bytes for int32, plus 16 bytes for RValue.
            context.EmitDuplicate(DataType.Int32, 4);
        }
        else
        {
            // Just a 32-bit integer to duplicate
            context.EmitDuplicate(DataType.Int32, 0);
        }

        // Emit instruction to push
        VariablePatch varPatch = new(VariableName, InstanceType.Self, VariableType.StackTop, BuiltinVariable is not null);
        context.Emit(Opcode.Push, varPatch, DataType.Variable);

        // Postfix expression: duplicate original value, and swap stack around for pop
        if (!isStatement && !isPre)
        {
            PrePostDuplicateAndSwap(context, conversionType);
        }

        // Push the expression
        context.Emit(Opcode.Push, (short)1, DataType.Int16);

        // Perform operation
        context.Emit(isIncrement ? Opcode.Add : Opcode.Subtract, DataType.Int32, DataType.Variable);

        // Prefix expression: duplicate new value, and swap stack around for pop
        if (!isStatement && isPre)
        {
            PrePostDuplicateAndSwap(context, conversionType);
        }

        // Simple variable store, but denote pop order using data types
        context.Emit(Opcode.Pop, varPatch, DataType.Int32, DataType.Variable);
    }
}
