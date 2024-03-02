using Underanalyzer.Mock;

namespace UnderanalyzerTest;

internal static class TestAssembly
{
    /// <summary>
    /// Utility function to reduce having to split lines in tests.
    /// </summary>
    public static GMCode GetCode(string assembly)
    {
        string[] lines = assembly.Split('\n');
        return VMAssembly.ParseAssemblyFromLines(lines);
    }
}
