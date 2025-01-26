/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a "for" loop in the AST.
/// </summary>
internal sealed class ForLoopNode : IASTNode
{
    /// <summary>
    /// Initializer of the for loop node.
    /// </summary>
    public IASTNode Initializer { get; private set; }

    /// <summary>
    /// Condition of the for loop node.
    /// </summary>
    public IASTNode Condition { get; private set; }

    /// <summary>
    /// Incrementor of the for loop node.
    /// </summary>
    public IASTNode Incrementor { get; private set; }

    /// <summary>
    /// Body of the while loop node.
    /// </summary>
    public IASTNode Body { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private ForLoopNode(TokenKeyword token, IASTNode initializer, IASTNode condition, IASTNode incrementor, IASTNode body)
    {
        NearbyToken = token;
        Initializer = initializer;
        Condition = condition;
        Incrementor = incrementor;
        Body = body;
    }

    /// <summary>
    /// Creates a for loop node, parsing from the given context's current position.
    /// </summary>
    public static ForLoopNode? Parse(ParseContext context)
    {
        // Parse "for" keyword
        if (context.EnsureToken(KeywordKind.For) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Ensure we have "(" here
        context.EnsureToken(SeparatorKind.GroupOpen);

        // Parse loop initializer
        IASTNode initializer;
        if (context.IsCurrentToken(SeparatorKind.Semicolon))
        {
            // No initializer; make an empty node
            initializer = EmptyNode.Create(context.Tokens[context.Position]);
            context.Position++;
        }
        else
        {
            // Parse initializer statement
            if (Statements.ParseStatement(context) is not IASTNode stmt)
            {
                return null;
            }
            initializer = stmt;

            // Skip only one semicolon if there is one
            if (context.IsCurrentToken(SeparatorKind.Semicolon))
            {
                context.Position++;
            }
        }

        // Parse loop condition
        IASTNode condition;
        if (context.IsCurrentToken(SeparatorKind.Semicolon))
        {
            // No condition; assume always true
            condition = new Int64Node(1, context.Tokens[context.Position]);
            context.Position++;
        }
        else
        {
            if (Expressions.ParseExpression(context) is not IASTNode expr)
            {
                return null;
            }
            condition = expr;

            // Skip only one semicolon if there is one
            if (context.IsCurrentToken(SeparatorKind.Semicolon))
            {
                context.Position++;
            }
        }

        // Parse loop incrementor, if present
        IASTNode incrementor;
        if (context.IsCurrentToken(SeparatorKind.GroupClose))
        {
            // No incrementor; make an empty block
            incrementor = EmptyNode.Create(context.Tokens[context.Position]);
        }
        else
        {
            // Parse incrementor statement
            if (Statements.ParseStatement(context) is not IASTNode stmt)
            {
                return null;
            }
            incrementor = stmt;

            // Skip all semicolons (nothing else we care about)
            context.SkipSemicolons();
        }

        // Ensure we have ")" here
        context.EnsureToken(SeparatorKind.GroupClose);

        // Parse loop body
        if (Statements.ParseStatement(context) is not IASTNode body)
        {
            return null;
        }

        // Create final statement
        return new ForLoopNode(tokenKeyword, initializer, condition, incrementor, body);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Initializer = Initializer.PostProcess(context);
        Condition = Condition.PostProcess(context);
        Incrementor = Incrementor.PostProcess(context);
        Body = Body.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Initializer
        Initializer.GenerateCode(context);

        // Branch target at the head, tail, and incrementor of the loop
        MultiBackwardBranchPatch headPatch = new(context);
        MultiForwardBranchPatch tailPatch = new();
        MultiForwardBranchPatch incrementorPatch = new();

        // Loop condition
        Condition.GenerateCode(context);
        context.ConvertDataType(DataType.Boolean);

        // Jump based on condition
        tailPatch.AddInstruction(context, context.Emit(Opcode.BranchFalse));

        // Enter loop context, and generate body
        context.PushControlFlowContext(new BasicLoopContext(tailPatch, incrementorPatch));
        Body.GenerateCode(context);

        // Incrementor, then exit loop context
        incrementorPatch.Patch(context);
        Incrementor.GenerateCode(context);
        context.PopControlFlowContext();

        // Loop tail
        headPatch.AddInstruction(context, context.Emit(Opcode.Branch));
        tailPatch.Patch(context);
    }
}
