using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an array literal in the AST.
/// </summary>
public class ArrayInitNode : IExpressionNode, IMacroResolvableNode, IConditionalValueNode
{
    /// <summary>
    /// List of elements in this array literal.
    /// </summary>
    public List<IExpressionNode> Elements { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public string ConditionalTypeName => "ArrayInit";
    public string ConditionalValue => "";

    public ArrayInitNode(List<IExpressionNode> elements)
    {
        Elements = elements;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        for (int i = 0; i < Elements.Count; i++)
        {
            Elements[i] = Elements[i].Clean(cleaner);
        }
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write('[');
        for (int i = 0; i < Elements.Count; i++)
        {
            Elements[i].Print(printer);
            if (i != Elements.Count - 1)
            {
                printer.Write(", ");
            }
        }
        printer.Write(']');
    }

    public IExpressionNode ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        if (type is IMacroTypeArrayInit typeArrayInit)
        {
            return typeArrayInit.Resolve(cleaner, this);
        }
        return null;
    }
}
