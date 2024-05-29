using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a 64-bit signed integer constant in the AST.
/// </summary>
public class Int64Node : IConstantNode<long>
{
    public long Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Int64;

    public Int64Node(long value)
    {
        Value = value;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        // TODO: macro/enum resolution

        // If we aren't detected as an enum yet, and we're within signed 32-bit range, we assume this is an unknown enum
        if (Value >= int.MinValue && Value <= int.MaxValue)
        {
            // Check if we have an unknown enum name to use (if null, we don't generate/use one at all)
            string unknownEnumName = cleaner.Context.Settings.UnknownEnumName;
            if (unknownEnumName is not null)
            {
                string enumValueName;
                if (cleaner.Context.UnknownEnumDeclaration is null)
                {
                    // Create a new unknown enum declaration, populated with this enum value
                    enumValueName = string.Format(cleaner.Context.Settings.UnknownEnumValuePattern, Value);
                    cleaner.Context.UnknownEnumDeclaration = new GMEnum(unknownEnumName, [new(enumValueName, Value)]);
                    cleaner.DeclareEnum(cleaner.Context.UnknownEnumDeclaration);
                }
                else
                {
                    // If the enum doesn't already contain this value, add this new one
                    if (cleaner.Context.UnknownEnumDeclaration.FindValue(Value) is not GMEnumValue gmEnumValue)
                    {
                        enumValueName = string.Format(cleaner.Context.Settings.UnknownEnumValuePattern, Value);
                        cleaner.Context.UnknownEnumDeclaration.AddValue(enumValueName, Value);
                    }
                    else
                    {
                        // We have an existing name already on the enum declaration; use it
                        enumValueName = gmEnumValue.Name;
                    }
                }
                
                // Turn into reference to this enum
                return new EnumValueNode(unknownEnumName, enumValueName);
            }
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write(Value);
    }
}
