/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer;

/// <summary>
/// Extension methods for <see cref="DataType"/>.
/// </summary>
internal static class VMDataTypeExtensions
{
    /// <summary>
    /// Given two <see cref="DataType"/> enumerations, this method returns the one that 
    /// should be biased towards in a binary operation.
    /// </summary>
    public static DataType BiasWith(this DataType type1, DataType type2)
    {
        // Type 1 and type 2 represent the left and right data types on the stack.
        // Choose whichever type has a higher bias, or if equal, the smaller numerical data type value.
        int bias1 = StackTypeBias(type1);
        int bias2 = StackTypeBias(type2);
        if (bias1 == bias2)
        {
            return (DataType)Math.Min((byte)type1, (byte)type2);
        }
        else
        {
            return (bias1 > bias2) ? type1 : type2;
        }
    }

    /// <summary>
    /// Returns the bias a given data type has in a binary operation. Larger is greater bias.
    /// </summary>
    private static int StackTypeBias(DataType type)
    {
        return type switch
        {
            DataType.Int32 or DataType.Boolean or DataType.String => 0,
            DataType.Double or DataType.Int64 => 1,
            DataType.Variable => 2,
            _ => throw new Exception("Unknown data type")
        };
    }
}
