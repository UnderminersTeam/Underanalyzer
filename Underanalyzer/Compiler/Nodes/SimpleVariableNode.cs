/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a simple variable reference in the AST.
/// </summary>
internal sealed class SimpleVariableNode : IAssignableASTNode, IVariableASTNode
{
    /// <inheritdoc/>
    public string VariableName { get; }

    /// <inheritdoc/>
    public IBuiltinVariable? BuiltinVariable { get; }

    /// <summary>
    /// Whether this variable node has an explicit instance type set on it.
    /// </summary>
    public bool HasExplicitInstanceType { get; private set; } = false;

    /// <summary>
    /// The explicit instance type set on this variable node, if <see cref="HasExplicitInstanceType"/> is <see langword="true"/>.
    /// </summary>
    public InstanceType ExplicitInstanceType { get; private set; }

    /// <summary>
    /// Whether this is a variable node as used in a function call.
    /// </summary>
    public bool IsFunctionCall { get; set; } = false;

    /// <summary>
    /// Whether this is a variable node that was collapsed from a <see cref="DotVariableNode"/>.
    /// </summary>
    public bool CollapsedFromDot { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    // Set of built-in argument variables
    private static readonly HashSet<string> _builtinArgumentVariables =
    [
        "argument0", "argument1", "argument2", "argument3",
        "argument4", "argument5", "argument6", "argument7",
        "argument8", "argument9", "argument10", "argument11",
        "argument12", "argument13", "argument14", "argument15",
        "argument"
    ];

    /// <summary>
    /// Creates a simple variable reference node, given the provided variable token.
    /// </summary>
    public SimpleVariableNode(TokenVariable token)
    {
        NearbyToken = token;
        VariableName = token.Text;
        BuiltinVariable = token.BuiltinVariable;
    }

    /// <summary>
    /// Creates a simple variable reference node, given the provided name and builtin variable.
    /// </summary>
    public SimpleVariableNode(string variableName, IBuiltinVariable? builtinVariable)
    {
        VariableName = variableName;
        BuiltinVariable = builtinVariable;
    }

    /// <summary>
    /// Sets an explicit instance type on this variable node.
    /// </summary>
    public void SetExplicitInstanceType(InstanceType instanceType)
    {
        ExplicitInstanceType = instanceType;
        HasExplicitInstanceType = true;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        return ResolveStandaloneType(context);
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO: check if this is a function and generate code accordingly
        // will need to handle general expressions, as well as FunctionName.variable_name (which uses static_get)

        // Get correct opcode to generate
        Opcode opcode = ExplicitInstanceType switch
        {
            InstanceType.Local =>   Opcode.PushLocal,
            InstanceType.Global =>  Opcode.PushGlobal,
            InstanceType.Builtin => Opcode.PushBuiltin,
            _ => (BuiltinVariable is null || !BuiltinVariable.IsGlobal) ? Opcode.Push : Opcode.PushBuiltin,
        };

        // Determine instance type
        InstanceType finalInstanceType = ExplicitInstanceType;

        // Emit instruction to push (and push data type)
        VariablePatch varPatch = new(VariableName, finalInstanceType, VariableType.Normal, BuiltinVariable is not null);
        if (IsFunctionCall && finalInstanceType == InstanceType.Self)
        {
            // Change instruction encoding to builtin (weird compiler quirk)
            varPatch.InstructionInstanceType = InstanceType.Builtin;
        }
        context.Emit(opcode, varPatch, DataType.Variable);
        context.PushDataType(DataType.Variable);
    }

    /// <inheritdoc/>
    public void GenerateAssignCode(BytecodeContext context)
    {
        // Simple variable store
        VariablePatch varPatch = new(VariableName, ExplicitInstanceType, VariableType.Normal, BuiltinVariable is not null);
        context.Emit(Opcode.Pop, varPatch, DataType.Variable, context.PopDataType());
    }

    /// <inheritdoc/>
    public void GenerateCompoundAssignCode(BytecodeContext context, IASTNode expression, Opcode operationOpcode)
    {
        // Push this variable
        VariablePatch varPatch = new(VariableName, ExplicitInstanceType, VariableType.Normal, BuiltinVariable is not null);
        context.Emit(Opcode.Push, varPatch, DataType.Variable);

        // Push the expression
        expression.GenerateCode(context);

        // Perform operation
        AssignNode.PerformCompoundOperation(context, operationOpcode);

        // Normal assign
        context.Emit(Opcode.Pop, varPatch, DataType.Variable, DataType.Variable);
    }

    /// <inheritdoc/>
    public void GeneratePrePostAssignCode(BytecodeContext context, bool isIncrement, bool isPre, bool isStatement)
    {
        // Push this variable
        VariablePatch varPatch = new(VariableName, ExplicitInstanceType, VariableType.Normal, BuiltinVariable is not null);
        context.Emit(Opcode.Push, varPatch, DataType.Variable);

        // Postfix expression: duplicate original value
        if (!isStatement && !isPre)
        {
            context.EmitDuplicate(DataType.Variable, 0);
            context.PushDataType(DataType.Variable);
        }

        // Push the expression
        context.Emit(Opcode.Push, (short)1, DataType.Int16);

        // Perform operation
        context.Emit(isIncrement ? Opcode.Add : Opcode.Subtract, DataType.Int32, DataType.Variable);

        // Prefix expression: duplicate new value
        if (!isStatement && isPre)
        {
            context.EmitDuplicate(DataType.Variable, 0);
            context.PushDataType(DataType.Variable);
        }

        // Normal assign
        context.Emit(Opcode.Pop, varPatch, DataType.Variable, DataType.Variable);
    }

    /// <summary>
    /// Resolves the final variable type (and scope in general) for a variable, given the current context, 
    /// the variable's name, and builtin variable information.
    /// </summary>
    public IAssignableASTNode ResolveStandaloneType(ISubCompileContext context)
    {
        // If an explicit instance type has already been defined, don't do anything else
        if (HasExplicitInstanceType)
        {
            return this;
        }

        // Resolve local variables (overrides everything else)
        if (context.CurrentScope.IsLocalDeclared(VariableName))
        {
            SetExplicitInstanceType(InstanceType.Local);
            return this;
        }

        // GMLv2 has other instance types to be resolved
        if (context.CompileContext.GameContext.UsingGMLv2)
        {
            // Resolve static variables
            if (context.CurrentScope.IsStaticDeclared(VariableName))
            {
                SetExplicitInstanceType(InstanceType.Static);
                return this;
            }

            // Resolve argument names
            if (context.CurrentScope.TryGetArgumentIndex(VariableName, out int argumentIndex))
            {
                // Create new variable node altogether in this case
                if (argumentIndex < 16)
                {
                    // Arguments 0 through 15 have unique variable names
                    SimpleVariableNode argVar = new($"argument{argumentIndex}", null);
                    argVar.SetExplicitInstanceType(InstanceType.Argument);
                    return argVar;
                }
                else
                {
                    // Arguments 16 and above use array accessors
                    SimpleVariableNode argVar = new("argument", null);
                    argVar.SetExplicitInstanceType(InstanceType.Argument);
                    NumberNode argNumberNode = new(argumentIndex, NearbyToken);
                    AccessorNode accessorArgVar = new(NearbyToken, argVar, AccessorNode.AccessorKind.Array, argNumberNode);
                    return accessorArgVar;
                }
            }

            // Resolve old builtin argument variables
            if (_builtinArgumentVariables.Contains(VariableName))
            {
                SetExplicitInstanceType(InstanceType.Argument);
                return this;
            }

            // Resolve builtin variables
            if (BuiltinVariable is not null)
            {
                SetExplicitInstanceType(BuiltinVariable.IsGlobal ? InstanceType.Builtin : InstanceType.Self);
                return this;
            }
        }

        // If nothing matched, default to self
        SetExplicitInstanceType(InstanceType.Self);
        return this;
    }

    /// <summary>
    /// Creates a simple variable node for the "undefined" GML keyword, which is implemented as a variable.
    /// </summary>
    public static SimpleVariableNode CreateUndefined(ParseContext context)
    {
        return new SimpleVariableNode("undefined", context.CompileContext.GameContext.Builtins.LookupBuiltinVariable("undefined"));
    }
}
