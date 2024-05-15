using System.Collections.Generic;
using Underanalyzer.Decompiler.ControlFlow;

namespace Underanalyzer.Decompiler.AST;

public class ASTBuilder
{
    /// <summary>
    /// The corresponding code context for this AST builder.
    /// </summary>
    public DecompileContext Context { get; }

    /// <summary>
    /// Reusable expression stack for instruction simulation. When non-empty after building a control flow node,
    /// usually signifies data that needs to get processed by the following control flow node.
    /// </summary>
    internal Stack<IExpressionNode> ExpressionStack { get => TopFragmentContext.ExpressionStack; }

    /// <summary>
    /// The index to start processing instructions for the next ControlFlow.Block we encounter.
    /// Used by code fragments to skip processing instructions twice.
    /// </summary>
    internal int StartBlockInstructionIndex { get; set; } = 0;

    /// <summary>
    /// List of arguments passed into a struct fragment.
    /// </summary>
    internal List<IExpressionNode> StructArguments { get => TopFragmentContext.StructArguments; set => TopFragmentContext.StructArguments = value; }

    /// <summary>
    /// Set of all local variables present in the current fragment.
    /// </summary>
    internal HashSet<string> LocalVariableNames { get => TopFragmentContext.LocalVariableNames; }

    /// <summary>
    /// Set of all local variables present in the current fragment.
    /// </summary>
    internal List<string> LocalVariableNamesList { get => TopFragmentContext.LocalVariableNamesList; }

    /// <summary>
    /// The stack used to manage fragment contexts.
    /// </summary>
    private Stack<ASTFragmentContext> FragmentContextStack { get; } = new();

    /// <summary>
    /// The current/top fragment context.
    /// </summary>
    internal ASTFragmentContext TopFragmentContext { get; private set; }

    /// <summary>
    /// Current queue of switch case expressions.
    /// </summary>
    internal Queue<IExpressionNode> SwitchCases { get; set; } = null;

    /// <summary>
    /// Initializes a new AST builder from the given code context.
    /// </summary>
    public ASTBuilder(DecompileContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Builds the AST for an entire code entry, starting from the root fragment node.
    /// </summary>
    public IStatementNode Build()
    {
        List<IStatementNode> output = new(1);
        PushFragmentContext(Context.FragmentNodes[0]);
        Context.FragmentNodes[0].BuildAST(this, output);
        PopFragmentContext();
        return output[0];
    }

    /// <summary>
    /// Builds a block starting from a control flow node, following all of its successors linearly,
    /// before stopping at a given node (or null, by default).
    /// </summary>
    internal BlockNode BuildBlock(IControlFlowNode startNode, IControlFlowNode stopAtNode = null)
    {
        BlockNode block = new(TopFragmentContext);

        // Advance through all successors, building out this block
        var currentNode = startNode;
        while (currentNode is not null && currentNode != stopAtNode)
        {
            currentNode.BuildAST(this, block.Children);

            if (currentNode.Successors.Count > 1)
            {
                throw new DecompilerException("Unexpected branch when building AST");
            }
            if (currentNode.Successors.Count == 1)
            {
                if (currentNode.Successors[0] == currentNode ||
                    currentNode.Successors[0].StartAddress < currentNode.StartAddress)
                {
                    throw new DecompilerException("Unresolved loop when building AST");
                }
                currentNode = currentNode.Successors[0];
            }
            else
            {
                currentNode = null;
            }
        }

        // If this block has more than 1 child, make sure it has curly braces around it
        if (block.Children.Count > 1)
        {
            block.UseBraces = true;
        }

        return block;
    }

    // List used for output of expressions, which should never have any statements
    private readonly List<IStatementNode> expressionOutput = new();

    /// <summary>
    /// Builds an expression (of unknown type) starting from a control flow node, 
    /// following all of its successors linearly.
    /// </summary>
    internal IExpressionNode BuildExpression(IControlFlowNode startNode, List<IStatementNode> output = null)
    {
        output ??= expressionOutput;
        int stackCountBefore = ExpressionStack.Count;

        // Advance through all successors, building expression
        var currentNode = startNode;
        while (currentNode is not null)
        {
            currentNode.BuildAST(this, output);

            if (currentNode.Successors.Count > 1)
            {
                throw new DecompilerException("Unexpected branch when building AST");
            }
            if (currentNode.Successors.Count == 1)
            {
                if (currentNode.Successors[0] == currentNode || 
                    currentNode.Successors[0].StartAddress < currentNode.StartAddress)
                {
                    throw new DecompilerException("Unresolved loop when building AST");
                }
                currentNode = currentNode.Successors[0];
            }
            else
            {
                currentNode = null;
            }
        }

        int stackCountAfter = ExpressionStack.Count;

        // Ensure we didn't produce any statements while evaluating expression
        if (expressionOutput.Count > 0)
        {
            throw new DecompilerException("Unexpected statement found while evaluating expression");
        }

        // Ensure we added exactly 1 expression to the stack while evaluating this expression (if desired)
        if (stackCountAfter != stackCountBefore + 1)
        {
            throw new DecompilerException(
                $"Unexpected change of stack count from {stackCountBefore} to " +
                $"{stackCountAfter} while evaluating expression");
        }

        // Return the expression that was evaluated
        return ExpressionStack.Pop();
    }

    /// <summary>
    /// Builds arbitrary expression AST starting from a control flow node, following all of its successors linearly.
    /// No statements can be created in this context, and at most a defined number of expressions can be created,
    /// or -1 if any number of expressions can be created.
    /// </summary>
    internal void BuildArbitrary(IControlFlowNode startNode, List<IStatementNode> output = null, int numAllowedExpressions = 0)
    {
        output ??= expressionOutput;
        int stackCountBefore = ExpressionStack.Count;

        // Advance through all successors, building expression
        var currentNode = startNode;
        while (currentNode is not null)
        {
            currentNode.BuildAST(this, output);

            if (currentNode.Successors.Count > 1)
            {
                throw new DecompilerException("Unexpected branch when building AST");
            }
            if (currentNode.Successors.Count == 1)
            {
                if (currentNode.Successors[0] == currentNode ||
                    currentNode.Successors[0].StartAddress < currentNode.StartAddress)
                {
                    throw new DecompilerException("Unresolved loop when building AST");
                }
                currentNode = currentNode.Successors[0];
            }
            else
            {
                currentNode = null;
            }
        }

        int stackCountAfter = ExpressionStack.Count;

        // Ensure we didn't produce any statements while evaluating (if desired)
        if (expressionOutput.Count > 0)
        {
            throw new DecompilerException("Unexpected statement found while evaluating arbitrary AST");
        }

        // Ensure we didn't add too many expressions to the stack
        if (numAllowedExpressions != -1 && stackCountAfter > (stackCountBefore + numAllowedExpressions))
        {
            throw new DecompilerException(
                $"Unexpected change of stack count from {stackCountBefore} to " +
                $"{stackCountAfter} while evaluating arbitrary AST");
        }
    }

    /// <summary>
    /// Pushes a new fragment onto the fragment context stack.
    /// Each fragment has its own expression stack, struct argument list, etc.
    /// </summary>
    internal void PushFragmentContext(Fragment fragment)
    {
        ASTFragmentContext context = new(fragment);
        TopFragmentContext?.Children.Add(context);
        FragmentContextStack.Push(context);
        TopFragmentContext = context;
    }

    /// <summary>
    /// Pops a fragment off of the fragment context stack.
    /// </summary>
    internal void PopFragmentContext()
    {
        ASTFragmentContext context = FragmentContextStack.Pop();
        if (context.ExpressionStack.Count > 0)
        {
            // TODO: maybe don't make this an exception, and instead use temp vars
            throw new DecompilerException("Data left over on stack");
        }

        // Add sub-function names to lookup, if any exist
        foreach (ASTFragmentContext child in context.Children)
        {
            if (child.FunctionName is not null)
            {
                context.SubFunctionNames[child.CodeEntryName] = child.FunctionName;
            }
        }

        // Update new top
        if (FragmentContextStack.Count > 0)
        {
            TopFragmentContext = FragmentContextStack.Peek();
        }
        else
        {
            TopFragmentContext = null;
        }
    }
}
