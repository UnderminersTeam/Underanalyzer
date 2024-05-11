using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Manages cleaning/postprocessing of the AST.
/// </summary>
public class ASTCleaner
{
    /// <summary>
    /// The decompilation context this is cleaning for.
    /// </summary>
    public DecompileContext Context { get; }

    /// <summary>
    /// List of arguments passed into a struct fragment.
    /// </summary>
    internal List<IExpressionNode> StructArguments { get => TopFragmentContext.StructArguments; set => TopFragmentContext.StructArguments = value; }

    /// <summary>
    /// Set of all local variables present in the current fragment.
    /// </summary>
    internal HashSet<string> LocalVariableNames { get => TopFragmentContext.LocalVariableNames; }

    /// <summary>
    /// The stack used to manage fragment contexts.
    /// </summary>
    private Stack<ASTFragmentContext> FragmentContextStack { get; } = new();

    /// <summary>
    /// The current/top fragment context.
    /// </summary>
    internal ASTFragmentContext TopFragmentContext { get; private set; }

    public ASTCleaner(DecompileContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Pushes a context onto the fragment context stack.
    /// Each fragment has its own expression stack, struct argument list, etc.
    /// </summary>
    internal void PushFragmentContext(ASTFragmentContext context)
    {
        FragmentContextStack.Push(context);
        TopFragmentContext = context;
    }

    /// <summary>
    /// Pops a fragment off of the fragment context stack.
    /// </summary>
    internal void PopFragmentContext()
    {
        FragmentContextStack.Pop();
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
