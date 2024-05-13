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
}

/// <summary>
/// Provided settings class that can be used by default.
/// </summary>
public class DecompileSettings : IDecompileSettings
{
    public string IndentString { get; set; } = "    ";
}
