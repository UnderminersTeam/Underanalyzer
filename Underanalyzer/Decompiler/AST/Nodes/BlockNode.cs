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

    private void CleanLocals()
    {
        if (FragmentContext.LocalVariableNames.Contains(VMConstants.TempReturnVariable))
        {
            FragmentContext.LocalVariableNames.Remove(VMConstants.TempReturnVariable);
            FragmentContext.LocalVariableNamesList.Remove(VMConstants.TempReturnVariable);
        }
        // TODO: try/catch loop locals, if possible?
    }

    private void CleanChildren(ASTCleaner cleaner)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i] = Children[i].Clean(cleaner);
            if (Children[i] is BlockNode block)
            {
                // Remove empty blocks
                if (block.Children.Count == 0)
                {
                    Children.RemoveAt(i);
                    i--;
                }
            }
            else if (Children[i] is ReturnNode returnNode)
            {
                // Check for return temp variable
                if (i > 0 && returnNode.Value is VariableNode returnVariable &&
                    returnVariable is { Variable.Name.Content: VMConstants.TempReturnVariable })
                {
                    if (Children[i - 1] is AssignNode assign && assign.Variable is VariableNode assignVariable &&
                        assignVariable is { Variable.Name.Content: VMConstants.TempReturnVariable })
                    {
                        // We found one - rewrite it as a normal return
                        Children[i - 1] = new ReturnNode(assign.Value);
                        Children.RemoveAt(i);
                        i--;
                    }
                }
            }
            else if (Children[i] is WhileLoopNode whileLoop)
            {
                // Check if we should convert this loop into a for loop
                if (!whileLoop.MustBeWhileLoop && i > 0 && Children[i - 1] is AssignNode initializer &&
                    whileLoop.Body.Children is [.., AssignNode incrementor])
                {
                    // For readability, just stick to integer and variable assignments/compound operations
                    if (initializer.Value is not (Int16Node or Int32Node or Int64Node or VariableNode) ||
                        (incrementor.Value is not (Int16Node or Int32Node or Int64Node or VariableNode) &&
                         incrementor.AssignKind != AssignNode.AssignType.Prefix &&
                         incrementor.AssignKind != AssignNode.AssignType.Postfix))
                    {
                        continue;
                    }
                    if (incrementor.AssignKind is not (AssignNode.AssignType.Compound or 
                        AssignNode.AssignType.Prefix or AssignNode.AssignType.Postfix))
                    {
                        continue;
                    }

                    // Also for readability, make sure the initializer and incrementor variables are similar
                    if (initializer.Variable is not VariableNode initVariable || 
                        incrementor.Variable is not VariableNode incVariable)
                    {
                        continue;
                    }
                    if (!initVariable.SimilarToInForIncrementor(incVariable))
                    {
                        continue;
                    }

                    // Convert into for loop!
                    BlockNode body = whileLoop.Body;
                    body.Children.RemoveAt(body.Children.Count - 1);
                    BlockNode incrementorBlock = new(body.FragmentContext);
                    incrementorBlock.Children.Add(incrementor);
                    Children.RemoveAt(i - 1);
                    i--;
                    Children[i] = new ForLoopNode(initializer, whileLoop.Condition, incrementorBlock, body);
                    Children[i] = Children[i].Clean(cleaner);
                }
            }
            else if (Children[i] is ForLoopNode forLoop)
            {
                // Check if this for loop needs an initializer, and if so (and there's a readable one), add it
                if (forLoop.Initializer is null && i > 0 && Children[i - 1] is AssignNode assign &&
                    assign.Value is (Int16Node or Int32Node or Int64Node or VariableNode) &&
                    forLoop.Condition is not null)
                {
                    forLoop.Initializer = assign;
                    Children.RemoveAt(i - 1);
                    i--;
                    Children[i] = forLoop.Clean(cleaner);
                }
            }
        }
    }

    public IFragmentNode Clean(ASTCleaner cleaner)
    {
        cleaner.PushFragmentContext(FragmentContext);
        CleanLocals();
        CleanChildren(cleaner);
        cleaner.PopFragmentContext();
        return this;
    }

    IStatementNode IASTNode<IStatementNode>.Clean(ASTCleaner cleaner)
    {
        cleaner.PushFragmentContext(FragmentContext);
        CleanLocals();
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
