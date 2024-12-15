/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace Underanalyzer.Compiler;

/// <summary>
/// Structure used to track data at the level of a specific function/event/script scope.
/// </summary>
internal sealed class FunctionScope
{
    // Set of locals declared for this scope
    private readonly HashSet<string> _declaredLocals = new(8);

    // List (in order) of locals declared for this scope
    private readonly List<string> _localsOrder = new(8);

    /// <summary>
    /// Declares a local variable for this function scope.
    /// </summary>
    /// <param name="name">Name of the local variable to be declared.</param>
    /// <returns>True if the local was not yet declared; false otherwise.</returns>
    public bool DeclareLocal(string name)
    {
        if (_declaredLocals.Add(name))
        {
            _localsOrder.Add(name);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns whether or not a local variable is declared for this function scope.
    /// </summary>
    /// <param name="name">Name of the local variable to check.</param>
    /// <returns>True if the local variable has been declared; false otherwise.</returns>
    public bool IsLocalDeclared(string name)
    {
        return _declaredLocals.Contains(name);
    }
}
