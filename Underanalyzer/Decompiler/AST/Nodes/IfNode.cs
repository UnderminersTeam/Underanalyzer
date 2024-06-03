using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an if statement in the AST.
/// </summary>
public class IfNode : IStatementNode
{
    /// <summary>
    /// The condition of the if statement.
    /// </summary>
    public IExpressionNode Condition { get; internal set; }

    /// <summary>
    /// The main (true) block of the if statement.
    /// </summary>
    public BlockNode TrueBlock { get; internal set; }

    /// <summary>
    /// The else (false) block of the if statement, or null if none exists.
    /// </summary>
    public BlockNode ElseBlock { get; internal set; }

    public bool SemicolonAfter => false;
    public bool EmptyLineBefore { get; private set; }
    public bool EmptyLineAfter { get; private set; }

    public IfNode(IExpressionNode condition, BlockNode trueBlock, BlockNode elseBlock = null)
    {
        Condition = condition;
        TrueBlock = trueBlock;
        ElseBlock = elseBlock;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Condition = Condition.Clean(cleaner);
        Condition.Group = false;
        TrueBlock.Clean(cleaner);
        ElseBlock?.Clean(cleaner);

        EmptyLineAfter = EmptyLineBefore = cleaner.Context.Settings.EmptyLineAroundBranchStatements;

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("if (");
        Condition.Print(printer);
        printer.Write(')');
        if (printer.Context.Settings.OpenBlockBraceOnSameLine)
        {
            printer.Write(' ');
        }
        TrueBlock.Print(printer);
        if (ElseBlock is not null)
        {
            if (printer.Context.Settings.OpenBlockBraceOnSameLine)
            {
                printer.Write(' ');
            }
            else
            {
                printer.EndLine();
                printer.StartLine();
            }
            printer.Write("else");
            if (ElseBlock is { Children: [IfNode elseIf] })
            {
                printer.Write(' ');
                elseIf.Print(printer);
            }
            else
            {
                if (printer.Context.Settings.OpenBlockBraceOnSameLine)
                {
                    printer.Write(' ');
                }
                ElseBlock.Print(printer);
            }
        }
    }
}
