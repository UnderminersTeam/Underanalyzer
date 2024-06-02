using System.Collections.Generic;
using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents the "new" keyword being used to instantiate an object in the AST.
/// </summary>
public class NewObjectNode : IExpressionNode, IStatementNode, IConditionalValueNode, IFunctionCallNode
{
    /// <summary>
    /// The function (constructor) being used.
    /// </summary>
    public IExpressionNode Function { get; private set; }

    /// <summary>
    /// The arguments passed into the function (constructor).
    /// </summary>
    public List<IExpressionNode> Arguments { get; private set; }

    public bool Duplicated { get; set; }
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;
    public bool SemicolonAfter { get => true; }
    public string FunctionName { get => (Function is FunctionReferenceNode functionRef) ? functionRef.Function.Name.Content : null; }

    public string ConditionalTypeName => "NewObject";
    public string ConditionalValue => ""; // TODO?

    public NewObjectNode(IExpressionNode function, List<IExpressionNode> arguments)
    {
        Function = function;
        Arguments = arguments;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        Function = Function.Clean(cleaner);
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }

        if (cleaner.GlobalMacroResolver.ResolveFunctionArgumentTypes(cleaner, FunctionName) is IMacroTypeFunctionArgs argsMacroType)
        {
            if (argsMacroType.Resolve(cleaner, this) is IFunctionCallNode resolved)
            {
                // We found a match!
                return resolved;
            }
        }

        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        Function = Function.Clean(cleaner);
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].Clean(cleaner);
        }
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }

        printer.Write("new ");
        Function.Print(printer);
        printer.Write('(');
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i].Print(printer);
            if (i != Arguments.Count - 1)
            {
                printer.Write(", ");
            }
        }
        printer.Write(')');

        if (Group)
        {
            printer.Write(')');
        }
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
