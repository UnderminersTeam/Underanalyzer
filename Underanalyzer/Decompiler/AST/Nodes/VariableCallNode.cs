using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a variable being called as a method/function in the AST.
/// </summary>
public class VariableCallNode : IExpressionNode, IStatementNode
{
    /// <summary>
    /// The function/method variable being called.
    /// </summary>
    public IExpressionNode Function { get; private set; }

    /// <summary>
    /// The instance the method is being called on.
    /// </summary>
    public IExpressionNode Instance { get; private set; }

    /// <summary>
    /// The arguments used in the call.
    /// </summary>
    public List<IExpressionNode> Arguments { get; }

    public bool Duplicated { get; set; }
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public bool SemicolonAfter { get => true; }

    public VariableCallNode(IExpressionNode function, IExpressionNode instance, List<IExpressionNode> arguments)
    {
        Function = function;
        Instance = instance;
        Arguments = arguments;
    }

    IExpressionNode IASTNode<IExpressionNode>.Clean(ASTCleaner cleaner)
    {
        Function = Function.Clean(cleaner);
        Instance = Instance.Clean(cleaner);
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }

        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        Function = Function.Clean(cleaner);
        Instance = Instance.Clean(cleaner);
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Instance is not InstanceTypeNode instType || instType.InstanceType != IGMInstruction.InstanceType.Self)
        {
            if (!Instance.Duplicated)
            {
                Instance.Print(printer);
                printer.Write('.');
            }
        }
        Function.Print(printer);
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
