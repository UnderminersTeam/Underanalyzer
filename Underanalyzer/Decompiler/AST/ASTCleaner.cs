using System.Collections.Generic;
using Underanalyzer.Decompiler.Macros;

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

    /// <summary>
    /// Helper to access the global macro resolver used for resolving macro types.
    /// </summary>
    internal IMacroTypeResolver GlobalMacroResolver => Context.GameContext.MacroTypeRegistry.Resolver;

    /// <summary>
    /// Helper to access an ID instance and object type union, for resolving macro types.
    /// </summary>
    internal IMacroType MacroInstanceIdOrObjectAsset
    {
        get
        {
            if (_macroInstanceIdOrObjectAsset is null)
            {
                if (!Context.GameContext.MacroTypeRegistry.TypeExists("Id.Instance") ||
                    !Context.GameContext.MacroTypeRegistry.TypeExists("Asset.Object"))
                {
                    return null;
                }
                _macroInstanceIdOrObjectAsset = Context.GameContext.MacroTypeRegistry.FindTypeUnion(["Id.Instance", "Asset.Object"]);
            }
            return _macroInstanceIdOrObjectAsset;
        }
    }
    private IMacroType _macroInstanceIdOrObjectAsset = null;

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
    internal ASTFragmentContext PopFragmentContext()
    {
        ASTFragmentContext popped = FragmentContextStack.Pop();
        if (FragmentContextStack.Count > 0)
        {
            TopFragmentContext = FragmentContextStack.Peek();
        }
        else
        {
            TopFragmentContext = null;
        }
        return popped;
    }

    /// <summary>
    /// Helper function to declare a new enum.
    /// </summary>
    internal void DeclareEnum(GMEnum gmEnum)
    {
        Context.EnumDeclarations.Add(gmEnum);
        Context.NameToEnumDeclaration[gmEnum.Name] = gmEnum;
    }
}
