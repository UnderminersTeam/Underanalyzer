/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a local variable declaration statement, for specific local variables.
/// </summary>
public class LocalVarDeclNode : IStatementNode
{
    public List<string> Locals { get; } = new(4);

    public bool SemicolonAfter => true;
    public bool EmptyLineAfter { get; set; } = false;
    public bool EmptyLineBefore { get; set; } = false;

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public IStatementNode PostClean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("var ");
        for (int i = 0; i < Locals.Count; i++)
        {
            printer.Write(Locals[i]);
            if (i != Locals.Count - 1)
            {
                printer.Write(", ");
            }
        }
    }

    public bool RequiresMultipleLines(ASTPrinter printer)
    {
        return false;
    }
}
