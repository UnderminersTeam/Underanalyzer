﻿namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a for loop in the AST.
/// </summary>
public class ForLoopNode : IStatementNode
{
    /// <summary>
    /// The initialization statement before the loop, or null if none.
    /// </summary>
    public IStatementNode Initializer { get; private set; }

    /// <summary>
    /// The condition of the loop.
    /// </summary>
    public IExpressionNode Condition { get; private set; }

    /// <summary>
    /// The code executed between iterations of the loop.
    /// </summary>
    public BlockNode Incrementor { get; private set; }

    /// <summary>
    /// The main block of the loop.
    /// </summary>
    public BlockNode Body { get; private set; }

    public bool SemicolonAfter { get => false; }

    public ForLoopNode(IStatementNode initializer, IExpressionNode condition, BlockNode incrementor, BlockNode body)
    {
        Initializer = initializer;
        Condition = condition;
        Incrementor = incrementor;
        Body = body;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Initializer = Initializer?.Clean(cleaner);
        Condition = Condition.Clean(cleaner);
        Condition.Group = false;
        Incrementor.Clean(cleaner);
        Body.Clean(cleaner);

        IStatementNode res = this;

        // Check if we're a for (;;) loop
        if (Condition is Int64Node i64 && i64.Value == 1 && Incrementor is { Children: [] })
        {
            // We have no condition or incrementor, so rewrite this as for (;;)
            Condition = null;
            Incrementor = null;

            if (Initializer is not BlockNode || Initializer is BlockNode block && block.Children is not [])
            {
                // Move initializer above loop
                BlockNode newBlock = new(cleaner.TopFragmentContext);
                newBlock.Children.Add(Initializer);
                newBlock.Children.Add(this);
                res = newBlock;
            }

            Initializer = null;
        }

        return res;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("for (");
        if (Condition is null && Incrementor is null)
        {
            if (Initializer is not null)
            {
                throw new DecompilerException("Expected initializer to be null in for (;;) loop");
            }
            printer.Write(";;");
        }
        else
        {
            Initializer?.Print(printer);
            printer.Write("; ");
            Condition.Print(printer);
            printer.Write("; ");
            Incrementor.Print(printer);
        }
        printer.Write(')');

        Body.Print(printer);
    }
}
