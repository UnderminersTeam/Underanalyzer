using System;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a try/catch, try/catch/finally, or try/finally statement in the AST.
/// </summary>
public class TryCatchNode : IStatementNode
{
    /// <summary>
    /// The block inside of "try".
    /// </summary>
    public BlockNode Try { get; }
   
    /// <summary>
    /// The block inside of "catch", or null if none exists.
    /// </summary>
    public BlockNode Catch { get; }

    /// <summary>
    /// The variable used to store the thrown value for the catch block, if Catch is not null.
    /// </summary>
    public VariableNode CatchVariable { get; }

    /// <summary>
    /// The block inside of "finally", or null if none exists.
    /// </summary>
    public BlockNode Finally { get; internal set; }

    public bool SemicolonAfter { get => false; }

    public TryCatchNode(BlockNode tryBlock, BlockNode catchBlock, VariableNode catchVariable)
    {
        Try = tryBlock;
        Catch = catchBlock;
        CatchVariable = catchVariable;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Try.Clean(cleaner);
        Try.UseBraces = true;
        if (Catch is not null)
        {
            Catch.Clean(cleaner);
            Catch.UseBraces = true;
        }
        CatchVariable?.Clean(cleaner);
        if (Finally is not null)
        {
            Finally.Clean(cleaner);
            Finally.UseBraces = true;
        }

        // TODO: handle a lot of cleanup here...

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("try");
        Try.Print(printer);
        if (Catch is not null)
        {
            // TODO: change based on code style
            printer.EndLine();
            printer.StartLine();
            printer.Write("catch (");
            CatchVariable.Print(printer);
            printer.Write(')');
            Catch.Print(printer);
        }
        if (Finally is not null)
        {
            // TODO: change based on code style
            printer.EndLine();
            printer.StartLine();
            printer.Write("finally");
            Finally.Print(printer);
        }
    }

    public class FinishFinallyNode : IStatementNode, IExpressionNode
    {
        public bool SemicolonAfter => false;
        public bool Duplicated { get; set; }
        public bool Group { get; set; } = false;
        public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

        public IStatementNode Clean(ASTCleaner cleaner)
        {
            return this;
        }

        IExpressionNode IASTNode<IExpressionNode>.Clean(ASTCleaner cleaner)
        {
            return this;
        }

        public void Print(ASTPrinter printer)
        {
            throw new NotImplementedException();
        }
    }
}
