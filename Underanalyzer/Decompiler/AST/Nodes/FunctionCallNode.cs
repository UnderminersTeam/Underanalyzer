using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a function call in the AST.
/// </summary>
public class FunctionCallNode : IExpressionNode, IStatementNode
{
    /// <summary>
    /// The function reference being called.
    /// </summary>
    public IGMFunction Function { get; }

    /// <summary>
    /// Arguments being passed into the function call.
    /// </summary>
    public List<IExpressionNode> Arguments { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public bool SemicolonAfter { get => true; }

    public FunctionCallNode(IGMFunction function, List<IExpressionNode> arguments)
    {
        Function = function;
        Arguments = arguments;
    }

    IExpressionNode IASTNode<IExpressionNode>.Clean(ASTCleaner cleaner)
    {
        // Clean up all arguments
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }

        // Handle special instance types
        switch (Function.Name.Content)
        {
            case VMConstants.SelfFunction:
                return new InstanceTypeNode(IGMInstruction.InstanceType.Self) { Duplicated = Duplicated, StackType = StackType };
            case VMConstants.OtherFunction:
                return new InstanceTypeNode(IGMInstruction.InstanceType.Other) { Duplicated = Duplicated, StackType = StackType };
            case VMConstants.GlobalFunction:
                return new InstanceTypeNode(IGMInstruction.InstanceType.Global) { Duplicated = Duplicated, StackType = StackType };
            case VMConstants.GetInstanceFunction:
                if (Arguments.Count == 0 || Arguments[0] is not Int16Node)
                {
                    throw new DecompilerException($"Expected 16-bit integer parameter to {VMConstants.GetInstanceFunction}");
                }
                Arguments[0].Duplicated = true;
                Arguments[0].StackType = StackType;
                return Arguments[0];
        }

        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        // Just clean up arguments here - special calls are only in expressions
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write(printer.LookupFunction(Function));
        printer.Write('(');
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i].Print(printer);
            if (i != Arguments.Count - 1)
            {
                printer.Write(", ");
            }
        }
        printer.Write(')');
    }
}
