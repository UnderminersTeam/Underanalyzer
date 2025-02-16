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
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a function call in the AST, not tied to any specific variable name.
/// </summary>
internal sealed class FunctionCallNode : IMaybeStatementASTNode
{
    /// <summary>
    /// Expression being called.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Arguments being used for this function call, in order.
    /// </summary>
    public List<IASTNode> Arguments { get; }

    /// <inheritdoc/>
    public bool IsStatement { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a function call node, parsing from the given context's current position,
    /// and given the provided expression being called.
    /// </summary>
    public FunctionCallNode(ParseContext context, TokenSeparator token, IASTNode expression)
    {
        NearbyToken = token;
        Expression = expression;
        Arguments = Functions.ParseCallArguments(context, 2047 /* TODO: change based on gamemaker version? */);
    }
    
    /// <summary>
    /// Creates a function call node with the given token, expression, and arguments.
    /// </summary>
    public FunctionCallNode(IToken? nearbyToken, IASTNode expression, List<IASTNode> arguments)
    {
        NearbyToken = nearbyToken;
        Expression = expression; 
        Arguments = arguments;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Expression = Expression.PostProcess(context);
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].PostProcess(context);
        }
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        VariablePatch finalVariable;

        // Push instance
        if (Expression is SimpleVariableNode simpleVar)
        {
            // Push self/other/global (or get instance from object ID)
            string functionToCall;
            int argsToUse;
            if (simpleVar.ExplicitInstanceType >= 0)
            {
                functionToCall = VMConstants.GetInstanceFunction;
                argsToUse = 1;

                // Generate object ID as number
                NumberNode.GenerateCode(context, (int)simpleVar.ExplicitInstanceType);
                context.ConvertDataType(DataType.Variable);
            }
            else
            {
                functionToCall = simpleVar.ExplicitInstanceType switch
                {
                    InstanceType.Other =>   VMConstants.OtherFunction,
                    InstanceType.Global =>  VMConstants.GlobalFunction,
                    _ =>                    VMConstants.SelfFunction
                };
                argsToUse = 0;
            }
            IBuiltinFunction? builtinFunctionToCall =
                context.CompileContext.GameContext.Builtins.LookupBuiltinFunction(functionToCall);
            context.EmitCall(new FunctionPatch(functionToCall, builtinFunctionToCall), argsToUse);

            // Make final variable patch
            finalVariable = new(simpleVar.VariableName, InstanceType.StackTop, VariableType.Normal, simpleVar.BuiltinVariable is not null);
        }
        else if (Expression is DotVariableNode dotVar)
        {
            // Push left expression
            dotVar.LeftExpression.GenerateCode(context);

            // If conversion is necessary, run function to get single instance
            if (context.ConvertDataType(DataType.Variable))
            {
                context.EmitCall(new FunctionPatch(VMConstants.GetInstanceFunction,
                    context.CompileContext.GameContext.Builtins.LookupBuiltinFunction(VMConstants.GetInstanceFunction)), 1);
            }

            // Make final variable patch
            finalVariable = new(dotVar.VariableName, InstanceType.StackTop, VariableType.Normal, dotVar.BuiltinVariable is not null);
        }
        else
        {
            throw new NotImplementedException();
        }

        // Push arguments in reverse order (so they get popped in normal order)
        for (int i = Arguments.Count - 1; i >= 0; i--)
        {
            Arguments[i].GenerateCode(context);
            context.ConvertDataType(DataType.Variable);
        }

        // Swap instance and arguments around on stack, and duplicate instance
        context.EmitDupSwap(DataType.Variable, (byte)Arguments.Count, 1);
        context.EmitDuplicate(DataType.Variable, 0);

        // Compile final variable
        context.Emit(Opcode.Push, finalVariable, DataType.Variable);

        // Emit actual call
        context.EmitCallVariable(Arguments.Count);
        context.PushDataType(DataType.Variable);

        // If this node is a statement, remove result from stack
        if (IsStatement)
        {
            context.Emit(Opcode.PopDelete, context.PopDataType());
        }
    }
}
