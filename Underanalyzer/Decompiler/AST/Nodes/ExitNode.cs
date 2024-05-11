using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an "exit"/"empty return" statement in the AST.
/// </summary>
public class ExitNode : IStatementNode
{
    public IStatementNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        // TODO: check if we're inside of a function (or script in GMS2) and use "return" instead
        printer.Write("exit");
    }
}
