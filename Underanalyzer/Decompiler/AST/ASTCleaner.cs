namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Manages cleaning/postprocessing of the AST.
/// </summary>
public class ASTCleaner
{
    /// <summary>
    /// The decompilation context this is cleaning for.
    /// </summary>
    public DecompileContext Context { get; }

    public ASTCleaner(DecompileContext context)
    {
        Context = context;
    }
}
