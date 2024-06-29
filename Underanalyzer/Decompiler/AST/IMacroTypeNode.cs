using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Interface for nodes that can have defined macro types when used in an expression.
/// </summary>
public interface IMacroTypeNode
{
    /// <summary>
    /// Returns the macro type for this node as used in an expression, or null if none exists.
    /// </summary>
    public IMacroType GetExpressionMacroType(ASTCleaner cleaner);
}
