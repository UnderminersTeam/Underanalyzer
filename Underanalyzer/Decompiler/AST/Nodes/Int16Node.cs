using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a 16-bit signed integer constant in the AST.
/// </summary>
public class Int16Node : IConstantNode<short>
{
    public short Value { get; }

    /// <summary>
    /// If true, this number was pushed with a normal Push instruction opcode,
    /// rather than the usual PushImmediate.
    /// </summary>
    public bool RegularPush { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int16;

    public Int16Node(short value, bool regularPush)
    {
        Value = value;
        RegularPush = regularPush;
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
