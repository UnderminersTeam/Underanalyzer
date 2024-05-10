using System;

namespace Underanalyzer.Decompiler;

public class DecompilerException : Exception
{
    public DecompilerException(string message)
        : base(message)
    {
    }

    public DecompilerException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
