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
    /// If true, semicolons are emitted after statements that generally have them.
    /// If false, some statements may still use semicolons to prevent ambiguity.
    /// </summary>
    public bool UseSemicolon { get; }

    /// <summary>
    /// If true, color constants are written in "#RRGGBB" notation, rather than the normal BGR ordering.
    /// Only applicable if "Constant.Color" is resolved as a macro type.
    /// </summary>
    public bool UseCSSColors { get; }

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
    /// If true, enum values that are detected in a code entry (including any unknown ones) will 
    /// be given declarations at the top of the code.
    /// </summary>
    public bool CreateEnumDeclarations { get; }

    /// <summary>
    /// Base type name for the enum representing all unknown enum values.
    /// Should be a valid enum name in GML, or null if the unknown enum should not be generated/used at all.
    /// </summary>
    public string UnknownEnumName { get; }

    /// <summary>
    /// Format string for the values in the enum representing all unknown enum values.
    /// Should be a valid enum value name in GML.
    /// </summary>
    public string UnknownEnumValuePattern { get; }

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
    public bool UseSemicolon { get; set; } = true;
    public bool UseCSSColors { get; set; } = true;
    public bool CleanupTry { get; set; } = true;
    public bool CleanupElseToContinue { get; set; } = true;
    public bool CreateEnumDeclarations { get; set; } = true;
    public string UnknownEnumName { get; set; } = "UnknownEnum";
    public string UnknownEnumValuePattern { get; set; } = "Value_{0}";
    public bool AllowLeftoverDataOnStack { get; set; } = false;
}
