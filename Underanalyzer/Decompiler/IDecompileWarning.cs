namespace Underanalyzer.Decompiler;

/// <summary>
/// Interface for types representing warnings emitted by the decompiler.
/// </summary>
public interface IDecompileWarning
{
    /// <summary>
    /// Human-readable message decribing the warning.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Code entry name where the warning was emitted.
    /// </summary>
    public string CodeEntryName { get; }
}
