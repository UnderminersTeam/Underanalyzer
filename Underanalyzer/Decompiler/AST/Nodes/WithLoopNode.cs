namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a with loop in the AST.
/// </summary>
public class WithLoopNode : IStatementNode
{
    /// <summary>
    /// The target of the with loop (object/instance).
    /// </summary>
    public IExpressionNode Target { get; private set; }

    /// <summary>
    /// The main block of the loop.
    /// </summary>
    public BlockNode Body { get; private set; }

    public bool SemicolonAfter { get => false; }

    public WithLoopNode(IExpressionNode target, BlockNode body)
    {
        Target = target;
        Body = body;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Target = Target.Clean(cleaner);
        Body.Clean(cleaner);
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("with (");
        Target.Print(printer);
        printer.Write(')');
        Body.Print(printer);
    }
}
