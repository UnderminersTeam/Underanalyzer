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
/// Represents a "repeat" loop in the AST.
/// </summary>
internal sealed class RepeatLoopNode : IASTNode
{
    /// <summary>
    /// Expression used for the number of times this repeat loop node repeats.
    /// </summary>
    public IASTNode TimesToRepeat { get; private set; }

    /// <summary>
    /// Body of the repeat loop node.
    /// </summary>
    public IASTNode Body { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private RepeatLoopNode(TokenKeyword token, IASTNode timesToRepeat, IASTNode body)
    {
        NearbyToken = token;
        TimesToRepeat = timesToRepeat;
        Body = body;
    }

    /// <summary>
    /// Creates a repeat loop node, parsing from the given context's current position.
    /// </summary>
    public static RepeatLoopNode? Parse(ParseContext context)
    {
        // Parse "repeat" keyword
        if (context.EnsureToken(KeywordKind.Repeat) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse loop repeat count expression
        if (Expressions.ParseExpression(context) is not IASTNode timesToRepeat)
        {
            return null;
        }

        // Parse loop body
        if (Statements.ParseStatement(context) is not IASTNode body)
        {
            return null;
        }

        // Create final statement
        return new RepeatLoopNode(tokenKeyword, timesToRepeat, body);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        TimesToRepeat = TimesToRepeat.PostProcess(context);
        Body = Body.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Initial number of times to repeat
        TimesToRepeat.GenerateCode(context);
        context.ConvertDataType(DataType.Int32);

        // Maintain a loop counter on the stack (rather than through any variable)
        context.Emit(Opcode.Duplicate, DataType.Int32);
        context.Emit(Opcode.Push, (int)0, DataType.Int32);
        context.Emit(Opcode.Compare, ComparisonType.LesserEqualThan, DataType.Int32, DataType.Int32);

        // Branch target at the tail and incrementor of the loop
        MultiForwardBranchPatch tailPatch = new();
        MultiForwardBranchPatch decrementorPatch = new();

        // Jump based on loop counter
        tailPatch.AddInstruction(context, context.Emit(Opcode.BranchTrue));

        // Body
        MultiBackwardBranchPatch bodyPatch = new(context);
        context.PushControlFlowContext(new RepeatLoopContext(tailPatch, decrementorPatch));
        Body.GenerateCode(context);
        context.PopControlFlowContext();

        // Decrement loop counter
        decrementorPatch.Patch(context);
        context.Emit(Opcode.Push, (int)1, DataType.Int32);
        context.Emit(Opcode.Subtract, DataType.Int32, DataType.Int32);
        context.Emit(Opcode.Duplicate, DataType.Int32);
        if (context.CompileContext.GameContext.UsingExtraRepeatInstruction)
        {
            context.Emit(Opcode.Convert, DataType.Int32, DataType.Boolean);
        }
        bodyPatch.AddInstruction(context, context.Emit(Opcode.BranchTrue));

        // Tail (clean up loop counter as well)
        tailPatch.Patch(context);
        context.Emit(Opcode.PopDelete, DataType.Int32);
    }
}
