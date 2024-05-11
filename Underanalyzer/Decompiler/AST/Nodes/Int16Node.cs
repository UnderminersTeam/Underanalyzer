using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a 16-bit signed integer constant in the AST.
/// </summary>
public class Int16Node : IConstantNode<short>
{
    public short Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int16;

    public Int16Node(short value)
    {
        Value = value;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        // TODO: handle asset/macro types
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write(Value);
    }
}
