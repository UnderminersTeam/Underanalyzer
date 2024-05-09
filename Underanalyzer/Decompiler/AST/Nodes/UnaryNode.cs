using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a unary expression, such as not (!) and bitwise negation (~).
/// </summary>
public class UnaryNode : IASTNode
{
    /// <summary>
    /// The expression that this operation is being performed on.
    /// </summary>
    public IASTNode Value { get; }

    /// <summary>
    /// The instruction that performs this operation, as in the code.
    /// </summary>
    public IGMInstruction Instruction { get; }

    bool IASTNode.Duplicated { get; set; }

    public UnaryNode(IASTNode value)
    {
        Value = value;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
