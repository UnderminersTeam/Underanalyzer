/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler;
using System.Collections.Generic;

namespace Underanalyzer.Mock;

/// <summary>
/// A default implementation of <see cref="IBuiltins"/>.
/// </summary>
public class BuiltinsMock : IBuiltins
{
    /// <summary>
    /// Mock list of constant double values.
    /// </summary>
    public Dictionary<string, double> ConstantDoubles = new()
    {
        { "self", -1 },
        { "other", -2 },
        { "all", -3 },
        { "noone", -4 },
        { "global", -5 },
    };

    /// <summary>
    /// Mock map of builtin functions
    /// </summary>
    public Dictionary<string, BuiltinFunctionMock> BuiltinFunctions = new()
    {
        { VMConstants.SelfFunction, new(VMConstants.SelfFunction, 0, 0) },
        { VMConstants.OtherFunction, new(VMConstants.OtherFunction, 0, 0) },
        { VMConstants.GlobalFunction, new(VMConstants.GlobalFunction, 0, 0) },
        { VMConstants.GetInstanceFunction, new(VMConstants.GetInstanceFunction, 1, 1) },
        { VMConstants.MethodFunction, new(VMConstants.MethodFunction, 2, 2) },
        { VMConstants.NullObjectFunction, new(VMConstants.NullObjectFunction, 0, 0) },
        { VMConstants.NewObjectFunction, new(VMConstants.NewObjectFunction, 0, int.MaxValue) },
        { VMConstants.NewArrayFunction, new(VMConstants.NewArrayFunction, 0, int.MaxValue) },
        { VMConstants.SetStaticFunction, new(VMConstants.SetStaticFunction, 0, 0) },
        { VMConstants.CopyStaticFunction, new(VMConstants.CopyStaticFunction, 1, 1) },
        { VMConstants.StaticGetFunction, new(VMConstants.StaticGetFunction, 1, 1) }
    };

    /// <inheritdoc/>
    public IBuiltinFunction? LookupBuiltinFunction(string name)
    {
        return BuiltinFunctions.GetValueOrDefault(name);
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
        return ConstantDoubles.TryGetValue(name, out value);
    }
}

public class BuiltinFunctionMock(string name, int minArguments, int maxArguments) : IBuiltinFunction
{
    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    public int MinArguments { get; } = minArguments;

    /// <inheritdoc/>
    public int MaxArguments { get; } = maxArguments;
}