using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a string constant in the AST.
/// </summary>
public class StringNode : IConstantNode<IGMString>
{
    public IGMString Value { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.String;

    public StringNode(IGMString value)
    {
        Value = value;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        ReadOnlySpan<char> content = Value.Content;
        if (printer.Context.GameContext.UsingGMS2OrLater)
        {
            // Handle string escaping.
            printer.Write('"');
            foreach (char c in content)
            {
                switch (c)
                {
                    case '\n':
                        printer.Write("\\n");
                        break;
                    case '\r':
                        printer.Write("\\r");
                        break;
                    case '\b':
                        printer.Write("\\b");
                        break;
                    case '\f':
                        printer.Write("\\f");
                        break;
                    case '\t':
                        printer.Write("\\t");
                        break;
                    case '\v':
                        printer.Write("\\v");
                        break;
                    case '\a':
                        printer.Write("\\a");
                        break;
                    case '\\':
                        printer.Write("\\\\");
                        break;
                    case '\"':
                        printer.Write("\\\"");
                        break;
                    default:
                        printer.Write(c);
                        break;
                }
            }
            printer.Write('"');
        }
        else
        {
            // We don't have any way of escaping strings - must concatenate multiple parts.
            // We also have the choice between ' and ", so use whichever results in less splits.
            int numDoubleQuotes = 0, numSingleQuotes = 0;
            foreach (char c in content)
            {
                if (c == '"')
                {
                    numDoubleQuotes++;
                }
                else if (c == '\'')
                {
                    numSingleQuotes++;
                }
            }
            char quoteChar = (numDoubleQuotes > numSingleQuotes) ? '\'' : '"';
            char splitChar = (numDoubleQuotes > numSingleQuotes) ? '"' : '\'';

            printer.Write(quoteChar);
            foreach (char c in content)
            {
                if (c == quoteChar)
                {
                    printer.Write(quoteChar);
                    printer.Write(" + ");
                    printer.Write(splitChar);
                    printer.Write(quoteChar);
                    printer.Write(splitChar);
                    printer.Write(" + ");
                    printer.Write(quoteChar);
                }
                else
                {
                    printer.Write(c);
                }
            }
            printer.Write(quoteChar);
        }
    }
}
