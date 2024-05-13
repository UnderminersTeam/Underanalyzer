using System;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an assignment statement in the AST.
/// </summary>
public class AssignNode : IStatementNode, IExpressionNode
{
    /// <summary>
    /// The variable being assigned to.
    /// </summary>
    public IExpressionNode Variable { get; private set; }

    /// <summary>
    /// The value being assigned.
    /// </summary>
    public IExpressionNode Value { get; private set; }

    /// <summary>
    /// The type of assignment being done.
    /// </summary>
    public AssignType AssignKind { get; private set; }

    /// <summary>
    /// For prefix/postfix/compound, this is the instruction used to do the operation.
    /// </summary>
    public IGMInstruction BinaryInstruction { get; private set; }

    public bool SemicolonAfter { get => true; }
    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public DataType StackType { get; set; } = DataType.Variable;

    /// <summary>
    /// Different types of assignments: normal (=), compound (e.g. +=), prefix/postfix (e.g. ++)
    /// </summary>
    public enum AssignType
    {
        Normal,
        Compound,
        Prefix,
        Postfix
    }

    public AssignNode(IExpressionNode variable, IExpressionNode value)
    {
        Variable = variable;
        Value = value;
        AssignKind = AssignType.Normal;
    }

    public AssignNode(IExpressionNode variable, IExpressionNode value, IGMInstruction binaryInstruction)
    {
        Variable = variable;
        Value = value;
        AssignKind = AssignType.Compound;
        BinaryInstruction = binaryInstruction;
    }

    public AssignNode(IExpressionNode variable, AssignType assignKind, IGMInstruction binaryInstruction)
    {
        Variable = variable;
        Value = null;
        AssignKind = assignKind;
        BinaryInstruction = binaryInstruction;
        // TODO: do we need a special StackType to prevent nesting stack size issues?
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Variable = Variable.Clean(cleaner);
        Value = Value?.Clean(cleaner);

        // Clean up any remaining postfix/compound operations
        if (AssignKind == AssignType.Normal && Variable is VariableNode variable && Value is BinaryNode binary)
        {
            if (binary.Left is VariableNode binVariable && binVariable.IdenticalToInExpression(variable))
            {
                // This is probably a compound operation

                // Check if we're a postfix operation
                if (binary.Instruction.Kind is Opcode.Add or Opcode.Subtract && 
                    binary.Right is Int16Node i16 && i16.Value == 1 && i16.RegularPush)
                {
                    AssignKind = AssignType.Postfix;
                    BinaryInstruction = binary.Instruction;
                    Value = null;

                    return this;
                }

                // Ensure we actually are a compound operation (Push vs. specialized Push instruction)
                if (cleaner.Context.OlderThanBytecode15 ||
                    binVariable.RegularPush || binVariable.Variable.InstanceType == InstanceType.Self)
                {
                    AssignKind = AssignType.Compound;
                    BinaryInstruction = binary.Instruction;
                    Value = binary.Right;

                    return this;
                }
            }
        }

        return this;
    }

    IExpressionNode IASTNode<IExpressionNode>.Clean(ASTCleaner cleaner)
    {
        Variable = Variable.Clean(cleaner);
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        // TODO: handle local variable declarations

        switch (AssignKind)
        {
            case AssignType.Normal:
                if (printer.StructArguments is not null)
                {
                    // We're inside a struct initialization block
                    Variable.Print(printer);
                    printer.Write(": ");
                    Value.Print(printer);
                }
                else
                {
                    // Normal assignment
                    Variable.Print(printer);
                    printer.Write(" = ");
                    Value.Print(printer);
                }
                break;
            case AssignType.Prefix:
                printer.Write((BinaryInstruction.Kind == Opcode.Add) ? "++" : "--");
                Variable.Print(printer);
                break;
            case AssignType.Postfix:
                Variable.Print(printer);
                printer.Write((BinaryInstruction.Kind == Opcode.Add) ? "++" : "--");
                break;
            case AssignType.Compound:
                Variable.Print(printer);
                printer.Write(BinaryInstruction.Kind switch
                {
                    Opcode.Add => " += ",
                    Opcode.Subtract => " -= ",
                    Opcode.Multiply => " *= ",
                    Opcode.Divide => " /= ",
                    Opcode.GMLModulo => " %= ",
                    Opcode.And => " &= ",
                    Opcode.Or => " |= ",
                    Opcode.Xor => " ^= ",
                    _ => throw new DecompilerException("Unknown binary instruction opcode in compound assignment")
                });
                Value.Print(printer);
                break;
        }
    }
}
