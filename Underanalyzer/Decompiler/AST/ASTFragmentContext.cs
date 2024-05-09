using System;
using System.Collections.Generic;
using System.Text;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Helper class used solely by <see cref="ASTBuilder"/> to manage the fragment context stack.
/// </summary>
internal class ASTFragmentContext
{
    public Stack<IASTNode> ExpressionStack { get; } = new();

    public List<IASTNode> StructArguments { get; set; } = null;
}
