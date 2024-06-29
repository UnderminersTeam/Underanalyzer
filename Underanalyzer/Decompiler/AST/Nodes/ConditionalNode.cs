using System;
using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a conditional expression in the AST.
/// </summary>
public class ConditionalNode : IMultiExpressionNode, IMacroResolvableNode, IConditionalValueNode
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

    public string ConditionalTypeName => "Conditional";
    public string ConditionalValue => ""; // TODO?

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
        if (Condition is IMultiExpressionNode)
        {
            Condition.Group = true;
        }
        if (True is IMultiExpressionNode)
        {
            True.Group = true;
        }
        if (False is IMultiExpressionNode)
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

    public IExpressionNode ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        if (type is IMacroTypeConditional conditional)
        {
            return conditional.Resolve(cleaner, this);
        }

        bool didAnything = false;

        if (True is IMacroResolvableNode trueResolvable && 
            trueResolvable.ResolveMacroType(cleaner, type) is IExpressionNode trueResolved)
        {
            True = trueResolved;
            didAnything = true;
        }
        if (False is IMacroResolvableNode falseResolvable &&
            falseResolvable.ResolveMacroType(cleaner, type) is IExpressionNode falseResolved)
        {
            False = falseResolved;
            didAnything = true;
        }

        return didAnything ? this : null;
    }
}
