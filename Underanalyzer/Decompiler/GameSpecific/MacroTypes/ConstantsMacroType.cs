using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.GameSpecific;

/// <summary>
/// A macro type usable for general-purpose constants.
/// </summary>
public class ConstantsMacroType : IMacroTypeInt32
{
    private Dictionary<int, string> ValueToConstantName { get; }

    /// <summary>
    /// Constructs a macro type from a dictionary of constant values, from value to name.
    /// </summary>
    public ConstantsMacroType(Dictionary<int, string> constants)
    {
        ValueToConstantName = new(constants);
    }

    /// <summary>
    /// Constructs a macro type from a dictionary of constant values, from name to value.
    /// </summary>
    public ConstantsMacroType(Dictionary<string, int> constants)
    {
        ValueToConstantName = new(constants.Count);
        foreach ((string name, int value) in constants)
        {
            ValueToConstantName[value] = name;
        }
    }

    /// <summary>
    /// Constructs a macro type from an enum, where value names are the constant names, 
    /// associated with their enum values.
    /// </summary>
    public ConstantsMacroType(Type enumType)
    {
        foreach (int value in Enum.GetValues(enumType))
        {
            ValueToConstantName[value] = Enum.GetName(enumType, value);
        }
    }

    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, int data)
    {
        if (ValueToConstantName.TryGetValue(data, out string name))
        {
            return new MacroValueNode(name);
        }
        return null;
    }
}
