using System;
using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a boolean constant in the AST.
/// </summary>
public class BooleanNode : IConstantNode<bool>, IConditionalValueNode
{
    public bool Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Boolean;

    public string ConditionalTypeName => "Boolean";
    public string ConditionalValue => Value ? "true" : "false";

    public BooleanNode(bool value)
    {
        Value = value;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write(Value ? "true" : "false");
    }

    public IExpressionNode ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        if (type is IMacroTypeConditional conditional)
        {
            return conditional.Resolve(cleaner, this);
        }
        return null;
    }
}
