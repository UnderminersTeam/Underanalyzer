/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace Underanalyzer.Decompiler;

/// <summary>
/// Describes the necessary settings properties for the decompiler.
/// </summary>
public interface IDecompileSettings
{
    // TODO: more settings :)
    
    
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
    /// If true, decompiler warnings will be printed as comments in the code.
    /// </summary>
    public bool PrintWarnings { get; }

    /// <summary>
    /// If true, macro declarations (such as enums) will be placed at the top of the code output, rather than the bottom.
    /// </summary>
    public bool MacroDeclarationsAtTop { get; }

    /// <summary>
    /// If true, an empty line will be inserted after local variable declarations belonging to a block.
    /// </summary>
    public bool EmptyLineAfterBlockLocals { get; }

    /// <summary>
    /// If true, an empty line will be inserted either before/after enum declarations, 
    /// depending on if placed at the top or bottom of the code.
    /// </summary>
    public bool EmptyLineAroundEnums { get; }

    /// <summary>
    /// If true, empty lines will be inserted before and/or after branch statements, unless at the start/end of a block.
    /// </summary>
    /// <remarks>
    /// This applies to <c>if</c>/<c>else</c>, <c>switch</c>, <c>try</c>/<c>catch</c>/<c>finally</c>, as well as all loops.
    /// </remarks>
    public bool EmptyLineAroundBranchStatements { get; }

    /// <summary>
    /// If true, empty lines will be inserted before case statements, unless at the start of a block.
    /// </summary>
    public bool EmptyLineBeforeSwitchCases { get; }

    /// <summary>
    /// If true, empty lines will be inserted after case statements, unless at the end of a block.
    /// </summary>
    public bool EmptyLineAfterSwitchCases { get; }

    /// <summary>
    /// If true, empty lines will be inserted before and/or after function declarations, unless at the start/end of a block,
    /// or in an expression (with the exception of in the right side of assignment statements).
    /// </summary>
    public bool EmptyLineAroundFunctionDeclarations { get; }

    /// <summary>
    /// If true, empty lines will be inserted before and/or after static initialization blocks, unless at the start/end of a block.
    /// </summary>
    public bool EmptyLineAroundStaticInitialization { get; }

    /// <summary>
    /// If true, opening curly braces at the start of blocks will be placed on the same line as the 
    /// current code, rather than on the next line.
    /// </summary>
    public bool OpenBlockBraceOnSameLine { get; }

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
    public bool PrintWarnings { get; set; } = true;
    public bool MacroDeclarationsAtTop { get; set; } = false;
    public bool EmptyLineAroundBranchStatements { get; set; } = false;
    public bool EmptyLineBeforeSwitchCases { get; set; } = false;
    public bool EmptyLineAfterSwitchCases { get; set; } = false;
    public bool EmptyLineAroundFunctionDeclarations { get; set; } = true;
    public bool EmptyLineAroundStaticInitialization { get; set; } = true;
    public bool OpenBlockBraceOnSameLine { get; set; } = false;
    public bool CleanupTry { get; set; } = true;
    public bool CleanupElseToContinue { get; set; } = true;
    public bool CreateEnumDeclarations { get; set; } = true;
    public string UnknownEnumName { get; set; } = "UnknownEnum";
    public string UnknownEnumValuePattern { get; set; } = "Value_{0}";
    public bool AllowLeftoverDataOnStack { get; set; } = false;
    public bool EmptyLineAfterBlockLocals { get; set; } = true;
    public bool EmptyLineAroundEnums { get; set; } = true;
}
