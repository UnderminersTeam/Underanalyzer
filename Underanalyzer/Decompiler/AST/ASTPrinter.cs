using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Manages the printing of all AST nodes.
/// </summary>
public class ASTPrinter
{
    /// <summary>
    /// The decompilation context this is printing for.
    /// </summary>
    public DecompileContext Context { get; private set; }

    /// <summary>
    /// The current string output of this printer. This should be used only when the result is needed.
    /// </summary>
    public string OutputString { get => stringBuilder.ToString(); }

    // Builder used to store resulting code
    private StringBuilder stringBuilder = new(128);

    // Management of indentation level
    private int indentLevel = 0;
    private List<string> indentStrings = new(4) { "" };
    private string indentString = "";

    // Management of newline placement
    private bool lineActive = false;
        
    public ASTPrinter(DecompileContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Indents the printer by the specified number of times (default 1).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Indent(int times = 1)
    {
        indentLevel += times;

        // Update cache of indent strings if needed
        for (int i = indentStrings.Count; i <= indentLevel; i++)
        {
            // TODO: support different indentation styles here, probably using Context.Settings
            indentStrings.Add(indentStrings[i - 1] + "    ");
        }

        // Set current indent string
        indentString = indentStrings[indentLevel];
    }

    /// <summary>
    /// Dedents the printer by the specified number of times (default 1).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dedent(int times = 1)
    {
        indentLevel -= times;

        // Ensure we don't dedent too far
        if (indentLevel < 0)
        {
            throw new InvalidOperationException("Indentation level was decreased more than it was increased");
        }

        // Set current indent string
        indentString = indentStrings[indentLevel];
    }

    /// <summary>
    /// Writes a character directly to the current position in the code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(char character)
    {
        stringBuilder.Append(character);
    }

    /// <summary>
    /// Writes a short value directly to the current position in the code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(short value)
    {
        stringBuilder.Append(value);
    }

    /// <summary>
    /// Writes an integer value directly to the current position in the code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int value)
    {
        stringBuilder.Append(value);
    }

    /// <summary>
    /// Writes a long value directly to the current position in the code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(long value)
    {
        stringBuilder.Append(value);
    }

    /// <summary>
    /// Writes text directly to the current position in the code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<char> text)
    {
        stringBuilder.Append(text);
    }

    /// <summary>
    /// Starts the current line of code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartLine()
    {
        if (lineActive)
        {
            // Prevent attempts to start the same line multiple times
            return;
        }
        stringBuilder.Append(indentString);
        lineActive = true;
    }

    /// <summary>
    /// Ends the current line of code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndLine()
    {
        if (!lineActive)
        {
            // Prevent attempts to end the same line multiple times
            return;
        }
        stringBuilder.Append('\n');
        lineActive = false;
    }

    /// <summary>
    /// Adds a semicolon to the current position in the code, if enabled.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Semicolon()
    {
        // TODO: use a setting to enable/disable this
        stringBuilder.Append(';');
    }

    /// <summary>
    /// Opens a block (with curly braces).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OpenBlock()
    {
        // TODO: handle other kinds of brace styles through settings
        EndLine();
        StartLine();
        Write('{');
        EndLine();
        Indent();
    }

    /// <summary>
    /// Closes a block (with curly braces).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CloseBlock()
    {
        // TODO: handle other kinds of brace styles through settings
        Dedent();
        StartLine();
        Write('}');
        EndLine();
    }
}
