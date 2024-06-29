using System.Collections.Generic;

namespace Underanalyzer.Decompiler.GameSpecific;

public class NamedArgumentResolver
{
    private Dictionary<string, List<string>> FunctionArgumentNames { get; }

    /// <summary>
    /// Initializes an empty named argument resolver.
    /// </summary>
    public NamedArgumentResolver()
    {
        FunctionArgumentNames = new();
    }

    /// <summary>
    /// Defines a list of named arguments for the function with the given code entry name.
    /// </summary>
    public void DefineCodeEntry(string codeEntry, IEnumerable<string> names)
    {
        FunctionArgumentNames.Add(codeEntry, new(names));
    }

    /// <summary>
    /// Resolves a named argument for a given code entry, with the given argument index.
    /// Returns the name, or null if none is defined.
    /// </summary>
    public string ResolveArgument(string codeEntryName, int argumentIndex)
    {
        if (FunctionArgumentNames.TryGetValue(codeEntryName, out List<string> names))
        {
            if (argumentIndex >= 0 && argumentIndex < names.Count)
            {
                return names[argumentIndex];
            }
        }
        return null;
    }
}