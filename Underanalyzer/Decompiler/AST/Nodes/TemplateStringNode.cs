/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Text;
using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a template string in the AST.
/// </summary>
public class TemplateStringNode(IGMString format, List<IExpressionNode> fields)
    : IExpressionNode, IConditionalValueNode
{
    /// <summary>
    /// The underlying format string.
    /// </summary>
    public IGMString Format { get; private set; } = format;

    /// <summary>
    /// The values to be inserted in the format string.
    /// </summary>
    public List<IExpressionNode> Fields { get; private set; } = fields;

    /// <inheritdoc/>
    public bool Duplicated { get; set; }

    /// <inheritdoc/>
    public bool Group { get; set; } = false;

    /// <inheritdoc/>
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public string ConditionalTypeName => "TemplateString";

    public string ConditionalValue => Format.Content;

    /// <inheritdoc/>
    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        for (int i = 0; i < Fields.Count; i++)
        {
            Fields[i] = Fields[i].Clean(cleaner);
        }
        return this;
    }

    public IEnumerable<IBaseASTNode> EnumerateChildren()
    {
        return Fields;
    }

    public IExpressionNode PostClean(ASTCleaner cleaner)
    {
        for (int i = 0; i < Fields.Count; i++)
        {
            Fields[i] = Fields[i].PostClean(cleaner);
        }
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("$\"");

        string format = Format.Content;
        // for each field, whether or not a placeholder exists
        bool[] fieldsSeen = new bool[Fields.Count];
        for (int i = 0; i < format.Length; i++)
        {
            if (format[i] != '{')
            {
                printer.Write(StringNode.EscapeChar(format[i]));
                continue;
            }

            // parse a placeholder

            i++;
            bool valid = i < format.Length && format[i] != '}';
            int index = 0;
            StringBuilder placeholderSb = new("\\{"); // also parse as text in case it's invalid
            while (i < format.Length && format[i] != '}')
            {
                placeholderSb.Append(StringNode.EscapeChar(format[i]));
                if (format[i] < '0' || format[i] > '9')
                {
                    valid = false;
                }
                if (valid)
                {
                    index = index * 10 + (format[i] - '0');
                }
                i++;
            }
            if (i >= format.Length)
            {
                printer.Write(placeholderSb.ToString());
                continue;
            }
            if (!valid)
            {
                printer.Write(placeholderSb.ToString());
                printer.Write("\\}");
                continue;
            }

            if (index >= fieldsSeen.Length)
            {
                // this should have been taken care of in the cleanup step...
                throw new DecompilerException("Out-of-range placeholder in template string format");
            }
            if (fieldsSeen[index])
            {
                // this too
                throw new DecompilerException("Duplicate placeholder in template string format");
            }
            fieldsSeen[index] = true;

            printer.Write('{');
            Fields[index].Print(printer);
            printer.Write('}');
        }

        printer.Write('"');
    }

    public bool RequiresMultipleLines(ASTPrinter printer)
    {
        foreach (var field in Fields)
        {
            if (field.RequiresMultipleLines(printer))
            {
                return true;
            }
        }
        return false;
    }

    public IExpressionNode? ResolveMacroType(ASTCleaner cleaner, IMacroType type)
    {
        throw new System.NotImplementedException();
    }
}