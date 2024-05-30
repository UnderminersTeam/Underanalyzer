using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// A macro type that is the union of multiple macro types, evaluted in the supplied order until one (or none) is found.
/// </summary>
public class UnionMacroType : IMacroTypeInt32, IMacroTypeInt64, IMacroTypeFunctionArgs, IMacroTypeArrayInit, IMacroTypeConditional
{
    private List<IMacroType> Types { get; }

    public UnionMacroType(IEnumerable<IMacroType> types)
    {
        Types = new(types);
    }

    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, int data)
    {
        foreach (IMacroType type in Types)
        {
            if (type is not IMacroTypeInt32 type32)
            {
                continue;
            }
            IExpressionNode result = type32.Resolve(cleaner, node, data);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
    }

    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, long data)
    {
        foreach (IMacroType type in Types)
        {
            if (type is not IMacroTypeInt64 type64)
            {
                continue;
            }
            IExpressionNode result = type64.Resolve(cleaner, node, data);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
    }

    public FunctionCallNode Resolve(ASTCleaner cleaner, FunctionCallNode functionCall)
    {
        foreach (IMacroType type in Types)
        {
            if (type is not IMacroTypeFunctionArgs typeFuncArgs)
            {
                continue;
            }
            FunctionCallNode result = typeFuncArgs.Resolve(cleaner, functionCall);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
    }

    public ArrayInitNode Resolve(ASTCleaner cleaner, ArrayInitNode arrayInit)
    {
        foreach (IMacroType type in Types)
        {
            if (type is not IMacroTypeArrayInit typeArrayInit)
            {
                continue;
            }
            ArrayInitNode result = typeArrayInit.Resolve(cleaner, arrayInit);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
    }

    public IExpressionNode Resolve(ASTCleaner cleaner, IConditionalValueNode node)
    {
        foreach (IMacroType type in Types)
        {
            if (type is not IMacroTypeConditional typeConditional)
            {
                continue;
            }
            IExpressionNode result = typeConditional.Resolve(cleaner, node);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
    }
}
