﻿/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;

namespace Underanalyzer.Compiler.Lexer;

/// <summary>
/// Helper for identifier/keyword lexing operations.
/// </summary>
internal static class Identifiers
{
    /// <summary>
    /// Parses an identifier/keyword from the given text position.
    /// </summary>
    public static int Parse(LexContext context, int startPosition)
    {
        int pos = ContiguousTextReader.ReadWhileIdentifier(context.Text, startPosition, out ReadOnlySpan<char> identifier);

        // Try to identify a keyword
        KeywordKind keywordKind = identifier switch
        {
            "if" => KeywordKind.If,       
            "then" => KeywordKind.Then,     
            "else" => KeywordKind.Else,     
            "switch" => KeywordKind.Switch,   
            "case" => KeywordKind.Case,
            "default" => KeywordKind.Default,
            "begin" => KeywordKind.Begin,
            "end" => KeywordKind.End,
            "break" => KeywordKind.Break,    
            "continue" => KeywordKind.Continue, 
            "exit" => KeywordKind.Exit,     
            "return" => KeywordKind.Return,   
            "while" => KeywordKind.While,    
            "for" => KeywordKind.For,      
            "repeat" => KeywordKind.Repeat,   
            "do" => KeywordKind.Do,       
            "until" => KeywordKind.Until,    
            "with" => KeywordKind.With,     
            "var" => KeywordKind.Var,      
            "globalvar" => KeywordKind.Globalvar,
            "not" => KeywordKind.Not,      
            "div" => KeywordKind.Div,      
            "mod" => KeywordKind.Mod,      
            "enum" => KeywordKind.Enum,     
            "try" => KeywordKind.Try,      
            "catch" => KeywordKind.Catch,    
            "finally" => KeywordKind.Finally,  
            "throw" => KeywordKind.Throw,    
            "new" => KeywordKind.New,      
            "delete" => KeywordKind.Delete,   
            "function" => KeywordKind.Function, 
            "static" => KeywordKind.Static,
            _ => KeywordKind.None
        };

        // If a keyword was found, create a keyword token, otherwise use a regular identifier
        if (keywordKind != KeywordKind.None)
        {
            context.Tokens.Add(new TokenKeyword(context, startPosition, keywordKind));
        }
        else
        {
            context.Tokens.Add(new TokenIdentifier(context, startPosition, identifier.ToString()));
        }

        return pos;
    }
}