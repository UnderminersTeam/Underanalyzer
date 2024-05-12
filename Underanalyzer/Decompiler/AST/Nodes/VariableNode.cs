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

    /// <summary>
    /// Returns true if the other variable is referencing an identical variable, within the same expression/statement.
    /// </summary>
    public bool IdenticalToInExpression(VariableNode other)
    {
        // Compare basic attributes
        if (Variable != other.Variable || ReferenceType != other.ReferenceType || Left.GetType() != other.Left.GetType())
        {
            return false;
        }

        // Compare left side
        if (Left is VariableNode leftVariable)
        {
            if (!leftVariable.IdenticalToInExpression(other.Left as VariableNode))
            {
                return false;
            }
        }
        else if (Left is InstanceTypeNode leftInstType)
        {
            if (leftInstType.InstanceType != (other.Left as InstanceTypeNode).InstanceType)
            {
                return false;
            }
        }
        else if (Left is Int16Node leftI16)
        {
            if (leftI16.Value != (other.Left as Int16Node).Value)
            {
                return false;
            }
        }
        else if (Left != other.Left)
        {
            // Default; just compare references
            return false;
        }

        // Compare array indices
        if (ArrayIndices is not null)
        {
            if (other.ArrayIndices is null)
            {
                return false;
            }
            if (ArrayIndices.Count != other.ArrayIndices.Count)
            {
                return false;
            }
            for (int i = 0; i < ArrayIndices.Count; i++)
            {
                // Compare index references directly, as these should be duplicated if in the same expression
                if (ArrayIndices[i] != other.ArrayIndices[i])
                {
                    return false;
                }
            }
        }
        else
        {
            if (other.ArrayIndices is not null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true if the other variable is referencing a very similar variable, in the context of a for loop.
    /// Always returns false if the variables have array indices.
    /// </summary>
    public bool SimilarToInForIncrementor(VariableNode other)
    {
        // Compare basic attributes
        if (Variable != other.Variable || ReferenceType != other.ReferenceType || Left.GetType() != other.Left.GetType())
        {
            return false;
        }

        // Compare left side
        if (Left is VariableNode leftVariable)
        {
            if (!leftVariable.IdenticalToInExpression(other.Left as VariableNode))
            {
                return false;
            }
        }
        else if (Left is InstanceTypeNode leftInstType)
        {
            if (leftInstType.InstanceType != (other.Left as InstanceTypeNode).InstanceType)
            {
                return false;
            }
        }
        else if (Left is Int16Node leftI16)
        {
            if (leftI16.Value != (other.Left as Int16Node).Value)
            {
                return false;
            }
        }
        else if (Left != other.Left)
        {
            // Default; just compare references
            return false;
        }

        // Don't allow array indices as for incrementor
        // TODO: perhaps relax this at some point, and do a deep expression comparison?
        if (ArrayIndices is not null || other.ArrayIndices is not null)
        {
            return false;
        }

        return true;
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

        // TODO: determine if Left needs to be grouped

        // Check if we're a struct argument
        if (cleaner.StructArguments is not null)
        {
            // Verify this is an argument array access
            int instType = (Left as Int16Node)?.Value ?? (int)((Left as InstanceTypeNode).InstanceType);
            if (instType == (int)InstanceType.Argument && 
                Variable is { Name.Content: "argument" } &&
                ArrayIndices is [Int16Node arrayIndex])
            {
                if (arrayIndex.Value >= 0 && arrayIndex.Value < cleaner.StructArguments.Count)
                {
                    // We found an argument from the outer context! Clean it (in the outer context) and return it.
                    IExpressionNode arg = cleaner.StructArguments[arrayIndex.Value];
                    ASTFragmentContext context = cleaner.PopFragmentContext();
                    arg = arg.Clean(cleaner);
                    cleaner.PushFragmentContext(context);
                    return arg;
                }
            }
        }

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
