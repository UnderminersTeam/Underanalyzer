using System;
namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a binary expression, such as basic two-operand arithmetic operations.
/// </summary>
public class BinaryNode : IASTNode
{
    /// <summary>
    /// Left side of the binary operation.
    /// </summary>
    public IASTNode Left { get; }

    /// <summary>
    /// Right side of the binary operation.
    /// </summary>
    public IASTNode Right { get; }
    
    /// <summary>
    /// The instruction that performs this operation, as in the code.
    /// </summary>
    public IGMInstruction Instruction { get; }

    bool IASTNode.Duplicated { get; set; } = false;

    public BinaryNode(IASTNode left, IASTNode right, IGMInstruction instruction)
    {
        Left = left;
        Right = right;  
        Instruction = instruction;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
