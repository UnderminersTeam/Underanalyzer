using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Simple lookup for names to macro types.
/// </summary>
public class NameMacroTypeResolver : IMacroTypeResolver
{
    private Dictionary<string, IMacroType> Variables { get; }
    private Dictionary<string, IMacroType> FunctionArguments { get; }
    private Dictionary<string, IMacroType> FunctionReturn { get; }

    /// <summary>
    /// Initializes an empty name resolver.
    /// </summary>
    public NameMacroTypeResolver()
    {
        Variables = new();
        FunctionArguments = new();
        FunctionReturn = new();
    }

    /// <summary>
    /// Initializes a name resolver with pre-populated data.
    /// </summary>
    public NameMacroTypeResolver(Dictionary<string, IMacroType> variables, 
                                 Dictionary<string, IMacroType> functionArguments, 
                                 Dictionary<string, IMacroType> functionReturn)
    {
        Variables = new(variables);
        FunctionArguments = new(functionArguments);
        FunctionReturn = new(functionReturn);
    }

    /// <summary>
    /// Defines a variable's macro type for this resolver.
    /// </summary>
    public void DefineVariableType(string name, IMacroType type)
    {
        Variables[name] = type;
    }

    /// <summary>
    /// Defines a function's arguments macro type for this resolver.
    /// </summary>
    public void DefineFunctionArgumentsType(string name, IMacroType type)
    {
        FunctionArguments[name] = type;
    }

    /// <summary>
    /// Defines a function's return macro type for this resolver.
    /// </summary>
    public void DefineFunctionReturnType(string name, IMacroType type)
    {
        FunctionReturn[name] = type;
    }

    public IMacroType ResolveVariableType(ASTCleaner cleaner, string variableName)
    {
        if (Variables.TryGetValue(variableName, out IMacroType macroType))
        {
            return macroType;
        }
        return null;
    }

    public IMacroType ResolveFunctionArgumentTypes(ASTCleaner cleaner, string functionName)
    {
        if (FunctionArguments.TryGetValue(functionName, out IMacroType macroType))
        {
            return macroType;
        }
        return null;
    }

    public IMacroType ResolveReturnValueType(ASTCleaner cleaner, string functionName)
    {
        if (FunctionReturn.TryGetValue(functionName, out IMacroType macroType))
        {
            return macroType;
        }
        return null;
    }
}