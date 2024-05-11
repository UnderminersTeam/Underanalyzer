using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a 32-bit signed integer constant in the AST.
/// </summary>
public class Int32Node : IConstantNode<int>
{
    public int Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int32;

    public Int32Node(int value)
    {
        Value = value;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        // TODO: macro (probably not asset) types
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write(Value);
    }
}
