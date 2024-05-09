using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// A function declaration within the AST.
/// </summary>
public class FunctionDeclNode : IFragmentNode
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

    bool IASTNode.Duplicated { get; set; } = false;

    public FunctionDeclNode(string name, bool isConstructor, IASTNode body)
    {
        Name = name;
        IsConstructor = isConstructor;
        Body = body;
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
