using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a unary expression, such as not (!) and bitwise negation (~).
/// </summary>
public class UnaryNode : IExpressionNode
{
    /// <summary>
    /// The expression that this operation is being performed on.
    /// </summary>
    public IExpressionNode Value { get; }

    /// <summary>
    /// The instruction that performs this operation, as in the code.
    /// </summary>
    public IGMInstruction Instruction { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; }

    public UnaryNode(IExpressionNode value, IGMInstruction instruction)
    {
        Value = value;
        Instruction = instruction;
        StackType = instruction.Type1;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
