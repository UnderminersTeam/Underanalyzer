namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a static initialization block in the AST.
/// </summary>
public class StaticInitNode : IStatementNode
{
    /// <summary>
    /// The main block of the static initialization.
    /// </summary>
    public BlockNode Body { get; private set; }

    public bool SemicolonAfter { get => false; }

    public StaticInitNode(BlockNode body)
    {
        Body = body;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Body.Clean(cleaner);
        Body.UseBraces = false;
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        bool prevStaticInitState = printer.TopFragmentContext.InStaticInitialization;
        printer.TopFragmentContext.InStaticInitialization = true;

        Body.Print(printer);

        printer.TopFragmentContext.InStaticInitialization = prevStaticInitState;
    }
}
