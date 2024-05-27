using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a single block of code in the AST.
/// Blocks can have an arbitrary number of child nodes.
/// </summary>
public class BlockNode : IFragmentNode, IBlockCleanupNode
{
    /// <summary>
    /// Whether or not curly braces are required for this block.
    /// </summary>
    public bool UseBraces { get; set; } = true;

    /// <summary>
    /// Whether this block is the block of a switch statement.
    /// </summary>
    public bool PartOfSwitch { get; set; } = false;

    /// <summary>
    /// Whether this block should declare all local variables in the current fragment at the top.
    /// </summary>
    public bool PrintLocalsAtTop { get; set; } = false;

    /// <summary>
    /// All children contained within this block.
    /// </summary>
    public List<IStatementNode> Children { get; internal set; } = new();

    public bool SemicolonAfter { get => false; }
    public ASTFragmentContext FragmentContext { get; }

    public BlockNode(ASTFragmentContext fragmentContext)
    {
        FragmentContext = fragmentContext;
    }

    public int BlockClean(ASTCleaner cleaner, BlockNode block, int i)
    {
        // Remove this block if empty
        if (Children.Count == 0)
        {
            block.Children.RemoveAt(i);
            return i - 1;
        }
        return i;
    }

    private void CleanChildren(ASTCleaner cleaner)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i] = Children[i].Clean(cleaner);
            if (Children[i] is IBlockCleanupNode blockCleanupNode)
            {
                // Clean this node with the additional context of this block
                i = blockCleanupNode.BlockClean(cleaner, this, i);
            }
        }
    }

    public IFragmentNode Clean(ASTCleaner cleaner)
    {
        cleaner.PushFragmentContext(FragmentContext);
        FragmentContext.RemoveLocal(VMConstants.TempReturnVariable);
        CleanChildren(cleaner);
        cleaner.PopFragmentContext();
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        cleaner.PushFragmentContext(FragmentContext);
        FragmentContext.RemoveLocal(VMConstants.TempReturnVariable);
        CleanChildren(cleaner);
        cleaner.PopFragmentContext();
        return this;
    }

    /// <summary>
    /// If this block has 0 or 2+ statements, returns this block.
    /// If this block has one statement, returns that statement.
    /// </summary>
    public IStatementNode GetShortestStatement()
    {
        if (Children.Count == 1)
        {
            return Children[0];
        }
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.PushFragmentContext(FragmentContext);

        if (PartOfSwitch)
        {
            // We're part of a switch statement, and so will do special processing for indentation
            printer.OpenBlock();

            bool switchCaseIndent = false;
            for (int i = 0; i < Children.Count; i++)
            {
                printer.StartLine();

                IStatementNode current = Children[i];

                current.Print(printer);
                if (current.SemicolonAfter)
                {
                    printer.Semicolon();
                }

                // Check if we need to handle indents for switch
                if ((i + 1) < Children.Count)
                {
                    if (current is SwitchCaseNode && Children[i + 1] is not SwitchCaseNode)
                    {
                        printer.Indent();
                        switchCaseIndent = true;
                    }
                    else if (switchCaseIndent && current is not SwitchCaseNode && Children[i + 1] is SwitchCaseNode)
                    {
                        printer.Dedent();
                        switchCaseIndent = false;
                    }
                }

                printer.EndLine();
            }

            if (switchCaseIndent)
            {
                printer.Dedent();
            }

            printer.CloseBlock();
        }
        else if (printer.StructArguments is not null)
        {
            // We're a struct initialization block
            printer.OpenBlock();

            for (int i = 0; i < Children.Count; i++)
            {
                printer.StartLine();

                Children[i].Print(printer);
                if (i != Children.Count - 1)
                {
                    // Write comma after struct members
                    printer.Write(',');
                }

                printer.EndLine();
            }

            printer.CloseBlock();
        }
        else
        {
            // Just a normal block
            if (UseBraces)
            {
                printer.OpenBlock();
            }

            List<string> localNames = FragmentContext.LocalVariableNamesList;
            if (PrintLocalsAtTop && localNames.Count > 0)
            {
                printer.StartLine();
                printer.Write("var ");
                for (int i = 0; i < localNames.Count; i++)
                {
                    printer.Write(localNames[i]);
                    if (i != localNames.Count - 1)
                    {
                        printer.Write(", ");
                    }
                }
                printer.Semicolon();
                printer.EndLine();
            }

            foreach (IStatementNode child in Children)
            {
                printer.StartLine();

                child.Print(printer);
                if (child.SemicolonAfter)
                {
                    printer.Semicolon();
                }

                printer.EndLine();
            }

            if (UseBraces)
            {
                printer.CloseBlock();
            }
        }

        printer.PopFragmentContext();
    }
}
