namespace Underanalyzer.Decompiler;

/// <summary>
/// Describes the necessary settings properties for the decompiler.
/// </summary>
public interface IDecompileSettings
{
    /// <summary>
    /// String used to indent, e.g. tabs or some amount of spaces generally.
    /// </summary>
    public string IndentString { get; }

    /// <summary>
    /// Whether try/catch/finally statements should have their compiler-generated control flow cleaned up.
    /// This cleanup can occasionally be inaccurate to the code that actually executes, due to multiple compiler bugs.
    /// </summary>
    public bool CleanupTry { get; }

    /// <summary>
    /// Whether empty if/else chains at the end of a loop body should be rewritten as continue statements, when possible.
    /// </summary>
    public bool CleanupElseToContinue { get; }

    /// <summary>
    /// Whether leftover data on the simulated VM stack will be allowed in decompilation output. 
    /// If false, an exception is thrown when data is left over on the stack at the end of a fragment.
    /// If true, a warning is added to the decompile context.
    /// </summary>
    public bool AllowLeftoverDataOnStack { get; }
}

/// <summary>
/// Provided settings class that can be used by default.
/// </summary>
public class DecompileSettings : IDecompileSettings
{
    public string IndentString { get; set; } = "    ";
    public bool CleanupTry { get; set; } = true;
    public bool CleanupElseToContinue { get; set; } = true;
    public bool AllowLeftoverDataOnStack { get; set; } = false;
}
