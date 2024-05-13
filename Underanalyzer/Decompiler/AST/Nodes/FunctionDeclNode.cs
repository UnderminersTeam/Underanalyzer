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
    /// The body of the function.
    /// </summary>
    public BlockNode Body { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public ASTFragmentContext FragmentContext { get; }
    public bool SemicolonAfter { get => false; }

    public FunctionDeclNode(string name, bool isConstructor, BlockNode body, ASTFragmentContext fragmentContext)
    {
        Name = name;
        IsConstructor = isConstructor;
        Body = body;
        FragmentContext = fragmentContext;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Body.Clean(cleaner);
        Body.UseBraces = true;
        Body.PrintLocalsAtTop = true;
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        Body.Clean(cleaner);
        Body.UseBraces = true;
        Body.PrintLocalsAtTop = true;
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (IsAnonymous)
        {
            printer.Write("function(");
        }
        else
        {
            printer.Write("function ");
            printer.Write(Name);
            printer.Write('(');
        }
        // TODO: handle argument names
        printer.Write(')');
        Body.Print(printer);
    }
}
