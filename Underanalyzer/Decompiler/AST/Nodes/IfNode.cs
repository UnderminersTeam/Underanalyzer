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
    public IExpressionNode Condition { get; private set; }

    /// <summary>
    /// The main (true) block of the if statement.
    /// </summary>
    public BlockNode TrueBlock { get; private set; }

    /// <summary>
    /// The else (false) block of the if statement, or null if none exists.
    /// </summary>
    public BlockNode ElseBlock { get; private set; }

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
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("if (");
        Condition.Print(printer);
        printer.Write(')');
        TrueBlock.Print(printer);
        if (ElseBlock is not null)
        {
            printer.StartLine();
            printer.Write("else");
            if (ElseBlock is { Children: [IfNode elseIf] })
            {
                printer.Write(' ');
                elseIf.Print(printer);
            }
            else
            {
                ElseBlock.Print(printer);
            }
        }
    }
}
