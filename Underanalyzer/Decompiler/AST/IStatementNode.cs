namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base interface for all statements in the AST.
/// </summary>
public interface IStatementNode : IASTNode<IStatementNode>
{
    /// <summary>
    /// When semicolons are enabled, this is true if the statement should have a semicolon printed after it.
    /// </summary>
    public bool SemicolonAfter { get; }

    /// <summary>
    /// If true, an empty line should be printed before this node, unless at the start of a block.
    /// </summary>
    public bool EmptyLineBefore { get; }

    /// <summary>
    /// If true, an empty line should be printed after this node, unless at the end of a block.
    /// </summary>
    public bool EmptyLineAfter { get; }
}
