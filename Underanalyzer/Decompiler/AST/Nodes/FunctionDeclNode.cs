using System;
using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// A function declaration within the AST.
/// </summary>
public class FunctionDeclNode : IFragmentNode, IExpressionNode, IConditionalValueNode
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

    public string ConditionalTypeName => "FunctionDecl";
    public string ConditionalValue => Name;

    public FunctionDeclNode(string name, bool isConstructor, BlockNode body, ASTFragmentContext fragmentContext)
    {
        Name = name;
        IsConstructor = isConstructor;
        Body = body;
        FragmentContext = fragmentContext;
    }

    private void CleanBody(ASTCleaner cleaner)
    {
        Body.Clean(cleaner);
        Body.UseBraces = true;
        Body.PrintLocalsAtTop = true;
        if (Body.FragmentContext.BaseParentCall is not null)
        {
            cleaner.PushFragmentContext(Body.FragmentContext);
            Body.FragmentContext.BaseParentCall = Body.FragmentContext.BaseParentCall.Clean(cleaner);
            cleaner.PopFragmentContext();
        }
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        CleanBody(cleaner);
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        CleanBody(cleaner);
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
        if (Body.FragmentContext.BaseParentCall is not null)
        {
            printer.Write(" : ");
            printer.PushFragmentContext(Body.FragmentContext);
            Body.FragmentContext.BaseParentCall.Print(printer);
            printer.PopFragmentContext();
        }
        if (IsConstructor)
        {
            printer.Write(" constructor");
        }
        Body.Print(printer);
    }

    public IExpressionNode ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        if (type is IMacroTypeConditional conditional)
        {
            return conditional.Resolve(cleaner, this);
        }
        return null;
    }
}
