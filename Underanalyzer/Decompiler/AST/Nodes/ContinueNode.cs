namespace Underanalyzer.Decompiler.AST;

public class ContinueNode : IStatementNode
{
    public bool SemicolonAfter => true;
    public bool EmptyLineBefore => false;
    public bool EmptyLineAfter => false;

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("continue");
    }
}
