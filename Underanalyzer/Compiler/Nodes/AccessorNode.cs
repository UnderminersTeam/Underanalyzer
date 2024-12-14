/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents an accessor/array index in the AST
/// </summary>
internal sealed class AccessorNode : IAssignableASTNode
{
    /// <summary>
    /// Expression being accessed/indexed by this accessor.
    /// </summary>
    public IASTNode Expression { get; }

    /// <summary>
    /// Kind of accessor.
    /// </summary>
    public AccessorKind Kind { get; }

    /// <summary>
    /// Expression inside of the accessor itself.
    /// </summary>
    public IASTNode? AccessorExpression { get; }

    /// <summary>
    /// Second expression inside of the accessor itself, if applicable.
    /// </summary>
    public IASTNode? AccessorExpression2 { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Kinds of accessors.
    /// </summary>
    public enum AccessorKind
    {
        Array,
        ArrayDirect,
        List,
        Map,
        Grid,
        Struct
    }

    /// <summary>
    /// Creates and parses an accessor node, given the provided expression and accessor kind.
    /// </summary>
    public AccessorNode(ParseContext context, TokenSeparator token, IASTNode expression, AccessorKind kind)
    {
        NearbyToken = token;
        Expression = expression;
        Kind = kind;

        // Strings are not allowed for these specific accessor kinds
        bool disallowStrings = kind is AccessorKind.Array or AccessorKind.ArrayDirect or
                                       AccessorKind.List or AccessorKind.Grid;

        // Parse the main accessor expression
        AccessorExpression = Expressions.ParseExpression(context);
        if (disallowStrings && AccessorExpression is StringNode)
        {
            context.CompileContext.PushError("String used in accessor that does not support strings", AccessorExpression.NearbyToken);
        }

        // Parse 2D array / grid secondary accessor expression
        if (kind is AccessorKind.Array or AccessorKind.Grid && context.IsCurrentToken(SeparatorKind.Comma))
        {
            context.Position++;
            AccessorExpression2 = Expressions.ParseExpression(context);
            if (disallowStrings && AccessorExpression2 is StringNode)
            {
                context.CompileContext.PushError("String used in accessor that does not support strings", AccessorExpression2.NearbyToken);
            }
        }
        else if (kind is AccessorKind.Grid)
        {
            context.CompileContext.PushError("Expected two arguments to grid accessor", token);
        }

        context.EnsureToken(SeparatorKind.ArrayClose);
    }

    private AccessorNode(IToken? token, IASTNode expression, AccessorKind kind, IASTNode accessorExpression)
    {
        NearbyToken = token;
        Expression = expression;
        Kind = kind;
        AccessorExpression = accessorExpression;
    }

    /// <summary>
    /// If this accessor node is for a 2D array, this converts the comma syntax to two separate accessors.
    /// </summary>
    public AccessorNode Convert2DArrayToTwoAccessors()
    {
        if (Kind is AccessorKind.Array && AccessorExpression2 is IASTNode expression2)
        {
            AccessorExpression2 = null;
            return new AccessorNode(NearbyToken, this, Kind, expression2);
        }
        return this;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        // TODO
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
