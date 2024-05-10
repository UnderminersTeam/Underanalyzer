using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a variable being called as a method/function in the AST.
/// </summary>
public class VariableCallNode : IExpressionNode
{
    /// <summary>
    /// The function/method variable being called.
    /// </summary>
    public IExpressionNode Function { get; }

    /// <summary>
    /// The instance the method is being called on.
    /// </summary>
    public IExpressionNode Instance { get; }

    /// <summary>
    /// The arguments used in the call.
    /// </summary>
    public List<IExpressionNode> Arguments { get; }

    public bool Duplicated { get; set; }
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public VariableCallNode(IExpressionNode function, IExpressionNode instance, List<IExpressionNode> arguments)
    {
        Function = function;
        Instance = instance;
        Arguments = arguments;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
