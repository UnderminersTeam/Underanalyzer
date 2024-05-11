using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.ControlFlow;

/// <summary>
/// Represents a break statement in the control flow graph.
/// </summary>
internal class BreakNode(int address, bool mayBeContinue = false) : IControlFlowNode
{
    public int StartAddress { get; set; } = address;

    public int EndAddress { get; set; } = address;

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = new();

    public bool Unreachable { get; set; } = false;

    /// <summary>
    /// If true, signifies that we *think* this is a break (e.g. from inside of a switch statement), 
    /// but in certain cases this can be a continue. Switch statement processing will resolve all of these.
    /// When true, the Children list will contain the unprocessed successors of the branch.
    /// </summary>
    public bool MayBeContinue { get; set; } = mayBeContinue;

    public override string ToString()
    {
        return $"{nameof(BreakNode)} (address {StartAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
    }

    public void BuildAST(ASTBuilder builder, List<IStatementNode> output)
    {
        output.Add(new AST.BreakNode());
    }
}
