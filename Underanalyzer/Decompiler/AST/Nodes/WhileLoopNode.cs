namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a while loop in the AST.
/// </summary>
public class WhileLoopNode : IStatementNode
{
    /// <summary>
    /// The condition of the loop.
    /// </summary>
    public IExpressionNode Condition { get; private set; }

    /// <summary>
    /// The main block of the loop.
    /// </summary>
    public BlockNode Body { get; private set; }

    /// <summary>
    /// True if this loop was specifically detected to be a while loop already.
    /// That is, if true, this cannot be rewritten as a for loop.
    /// </summary>
    public bool MustBeWhileLoop { get; }

    public bool SemicolonAfter { get => false; }

    public WhileLoopNode(IExpressionNode condition, BlockNode body, bool mustBeWhileLoop)
    {
        Condition = condition;
        Body = body;
        MustBeWhileLoop = mustBeWhileLoop;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        // TODO: check if we should rewrite as a for loop here. will need to consider assignment statement before, though
        Condition = Condition.Clean(cleaner);
        Condition.Group = false;
        Body.Clean(cleaner);

        if (!MustBeWhileLoop)
        {
            // Check if we can turn into a for (;;) loop
            if (Condition is Int64Node i64 && i64.Value == 1)
            {
                return new ForLoopNode(null, null, null, Body);
            }
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("while (");
        Condition.Print(printer);
        printer.Write(')');
        Body.Print(printer);
    }
}
