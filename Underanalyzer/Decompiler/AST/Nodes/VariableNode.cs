using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a variable reference in the AST.
/// </summary>
public class VariableNode : IExpressionNode
{
    /// <summary>
    /// The variable being referenced.
    /// </summary>
    public IGMVariable Variable { get; }

    /// <summary>
    /// The type of the variable reference.
    /// </summary>
    public IGMInstruction.VariableType ReferenceType { get; }

    /// <summary>
    /// The left side of the variable (before a dot, usually).
    /// </summary>
    public IExpressionNode Left { get; internal set; }

    /// <summary>
    /// For array accesses, this is not null, and contains all array indexing operations on this variable.
    /// </summary>
    public List<IExpressionNode> ArrayIndices { get; internal set; }

    /// <summary>
    /// If true, means that this variable was pushed with a normal <see cref="IGMInstruction.Opcode.Push"/> opcode.
    /// </summary>
    public bool RegularPush { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public VariableNode(IGMVariable variable, IGMInstruction.VariableType referenceType, bool regularPush = false)
    {
        Variable = variable;
        ReferenceType = referenceType;
        RegularPush = regularPush;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Left = Left.Clean(cleaner);
        for (int i = 0; i < ArrayIndices.Count; i++)
        {
            ArrayIndices[i] = ArrayIndices[i].Clean(cleaner);
        }
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
