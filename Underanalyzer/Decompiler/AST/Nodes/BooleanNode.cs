using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a boolean constant in the AST.
/// </summary>
public class BooleanNode : IConstantNode<bool>
{
    public bool Value { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Boolean;

    public BooleanNode(bool value)
    {
        Value = value;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
