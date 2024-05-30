using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Macro type resolver for a global game context. Delegates resolution to individual code entries, then global lookup.
/// Nothing within this should be modified while decompilation is in progress that uses this object.
/// </summary>
public class GlobalMacroTypeResolver : IMacroTypeResolver
{
    public NameMacroTypeResolver GlobalNames { get; set; }
    private Dictionary<string, NameMacroTypeResolver> CodeEntryNames { get; }

    /// <summary>
    /// Initializes an empty global resolver.
    /// </summary>
    public GlobalMacroTypeResolver()
    {
        GlobalNames = new NameMacroTypeResolver();
        CodeEntryNames = new();
    }

    /// <summary>
    /// Defines a name resolver for a specific code entry.
    /// </summary>
    public void DefineCodeEntry(string codeEntry, NameMacroTypeResolver resolver)
    {
        CodeEntryNames[codeEntry] = resolver;
    }

    public IMacroType ResolveVariableType(ASTCleaner cleaner, string variableName)
    {
        if (CodeEntryNames.TryGetValue(cleaner.TopFragmentContext.CodeEntryName, out NameMacroTypeResolver resolver))
        {
            IMacroType resolved = resolver.ResolveVariableType(cleaner, variableName);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        return GlobalNames.ResolveVariableType(cleaner, variableName);
    }

    public IMacroType ResolveFunctionArgumentTypes(ASTCleaner cleaner, string functionName)
    {
        if (CodeEntryNames.TryGetValue(cleaner.TopFragmentContext.CodeEntryName, out NameMacroTypeResolver resolver))
        {
            IMacroType resolved = resolver.ResolveFunctionArgumentTypes(cleaner, functionName);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        return GlobalNames.ResolveFunctionArgumentTypes(cleaner, functionName);
    }

    public IMacroType ResolveReturnValueType(ASTCleaner cleaner, string functionName)
    {
        if (CodeEntryNames.TryGetValue(cleaner.TopFragmentContext.CodeEntryName, out NameMacroTypeResolver resolver))
        {
            IMacroType resolved = resolver.ResolveReturnValueType(cleaner, functionName);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        return GlobalNames.ResolveReturnValueType(cleaner, functionName);
    }
}
