using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a binary expression, such as basic two-operand arithmetic operations.
/// </summary>
public class BinaryNode : IExpressionNode
{
    /// <summary>
    /// Left side of the binary operation.
    /// </summary>
    public IExpressionNode Left { get; }

    /// <summary>
    /// Right side of the binary operation.
    /// </summary>
    public IExpressionNode Right { get; }
    
    /// <summary>
    /// The instruction that performs this operation, as in the code.
    /// </summary>
    public IGMInstruction Instruction { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; }

    public BinaryNode(IExpressionNode left, IExpressionNode right, IGMInstruction instruction)
    {
        Left = left;
        Right = right;  
        Instruction = instruction;
        StackType = instruction.Type2;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
