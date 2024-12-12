/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler;

namespace Underanalyzer.Mock;

/// <summary>
/// A default implementation of <see cref="IBuiltins"/>.
/// </summary>
public class BuiltinsMock : IBuiltins
{
    /// <inheritdoc/>
    public IBuiltinFunction? LookupBuiltinFunction(string name)
    {
        // TODO: implement
        return null;
    }

    /// <inheritdoc/>
    public IBuiltinVariable? LookupBuiltinVariable(string name)
    {
        // TODO: implement
        return null;
    }

    /// <inheritdoc/>
    public bool LookupConstantDouble(string name, out double value)
    {
        // TODO: implement
        value = 0;
        return false;
    }
}
