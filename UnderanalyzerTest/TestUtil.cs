using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

internal static class TestUtil
{
    /// <summary>
    /// Utility function to reduce having to split lines in tests.
    /// </summary>
    public static GMCode GetCode(string assembly)
    {
        string[] lines = assembly.Split('\n');
        return VMAssembly.ParseAssemblyFromLines(lines);
    }

    /// <summary>
    /// Asserts that for every predecessor, there is a corresponding successor, and vice versa.
    /// </summary>
    public static void VerifyFlowDirections(IEnumerable<IControlFlowNode> nodes)
    {
        foreach (var node in nodes)
        {
            foreach (var pred in node.Predecessors)
            {
                Assert.Contains(node, pred.Successors);
            }
            foreach (var succ in node.Successors)
            {
                Assert.Contains(node, succ.Predecessors);
            }
        }
    }
}
