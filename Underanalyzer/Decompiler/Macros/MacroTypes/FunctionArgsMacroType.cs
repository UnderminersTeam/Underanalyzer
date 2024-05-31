using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Macro type that matches an array of macro types to a function call.
/// </summary>
public class FunctionArgsMacroType : IMacroType, IMacroTypeFunctionArgs
{
    private List<IMacroType> Types { get; }

    public FunctionArgsMacroType(IEnumerable<IMacroType> types)
    {
        Types = new(types);
    }

    /// <summary>
    /// Resolves this macro type for a given function call in the AST.
    /// </summary>
    public FunctionCallNode Resolve(ASTCleaner cleaner, FunctionCallNode call)
    {
        if (Types.Count != call.Arguments.Count)
        {
            return null;
        }

        bool didAnything = false;

        List<IExpressionNode> resolved = new(Types.Count);
        for (int i = 0; i < call.Arguments.Count; i++)
        {
            if (Types[i] is null || call.Arguments[i] is not IMacroResolvableNode node)
            {
                // Current type is not defined, or current argument is not resolvable, so just use existing argument
                resolved.Add(call.Arguments[i]);
                continue;
            }

            if (node.ResolveMacroType(cleaner, Types[i]) is not IExpressionNode nodeResolved)
            {
                // Failed to resolve current argument's macro type.
                // If the type is a conditional which is required in this scope, then fail this resolution;
                // otherwise, use existing argument.
                if (Types[i] is IMacroTypeConditional conditional && conditional.Required)
                {
                    return null;
                }
                resolved.Add(call.Arguments[i]);
                continue;
            }

            // Add to resolved arguments list
            resolved.Add(nodeResolved);
            didAnything = true;
        }

        if (!didAnything)
        {
            return null;
        }

        // Assign resolved values to arguments
        for (int i = 0; i < resolved.Count; i++)
        {
            call.Arguments[i] = resolved[i];
        }

        return call;
    }
}
