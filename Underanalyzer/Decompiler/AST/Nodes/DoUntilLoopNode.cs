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

    public bool SemicolonAfter => true;
    public bool EmptyLineBefore { get; private set; }
    public bool EmptyLineAfter { get; private set; }

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

        EmptyLineAfter = EmptyLineBefore = cleaner.Context.Settings.EmptyLineAroundBranchStatements;

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("do");
        if (printer.Context.Settings.OpenBlockBraceOnSameLine)
        {
            printer.Write(' ');
            Body.Print(printer);
            printer.Write(' ');
        }
        else
        {
            Body.Print(printer);
            printer.EndLine();
            printer.StartLine();
        }
        printer.Write("until (");
        Condition.Print(printer);
        printer.Write(')');
    }
}
