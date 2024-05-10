using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a function call in the AST.
/// </summary>
public class FunctionReferenceNode : IExpressionNode
{
    /// <summary>
    /// The function being referenced.
    /// </summary>
    public IGMFunction Function { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int32;

    public FunctionReferenceNode(IGMFunction function)
    {
        Function = function;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
