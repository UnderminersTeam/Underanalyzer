using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.ControlFlow;

/// <summary>
/// Represents a try..catch statement in GML code.
/// Notably, this does NOT include the "finally" block, which is detected later on in the process.
/// </summary>
internal class TryCatch : IControlFlowNode
{
    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = [null, null];

    public bool Unreachable { get; set; } = false;

    /// <summary>
    /// The "try" block of the try statement.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this has its predecessors disconnected. 
    /// All paths exiting from it are also isolated from the external graph.
    /// </remarks>
    public IControlFlowNode Try { get => Children[0]; private set => Children[0] = value; }

    /// <summary>
    /// The "catch" block of the try statement, or null if none exists.
    /// </summary>
    /// <remarks>
    /// Upon being processed, this has its predecessors disconnected.
    /// All paths exiting from it are also isolated from the external graph.
    /// </remarks>
    public IControlFlowNode Catch { get => Children[1]; private set => Children[1] = value; }

    public TryCatch(int startAddress, int endAddress, IControlFlowNode tryNode, IControlFlowNode catchNode)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
        Try = tryNode;
        Catch = catchNode;
    }

    /// <summary>
    /// Finds all try/catch statements present in a list of blocks, and updates the control flow graph accordingly.
    /// </summary>
    public static List<TryCatch> FindTryCatch(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks;

        List<TryCatch> res = new();

        foreach (var block in blocks)
        {
            // Check for start block
            if (block.Instructions.Count == 6)
            {
                IGMInstruction call = block.Instructions[^2];
                if (call.Kind == IGMInstruction.Opcode.Call &&
                    call.Function.Name?.Content == VMConstants.TryHookFunction)
                {
                    // Get components of our try..catch statement
                    IControlFlowNode tryNode = block.Successors[0];
                    IControlFlowNode catchNode = block.Successors.Count >= 3 ? block.Successors[2] : null;
                    IControlFlowNode endNode = block.Successors[1];

                    TryCatch tc = new(block.StartAddress, endNode.StartAddress, tryNode, catchNode);
                    res.Add(tc);

                    // Remove predecessor of try node
                    IControlFlowNode.DisconnectPredecessor(tryNode, 0);

                    if (catchNode is not null)
                    {
                        // Remove predecessor of catch node
                        IControlFlowNode.DisconnectPredecessor(catchNode, 0);

                        // Remove branch instruction from end node's second predecessor, i.e.
                        // the end of the try block
                        Block tryEndBlock = endNode.Predecessors[1] as Block;
                        if (tryEndBlock == block || tryEndBlock.StartAddress >= catchNode.StartAddress)
                        {
                            throw new Exception("Failed to find end of try block");
                        }
                        if (tryEndBlock.Instructions.Count < 1 ||
                            tryEndBlock.Instructions[^1].Kind != IGMInstruction.Opcode.Branch)
                        {
                            throw new Exception("Expected Branch at end of try block");
                        }
                        tryEndBlock.Instructions.RemoveAt(tryEndBlock.Instructions.Count - 1);

                        // Remove instructions from the end of the catch block
                        Block catchEndBlock = blocks[(endNode as Block).BlockIndex - 1];
                        if (catchEndBlock.Instructions is not
                            [.., { Kind: IGMInstruction.Opcode.Call }, { Kind: IGMInstruction.Opcode.PopDelete },
                            { Kind: IGMInstruction.Opcode.Branch }])
                        {
                            throw new Exception("Expected finish catch and Branch at end of catch block");
                        }
                        catchEndBlock.Instructions.RemoveRange(catchEndBlock.Instructions.Count - 3, 3);

                        // Reroute end of the catch block into our end node (temporarily)
                        IControlFlowNode.DisconnectSuccessor(catchEndBlock, 0);
                        catchEndBlock.Successors.Add(endNode);
                        endNode.Predecessors.Add(catchEndBlock);
                    }

                    // Disconnect start node from end node
                    IControlFlowNode.DisconnectPredecessor(endNode, 0);

                    // Add new empty node to act as a meet point for both try and catch blocks
                    EmptyNode empty = new(endNode.StartAddress);
                    IControlFlowNode.InsertPredecessors(endNode, empty, tc.StartAddress);

                    // Disconnect new empty node from the end node
                    IControlFlowNode.DisconnectSuccessor(empty, 0);

                    // Remove instructions from initial block
                    block.Instructions.Clear();

                    // Remove try unhook instructions from end node
                    Block endBlock = endNode as Block;
                    if (endBlock.Instructions[0].Kind != IGMInstruction.Opcode.Call ||
                        endBlock.Instructions[0].Function.Name?.Content != VMConstants.TryUnhookFunction)
                    {
                        throw new Exception("Expected try unhook in end node");
                    }
                    endBlock.Instructions.RemoveRange(0, 2);

                    // Insert into graph (manually, here)
                    if (block.Successors.Count != 0)
                        throw new Exception("Expected no successors for try start block");
                    block.Successors.Add(tc);
                    tc.Predecessors.Add(block);
                    if (endNode.Predecessors.Count != 0)
                        throw new Exception("Expected no predecessors for try end block");
                    tc.Successors.Add(endNode);
                    endNode.Predecessors.Add(tc);

                    continue;
                }
            }

            // Check for finally block
            if (block.Instructions.Count >= 3)
            {
                IGMInstruction call = block.Instructions[^3];
                if (call.Kind == IGMInstruction.Opcode.Call &&
                    call.Function.Name?.Content == VMConstants.FinishFinallyFunction)
                {
                    // Remove redundant branch instruction for later operation.
                    // We leave final blocks for post-processing on the syntax tree due to complexity.
                    if (block.Instructions[^1].Kind != IGMInstruction.Opcode.Branch)
                        throw new Exception("Expected Branch after finally block");
                    block.Instructions.RemoveAt(block.Instructions.Count - 1);
                }
            }
        }

        ctx.TryCatchNodes = res;
        return res;
    }

    public void BuildAST(ASTBuilder builder, List<IASTNode> output)
    {
        throw new NotImplementedException();
    }
}
