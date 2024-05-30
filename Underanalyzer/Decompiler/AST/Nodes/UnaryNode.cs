using Underanalyzer.Decompiler.Macros;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a unary expression, such as not (!) and bitwise negation (~).
/// </summary>
public class UnaryNode : IExpressionNode, IConditionalValueNode
{
    /// <summary>
    /// The expression that this operation is being performed on.
    /// </summary>
    public IExpressionNode Value { get; private set; }

    /// <summary>
    /// The instruction that performs this operation, as in the code.
    /// </summary>
    public IGMInstruction Instruction { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; }

    public string ConditionalTypeName => "Unary";
    public string ConditionalValue => ""; // TODO?

    public UnaryNode(IExpressionNode value, IGMInstruction instruction)
    {
        Value = value;
        Instruction = instruction;
        StackType = instruction.Type1;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Value = Value.Clean(cleaner);

        // Ensure operation applies to entire node
        if (Value is BinaryNode or ShortCircuitNode)
        {
            Value.Group = true;
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }

        char op = Instruction switch
        {
            { Kind: Opcode.Negate } => '-',
            { Kind: Opcode.Not, Type1: DataType.Boolean } => '!',
            { Kind: Opcode.Not } => '~',
            _ => throw new DecompilerException("Failed to match unary instruction to character")
        };
        printer.Write(op);

        Value.Print(printer);

        if (Group)
        {
            printer.Write(')');
        }
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
