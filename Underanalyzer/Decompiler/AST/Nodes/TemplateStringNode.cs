/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
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
        int nextExpectedFieldIndex = 0;
        int maxExpectedFieldIndex = Fields.Count - 1;
        for (int i = 0; i < format.Length; i++)
        {
            // Print non-placeholder starting characters normally
            if (format[i] != '{')
            {
                printer.Write(StringNode.EscapeChar(format[i]));
                continue;
            }

            // Parse the placeholder (may not necessarily be valid)
            int startIndex = i + 1;
            int j = startIndex;
            bool invalidCharacter = false;
            while (j < format.Length && format[j] != '}')
            {
                if (format[j] < '0' || format[j] > '9')
                {
                    invalidCharacter = true;
                    break;
                }
                j++;
            }
            if (invalidCharacter || j >= format.Length || j == startIndex)
            {
                // Invalid - escape the starting character!
                printer.Write("\\{");
                continue;
            }
            ReadOnlySpan<char> placeholderNumberText = format.AsSpan()[startIndex..j];
            if (!int.TryParse(placeholderNumberText, out int fieldIndex))
            {
                // Invalid - escape the starting character!
                printer.Write("\\{");
                continue;
            }
            if (fieldIndex < 0 || fieldIndex > maxExpectedFieldIndex || fieldIndex != nextExpectedFieldIndex)
            {
                // Totally invalid or unexpected field index... this should have been handled already!
                throw new DecompilerException("Unexpected string template placeholder index");
            }

            // Print the actual field
            printer.Write('{');
            Fields[fieldIndex].Print(printer);
            printer.Write('}');

            // Move on to next field - should be in linear order for regular interpolated strings
            nextExpectedFieldIndex++;

            // Advance to the end of the placeholder
            i = j;
        }

        // If we didn't use exactly the number of expected indices, that's a problem... this should've been handled already!
        if (nextExpectedFieldIndex != (maxExpectedFieldIndex + 1))
        {
            throw new DecompilerException("Unexpected final string template placeholder index");
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
        if (type is IMacroTypeConditional conditional)
        {
            return conditional.Resolve(cleaner, this);
        }
        return null;
    }
}
