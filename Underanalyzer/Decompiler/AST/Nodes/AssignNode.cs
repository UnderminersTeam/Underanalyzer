using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an assignment statement in the AST.
/// </summary>
public class AssignNode : IStatementNode
{
    /// <summary>
    /// The variable being assigned to.
    /// </summary>
    public IExpressionNode Variable { get; private set; }

    /// <summary>
    /// The value being assigned.
    /// </summary>
    public IExpressionNode Value { get; private set; }

    public bool SemicolonAfter { get => true; }

    public AssignNode(IExpressionNode variable, IExpressionNode value)
    {
        Variable = variable;
        Value = value;
    }

    // TODO: compound operations

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Variable = Variable.Clean(cleaner);
        Value = Value.Clean(cleaner);

        // TODO: clean up compound assignment operations

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        // TODO: handle compound assignment operations
        // TODO: handle struct variable initialization
        // TODO: handle local variable declarations

        Variable.Print(printer);
        printer.Write(" = ");
        Value.Print(printer);
    }
}
