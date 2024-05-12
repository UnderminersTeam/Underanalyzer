using System;
using System.Collections.Generic;
using static Underanalyzer.IGMInstruction;

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
        if (ArrayIndices is not null)
        {
            for (int i = 0; i < ArrayIndices.Count; i++)
            {
                ArrayIndices[i] = ArrayIndices[i].Clean(cleaner);
            }
        }
        // TODO: check if we're a struct argument here?
        // TODO: determine if Left needs to be grouped
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        // Print out left side, if necessary
        Int16Node leftI16 = Left as Int16Node;
        InstanceTypeNode leftInstType = Left as InstanceTypeNode;
        if (leftI16 is not null || leftInstType is not null)
        {
            // Basic numerical instance type
            int value = leftI16?.Value ?? (int)leftInstType.InstanceType;
            if (value < 0)
            {
                // GameMaker constant instance types
                switch (value)
                {
                    case (int)InstanceType.Self:
                        if (printer.LocalVariableNames.Contains(Variable.Name.Content))
                        {
                            // Need an explicit self in order to not conflict with local
                            printer.Write("self.");
                        }
                        break;
                    case (int)InstanceType.Other:
                        printer.Write("other.");
                        break;
                    case (int)InstanceType.All:
                        printer.Write("all.");
                        break;
                    case (int)InstanceType.Global:
                        printer.Write("global.");
                        break;
                    // TODO: unsure if we need to handle static
                }
            }
            else if (ReferenceType == VariableType.Instance)
            {
                // Room instance ID
                // TODO: verify this is correct
                printer.Write('(');
                printer.Write(value + 100000);
                printer.Write(").");
            }
            else
            {
                // Check if we have an object asset name to use
                string objectName = printer.Context.GameContext.GetAssetName(value, AssetType.Object);

                if (objectName is not null)
                {
                    // Object asset
                    printer.Write(objectName);
                    printer.Write('.');
                }
                else
                {
                    // Unknown number ID
                    printer.Write('(');
                    printer.Write(value);
                    printer.Write(").");
                }
            }
        }
        else
        {
            // Some expression on the left
            Left.Print(printer);
            printer.Write('.');
        }

        // Variable name
        printer.Write(Variable.Name.Content);

        if (ArrayIndices is not null)
        {
            // Print array indices
            if (printer.Context.GMLv2)
            {
                // For GMLv2
                foreach (IExpressionNode index in ArrayIndices)
                {
                    printer.Write('[');
                    index.Print(printer);
                    printer.Write(']');
                }
            }
            else
            {
                // For GMLv1
                printer.Write('[');
                ArrayIndices[0].Print(printer);
                if (ArrayIndices.Count == 2)
                {
                    printer.Write(", ");
                    ArrayIndices[1].Print(printer);
                }
                printer.Write(']');
            }
        }
    }
}
