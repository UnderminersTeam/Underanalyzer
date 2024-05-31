using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a reference to a single enum value in the AST.
/// </summary>
public class EnumValueNode : IExpressionNode, IMacroResolvableNode, IConditionalValueNode
{
    /// <summary>
    /// The name of the base enum type being referenced.
    /// </summary>
    public string EnumName { get; }

    /// <summary>
    /// The name of the value on the enum being referenced.
    /// </summary>
    public string EnumValueName { get; }

    /// <summary>
    /// The raw value of the enum value.
    /// </summary>
    public long EnumValue { get; }

    /// <summary>
    /// If true, this enum value node references an unknown enum.
    /// </summary>
    public bool IsUnknownEnum { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int64;

    public string ConditionalTypeName => "EnumValue";
    public string ConditionalValue => IsUnknownEnum ? EnumValue.ToString() : $"{EnumName}.{EnumValueName}";

    public EnumValueNode(string enumName, string enumValueName, long enumValue, bool isUnknownEnum)
    {
        EnumName = enumName;
        EnumValueName = enumValueName;
        EnumValue = enumValue;
        IsUnknownEnum = isUnknownEnum;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        if (Group)
        {
            printer.Write('(');
        }
        printer.Write(EnumName);
        printer.Write('.');
        printer.Write(EnumValueName);
        if (Group)
        {
            printer.Write(')');
        }
    }

    public IExpressionNode ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        if (type is IMacroTypeInt64 type64)
        {
            string enumNameBefore = EnumName;
            if (type64.Resolve(cleaner, this, EnumValue) is IExpressionNode resolved)
            {
                // Dereference the unknown enum, if applicable
                if (IsUnknownEnum && (resolved is not EnumValueNode enumValueNode || enumValueNode.EnumName != enumNameBefore))
                {
                    cleaner.Context.UnknownEnumReferenceCount--;
                    if (cleaner.Context.UnknownEnumReferenceCount == 0)
                    {
                        // Remove declaration altogether - it's no longer referenced
                        cleaner.Context.NameToEnumDeclaration.Remove(EnumName);
                        cleaner.Context.EnumDeclarations.Remove(cleaner.Context.UnknownEnumDeclaration);
                        cleaner.Context.UnknownEnumDeclaration = null;
                    }
                }
                return resolved;
            }
        }
        return null;
    }
}
