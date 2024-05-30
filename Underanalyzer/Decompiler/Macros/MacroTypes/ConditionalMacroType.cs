using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Base abstract type for all macro types that evaluate/verify a condition before passing through to another macro type.
/// </summary>
public abstract class ConditionalMacroType : IMacroTypeInt32, IMacroTypeInt64, IMacroTypeFunctionArgs, IMacroTypeArrayInit, IMacroTypeConditional
{
    /// <summary>
    /// The inner type that this conditional macro type holds, which will be used after verifying the condition.
    /// If null, then there is no inner type, and the node being resolved will be passed through.
    /// </summary>
    public IMacroType InnerType { get; }

    /// <summary>
    /// Base constructor for all conditional macro types; requires inner type.
    /// Inner type can be null, specifying that the node will be passed through when resolved.
    /// </summary>
    public ConditionalMacroType(IMacroType innerType)
    {
        InnerType = innerType;
    }

    /// <summary>
    /// Evaluates the condition on the given node, returning true if successful, or false if not.
    /// </summary>
    public abstract bool EvaluateCondition(ASTCleaner cleaner, IConditionalValueNode node);


    /// <summary>
    /// Resolves the macro type with an arbitrary conditional node, passing through the node if successful;  null otherwise.
    /// Inner type must be null for this to resolve anything.
    /// </summary>
    public IExpressionNode Resolve(ASTCleaner cleaner, IConditionalValueNode node)
    {
        if (InnerType is not null)
        {
            // We have an inner type, and no way to resolve it
            return null;
        }

        if (!EvaluateCondition(cleaner, node))
        {
            // Condition failure
            return null;
        }

        // Condition success: pass through node
        return node;
    }

    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, int data)
    {
        if (node is not IConditionalValueNode conditionalValueNode)
        {
            // Node can't be checked for conditional values
            return null;
        }

        // Check whether we specify any inner type at all
        if (InnerType is not null)
        {
            if (InnerType is not IMacroTypeInt32 innerTypeInt32)
            {
                // Inner type is mismatched with Int32 requirement
                return null;
            }
            if (!EvaluateCondition(cleaner, conditionalValueNode))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass along to inner type
            return innerTypeInt32.Resolve(cleaner, node, data);
        }
        else
        {
            if (!EvaluateCondition(cleaner, conditionalValueNode))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass through node
            return node;
        }
    }

    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, long data)
    {
        if (node is not IConditionalValueNode conditionalValueNode)
        {
            // Node can't be checked for conditional values
            return null;
        }

        // Check whether we specify any inner type at all
        if (InnerType is not null)
        {
            if (InnerType is not IMacroTypeInt64 innerTypeInt64)
            {
                // Inner type is mismatched with Int64 requirement
                return null;
            }

            if (!EvaluateCondition(cleaner, conditionalValueNode))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass along to inner type
            return innerTypeInt64.Resolve(cleaner, node, data);
        }
        else
        {
            if (!EvaluateCondition(cleaner, conditionalValueNode))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass through node
            return node;
        }
    }

    public FunctionCallNode Resolve(ASTCleaner cleaner, FunctionCallNode call)
    {
        // Check whether we specify any inner type at all
        if (InnerType is not null)
        {
            if (InnerType is not IMacroTypeFunctionArgs innerTypeFunctionArgs)
            {
                // Inner type is mismatched with FunctionArgs requirement
                return null;
            }

            if (!EvaluateCondition(cleaner, call))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass along to inner type
            return innerTypeFunctionArgs.Resolve(cleaner, call);
        }
        else
        {
            if (!EvaluateCondition(cleaner, call))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass through node
            return call;
        }
    }

    public ArrayInitNode Resolve(ASTCleaner cleaner, ArrayInitNode arrayInit)
    {
        // Check whether we specify any inner type at all
        if (InnerType is not null)
        {
            if (InnerType is not IMacroTypeArrayInit innerTypeArrayInit)
            {
                // Inner type is mismatched with FunctionArgs requirement
                return null;
            }

            if (!EvaluateCondition(cleaner, arrayInit))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass along to inner type
            return innerTypeArrayInit.Resolve(cleaner, arrayInit);
        }
        else
        {
            if (!EvaluateCondition(cleaner, arrayInit))
            {
                // Condition failure
                return null;
            }

            // Condition success: pass through node
            return arrayInit;
        }
    }
}
