/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a function call in the AST.
/// </summary>
public class FunctionCallNode(IGMFunction function, List<IExpressionNode> arguments) 
    : IExpressionNode, IStatementNode, IMacroTypeNode, IMacroResolvableNode, IConditionalValueNode, IFunctionCallNode
{
    /// <summary>
    /// The function reference being called.
    /// </summary>
    public IGMFunction Function { get; } = function;

    /// <summary>
    /// Arguments being passed into the function call.
    /// </summary>
    public List<IExpressionNode> Arguments { get; } = arguments;

    /// <inheritdoc/>
    public bool Duplicated { get; set; } = false;

    /// <inheritdoc/>
    public bool Group { get; set; } = false;

    /// <inheritdoc/>
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    /// <inheritdoc/>
    public bool SemicolonAfter => true;

    /// <inheritdoc/>
    public bool EmptyLineBefore { get => false; set => _ = value; }

    /// <inheritdoc/>
    public bool EmptyLineAfter { get => false; set => _ = value; }

    /// <inheritdoc/>
    public string FunctionName { get => Function.Name.Content; }

    /// <inheritdoc/>
    public string ConditionalTypeName => "FunctionCall";

    /// <inheritdoc/>
    public string ConditionalValue => Function.Name.Content;

    /// <inheritdoc/>
    IExpressionNode IASTNode<IExpressionNode>.Clean(ASTCleaner cleaner)
    {
        // Clean up all arguments
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }

        // Handle special instance types and template strings
        switch (Function.Name.Content)
        {
            case VMConstants.SelfFunction:
                return new InstanceTypeNode(IGMInstruction.InstanceType.Self, true) { Duplicated = Duplicated, StackType = StackType };
            case VMConstants.OtherFunction:
                return new InstanceTypeNode(IGMInstruction.InstanceType.Other, true) { Duplicated = Duplicated, StackType = StackType };
            case VMConstants.GlobalFunction:
                return new InstanceTypeNode(IGMInstruction.InstanceType.Global, true) { Duplicated = Duplicated, StackType = StackType };
            case VMConstants.GetInstanceFunction:
                if (Arguments.Count == 0 || Arguments[0] is not (Int16Node or Int32Node))
                {
                    throw new DecompilerException($"Expected 16-bit or 32-bit integer parameter to {VMConstants.GetInstanceFunction}");
                }
                if (Arguments[0] is Int32Node int32)
                {
                    // If not using room instance references, transform into one for cleanliness.
                    // If room instance references *are* being used, this would instead already be an AssetReferenceNode.
                    if (!cleaner.Context.GameContext.UsingRoomInstanceReferences)
                    {
                        Arguments[0] = new AssetReferenceNode(int32.Value, AssetType.RoomInstance);
                    }
                }
                Arguments[0].Duplicated = true;
                Arguments[0].StackType = StackType;
                return Arguments[0];
            case VMConstants.TemplateStringFunction:
                {
                    IGameContext context = cleaner.Context.GameContext;
                    if (context.UsingTemplateStrings && !context.UsingModernTemplateStrings)
                    {
                        return CleanupTemplateString(cleaner, false);
                    }
                    break;
                }
            case VMConstants.ModernTemplateStringFunction:
                {
                    IGameContext context = cleaner.Context.GameContext;
                    if (context.UsingTemplateStrings && context.UsingModernTemplateStrings)
                    {
                        return CleanupTemplateString(cleaner, true);
                    }
                    break;
                }
        }

        return CleanupMacroTypes(cleaner);
    }

    /// <inheritdoc/>
    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        // Just clean up arguments here - special calls are only in expressions
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }

        return CleanupMacroTypes(cleaner);
    }

    /// <inheritdoc/>
    IExpressionNode IASTNode<IExpressionNode>.PostClean(ASTCleaner cleaner)
    {
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].PostClean(cleaner);
        }
        return this;
    }

    /// <inheritdoc/>
    IStatementNode IASTNode<IStatementNode>.PostClean(ASTCleaner cleaner)
    {
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].PostClean(cleaner);
        }
        return this;
    }

    /// <summary>
    /// During cleanup, determines/resolves macro types for this node if possible.
    /// </summary>
    private IFunctionCallNode CleanupMacroTypes(ASTCleaner cleaner)
    {
        string functionName = Function.Name.Content;

        if (functionName == VMConstants.ScriptExecuteFunction)
        {
            // Special case: our actual function name is the script index theoretically stored in the first argument.
            // Try finding the script/function name.
            if (Arguments is [Int16Node scriptIndexInt16, ..])
            {
                if (cleaner.Context.GameContext.GetAssetName(AssetType.Script, scriptIndexInt16.Value) is string name)
                {
                    // We found a script!
                    functionName = name;

                    // Update first argument with this name, as well, as it won't get resolved otherwise
                    Arguments[0] = new MacroValueNode(functionName);
                }
            }
            else if (Arguments is [FunctionReferenceNode functionReference, ..])
            {
                // We found a function!
                functionName = functionReference.Function.Name.Content;
            }
            else if (Arguments is [AssetReferenceNode { AssetType: AssetType.Script } assetReference, ..])
            {
                if (cleaner.Context.GameContext.GetAssetName(AssetType.Script, assetReference.AssetId) is string name)
                {
                    // We found a script!
                    functionName = name;
                }
            }
        }

        if (cleaner.GlobalMacroResolver.ResolveFunctionArgumentTypes(cleaner, functionName) is IMacroTypeFunctionArgs argsMacroType)
        {
            if (argsMacroType.Resolve(cleaner, this) is IFunctionCallNode resolved)
            {
                // We found a match!
                return resolved;
            }
        }

        // No resolution found
        return this;
    }

    private IExpressionNode CleanupTemplateString(ASTCleaner cleaner, bool isModern)
    {
        // Don't attempt cleanup if not enabled
        if (!cleaner.Context.Settings.CleanupTemplateStrings)
        {
            return this;
        }

        // Make sure format is valid; fall back to function call and return as-is, otherwise
        if (Arguments is not [StringNode { Value: { Content: string format } formatRef }, ..])
        {
            return this;
        }
        if (Arguments.Count < 2)
        {
            return this;
        }

        // for each field, whether or not a placeholder exists
        int nextExpectedFieldIndex = 0;
        int maxExpectedFieldIndex = Arguments.Count - 2;
        for (int i = 0; i < format.Length; i++)
        {
            // Skip non-placeholder starting characters
            if (format[i] != '{')
            {
                continue;
            }

            // If the next character is the same, it can be treated as an escape on non-modern versions, and
            // we probably just want to bail to preserve behavior... (e.g. if ported to other GM versions)
            if (!isModern && (i + 1) < format.Length && format[i + 1] == '{')
            {
                return this;
            }

            // Parse the placeholder (may not necessarily be valid)
            int startIndex = i + 1;
            int j = startIndex;
            bool invalidCharacter = false;
            while (j < format.Length && format[j] != '}')
            {
                if (format[j] < '0' || format[j] > '9')
                {
                    invalidCharacter = true;
                    break;
                }
                j++;
            }
            if (invalidCharacter || j >= format.Length || j == startIndex)
            {
                continue;
            }
            ReadOnlySpan<char> placeholderNumberText = format.AsSpan()[startIndex..j];
            if (!int.TryParse(placeholderNumberText, out int fieldIndex))
            {
                continue;
            }
            if (fieldIndex < 0 || fieldIndex > maxExpectedFieldIndex || fieldIndex != nextExpectedFieldIndex)
            {
                // Totally invalid or unexpected field index... bail!
                return this;
            }

            // Move on to next field - should be in linear order for regular interpolated strings
            nextExpectedFieldIndex++;

            // Advance to the end of the placeholder
            i = j;
        }

        // If we didn't use exactly the number of expected indices, bail!
        if (nextExpectedFieldIndex != (maxExpectedFieldIndex + 1))
        {
            return this;
        }

        // No checks failed, and this seems to be a valid (possible) template string!
        Arguments.RemoveAt(0);
        return new TemplateStringNode(formatRef, Arguments);
    }

    /// <inheritdoc/>
    public void Print(ASTPrinter printer)
    {
        printer.Write(printer.LookupFunction(Function));
        printer.Write('(');
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i].Print(printer);
            if (i != Arguments.Count - 1)
            {
                printer.Write(", ");
            }
        }
        printer.Write(')');
    }

    /// <inheritdoc/>
    public bool RequiresMultipleLines(ASTPrinter printer)
    {
        foreach (IExpressionNode arg in Arguments)
        {
            if (arg.RequiresMultipleLines(printer))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public IMacroType? GetExpressionMacroType(ASTCleaner cleaner)
    {
        return cleaner.GlobalMacroResolver.ResolveReturnValueType(cleaner, Function.Name.Content);
    }

    /// <inheritdoc/>
    public IExpressionNode? ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        if (type is IMacroTypeConditional conditional)
        {
            return conditional.Resolve(cleaner, this);
        }

        // For choose(...), propagate type to all parameters
        if (Function.Name.Content == VMConstants.ChooseFunction)
        {
            bool didAnything = false;

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (Arguments[i] is IMacroResolvableNode argResolvable &&
                    argResolvable.ResolveMacroType(cleaner, type) is IExpressionNode argResolved)
                {
                    Arguments[i] = argResolved;
                    didAnything = true;
                }
            }

            return didAnything ? this : null;
        }

        return null;
    }

    /// <inheritdoc/>
    public IEnumerable<IBaseASTNode> EnumerateChildren()
    {
        return Arguments;
    }
}
