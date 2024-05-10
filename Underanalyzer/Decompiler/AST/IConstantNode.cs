namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base node for values that have a constant value.
/// </summary>
public interface IConstantNode<T> : IExpressionNode
{
    /// <summary>
    /// The constant value of this node.
    /// </summary>
    public T Value { get; }
}
