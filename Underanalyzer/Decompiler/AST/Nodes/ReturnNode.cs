using System;
namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a "return" statement (with a value) in the AST.
/// </summary>
public class ReturnNode : IStatementNode
{
    /// <summary>
    /// Expression being returned.
    /// </summary>
    public IExpressionNode Value { get; private set; }

    public bool SemicolonAfter { get => true; }

    public ReturnNode(IExpressionNode value)
    {
        Value = value;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Value = Value.Clean(cleaner);
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("return ");
        Value.Print(printer);
    }
}
