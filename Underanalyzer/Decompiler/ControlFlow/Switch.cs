﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.ControlFlow;

internal class Switch : IControlFlowNode
{
    /// <summary>
    /// Initial detection data for a switch statement, used to prevent calculations being done twice.
    /// </summary>
    public class SwitchDetectionData
    {
        public Block EndBlock { get; set; } = null;
        public Block ContinueBlock { get; set; } = null;
        public Block ContinueSkipBlock { get; set; } = null;
        public Block EndOfCaseBlock { get; set; } = null;
        public Block DefaultBranchBlock { get; set; } = null;
    }

    public class CaseJumpNode(int address) : IControlFlowNode
    {
        public int StartAddress { get; private set; } = address;

        public int EndAddress { get; private set; } = address;

        public List<IControlFlowNode> Predecessors { get; } = new();

        public List<IControlFlowNode> Successors { get; } = new();

        public IControlFlowNode Parent { get; set; } = null;

        public List<IControlFlowNode> Children { get; } = new();

        public bool Unreachable { get; set; } = false;

        public override string ToString()
        {
            return $"{nameof(CaseJumpNode)} (address {StartAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
        }

        public void BuildAST(ASTBuilder builder, List<IStatementNode> output)
        {
            // Queue our expression to be used later, when the case destination is processed
            builder.SwitchCases.Enqueue(builder.ExpressionStack.Pop());

            // Get rid of duplicated expression
            builder.ExpressionStack.Pop();
        }
    }

    public class CaseDestinationNode(int address) : IControlFlowNode
    {
        public int StartAddress { get; private set; } = address;

        public int EndAddress { get; private set; } = address;

        public List<IControlFlowNode> Predecessors { get; } = new();

        public List<IControlFlowNode> Successors { get; } = new();

        public IControlFlowNode Parent { get; set; } = null;

        public List<IControlFlowNode> Children { get; } = new();

        public bool Unreachable { get; set; } = false;

        public bool IsDefault { get; set; } = false;

        public override string ToString()
        {
            return $"{nameof(CaseDestinationNode)} (address {StartAddress}, {Predecessors.Count} predecessors, {Successors.Count} successors)";
        }

        public void BuildAST(ASTBuilder builder, List<IStatementNode> output)
        {
            if (IsDefault)
            {
                // Just a simple default
                output.Add(new SwitchCaseNode(null));
            }
            else
            {
                // Retrieve expression from earlier evaluation
                output.Add(new SwitchCaseNode(builder.SwitchCases.Dequeue()));
            }
        }
    }

    public int StartAddress { get; private set; }

    public int EndAddress { get; private set; }

    public List<IControlFlowNode> Predecessors { get; } = new();

    public List<IControlFlowNode> Successors { get; } = new();

    public IControlFlowNode Parent { get; set; } = null;

    public List<IControlFlowNode> Children { get; } = [null, null, null];

    public bool Unreachable { get; set; } = false;

    /// <summary>
    /// The first block that begins the chain of case conditions. Should always be a Block.
    /// </summary>
    public IControlFlowNode Cases { get => Children[0]; private set => Children[0] = value; }

    /// <summary>
    /// The first node of the switch statement body. Should always be a CaseDestinationNode, or null.
    /// </summary>
    public IControlFlowNode Body { get => Children[1]; private set => Children[1] = value; }

    /// <summary>
    /// An optional successor chain of case destinations (null if none was necessary).
    /// Specifically, those that appear at the very end of the switch statement and have no code.
    /// </summary>
    public IControlFlowNode EndCaseDestinations { get => Children[2]; private set => Children[2] = value; }

    public Switch(int startAddress, int endAddress, IControlFlowNode cases, IControlFlowNode body, IControlFlowNode endCaseDestinations)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
        Cases = cases;
        Body = body;
        EndCaseDestinations = endCaseDestinations;
    }

    private static void DetectionPass(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks;
        Stack<int> switchTops = new();

        // Go backwards; first good candidate block must be the end of a switch statement
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            Block block = blocks[i];

            // Leave switch statements that we're already past
            if (switchTops.Count > 0 && block.StartAddress <= switchTops.Peek())
            {
                switchTops.Pop();
            }

            // Ensure PopDelete is at start of block
            if (block.Instructions is not [{ Kind: IGMInstruction.Opcode.PopDelete }, ..])
            {
                continue;
            }

            // Ensure our earliest predecessor is a block ending with Branch (excluding blocks ending in BranchTrue, as those are cases)
            if (block.Predecessors.Count == 0)
            {
                continue;
            }
            IControlFlowNode earliestPredecessor = null;
            for (int j = 0; j < block.Predecessors.Count; j++)
            {
                if (block.Predecessors[j] is Block predCaseBlock &&
                    predCaseBlock.Instructions is [.., { Kind: IGMInstruction.Opcode.BranchTrue }])
                {
                    continue;
                }
                if (earliestPredecessor is null ||
                    block.Predecessors[j].StartAddress < earliestPredecessor.StartAddress)
                {
                    earliestPredecessor = block.Predecessors[j];
                }
            }
            if (earliestPredecessor is not Block predBlock ||
                predBlock.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
            {
                continue;
            }

            // If this block ends with return/exit, we need to check that this isn't an early
            // return/exit from within a switch statement.
            if (block.Instructions[^1].Kind is IGMInstruction.Opcode.Exit or IGMInstruction.Opcode.Return)
            {
                // Count how many PopDelete instructions we have in this block.
                int numPopDeletes = 1;
                for (int j = 1; j < block.Instructions.Count; j++)
                {
                    if (block.Instructions[j].Kind == IGMInstruction.Opcode.PopDelete)
                    {
                        numPopDeletes++;
                    }
                }

                // If this number isn't equal to the number of switches we're inside of + 1,
                // then this is an early return/exit.
                if (numPopDeletes != switchTops.Count + 1)
                {
                    continue;
                }
            }

            // We've found the end of a switch statement
            ctx.SwitchEndBlocks.Add(block);
            switchTops.Push(earliestPredecessor.StartAddress);

            // Create detection data
            SwitchDetectionData data = new()
            {
                EndBlock = block
            };
            ctx.SwitchData.Add(data);

            // Check if we have a continue block immediately preceding this end block
            if (block.BlockIndex == 0)
            {
                continue;
            }
            Block previousBlock = blocks[block.BlockIndex - 1];
            if (previousBlock.Instructions is not
                [{ Kind: IGMInstruction.Opcode.PopDelete }, { Kind: IGMInstruction.Opcode.Branch }])
            {
                continue;
            }

            // This block should be a continue block, but additionally check we have a branch around it
            if (previousBlock.BlockIndex == 0)
            {
                continue;
            }
            Block previousPreviousBlock = blocks[previousBlock.BlockIndex - 1];
            if (previousPreviousBlock.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
            {
                continue;
            }
            if (previousPreviousBlock.Successors.Count != 1 || previousPreviousBlock.Successors[0] != block)
            {
                continue;
            }

            // This is definitely a switch continue block
            ctx.SwitchContinueBlocks.Add(previousBlock);
            ctx.SwitchIgnoreJumpBlocks.Add(previousPreviousBlock);
            data.ContinueBlock = previousBlock;
            data.ContinueSkipBlock = previousPreviousBlock;

            // Prevent this block from processing during the next iteration
            i--;
        }
    }

    private static void DetailPass(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks;

        foreach (SwitchDetectionData data in ctx.SwitchData)
        {
            // Find first predecessor that ends in Branch (should be the first one that *doesn't* end in BranchTrue)
            Block firstBranchPredecessor = null;
            foreach (IControlFlowNode pred in data.EndBlock.Predecessors)
            {
                if (pred is Block predBlock && predBlock.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }])
                {
                    firstBranchPredecessor = predBlock;
                    break;
                }
            }
            if (firstBranchPredecessor == null)
            {
                throw new DecompilerException("Failed to find end of switch cases");
            }

            // Need to detect whether or not we have a default case in this switch.
            // If previous block ends with Branch, then:
            //  - If it branches beyond the end of the switch (or backwards), then it can't be the default branch itself.
            //    Also, if the current block is a switch end block, the previous block is also not a default branch...
            //      -> Fall into case where previous block doesn't end with Branch
            //  - If it branches into the switch, then it's clearly the default branch
            // If the previous block doesn't end with Branch, then:
            //  - If the next block is Unreachable, and only contains Branch, then firstBranchPredecessor is the default branch
            //  - Otherwise, there's no default branch
            data.EndOfCaseBlock = firstBranchPredecessor;
            bool prevBlockIsDefaultBranch;
            if (firstBranchPredecessor.BlockIndex >= 1 && !ctx.SwitchEndBlocks.Contains(firstBranchPredecessor))
            {
                Block prevBlock = blocks[firstBranchPredecessor.BlockIndex - 1];
                if (prevBlock.Instructions is not [.., { Kind: IGMInstruction.Opcode.Branch }])
                {
                    prevBlockIsDefaultBranch = false;
                }
                else if (prevBlock.Successors[0].StartAddress > data.EndBlock.StartAddress ||
                         prevBlock.Successors[0].StartAddress <= prevBlock.StartAddress)
                {
                    prevBlockIsDefaultBranch = false;
                }
                else
                {
                    prevBlockIsDefaultBranch = true;
                    data.DefaultBranchBlock = prevBlock;
                }
            }
            else
            {
                prevBlockIsDefaultBranch = false;
            }
            if (!prevBlockIsDefaultBranch)
            {
                Block nextBlock = blocks[firstBranchPredecessor.BlockIndex + 1];
                if (nextBlock.Unreachable && nextBlock.Instructions is [{ Kind: IGMInstruction.Opcode.Branch }])
                {
                    data.DefaultBranchBlock = firstBranchPredecessor;
                    data.EndOfCaseBlock = nextBlock;
                }
            }

            // Update list of blocks that we should ignore
            ctx.SwitchIgnoreJumpBlocks.Add(data.EndOfCaseBlock);
            if (data.DefaultBranchBlock is not null)
            {
                ctx.SwitchIgnoreJumpBlocks.Add(data.DefaultBranchBlock);
            }
        }
    }

    /// <summary>
    /// Scans for all blocks representing the end of a switch statement, the "continue" block of a switch statement,
    /// as well as other important branch blocks that should not be touched by binary branch break/continue detection.
    /// Stores this data for later use when creating/inserting actual switch statements into the graph.
    /// </summary>
    public static void FindSwitchStatements(DecompileContext ctx)
    {
        ctx.SwitchEndBlocks = new();
        ctx.SwitchData = new();
        ctx.SwitchContinueBlocks = new();
        ctx.SwitchIgnoreJumpBlocks = new();

        // First pass: simply detect the end blocks of switch statements, as well as continue blocks.
        // We do this first as this requires a special algorithm to prevent false positives/negatives.
        DetectionPass(ctx);

        // Second pass: find details about the remaining important blocks.
        // We do this second as this sometimes requires knowledge of surrounding switch statements.
        DetailPass(ctx);
    }

    /// <summary>
    /// Finds all switch statements in the code entry (given data from all the earlier control flow passes),
    /// and inserts them into the graph accordingly.
    /// </summary>
    public static List<Switch> InsertSwitchStatements(DecompileContext ctx)
    {
        List<Switch> res = new();

        for (int j = ctx.SwitchData.Count - 1; j >= 0; j--)
        {
            SwitchDetectionData data = ctx.SwitchData[j];

            // Find all cases
            IControlFlowNode currentNode = data.EndOfCaseBlock;
            List<Block> caseBranches = new();
            while (currentNode is not null)
            {
                if (currentNode is Block currentBlock)
                {
                    if (currentBlock.Instructions is [.., { Kind: IGMInstruction.Opcode.BranchTrue }])
                    {
                        // We've found a case!
                        caseBranches.Add(currentBlock);
                    }

                    if (ctx.SwitchEndBlocks.Contains(currentBlock))
                    {
                        // We're at the end of another switch statement - do not continue
                        break;
                    }
                }

                if (currentNode.Predecessors.Count != 1)
                {
                    // We have either nowhere left to go, or a nonlinear branch here - do not continue
                    break;
                }

                currentNode = currentNode.Predecessors[0];
            }

            // Update graph for all cases (in reverse; we found them backwards)
            // First pass: update chain of conditions
            IControlFlowNode startOfBody = null;
            IControlFlowNode endCaseDestinations = null;
            IControlFlowNode endCaseDestinationsEnd = null;
            List<IControlFlowNode> caseDestinationNodes = new();
            for (int i = caseBranches.Count - 1; i >= 0; i--)
            {
                Block currentBlock = caseBranches[i];
                caseDestinationNodes.Add(currentBlock.Successors[1]);

                // Clear out the Compare & BranchTrue and replace it with a CaseJumpNode
                if (currentBlock.Instructions[^2].Kind != IGMInstruction.Opcode.Compare)
                {
                    throw new DecompilerException("Expected Compare instruction in switch case");
                }
                currentBlock.Instructions.RemoveRange(currentBlock.Instructions.Count - 2, 2);
                IControlFlowNode.DisconnectSuccessor(currentBlock, 1);
                CaseJumpNode caseJumpNode = new(currentBlock.EndAddress);
                IControlFlowNode.InsertSuccessor(currentBlock, 0, caseJumpNode);
                if (i == 0)
                {
                    // If we're the last case, disconnect the chain here
                    IControlFlowNode.DisconnectSuccessor(caseJumpNode, 0);
                }
            }
            // First pass (part two): also update default case
            IControlFlowNode defaultDestinationNode = null;
            if (data.DefaultBranchBlock is not null)
            {
                Block defaultBlock = data.DefaultBranchBlock;
                defaultDestinationNode = defaultBlock.Successors[0];

                // Clear out Branch and disconnect successors (multiple successors because unreachable blocks are possible)
                defaultBlock.Instructions.RemoveAt(defaultBlock.Instructions.Count - 1);
                for (int i = defaultBlock.Successors.Count - 1; i >= 0; i--)
                {
                    IControlFlowNode.DisconnectSuccessor(defaultBlock, i);
                }
            }
            // Second pass: update destinations
            foreach (IControlFlowNode caseDestination in caseDestinationNodes)
            {
                // Insert case destination node before destination
                CaseDestinationNode caseDestNode = new(caseDestination.StartAddress);
                if (caseDestination.StartAddress >= (data.ContinueSkipBlock?.StartAddress ?? data.EndBlock.StartAddress))
                {
                    // Our destination is at the very end of the switch statement
                    if (endCaseDestinations is null)
                    {
                        endCaseDestinations = caseDestNode;
                        endCaseDestinationsEnd = caseDestNode;
                    }
                    else
                    {
                        endCaseDestinationsEnd.Successors.Add(caseDestNode);
                        caseDestNode.Predecessors.Add(endCaseDestinationsEnd);
                        endCaseDestinationsEnd = caseDestNode;
                    }
                }
                else
                {
                    IControlFlowNode.InsertPredecessorsAll(caseDestination, caseDestNode);

                    // Update the start of the switch body
                    if (startOfBody is null)
                    {
                        startOfBody = caseDestNode;
                    }
                    else if (caseDestNode.StartAddress < startOfBody.StartAddress)
                    {
                        startOfBody = caseDestNode;
                    }
                }
            }
            // Second pass (part two): update destination for default case
            if (defaultDestinationNode is not null)
            {
                // Insert default case destination node before destination
                CaseDestinationNode caseDestNode = new(defaultDestinationNode.StartAddress)
                {
                    IsDefault = true
                };
                if (defaultDestinationNode.StartAddress >= (data.ContinueSkipBlock?.StartAddress ?? data.EndBlock.StartAddress))
                {
                    // Our destination is at the very end of the switch statement
                    if (endCaseDestinations is null)
                    {
                        endCaseDestinations = caseDestNode;
                    }
                    else
                    {
                        endCaseDestinationsEnd.Successors.Add(caseDestNode);
                        caseDestNode.Predecessors.Add(endCaseDestinationsEnd);
                        endCaseDestinationsEnd = caseDestNode;
                    }
                }
                else
                {
                    IControlFlowNode.InsertPredecessorsAll(defaultDestinationNode, caseDestNode);

                    // Update the start of the switch body
                    if (startOfBody is null)
                    {
                        startOfBody = caseDestNode;
                    }
                    else if (caseDestNode.StartAddress < startOfBody.StartAddress)
                    {
                        startOfBody = caseDestNode;
                    }
                }
            }

            // Remove branch from end of case block
            if (data.EndOfCaseBlock is not null)
            {
                Block endOfCaseBlock = data.EndOfCaseBlock;

                // Clear out Branch and disconnect successor
                endOfCaseBlock.Instructions.RemoveAt(endOfCaseBlock.Instructions.Count - 1);
                IControlFlowNode.DisconnectSuccessor(endOfCaseBlock, 0);
            }

            // Remove continue block (and branch around it) if it exists
            if (data.ContinueBlock is not null)
            {
                Block continueBlock = data.ContinueBlock;
                continueBlock.Instructions.Clear();
                for (int i = continueBlock.Predecessors.Count - 1; i >= 0; i--)
                {
                    IControlFlowNode.DisconnectPredecessor(continueBlock, i);
                }
                for (int i = continueBlock.Successors.Count - 1; i >= 0; i--)
                {
                    IControlFlowNode.DisconnectSuccessor(continueBlock, i);
                }

                Block skipContinueBlock = data.ContinueSkipBlock;
                skipContinueBlock.Instructions.RemoveAt(skipContinueBlock.Instructions.Count - 1);
                for (int i = skipContinueBlock.Predecessors.Count - 1; i >= 0; i--)
                {
                    IControlFlowNode.DisconnectPredecessor(skipContinueBlock, i);
                }
                for (int i = skipContinueBlock.Successors.Count - 1; i >= 0; i--)
                {
                    IControlFlowNode.DisconnectSuccessor(skipContinueBlock, i);
                }
            }

            // Disconnect all branches going into the end node, and remove PopDelete
            Block endBlock = data.EndBlock;
            endBlock.Instructions.RemoveAt(0);
            IControlFlowNode endNode = endBlock;
            while (endNode.Parent is not null)
            {
                endNode = endNode.Parent;
            }
            for (int i = endNode.Predecessors.Count - 1; i >= 0; i--)
            {
                IControlFlowNode.DisconnectPredecessor(endNode, i);
            }

            // Construct actual switch node
            Block startOfStatement = (caseBranches.Count > 0) ? caseBranches[^1] : (data.DefaultBranchBlock ?? data.EndOfCaseBlock);
            Switch switchNode = new(startOfStatement.StartAddress, endNode.StartAddress, startOfStatement, startOfBody, endCaseDestinations);
            IControlFlowNode.InsertStructure(startOfStatement, endNode, switchNode);
            res.Add(switchNode);

            // Update parent status of Cases/Body
            switchNode.Parent = startOfStatement.Parent;
            startOfStatement.Parent = switchNode;
            if (startOfBody is not null)
            {
                startOfBody.Parent = switchNode;
            }
        }

        ctx.SwitchNodes = res;
        return res;
    }

    public void BuildAST(ASTBuilder builder, List<IStatementNode> output)
    {
        // Begin new switch case queue for this statement
        var prevSwitchCases = builder.SwitchCases;
        builder.SwitchCases = new(8);

        // Evaluate case expressions
        builder.BuildArbitrary(Cases, 1);

        // All that's left on stack is the expression we're switching on
        IExpressionNode expression = builder.ExpressionStack.Pop();

        // Evaluate block
        BlockNode body = builder.BuildBlock(Body);
        body.UseBraces = true;
        body.PartOfSwitch = true;

        // Add statement
        output.Add(new SwitchNode(expression, body));

        // Restore previous switch case queue
        builder.SwitchCases = prevSwitchCases;
    }
}