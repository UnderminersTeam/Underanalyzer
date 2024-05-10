using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a string constant in the AST.
/// </summary>
public class StringNode : IConstantNode<IGMString>
{
    public IGMString Value { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.String;

    public StringNode(IGMString value)
    {
        Value = value;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
