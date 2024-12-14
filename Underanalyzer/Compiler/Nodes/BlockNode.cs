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
/// Represents a block of code in the AST.
/// </summary>
internal sealed class BlockNode : IASTNode
{
    /// <summary>
    /// List of statements belonging to this block node.
    /// </summary>
    public List<IASTNode> Children { get; } = new(32);

    /// <inheritdoc/>
    public IToken? NearbyToken { get; private set; } = null;

    /// <summary>
    /// Parses statements into this block node, as the root block of the code entry.
    /// </summary>
    public void ParseRoot(ParseContext context)
    {
        context.SkipSemicolons();
        while (!context.EndOfCode)
        {
            if (Statements.ParseStatement(context) is IASTNode node)
            {
                Children.Add(node);
            }
            else
            {
                // Failed to parse statement; stop parsing this block.
                break;
            }
            context.SkipSemicolons();
        }
        if (Children.Count > 0)
        {
            NearbyToken = Children[0].NearbyToken;
        }
    }

    /// <summary>
    /// Parses statements into this block node, as a regular block, which expects opening/closing braces.
    /// </summary>
    public void ParseRegular(ParseContext context)
    {
        NearbyToken = context.EnsureToken(SeparatorKind.BlockOpen, KeywordKind.Begin);
        context.SkipSemicolons();
        while (!context.EndOfCode && !context.IsCurrentToken(SeparatorKind.BlockClose, KeywordKind.End))
        {
            if (Statements.ParseStatement(context) is IASTNode node)
            {
                Children.Add(node);
            }
            else
            {
                // Failed to parse statement; stop parsing this block.
                break;
            }
            context.SkipSemicolons();
        }
        context.EnsureToken(SeparatorKind.BlockClose, KeywordKind.End);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i] = Children[i].PostProcess(context);
        }
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        foreach (IASTNode statement in Children)
        {
            statement.GenerateCode(context);
        }
    }
}
