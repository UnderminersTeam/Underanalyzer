using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Conditional for matching an AST node by *not* having a type name and/or value.
/// </summary>
public class MatchNotMacroType : ConditionalMacroType
{
    /// <summary>
    /// Type name to *not* match.
    /// </summary>
    public string ConditionalTypeName { get; }

    /// <summary>
    /// Value content to *not* match, or null if none.
    /// </summary>
    public string ConditionalValue { get; }

    public MatchNotMacroType(IMacroType innerType, string typeName, string value = null) : base(innerType)
    {
        ConditionalTypeName = typeName;
        ConditionalValue = value;
    }

    public override bool EvaluateCondition(ASTCleaner cleaner, IConditionalValueNode node)
    {
        if (ConditionalValue is not null && node.ConditionalValue != ConditionalValue)
        {
            return true;
        }
        if (ConditionalTypeName is null)
        {
            return false;
        }
        return node.ConditionalTypeName != ConditionalTypeName;
    }
}
