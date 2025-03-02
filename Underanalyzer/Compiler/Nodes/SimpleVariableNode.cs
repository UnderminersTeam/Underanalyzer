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

    /// <summary>
    /// Whether this is a variable node currently on the leftmost side of a <see cref="DotVariableNode"/>.
    /// </summary>
    public bool LeftmostSideOfDot { get; set; } = false;

    /// <summary>
    /// Whether this is a variable node that is assigned to as part of a struct instantiation.
    /// </summary>
    public bool StructVariable { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; init; }

    // Set of built-in argument variables
    private static readonly HashSet<string> _builtinArgumentVariables =
    [
        "argument0", "argument1", "argument2", "argument3",
        "argument4", "argument5", "argument6", "argument7",
        "argument8", "argument9", "argument10", "argument11",
        "argument12", "argument13", "argument14", "argument15"
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
    /// Creates a simple variable reference node, given the provided name, builtin variable, and instance type.
    /// </summary>
    public SimpleVariableNode(string variableName, IBuiltinVariable? builtinVariable, InstanceType explicitInstanceType)
    {
        VariableName = variableName;
        BuiltinVariable = builtinVariable;
        SetExplicitInstanceType(explicitInstanceType);
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
    public IASTNode Duplicate(ParseContext context)
    {
        return new SimpleVariableNode(VariableName, BuiltinVariable)
        {
            ExplicitInstanceType = ExplicitInstanceType,
            HasExplicitInstanceType = HasExplicitInstanceType,
            IsFunctionCall = IsFunctionCall,
            CollapsedFromDot = CollapsedFromDot,
            LeftmostSideOfDot = LeftmostSideOfDot,
            NearbyToken = NearbyToken
        };
    }

    /// <summary>
    /// Creates a variable patch for this simple variable node.
    /// </summary>
    private VariablePatch CreateVariablePatch(BytecodeContext context)
    {
        VariablePatch varPatch = new(VariableName, ExplicitInstanceType, VariableType.Normal, BuiltinVariable is not null);

        if (ExplicitInstanceType == InstanceType.Self && !StructVariable)
        {
            // Change instruction encoding to builtin (weird compiler quirk), when either a function call,
            // or in newer GML versions when not on the RHS of a dot variable.
            if (IsFunctionCall || (!CollapsedFromDot && context.CompileContext.GameContext.UsingSelfToBuiltin))
            {
                varPatch.InstructionInstanceType = InstanceType.Builtin;
            }
        }

        return varPatch;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Check if this is a function and generate code accordingly
        if (ExplicitInstanceType == InstanceType.Self && !CollapsedFromDot && 
            (context.IsFunctionDeclaredInCurrentScope(VariableName) || context.IsGlobalFunctionName(VariableName) ||
             context.CompileContext.GameContext.GetScriptId(VariableName, out _)))
        {
            if (context.CompileContext.GameContext.UsingAssetReferences)
            {
                if (!LeftmostSideOfDot && 
                    context.CompileContext.ScriptKind == CompileScriptKind.GlobalScript && 
                    context.CurrentScope == context.RootScope && 
                    context.CurrentScope.IsFunctionDeclared(VariableName))
                {
                    // Push script reference (for local functions inside of root scope in global scripts)
                    context.Emit(ExtendedOpcode.PushReference, new LocalFunctionPatch(null, context.CurrentScope, VariableName));
                    context.PushDataType(DataType.Variable);
                }
                else if (!LeftmostSideOfDot && 
                        (context.CompileContext.GameContext.GetScriptIdByFunctionName(VariableName, out int assetId) &&
                         context.CompileContext.GameContext.GetScriptId(VariableName, out _)))
                {
                    // Push script reference (for existing global functions with scripts associated with them)
                    context.Emit(ExtendedOpcode.PushReference, assetId);
                    context.PushDataType(DataType.Variable);
                }
                else
                {
                    // Push function reference
                    context.EmitPushFunction(new FunctionPatch(context.CurrentScope, VariableName,
                                                               context.CompileContext.GameContext.Builtins.LookupBuiltinFunction(VariableName)));
                    context.PushDataType(DataType.Int32);
                }
            }
            else if (context.CompileContext.GameContext.UsingGMLv2)
            {
                // Push function reference
                context.EmitPushFunction(new FunctionPatch(context.CurrentScope, VariableName, 
                                                           context.CompileContext.GameContext.Builtins.LookupBuiltinFunction(VariableName)));
                context.PushDataType(DataType.Int32);
            }
            else
            {
                // Push script ID
                if (context.CompileContext.GameContext.GetScriptId(VariableName, out int scriptId))
                {
                    NumberNode.GenerateCode(context, scriptId);
                }
                else
                {
                    context.CompileContext.PushError($"Failed to find script with name \"{VariableName}\" (note: cannot use built-in functions directly in this GameMaker version)", NearbyToken);
                    context.PushDataType(DataType.Int32);
                }
            }

            // If leftmost side of dot, generate static_get call
            if (context.CompileContext.GameContext.UsingGMLv2 && LeftmostSideOfDot)
            {
                context.ConvertDataType(DataType.Variable);
                context.EmitCall(FunctionPatch.FromBuiltin(context, VMConstants.StaticGetFunction), 1);
                context.PushDataType(DataType.Variable);
            }
            return;
        }

        // Get correct opcode to generate
        Opcode opcode = ExplicitInstanceType switch
        {
            InstanceType.Local =>   Opcode.PushLocal,
            InstanceType.Global =>  Opcode.PushGlobal,
            InstanceType.Builtin => Opcode.PushBuiltin,
            InstanceType.Argument => Opcode.Push,
            _ => (BuiltinVariable is null || !BuiltinVariable.IsGlobal) ? Opcode.Push : Opcode.PushBuiltin,
        };

        // Emit instruction to push (and push data type)
        context.Emit(opcode, CreateVariablePatch(context), DataType.Variable);
        context.PushDataType(DataType.Variable);
    }

    /// <inheritdoc/>
    public void GenerateAssignCode(BytecodeContext context)
    {
        // Simple variable store
        context.Emit(Opcode.Pop, CreateVariablePatch(context), DataType.Variable, context.PopDataType());
    }

    /// <inheritdoc/>
    public void GenerateCompoundAssignCode(BytecodeContext context, IASTNode expression, Opcode operationOpcode)
    {
        // Push this variable
        VariablePatch varPatch = CreateVariablePatch(context);
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
        VariablePatch varPatch = CreateVariablePatch(context);
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
                    string argName = $"argument{argumentIndex}";
                    SimpleVariableNode argVar = new(argName, context.CompileContext.GameContext.Builtins.LookupBuiltinVariable(argName));
                    argVar.SetExplicitInstanceType(InstanceType.Argument);
                    return argVar;
                }
                else
                {
                    // Arguments 16 and above use array accessors
                    const string argName = "argument";
                    SimpleVariableNode argVar = new(argName, context.CompileContext.GameContext.Builtins.LookupBuiltinVariable(argName));
                    argVar.SetExplicitInstanceType(InstanceType.Argument);
                    NumberNode argNumberNode = new(argumentIndex, NearbyToken);
                    AccessorNode accessorArgVar = new(NearbyToken, argVar, AccessorNode.AccessorKind.Array, argNumberNode);
                    return accessorArgVar;
                }
            }

            // Resolve old builtin argument variables
            if (_builtinArgumentVariables.Contains(VariableName))
            {
                SetExplicitInstanceType(InstanceType.Builtin);
                return this;
            }

            // Resolve argument array
            if (VariableName == "argument")
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
