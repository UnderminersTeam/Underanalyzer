/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace Underanalyzer.Mock;

/// <summary>
/// A default implementation of the <see cref="IGMCode"/> interface.
/// </summary>
public class GMCode : IGMCode
{
    /// <summary>
    /// The name of the code entry.
    /// </summary>
    public GMString Name { get; set; }
    
    /// <inheritdoc/>
    public int Length { get; set; } = 0;
    
    /// <summary>
    /// A list of instructions this entry has.
    /// </summary>
    public List<GMInstruction> Instructions { get; set; }
    
    /// <summary>
    /// The parent code entry.
    /// </summary>
    public GMCode Parent { get; set; } = null;
    
    /// <summary>
    /// A list of child code entries.
    /// </summary>
    public List<GMCode> Children { get; set; } = new();
    
    /// <inheritdoc/>
    public int StartOffset { get; set; } = 0;
    
    /// <inheritdoc/>
    public int ArgumentCount { get; set; } = 1;
    
    /// <inheritdoc/>
    public int LocalCount { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="GMCode"/> class.
    /// </summary>
    /// <param name="name">The name of the code entry.</param>
    /// <param name="instructions">A list of instructions.</param>
    public GMCode(string name, List<GMInstruction> instructions)
    {
        Name = new(name);
        Instructions = instructions;
    }

    // Interface implementation
    
    /// <inheritdoc/>
    IGMString IGMCode.Name => Name;
    
    /// <inheritdoc/>
    public int InstructionCount => Instructions.Count;
    
    /// <inheritdoc/>
    IGMCode IGMCode.Parent => Parent;
    
    /// <inheritdoc/>
    public int ChildCount => Children.Count;

    
    /// <inheritdoc/>
    public IGMCode GetChild(int index) => Children[index];
    
    /// <inheritdoc/>
    public IGMInstruction GetInstruction(int index) => Instructions[index];

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(GMCode)}: {Name.Content} ({Instructions.Count} instructions, length {Length}, {ArgumentCount} args, {LocalCount} locals, offset {StartOffset})";
    }
}

/// <summary>
/// A default implementation of the <see cref="IGMInstruction"/> interface.
/// </summary>
public class GMInstruction : IGMInstruction
{
    /// <inheritdoc/>
    public int Address { get; set; }
    
    /// <inheritdoc/>
    public IGMInstruction.Opcode Kind { get; set; }
    
    /// <inheritdoc/>
    public IGMInstruction.ExtendedOpcode ExtKind { get; set; }
    
    /// <inheritdoc/>
    public IGMInstruction.ComparisonType ComparisonKind { get; set; }
    
    /// <inheritdoc/>
    public IGMInstruction.DataType Type1 { get; set; }
    
    /// <inheritdoc/>
    public IGMInstruction.DataType Type2 { get; set; }
    
    /// <inheritdoc/>
    public IGMInstruction.InstanceType InstType { get; set; }
    
    /// <inheritdoc/>
    public IGMVariable Variable { get; set; }
    
    /// <inheritdoc/>
    public IGMFunction Function { get; set; }
    
    /// <inheritdoc/>
    public IGMInstruction.VariableType ReferenceVarType { get; set; }
    
    /// <inheritdoc/>
    public double ValueDouble { get; set; }
    
    /// <inheritdoc/>
    public short ValueShort { get; set; }
    
    /// <inheritdoc/>
    public int ValueInt { get; set; }
    
    /// <inheritdoc/>
    public long ValueLong { get; set; }
    
    /// <inheritdoc/>
    public bool ValueBool { get; set; }
    
    /// <inheritdoc/>
    public IGMString ValueString { get; set; }
    
    /// <inheritdoc/>
    public int BranchOffset { get => ValueInt; set => ValueInt = value; }
    
    /// <inheritdoc/>
    public bool PopWithContextExit { get => ValueBool; set => ValueBool = value; }
    
    /// <inheritdoc/>
    public byte DuplicationSize { get; set; }
    
    /// <inheritdoc/>
    public byte DuplicationSize2 { get; set; }
    
    /// <inheritdoc/>
    public int ArgumentCount { get => ValueInt; set => ValueInt = value; }
    
    /// <inheritdoc/>
    public int PopSwapSize { get => ValueInt; set => ValueInt = value; }
    
    /// <inheritdoc/>
    public int AssetReferenceId { get => ValueInt; set => ValueInt = value; }
    
    /// <inheritdoc cref="GetAssetReferenceType"/>
    public AssetType AssetReferenceType { get; set; }
    
    /// <inheritdoc/>
    public AssetType GetAssetReferenceType(IGameContext context) => AssetReferenceType;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(GMInstruction)}: {Kind} (address {Address})";
    }
}

/// <summary>
/// A default implementation of the <see cref="IGMString"/> interface.
/// </summary>
public class GMString : IGMString
{
    /// <inheritdoc/>
    public string Content { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GMString"/> class.
    /// </summary>
    /// <param name="content">The content of a string.</param>
    public GMString(string content) => Content = content;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(GMString)}: {Content}";
    }
}

/// <summary>
/// A default implementation of the <see cref="IGMVariable"/> interface.
/// </summary>
public class GMVariable : IGMVariable
{
    /// <inheritdoc/>
    public IGMString Name { get; set; }

    /// <inheritdoc/>
    public IGMInstruction.InstanceType InstanceType { get; set; }

    /// <inheritdoc/>
    public int VariableID { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(GMVariable)}: {Name.Content} ({InstanceType})";
    }
}

/// <summary>
/// Represents a comparison operation between <see cref="GMVariable"/>s.
/// </summary>
public class GMVariableComparer : IEqualityComparer<GMVariable>
{
    /// <inheritdoc/>
    public bool Equals(GMVariable x, GMVariable y)
    {
        return x.Name.Content == y.Name.Content && x.InstanceType == y.InstanceType && x.VariableID == y.VariableID;
    }

    /// <inheritdoc/>
    public int GetHashCode(GMVariable obj)
    {
        return (obj.Name.Content, obj.InstanceType, obj.VariableID).GetHashCode();
    }
}

/// <summary>
/// A default implementation of the <see cref="IGMFunction"/> interface.
/// </summary>
public class GMFunction : IGMFunction
{
    /// <inheritdoc/>
    public IGMString Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GMFunction"/> class.
    /// </summary>
    /// <param name="name">The function name.</param>
    public GMFunction(string name) => Name = new GMString(name);

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(GMFunction)}: {Name.Content}";
    }
}

/// <summary>
/// Represents a comparison operation between <see cref="GMFunction"/>s. 
/// </summary>
public class GMFunctionComparer : IEqualityComparer<GMFunction>
{
    /// <inheritdoc/>
    public bool Equals(GMFunction x, GMFunction y)
    {
        return x.Name.Content == y.Name.Content;
    }

    /// <inheritdoc/>
    public int GetHashCode(GMFunction obj)
    {
        return obj.Name.Content.GetHashCode();
    }
}
