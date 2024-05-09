using System.Collections.Generic;

using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Handles simulating VM instructions within a single control flow block.
/// </summary>
internal class BlockSimulator
{
    /// <summary>
    /// Simulates a single control flow block, outputting to the output list.
    /// </summary>
    public static void Simulate(ASTBuilder builder, List<IASTNode> output, ControlFlow.Block block)
    {
        for (int i = builder.StartBlockInstructionIndex; i < block.Instructions.Count; i++)
        {
            IGMInstruction instr = block.Instructions[i];

            switch (instr.Kind)
            {
                case Opcode.Add:
                case Opcode.Subtract:
                case Opcode.Multiply:
                case Opcode.Divide:
                case Opcode.And:
                case Opcode.Or:
                case Opcode.GMLModulo:
                case Opcode.GMLDivRemainder:
                case Opcode.Xor:
                case Opcode.ShiftLeft:
                case Opcode.ShiftRight:
                case Opcode.Compare:
                    SimulateBinary(builder, instr);
                    break;
                case Opcode.Not:
                case Opcode.Negate:
                    output.Add(new UnaryNode(builder.ExpressionStack.Pop()));
                    break;
                case Opcode.Return:
                    output.Add(new ReturnNode(builder.ExpressionStack.Pop()));
                    break;
                case Opcode.Exit:
                    output.Add(new ExitNode());
                    break;
                case Opcode.PopDelete:
                    SimulatePopDelete(builder, output);
                    break;
                // TODO: many operations
            }
        }

        builder.StartBlockInstructionIndex = 0;
    }

    /// <summary>
    /// Simulates a single binary instruction.
    /// </summary>
    private static void SimulateBinary(ASTBuilder builder, IGMInstruction instr)
    {
        IASTNode right = builder.ExpressionStack.Pop();
        IASTNode left = builder.ExpressionStack.Pop();
        builder.ExpressionStack.Push(new BinaryNode(left, right, instr));
    }

    /// <summary>
    /// Simulates a single PopDelete instruction.
    /// </summary>
    private static void SimulatePopDelete(ASTBuilder builder, List<IASTNode> output)
    {
        if (builder.ExpressionStack.Count == 0)
        {
            // Can occasionally occur with early exit cleanup
            return;
        }

        IASTNode node = builder.ExpressionStack.Pop();
        if (node.Duplicated || node is VariableNode)
        {
            // Disregard unnecessary expressions
            return;
        }

        // Node is simply a normal statement (often seen with function calls)
        output.Add(node);
    }
}
