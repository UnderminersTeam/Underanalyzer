using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a string constant in the AST.
/// </summary>
public class StringNode : IConstantNode<IGMString>
{
    public IGMString Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.String;

    public StringNode(IGMString value)
    {
        Value = value;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        // TODO: escape string content depending on game context/version
        printer.Write('"');
        printer.Write(Value.Content);
        printer.Write('"');
    }
}
