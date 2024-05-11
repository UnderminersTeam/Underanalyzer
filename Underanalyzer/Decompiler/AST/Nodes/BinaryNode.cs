using System;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a binary expression, such as basic two-operand arithmetic operations.
/// </summary>
public class BinaryNode : IExpressionNode
{
    /// <summary>
    /// Left side of the binary operation.
    /// </summary>
    public IExpressionNode Left { get; private set; }

    /// <summary>
    /// Right side of the binary operation.
    /// </summary>
    public IExpressionNode Right { get; private set; }
    
    /// <summary>
    /// The instruction that performs this operation, as in the code.
    /// </summary>
    public IGMInstruction Instruction { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; }

    public BinaryNode(IExpressionNode left, IExpressionNode right, IGMInstruction instruction)
    {
        Left = left;
        Right = right;  
        Instruction = instruction;
        StackType = instruction.Type2;
    }

    private void CheckGroup(IExpressionNode node)
    {
        // TODO: verify that this works for all cases
        if (node is BinaryNode binary)
        {
            if (binary.Instruction.Kind != Instruction.Kind)
            {
                binary.Group = true;
            }
            if (binary.Instruction.Kind == Opcode.Compare && binary.Instruction.ComparisonKind != Instruction.ComparisonKind)
            {
                binary.Group = true;
            }
        }
        else if (node is ShortCircuitNode sc)
        {
            sc.Group = true;
        }
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Left = Left.Clean(cleaner);
        Right = Right.Clean(cleaner);

        CheckGroup(Left);
        CheckGroup(Right);

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }

        Left.Print(printer);

        string op = Instruction switch
        {
            { Kind: Opcode.Add } => " + ",
            { Kind: Opcode.Subtract } => " - ",
            { Kind: Opcode.Multiply } => " * ",
            { Kind: Opcode.Divide } => " / ",
            { Kind: Opcode.GMLDivRemainder } => " div ",
            { Kind: Opcode.GMLModulo } => " % ",
            { Kind: Opcode.And, Type1: DataType.Boolean, Type2: DataType.Boolean } => " && ",
            { Kind: Opcode.And } => " & ",
            { Kind: Opcode.Or, Type1: DataType.Boolean, Type2: DataType.Boolean } => " || ",
            { Kind: Opcode.Or } => " | ",
            { Kind: Opcode.Xor, Type1: DataType.Boolean, Type2: DataType.Boolean } => " ^^ ",
            { Kind: Opcode.Xor } => " ^ ",
            { Kind: Opcode.ShiftLeft } => " << ",
            { Kind: Opcode.ShiftRight } => " >> ",
            { Kind: Opcode.Compare, ComparisonKind: ComparisonType.Lesser } => " < ",
            { Kind: Opcode.Compare, ComparisonKind: ComparisonType.LesserEqual } => " <= ",
            { Kind: Opcode.Compare, ComparisonKind: ComparisonType.Equal } => " == ",
            { Kind: Opcode.Compare, ComparisonKind: ComparisonType.NotEqual } => " != ",
            { Kind: Opcode.Compare, ComparisonKind: ComparisonType.GreaterEqual } => " >= ",
            { Kind: Opcode.Compare, ComparisonKind: ComparisonType.Greater } => " > ",
            _ => throw new DecompilerException("Failed to match binary instruction to string")
        };
        printer.Write(op);

        Right.Print(printer);

        if (Group)
        {
            printer.Write(')');
        }
    }
}
