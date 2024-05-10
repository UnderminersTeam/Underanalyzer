using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an assignment statement in the AST.
/// </summary>
public class AssignNode : IASTNode
{
    /// <summary>
    /// The variable being assigned to.
    /// </summary>
    public IExpressionNode Variable { get; }

    /// <summary>
    /// The value being assigned.
    /// </summary>
    public IExpressionNode Value { get; }

    public AssignNode(IExpressionNode variable, IExpressionNode value)
    {
        Variable = variable;
        Value = value;
    }

    // TODO: compound operations

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
