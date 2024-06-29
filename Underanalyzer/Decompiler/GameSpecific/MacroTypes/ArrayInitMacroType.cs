using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.GameSpecific;

/// <summary>
/// Macro type that matches a macro type to an inline array initialization.
/// </summary>
public class ArrayInitMacroType : IMacroTypeArrayInit
{
    public IMacroType Type { get; }

    public ArrayInitMacroType(IMacroType type)
    {
        Type = type;
    }

    /// <summary>
    /// Resolves this macro type for a given array initialization in the AST.
    /// </summary>
    public ArrayInitNode Resolve(ASTCleaner cleaner, ArrayInitNode array)
    {
        bool didAnything = false;

        for (int i = 0; i < array.Elements.Count; i++)
        {
            if (array.Elements[i] is not IMacroResolvableNode node)
            {
                // Current element is not resolvable as a macro
                continue;
            }

            if (node.ResolveMacroType(cleaner, Type) is not IExpressionNode nodeResolved)
            {
                // Failed to resolve current element's macro type
                continue;
            }

            // Update element
            array.Elements[i] = nodeResolved;
            didAnything = true;
        }

        return didAnything ? array : null;
    }
}
