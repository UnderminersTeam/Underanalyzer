using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Interface for all nodes that can be (in whole or in part) resolved as a specific macro type.
/// When anything at all is resolved, a new copy of the entire node is returned with the resolutions.
/// </summary>
public interface IMacroResolvableNode : IExpressionNode
{
    /// <summary>
    /// Returns the node, but with macros resolved using the given macro type.
    /// If any modifications are made, this should return a reference; otherwise, null.
    /// </summary>
    public IExpressionNode ResolveMacroType(ASTCleaner cleaner, IMacroType type);
}
