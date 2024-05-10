using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a 64-bit signed integer constant in the AST.
/// </summary>
public class Int64Node : IConstantNode<long>
{
    public long Value { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int64;

    public Int64Node(long value)
    {
        Value = value;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
