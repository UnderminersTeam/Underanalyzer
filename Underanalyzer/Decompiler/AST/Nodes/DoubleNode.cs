using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a double constant in the AST.
/// </summary>
public class DoubleNode : IConstantNode<double>
{
    public double Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Double;

    public DoubleNode(double value)
    {
        Value = value;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }
}
