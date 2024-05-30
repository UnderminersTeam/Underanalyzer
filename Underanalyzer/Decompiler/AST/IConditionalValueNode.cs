namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Interface for all AST nodes that can be used in conditional comparisons.
/// </summary>
public interface IConditionalValueNode : IMacroResolvableNode
{
    public string ConditionalTypeName { get; }
    public string ConditionalValue { get; }
}
