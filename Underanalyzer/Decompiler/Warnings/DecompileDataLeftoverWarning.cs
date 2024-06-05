/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace Underanalyzer.Decompiler.Warnings;

/// <summary>
/// Represents a warning that occurs when data is left over on the VM stack at the end of a fragment.
/// </summary>
/// <remarks>With the default settings, this is not a warning, and is instead an exception.</remarks>
public class DecompileDataLeftoverWarning : IDecompileWarning
{
    /// <inheritdoc/>
    public string Message => $"Data left over on VM stack at end of fragment ({NumberOfElements} elements).";
    
    /// <inheritdoc/>
    public string CodeEntryName { get; }
    
    /// <summary>
    /// How many unread elements on the stack are left.
    /// </summary>
    public int NumberOfElements { get; }

    internal DecompileDataLeftoverWarning(int numberOfElements, string codeEntryName)
    {
        NumberOfElements = numberOfElements;
        CodeEntryName = codeEntryName;
    }
}
