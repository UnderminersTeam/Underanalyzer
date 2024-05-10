using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

public class NewObjectNode : IExpressionNode
{
    public IExpressionNode Function { get; }
    public List<IExpressionNode> Arguments { get; }

    public bool Duplicated { get; set; }
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public NewObjectNode(IExpressionNode function, List<IExpressionNode> arguments)
    {
        Function = function;
        Arguments = arguments;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
