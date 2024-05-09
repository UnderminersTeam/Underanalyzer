using System;
using System.Collections.Generic;

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
    internal Stack<IASTNode> ExpressionStack { get => TopFragmentContext.ExpressionStack; }

    /// <summary>
    /// The index to start processing instructions for the next ControlFlow.Block we encounter.
    /// Used by code fragments to skip processing instructions twice.
    /// </summary>
    internal int StartBlockInstructionIndex { get; set; } = 0;

    /// <summary>
    /// List of arguments passed into a struct fragment.
    /// </summary>
    internal List<IASTNode> StructArguments { get => TopFragmentContext.StructArguments; set => TopFragmentContext.StructArguments = value; }

    // Variables managing the fragment context stack state
    private Stack<ASTFragmentContext> FragmentContextStack { get; } = new();
    private ASTFragmentContext TopFragmentContext { get; set; }

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
    public IASTNode Build()
    {
        List<IASTNode> output = new(1);
        PushFragmentContext();
        Context.FragmentNodes[0].BuildAST(this, output);
        return output[0];
    }

    /// <summary>
    /// Builds a block starting from a control flow node, following all of its successors linearly.
    /// </summary>
    internal BlockNode BuildBlock(ControlFlow.IControlFlowNode startNode)
    {
        BlockNode block = new();

        // Advance through all successors, building out this block
        var currentNode = startNode;
        while (currentNode is not null)
        {
            currentNode.BuildAST(this, block.Children);

            if (currentNode.Successors.Count > 1)
            {
                throw new Exception("Unexpected branch");
            }
            if (currentNode.Successors.Count == 1)
            {
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

    /// <summary>
    /// Pushes a new fragment onto the fragment context stack.
    /// Each fragment has its own expression stack, struct argument list, etc.
    /// </summary>
    internal void PushFragmentContext()
    {
        ASTFragmentContext context = new();
        FragmentContextStack.Push(context);
        TopFragmentContext = context;
    }

    /// <summary>
    /// Pops a fragment off of the fragment context stack.
    /// </summary>
    internal void PopFragmentContext()
    {
        FragmentContextStack.Pop();
        TopFragmentContext = FragmentContextStack.Peek();
    }
}
