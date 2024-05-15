using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a conditional expression in the AST.
/// </summary>
public class ConditionalNode : IExpressionNode
{
    /// <summary>
    /// The condition of the conditional expression.
    /// </summary>
    public IExpressionNode Condition { get; private set; }

    /// <summary>
    /// The expression that is returned when the condition is true.
    /// </summary>
    public IExpressionNode True { get; private set; }

    /// <summary>
    /// The expression that is returned when the condition is false.
    /// </summary>
    public IExpressionNode False { get; private set; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public ConditionalNode(IExpressionNode condition, IExpressionNode trueExpr, IExpressionNode falseExpr)
    {
        Condition = condition;
        True = trueExpr;
        False = falseExpr;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Condition = Condition.Clean(cleaner);
        True = True.Clean(cleaner);
        False = False.Clean(cleaner);

        // Ensure proper precedence
        if (Condition is BinaryNode or ShortCircuitNode or ConditionalNode or NullishCoalesceNode)
        {
            Condition.Group = true;
        }
        if (True is BinaryNode or ShortCircuitNode or ConditionalNode or NullishCoalesceNode)
        {
            True.Group = true;
        }
        if (False is BinaryNode or ShortCircuitNode or ConditionalNode or NullishCoalesceNode)
        {
            False.Group = true;
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }

        Condition.Print(printer);
        printer.Write(" ? ");
        True.Print(printer);
        printer.Write(" : ");
        False.Print(printer);

        if (Group)
        {
            printer.Write(')');
        }
    }
}
