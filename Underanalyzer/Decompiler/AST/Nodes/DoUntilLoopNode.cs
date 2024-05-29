namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a do..until loop in the AST.
/// </summary>
public class DoUntilLoopNode : IStatementNode
{
    /// <summary>
    /// The main block of the loop.
    /// </summary>
    public BlockNode Body { get; private set; }

    /// <summary>
    /// The condition of the loop.
    /// </summary>
    public IExpressionNode Condition { get; private set; }

    public bool SemicolonAfter { get => true; }

    public DoUntilLoopNode(BlockNode body, IExpressionNode condition)
    {
        Condition = condition;
        Body = body;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        ElseToContinueCleanup.Clean(cleaner, Body);
        Body.Clean(cleaner);
        Condition = Condition.Clean(cleaner);
        Condition.Group = false;

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("do");
        Body.Print(printer);
        // TODO: change depending on code style
        printer.EndLine();
        printer.StartLine();
        printer.Write("until (");
        Condition.Print(printer);
        printer.Write(')');
    }
}
