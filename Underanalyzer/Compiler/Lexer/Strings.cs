/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Text;

namespace Underanalyzer.Compiler.Lexer;

/// <summary>
/// Helper for string literal lexing operations.
/// </summary>
internal static class Strings
{
    /// <summary>
    /// Parses a verbatim string literal from the given text position.
    /// </summary>
    public static int ParseVerbatim(LexContext context, int startPosition, char startChar)
    {
        int pos = startPosition;
        if (startChar == '@')
        {
            // Modern verbatim string; advance by one character
            startChar = context.Text[pos + 1];
            pos++;
        }    

        pos = ContiguousTextReader.ReadUntilChar(context.Text, pos + 1, startChar, out ReadOnlySpan<char> str);

        // If at the end of the code, the string was not correctly closed
        if (pos >= context.Text.Length)
        {
            context.CompileContext.PushError("String not closed", context, startPosition);
            return pos;
        }

        context.Tokens.Add(new TokenString(context, startPosition, context.Text[startPosition..(pos + 1)], str.ToString()));

        return pos + 1;
    }

    /// <summary>
    /// Parses a regular string literal from the given text position.
    /// </summary>
    public static int ParseRegular(LexContext context, int startPosition)
    {
        ReadOnlySpan<char> text = context.Text;
        StringBuilder sb = new(64);
        int pos = startPosition + 1;
        bool newlineErroredAlready = false;
        while (pos < text.Length)
        {
            char c = text[pos];

            // Stop at closing quote
            if (c == '"')
            {
                break;
            }

            pos = ParseChar(context, pos, sb, ref newlineErroredAlready);
        }

        // If at the end of the code, the string was not correctly closed
        if (pos >= text.Length)
        {
            context.CompileContext.PushError("String not closed", context, startPosition);
            return pos;
        }

        context.Tokens.Add(new TokenString(context, startPosition, text[startPosition..(pos + 1)].ToString(), sb.ToString()));

        return pos + 1;
    }

    /// <summary>
    /// Parses a template string literal from the given text position.
    /// </summary>
    public static int ParseTemplate(LexContext context, int startPosition)
    {
        ReadOnlySpan<char> text = context.Text;
        StringBuilder sb = new(64);
        int pos = startPosition + 2;
        bool newlineErroredAlready = false;

        context.Tokens.Add(new TokenTemplateStringStart(context, pos));
        while (pos < text.Length)
        {
            char c = text[pos];

            // Stop at closing quote
            if (c == '"')
            {
                break;
            }

            // Start of field
            if (c == '{')
            {
                context.Tokens.Add(new TokenSeparator(context, pos, SeparatorKind.BlockOpen));
                pos++;
                pos = context.Tokenize(pos, true);
                // '}' token is consumed by Tokenize
                continue;
            }

            // Any other character
            int tokenStartPos = pos;
            sb.Clear();
            while (pos < text.Length && text[pos] is not ('"' or '{'))
            {
                pos = ParseChar(context, pos, sb, ref newlineErroredAlready);
            }

            // If at the end of the code, the string was not correctly closed
            if (pos >= text.Length)
            {
                context.CompileContext.PushError("String not closed", context, startPosition);
                return pos;
            }

            // If we've read any characters, make a token for that section of the string
            if (pos != tokenStartPos)
            {
                context.Tokens.Add(new TokenTemplateStringMiddle(context, pos, text[tokenStartPos..pos].ToString(), sb.ToString()));
            }
        }

        // If at the end of the code, the string was not correctly closed
        if (pos >= text.Length)
        {
            context.CompileContext.PushError("String not closed", context, startPosition);
            return pos;
        }
        
        context.Tokens.Add(new TokenTemplateStringEnd(context, pos));

        return pos + 1;
    }
    
    /// <summary>
    /// Parses a single character within a string, appending it to the supplied StringBuilder, and returning the new text position.
    /// </summary>
    private static int ParseChar(LexContext context, int pos, StringBuilder sb, ref bool newlineErroredAlready)
    {
        ReadOnlySpan<char> text = context.Text;
        char c = text[pos];

        // Error on newlines
        if (c == '\n')
        {
            if (!newlineErroredAlready)
            {
                newlineErroredAlready = true;
                context.CompileContext.PushError("Direct newline found in string (should use \"\\n\" instead, or a raw string literal)", context, pos);
            }
            pos++;
            return pos;
        }

        if (c != '\\' || (pos + 1) >= text.Length)
        {
            // All non-backslash characters are just normal text
            sb.Append(c);
            pos++;
            return pos;
        }

        // Handle escape sequences
        int escapeStartPos = pos;
        char escapedChar = text[pos + 1];
        pos += 2;

        switch (escapedChar)
        {
            case 'a':
                sb.Append('\a');
                break;
            case 'b':
                sb.Append('\b');
                break;
            case 'f':
                sb.Append('\f');
                break;
            case 'n':
                sb.Append('\n');
                break;
            case 'r':
                sb.Append('\r');
                break;
            case 't':
                sb.Append('\t');
                break;
            case 'v':
                sb.Append('\v');
                break;
            case 'u':
                {
                    // Unicode character (as hex)
                    int result = 0;
                    int charsRead = 0;
                    while (pos < text.Length && charsRead < 6)
                    {
                        // Read current character as 4 bits (one hex digit)
                        int curr = HexCharToInt(text[pos]);
                        if (curr == -1)
                        {
                            // Not a valid hex digit, so stop here
                            break;
                        }
                        result = (result << 4) + curr;

                        pos++;
                        charsRead++;
                    }

                    // If at least one hex character was read, try to convert it all to a character
                    if (charsRead != 0)
                    {
                        try
                        {
                            sb.Append(char.ConvertFromUtf32(result));
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            context.CompileContext.PushError("\\u character code not valid in string", context, escapeStartPos);
                        }
                    }
                }
                break;
            case 'x':
                {
                    // Hex character
                    int result = 0;
                    int charsRead = 0;
                    while (pos < text.Length && charsRead < 2)
                    {
                        // Read current character as 4 bits (one hex digit)
                        int curr = HexCharToInt(text[pos]);
                        if (curr == -1)
                        {
                            // Not a valid hex digit, so stop here
                            break;
                        }
                        result = (result << 4) + curr;

                        pos++;
                        charsRead++;
                    }

                    // If exactly two characters were read, convert to a single character
                    if (charsRead == 2)
                    {
                        sb.Append((char)result);
                    }
                    else
                    {
                        context.CompileContext.PushError("\\x character code needs exactly 2 hex digits", context, escapeStartPos);
                    }
                }
                break;
            default:
                {
                    if (escapedChar >= '0' && escapedChar <= '7')
                    {
                        // Octal character
                        int result = 0;
                        int charsRead = 0;
                        pos--;
                        while (pos < text.Length && charsRead < 3)
                        {
                            // Read current character as octal
                            char octalChar = text[pos];
                            if (octalChar < '0' || octalChar > '7')
                            {
                                // Not a valid octal digit, so stop here
                                break;
                            }
                            result = (result * 8) + (octalChar - '0');

                            pos++;
                            charsRead++;
                        }

                        // If exactly three characters were read, convert to a single character
                        if (charsRead == 3)
                        {
                            sb.Append((char)result);
                        }
                        else
                        {
                            context.CompileContext.PushError($"\\{escapedChar}?? octal value in string is missing valid octal characters", context, escapeStartPos);
                        }
                    }
                    else
                    {
                        // Verbatim character
                        sb.Append(escapedChar);
                    }
                }
                break;
        }

        return pos;
    }

    /// <summary>
    /// Converts a hex character to its integer equivalent, or -1 if not a valid hex digit. 
    /// </summary>
    private static int HexCharToInt(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }
        if (c >= 'A' && c <= 'F')
        {
            return 10 + (c - 'A');
        }
        if (c >= 'a' && c <= 'f')
        {
            return 10 + (c - 'a');
        }
        return -1;
    }
}
