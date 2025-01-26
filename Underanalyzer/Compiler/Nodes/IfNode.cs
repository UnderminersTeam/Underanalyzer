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
/// Represents an "if" statement in the AST.
/// </summary>
internal sealed class IfNode : IASTNode
{
    /// <summary>
    /// Condition of the if statement node.
    /// </summary>
    public IASTNode Condition { get; private set; }

    /// <summary>
    /// True statement/block of the if statement node.
    /// </summary>
    public IASTNode TrueStatement { get; private set; }

    /// <summary>
    /// False/else statement/block of the if statement node.
    /// </summary>
    public IASTNode? FalseStatement { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    public IfNode(IToken? token, IASTNode condition, IASTNode trueStatement, IASTNode? falseStatement)
    {
        NearbyToken = token;
        Condition = condition;
        TrueStatement = trueStatement;
        FalseStatement = falseStatement;
    }

    /// <summary>
    /// Creates an if statement node, parsing from the given context's current position.
    /// </summary>
    public static IfNode? Parse(ParseContext context)
    {
        // Parse "if" keyword
        if (context.EnsureToken(KeywordKind.If) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse if condition
        if (Expressions.ParseExpression(context) is not IASTNode condition)
        {
            return null;
        }

        // Skip "then" keyword, if present
        if (context.IsCurrentToken(KeywordKind.Then))
        {
            context.Position++;
        }

        // Parse true block
        if (Statements.ParseStatement(context) is not IASTNode trueStatement)
        {
            return null;
        }

        // Parse else/false block, if present
        IASTNode? falseStatement = null;
        context.SkipSemicolons();
        if (context.IsCurrentToken(KeywordKind.Else))
        {
            context.Position++;
            falseStatement = Statements.ParseStatement(context);
            if (falseStatement is null)
            {
                return null;
            }
        }

        // Create final statement node
        return new IfNode(tokenKeyword, condition, trueStatement, falseStatement);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        // Optimize condition to see if it becomes truthy or falsey at compile time
        Condition = Condition.PostProcess(context);

        // Optimize if (true)
        if (Condition is BooleanNode { Value: true } or NumberNode { Value: > 0.5 } or Int64Node { Value: >= 1 })
        {
            return TrueStatement.PostProcess(context);
        }

        // Optimize if (false)
        if (Condition is BooleanNode { Value: false } or NumberNode { Value: <= 0.5 } or Int64Node { Value: < 1 })
        {
            return FalseStatement?.PostProcess(context) ?? EmptyNode.Create();
        }

        // Post process true and false branches if no optimization was performed on condition
        TrueStatement = TrueStatement.PostProcess(context);
        FalseStatement = FalseStatement?.PostProcess(context);

        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Generate loop condition, and convert to boolean
        Condition.GenerateCode(context);
        context.ConvertDataType(DataType.Boolean);

        // Jump based on condition
        SingleForwardBranchPatch conditionBranch = new(context.Emit(Opcode.BranchFalse));

        // True statement
        TrueStatement.GenerateCode(context);

        // False statement, if present
        if (FalseStatement is not null)
        {
            SingleForwardBranchPatch skipElseBranch = new(context.Emit(Opcode.Branch));
            conditionBranch.Patch(context);
            FalseStatement.GenerateCode(context);
            skipElseBranch.Patch(context);
        }
        else
        {
            conditionBranch.Patch(context);
        }
    }
}
