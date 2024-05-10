namespace Underanalyzer.Decompiler.AST;

public interface IExpressionNode : IASTNode
{
    /// <summary>
    /// If true, this node was duplicated during simulation.
    /// </summary>
    public bool Duplicated { get; internal set; }

    /// <summary>
    /// The data type assigned to this node on the simulated VM stack.
    /// </summary>
    public IGMInstruction.DataType StackType { get; internal set; }
}
