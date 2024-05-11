namespace Underanalyzer.Decompiler.AST;

public class BreakNode : IStatementNode
{
    public bool SemicolonAfter { get => true; }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("break");
    }
}
