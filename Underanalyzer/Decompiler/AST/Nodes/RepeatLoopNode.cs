namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a repeat loop in the AST.
/// </summary>
public class RepeatLoopNode : IStatementNode
{
    /// <summary>
    /// The number of times the loop repeats.
    /// </summary>
    public IExpressionNode TimesToRepeat { get; private set; }

    /// <summary>
    /// The main block of the loop.
    /// </summary>
    public BlockNode Body { get; private set; }

    public bool SemicolonAfter => false;
    public bool EmptyLineBefore { get; private set; }
    public bool EmptyLineAfter { get; private set; }

    public RepeatLoopNode(IExpressionNode timesToRepeat, BlockNode body)
    {
        TimesToRepeat = timesToRepeat;
        Body = body;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        TimesToRepeat = TimesToRepeat.Clean(cleaner);
        ElseToContinueCleanup.Clean(cleaner, Body);
        Body.Clean(cleaner);

        EmptyLineAfter = EmptyLineBefore = cleaner.Context.Settings.EmptyLineAroundBranchStatements;

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("repeat (");
        TimesToRepeat.Print(printer);
        printer.Write(')');
        if (printer.Context.Settings.OpenBlockBraceOnSameLine)
        {
            printer.Write(' ');
        }
        Body.Print(printer);
    }
}
