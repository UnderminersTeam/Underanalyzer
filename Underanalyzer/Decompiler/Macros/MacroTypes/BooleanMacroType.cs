using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Macro type for booleans, in older GML versions.
/// </summary>
public class BooleanMacroType : IMacroTypeInt32
{
    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, int data)
    {
        // Ensure we don't resolve this on newer GameMaker versions where this is unnecessary
        if (cleaner.Context.GameContext.UsingTypedBooleans)
        {
            return null;
        }

        // Simply check if 0 or 1 exactly...
        return data switch
        {
            0 => new MacroValueNode("false"),
            1 => new MacroValueNode("true"),
            _ => null
        };
    }
}
