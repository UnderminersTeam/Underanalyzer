using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a switch statement in the AST.
/// </summary>
public class SwitchNode : IStatementNode
{
    /// <summary>
    /// The expression being switched upon.
    /// </summary>
    public IExpressionNode Expression { get; private set; }

    /// <summary>
    /// The main block of the switch statement.
    /// </summary>
    public BlockNode Body { get; private set; }

    public bool SemicolonAfter { get => false; }

    public SwitchNode(IExpressionNode expression, BlockNode body)
    {
        Expression = expression;
        Body = body;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Expression = Expression.Clean(cleaner);
        Body.Clean(cleaner);

        // Handle macro type resolution for cases
        if (Expression is IMacroTypeNode exprTypeNode && exprTypeNode.GetExpressionMacroType(cleaner) is IMacroType exprMacroType)
        {
            foreach (IStatementNode statement in Body.Children)
            {
                if (statement is SwitchCaseNode caseNode && caseNode.Expression is IMacroResolvableNode exprResolvable &&
                    exprResolvable.ResolveMacroType(cleaner, exprMacroType) is IExpressionNode exprResolved)
                {
                    caseNode.Expression = exprResolved;
                }
            }
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("switch (");
        Expression.Print(printer);
        printer.Write(')');
        Body.Print(printer);
    }
}
