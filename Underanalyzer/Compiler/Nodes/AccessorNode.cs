/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

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
    public IASTNode AccessorExpression { get; }

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

    public AccessorNode(IToken? token, IASTNode expression, AccessorKind kind, IASTNode accessorExpression, IASTNode? accessorExpression2 = null)
    {
        NearbyToken = token;
        Expression = expression;
        Kind = kind;
        AccessorExpression = accessorExpression;
        AccessorExpression2 = accessorExpression2;
    }

    /// <summary>
    /// Creates and parses an accessor node, given the provided expression and accessor kind.
    /// </summary>
    public static AccessorNode? Parse(ParseContext context, TokenSeparator token, IASTNode expression, AccessorKind kind)
    {
        // Strings are not allowed for these specific accessor kinds
        bool disallowStrings = kind is AccessorKind.Array or AccessorKind.ArrayDirect or
                                       AccessorKind.List or AccessorKind.Grid;

        // Parse the main accessor expression
        if (Expressions.ParseExpression(context) is not IASTNode accessorExpression)
        {
            return null;
        }
        if (disallowStrings && accessorExpression is StringNode)
        {
            context.CompileContext.PushError("String used in accessor that does not support strings", accessorExpression.NearbyToken);
        }

        // Parse 2D array / grid secondary accessor expression
        IASTNode? accessorExpression2 = null;
        if (kind is AccessorKind.Array or AccessorKind.Grid && context.IsCurrentToken(SeparatorKind.Comma))
        {
            context.Position++;
            accessorExpression2 = Expressions.ParseExpression(context);
            if (accessorExpression2 is null)
            {
                return null;
            }
            if (disallowStrings && accessorExpression2 is StringNode)
            {
                context.CompileContext.PushError("String used in accessor that does not support strings", accessorExpression2.NearbyToken);
            }
        }
        else if (kind is AccessorKind.Grid)
        {
            context.CompileContext.PushError("Expected two arguments to grid accessor", token);
        }

        // All accessors end in "]"
        context.EnsureToken(SeparatorKind.ArrayClose);

        // Create final node
        return new AccessorNode(token, expression, kind, accessorExpression, accessorExpression2);
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
