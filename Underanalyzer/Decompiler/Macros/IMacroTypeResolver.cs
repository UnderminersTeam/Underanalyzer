using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Base interface for all macro type resolvers.
/// Different types of macro type resolvers work in different contexts.
/// </summary>
public interface IMacroTypeResolver
{
    /// <summary>
    /// Resolves a macro type for a variable name on this resolver, or null if none is found.
    /// </summary>
    public IMacroType ResolveVariableType(ASTCleaner cleaner, string variableName);

    /// <summary>
    /// Resolves a macro type for a function's arguments on this resolver, or null if none is found.
    /// </summary>
    public IMacroType ResolveFunctionArgumentTypes(ASTCleaner cleaner, string functionName);

    /// <summary>
    /// Resolves a macro type for a function's return value on this resolver, or null if none is found.
    /// </summary>
    public IMacroType ResolveReturnValueType(ASTCleaner cleaner, string functionName);
}
