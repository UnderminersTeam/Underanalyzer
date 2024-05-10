using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a function call in the AST.
/// </summary>
public class FunctionCallNode : IExpressionNode
{
    /// <summary>
    /// The function reference being called.
    /// </summary>
    public IGMFunction Function { get; }

    /// <summary>
    /// Arguments being passed into the function call.
    /// </summary>
    public List<IExpressionNode> Arguments { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public FunctionCallNode(IGMFunction function, List<IExpressionNode> arguments)
    {
        Function = function;
        Arguments = arguments;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
