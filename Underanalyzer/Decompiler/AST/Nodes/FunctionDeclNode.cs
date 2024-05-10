using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// A function declaration within the AST.
/// </summary>
public class FunctionDeclNode : IFragmentNode, IExpressionNode
{
    /// <summary>
    /// Name of the function, or null if anonymous.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// If true, this function is unnamed (anonymous).
    /// </summary>
    public bool IsAnonymous { get => Name is null; }

    /// <summary>
    /// If true, this function is a constructor function.
    /// </summary>
    public bool IsConstructor { get; }

    /// <summary>
    /// The body of the function (typically just a block).
    /// </summary>
    public IASTNode Body { get; }

    public bool Duplicated { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public ASTFragmentContext FragmentContext { get; }

    public FunctionDeclNode(string name, bool isConstructor, IASTNode body, ASTFragmentContext fragmentContext)
    {
        Name = name;
        IsConstructor = isConstructor;
        Body = body;
        FragmentContext = fragmentContext;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }

    void IASTNode.Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
