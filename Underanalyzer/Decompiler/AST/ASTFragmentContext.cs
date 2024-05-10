using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a single fragment context within the AST.
/// </summary>
public class ASTFragmentContext
{
    /// <summary>
    /// Current working VM expression stack.
    /// </summary>
    internal Stack<IExpressionNode> ExpressionStack { get; } = new();

    /// <summary>
    /// If not null, represents the list of arguments getting passed into this fragment (which is a struct).
    /// </summary>
    public List<IExpressionNode> StructArguments { get; internal set; } = null;

    /// <summary>
    /// Contains all local variables referenced from within this fragment.
    /// </summary>
    public HashSet<string> LocalVariableNames { get; } = new();
}
