/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Nodes;

namespace Underanalyzer.Compiler.Parser;

/// <summary>
/// A context for the parser, for a single code entry.
/// </summary>
internal sealed class ParseContext(CompileContext context, List<IToken> tokens)
{
    /// <summary>
    /// The compile context for the overarching code entry.
    /// </summary>
    public CompileContext CompileContext { get; } = context;

    /// <summary>
    /// List of tokens to be parsed by this context.
    /// </summary>
    public List<IToken> Tokens { get; } = tokens;

    /// <summary>
    /// Root node as parsed by this context.
    /// </summary>
    public IASTNode? Root { get; private set; } = null;

    /// <summary>
    /// List of tokens to be parsed by this context.
    /// </summary>
    public int Position { get; set; } = 0;

    /// <summary>
    /// True if reached the end of code; false otherwise.
    /// </summary>
    public bool EndOfCode => Position >= Tokens.Count;

    /// <summary>
    /// Performs a full parse of the list of tokens, returning the root block 
    /// node of the resulting AST.
    /// </summary>
    public void Parse()
    {
        BlockNode root = new();
        Root = root;
        root.ParseRoot(this);
    }

    /// <summary>
    /// Skips semicolon tokens.
    /// </summary>
    public void SkipSemicolons()
    {
        while (!EndOfCode && Tokens[Position] is TokenSeparator { Kind: SeparatorKind.Semicolon })
        {
            Position++;
        }
    }

    /// <summary>
    /// Ensures that there is a token of a given separator type at the current position.
    /// Pushes an error if unsuccessful.
    /// </summary>
    /// <returns>The matching token, or null if unsuccessful.</returns>
    public IToken? EnsureToken(SeparatorKind kind)
    {
        if (EndOfCode)
        {
            CompileContext.PushError($"Unexpected end of code (expected '{TokenSeparator.KindToString(kind)}')");
            return null;
        }

        IToken token = Tokens[Position++];
        if (token is not TokenSeparator separator || separator.Kind != kind)
        {
            CompileContext.PushError($"Expected '{TokenSeparator.KindToString(kind)}', got '{token}'", token);
            return null;
        }

        return token;
    }

    /// <summary>
    /// Ensures that there is a token of a given separator or keyword type at the current position.
    /// Pushes an error if unsuccessful.
    /// </summary>
    /// <returns>True if successful; false otherwise.</returns>
    public IToken? EnsureToken(SeparatorKind separatorKind, KeywordKind keywordKind)
    {
        if (EndOfCode)
        {
            CompileContext.PushError($"Unexpected end of code (expected '{TokenSeparator.KindToString(separatorKind)}' or '{TokenKeyword.KindToString(keywordKind)}')");
            return null;
        }

        IToken token = Tokens[Position++];
        if ((token is not TokenSeparator separator || separator.Kind != separatorKind) &&
            (token is not TokenKeyword keyword || keyword.Kind != keywordKind))
        {
            CompileContext.PushError($"Expected '{TokenSeparator.KindToString(separatorKind)}' or '{TokenKeyword.KindToString(keywordKind)}', got '{token}'", token);
            return null;
        }

        return token;
    }

    /// <summary>
    /// Ensures that there is a token of a given keyword type at the current position.
    /// Pushes an error if unsuccessful.
    /// </summary>
    /// <returns>True if successful; false otherwise.</returns>
    public IToken? EnsureToken(KeywordKind kind)
    {
        if (EndOfCode)
        {
            CompileContext.PushError($"Unexpected end of code (expected '{TokenKeyword.KindToString(kind)}')");
            return null;
        }

        IToken token = Tokens[Position++];
        if (token is not TokenKeyword separator || separator.Kind != kind)
        {
            CompileContext.PushError($"Expected '{TokenKeyword.KindToString(kind)}', got '{token}'", token);
            return null;
        }

        return token;
    }

    /// <summary>
    /// Returns true if the current token is a given separator type; false otherwise.
    /// </summary>
    public bool IsCurrentToken(SeparatorKind kind)
    {
        if (EndOfCode)
        {
            return false;
        }
        return Tokens[Position] is TokenSeparator separator && separator.Kind == kind;
    }

    /// <summary>
    /// Returns true if the current token is a given keyword type; false otherwise.
    /// </summary>
    public bool IsCurrentToken(KeywordKind kind)
    {
        if (EndOfCode)
        {
            return false;
        }
        return Tokens[Position] is TokenKeyword keyword && keyword.Kind == kind;
    }

    /// <summary>
    /// Returns true if the current token is a given separator type or keyword type; false otherwise.
    /// </summary>
    public bool IsCurrentToken(SeparatorKind separatorKind, KeywordKind keywordKind)
    {
        if (EndOfCode)
        {
            return false;
        }
        return Tokens[Position] is TokenSeparator separator && separator.Kind == separatorKind ||
               Tokens[Position] is TokenKeyword keyword && keyword.Kind == keywordKind;
    }
}
