﻿/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Errors;
using Underanalyzer.Compiler.Lexer;

namespace Underanalyzer.Compiler;

/// <summary>
/// A compilation context belonging to a single code entry in a game.
/// </summary>
public sealed class CompileContext(IGameContext gameContext)
{
    /// <summary>
    /// The game context this compile context belongs to.
    /// </summary>
    public IGameContext GameContext { get; } = gameContext;

    /// <summary>
    /// Macros declared by this code, or otherwise externally added.
    /// </summary>
    public Dictionary<string, Macro> Macros { get; } = new(4);

    /// <summary>
    /// List of errors generated by this compile context.
    /// </summary>
    public List<ICompileError> Errors { get; } = new(4);

    /// <summary>
    /// Pushes a lexer error to the list of errors generated for this compile context.
    /// </summary>
    internal void PushError(string message, LexContext lexContext, int textPosition)
    {
        Errors.Add(new LexerError(message, lexContext, textPosition));
    }

    /// <summary>
    /// Pushes a parser error to the list of errors generated for this compile context.
    /// </summary>
    internal void PushError(string message, IToken? nearbyToken = null)
    {
        Errors.Add(new ParserError(message, nearbyToken));
    }
}
