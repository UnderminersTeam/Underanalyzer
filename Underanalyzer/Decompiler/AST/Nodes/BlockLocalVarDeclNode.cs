using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a local variable declaration statement for the enclosing block.
/// </summary>
public class BlockLocalVarDeclNode : IStatementNode
{
    public bool SemicolonAfter => true;
    public bool EmptyLineAfter { get; internal set; }
    public bool EmptyLineBefore => false;

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        List<string> localNames = printer.TopFragmentContext.LocalVariableNamesList;
        if (localNames.Count > 0)
        {
            printer.Write("var ");
            for (int i = 0; i < localNames.Count; i++)
            {
                printer.Write(localNames[i]);
                if (i != localNames.Count - 1)
                {
                    printer.Write(", ");
                }
            }
        }
    }

    public bool RequiresMultipleLines(ASTPrinter printer)
    {
        return false;
    }
}
