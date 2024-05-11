namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a switch case in the AST.
/// </summary>
public class SwitchCaseNode : IStatementNode
{
    /// <summary>
    /// The case expression, or null if default.
    /// </summary>
    public IExpressionNode Expression { get; private set; }

    public bool SemicolonAfter { get => false; }

    public SwitchCaseNode(IExpressionNode expression)
    {
        Expression = expression;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Expression = Expression?.Clean(cleaner);
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Expression is not null)
        {
            printer.Write("case ");
            Expression.Print(printer);
            printer.Write(':');
        }
        else
        {
            printer.Write("default:");
        }
    }
}
