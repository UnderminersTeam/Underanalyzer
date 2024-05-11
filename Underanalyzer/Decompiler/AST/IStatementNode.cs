namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base interface for all statements in the AST.
/// </summary>
public interface IStatementNode : IASTNode<IStatementNode>
{
    public bool SemicolonAfter { get; }
}
