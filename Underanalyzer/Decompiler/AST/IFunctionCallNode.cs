using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Base interface for all nodes that call a function in the AST.
/// </summary>
public interface IFunctionCallNode : IConditionalValueNode, IStatementNode, IExpressionNode
{
    /// <summary>
    /// Name of the function being called, or null if none.
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    /// List of arguments used to call the function with.
    /// </summary>
    public List<IExpressionNode> Arguments { get; }
}
