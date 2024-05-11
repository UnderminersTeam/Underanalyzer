namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base interface for all expressions in the AST.
/// </summary>
public interface IExpressionNode : IASTNode<IExpressionNode>
{
    /// <summary>
    /// If true, this node was duplicated during simulation.
    /// </summary>
    public bool Duplicated { get; internal set; }

    /// <summary>
    /// Whether or not this expression has to be separately grouped with parentheses.
    /// </summary>
    public bool Group { get; internal set; }

    /// <summary>
    /// The data type assigned to this node on the simulated VM stack.
    /// </summary>
    public IGMInstruction.DataType StackType { get; internal set; }
}
