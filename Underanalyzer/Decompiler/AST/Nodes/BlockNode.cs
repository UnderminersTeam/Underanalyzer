using System;
using System.Collections.Generic;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a single block of code in the AST.
/// Blocks can have an arbitrary number of child nodes.
/// </summary>
public class BlockNode : IFragmentNode
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
    /// All children contained within this block.
    /// </summary>
    public List<IStatementNode> Children { get; internal set; } = new();

    public bool SemicolonAfter { get => false; }
    public ASTFragmentContext FragmentContext { get; }

    public BlockNode(ASTFragmentContext fragmentContext)
    {
        FragmentContext = fragmentContext;
    }

    public IFragmentNode Clean(ASTCleaner cleaner)
    {
        cleaner.PushFragmentContext(FragmentContext);
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i] = Children[i].Clean(cleaner);
            if (Children[i] is BlockNode block && block.Children.Count == 0)
            {
                // Remove this empty node
                Children.RemoveAt(i);
                i--;
            }
        }
        cleaner.PopFragmentContext();
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        cleaner.PushFragmentContext(FragmentContext);
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i] = Children[i].Clean(cleaner);
            if (Children[i] is BlockNode block && block.Children.Count == 0)
            {
                // Remove this empty node
                Children.RemoveAt(i);
                i--;
            }
        }
        cleaner.PopFragmentContext();
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

                current.Print(printer);
                if (current.SemicolonAfter)
                {
                    printer.Semicolon();
                }

                printer.EndLine();
            }

            if (switchCaseIndent)
            {
                printer.Dedent();
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

            foreach (IStatementNode child in Children)
            {
                printer.StartLine();

                child.Print(printer);
                if (printer.StructArguments is not null)
                {
                    // Write comma after struct member, always
                    printer.Write(',');
                }
                else if (child.SemicolonAfter)
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
