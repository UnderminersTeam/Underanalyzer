namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Interface for nodes that need special processing in the context of a BlockNode.
/// </summary>
public interface IBlockCleanupNode
{
    /// <summary>
    /// Cleans the current node within the context of a block, at the specified index.
    /// Returns the new index to continue cleaning from within the block.
    /// </summary>
    public int BlockClean(ASTCleaner cleaner, BlockNode block, int i);
}
