/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Reflection.Emit;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents an accessor/array index in the AST.
/// </summary>
internal sealed class AccessorNode : IAssignableASTNode
{
    /// <summary>
    /// Expression being accessed/indexed by this accessor.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Kind of accessor.
    /// </summary>
    public AccessorKind Kind { get; }

    /// <summary>
    /// Expression inside of the accessor itself.
    /// </summary>
    public IASTNode AccessorExpression { get; private set; }

    /// <summary>
    /// Second expression inside of the accessor itself, if applicable.
    /// </summary>
    public IASTNode? AccessorExpression2 { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Kinds of accessors.
    /// </summary>
    public enum AccessorKind
    {
        Array,
        ArrayDirect,
        List,
        Map,
        Grid,
        Struct
    }

    public AccessorNode(IToken? nearbyToken, IASTNode expression, AccessorKind kind, IASTNode accessorExpression, IASTNode? accessorExpression2 = null)
    {
        NearbyToken = nearbyToken;
        Expression = expression;
        Kind = kind;
        AccessorExpression = accessorExpression;
        AccessorExpression2 = accessorExpression2;
    }

    /// <summary>
    /// Creates and parses an accessor node, given the provided expression and accessor kind.
    /// </summary>
    public static AccessorNode? Parse(ParseContext context, TokenSeparator token, IASTNode expression, AccessorKind kind)
    {
        // Strings are not allowed for these specific accessor kinds
        bool disallowStrings = kind is AccessorKind.Array or AccessorKind.ArrayDirect or
                                       AccessorKind.List or AccessorKind.Grid;

        // Parse the main accessor expression
        if (Expressions.ParseExpression(context) is not IASTNode accessorExpression)
        {
            return null;
        }
        if (disallowStrings && accessorExpression is StringNode)
        {
            context.CompileContext.PushError("String used in accessor that does not support strings", accessorExpression.NearbyToken);
        }

        // Parse 2D array / grid secondary accessor expression
        IASTNode? accessorExpression2 = null;
        if (kind is AccessorKind.Array or AccessorKind.Grid && context.IsCurrentToken(SeparatorKind.Comma))
        {
            context.Position++;
            accessorExpression2 = Expressions.ParseExpression(context);
            if (accessorExpression2 is null)
            {
                return null;
            }
            if (disallowStrings && accessorExpression2 is StringNode)
            {
                context.CompileContext.PushError("String used in accessor that does not support strings", accessorExpression2.NearbyToken);
            }
        }
        else if (kind is AccessorKind.Grid)
        {
            context.CompileContext.PushError("Expected two arguments to grid accessor", token);
        }

        // All accessors end in "]"
        context.EnsureToken(SeparatorKind.ArrayClose);

        // Create final node
        return new AccessorNode(token, expression, kind, accessorExpression, accessorExpression2);
    }

    /// <summary>
    /// If this accessor node is for a 2D array, this converts the comma syntax to two separate accessors.
    /// </summary>
    public AccessorNode Convert2DArrayToTwoAccessors()
    {
        if (Kind is AccessorKind.Array && AccessorExpression2 is IASTNode expression2)
        {
            AccessorExpression2 = null;
            return new AccessorNode(NearbyToken, this, Kind, expression2);
        }
        return this;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Expression = Expression.PostProcess(context);
        AccessorExpression = AccessorExpression.PostProcess(context);
        AccessorExpression2 = AccessorExpression2?.PostProcess(context);

        // TODO: perform post-processing to convert all non-Array accessors to function calls

        return this;
    }

    /// <summary>
    /// Generates common code for generating array accessors on <see cref="IVariableASTNode"/> expressions.
    /// </summary>
    /// <returns>
    /// The <see cref="InstanceType"/> to use for the corresponding <see cref="Opcode.Push"/> or <see cref="Opcode.Pop"/>.
    /// </returns>
    private InstanceType GenerateVariableCode(BytecodeContext context, IVariableASTNode variable)
    {
        // Generate instance code, and determine instance type to use for pushing/popping
        InstanceType instanceType;
        if (variable is SimpleVariableNode simpleVariable)
        {
            // Generate instance type
            NumberNode.GenerateCode(context, (int)simpleVariable.ExplicitInstanceType);
            context.ConvertToInstanceId();

            // In GMLv2, instance type is always Self. Otherwise, use variable's type.
            if (context.CompileContext.GameContext.UsingGMLv2)
            {
                instanceType = InstanceType.Self;
            }
            else
            {
                instanceType = simpleVariable.ExplicitInstanceType;
            }
        }
        else if (variable is DotVariableNode dotVariable)
        {
            // Generate instance on left side of dot, and convert to instance ID
            dotVariable.LeftExpression.GenerateCode(context);
            context.ConvertToInstanceId();

            // Self instance type is always used for stacktop
            instanceType = InstanceType.Self;
        }
        else
        {
            throw new InvalidOperationException();
        }

        // Generate array index
        AccessorExpression.GenerateCode(context);
        context.ConvertDataType(DataType.Int32);

        return instanceType;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Generate differently depending on expression
        if (Expression is IVariableASTNode variable)
        {
            // Generate common code to prepare for push
            InstanceType pushInstanceType = GenerateVariableCode(context, variable);

            // Simple variable push
            VariablePatch varPatch = new(variable.VariableName, pushInstanceType, VariableType.Array, variable.BuiltinVariable is not null);
            context.Emit(Opcode.Push, varPatch, DataType.Variable);
            context.PushDataType(DataType.Variable);
        }
        else
        {
            // TODO: multiple accessors chained, and function calls
        }
    }

    /// <inheritdoc/>
    public void GenerateAssignCode(BytecodeContext context)
    {
        // In GMLv2, expression being assigned is converted to a variable type
        DataType storeType = context.PopDataType();
        if (storeType != DataType.Variable && context.CompileContext.GameContext.UsingGMLv2)
        {
            context.Emit(Opcode.Convert, storeType, DataType.Variable);
            storeType = DataType.Variable;
        }

        // Generate differently depending on expression
        if (Expression is IVariableASTNode variable)
        {
            // Generate common code to prepare for pop
            InstanceType popInstanceType = GenerateVariableCode(context, variable);

            // Simple variable store
            VariablePatch varPatch = new(variable.VariableName, popInstanceType, VariableType.Array, variable.BuiltinVariable is not null);
            context.Emit(Opcode.Pop, varPatch, DataType.Variable, storeType);
        }
        else
        {
            // TODO: multiple accessors chained, and function calls
        }
    }
}
