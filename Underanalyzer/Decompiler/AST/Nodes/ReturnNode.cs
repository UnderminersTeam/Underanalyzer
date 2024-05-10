using System;
namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a "return" statement (with a value) in the AST.
/// </summary>
public class ReturnNode : IASTNode
{
    /// <summary>
    /// Expression being returned.
    /// </summary>
    public IASTNode Value { get; }

    public ReturnNode(IASTNode value)
    {
        Value = value;
    }

    public void Print(ASTPrinter printer)
    {
        throw new NotImplementedException();
    }
}
