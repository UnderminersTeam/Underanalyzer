using System;
using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a function reference in the AST.
/// </summary>
public class FunctionReferenceNode : IExpressionNode, IConditionalValueNode
{
    /// <summary>
    /// The function being referenced.
    /// </summary>
    public IGMFunction Function { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int32;

    public string ConditionalTypeName => "FunctionReference";
    public string ConditionalValue => Function.Name.Content;

    public FunctionReferenceNode(IGMFunction function)
    {
        Function = function;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write(printer.LookupFunction(Function));
    }

    public bool RequiresMultipleLines(ASTPrinter printer)
    {
        return false;
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
