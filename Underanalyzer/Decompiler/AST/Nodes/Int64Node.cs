using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a 64-bit signed integer constant in the AST.
/// </summary>
public class Int64Node : IConstantNode<long>
{
    public long Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int64;

    public Int64Node(long value)
    {
        Value = value;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write(Value);
    }
}
