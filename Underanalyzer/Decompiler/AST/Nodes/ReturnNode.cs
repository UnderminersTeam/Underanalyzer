using Underanalyzer.Decompiler.GameSpecific;

namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents a "return" statement (with a value) in the AST.
/// </summary>
public class ReturnNode : IStatementNode, IBlockCleanupNode
{
    /// <summary>
    /// Expression being returned.
    /// </summary>
    public IExpressionNode Value { get; private set; }

    public bool SemicolonAfter => true;
    public bool EmptyLineBefore => false;
    public bool EmptyLineAfter => false;

    public ReturnNode(IExpressionNode value)
    {
        Value = value;
    }

    public IStatementNode Clean(ASTCleaner cleaner)
    {
        Value = Value.Clean(cleaner);

        // Handle macro type resolution
        if (Value is IMacroResolvableNode valueResolvable && 
            cleaner.GlobalMacroResolver.ResolveReturnValueType(cleaner, cleaner.TopFragmentContext.CodeEntryName) is IMacroType returnMacroType &&
            valueResolvable.ResolveMacroType(cleaner, returnMacroType) is IExpressionNode valueResolved)
        {
            Value = valueResolved;
        }

        return this;
    }

    public void Print(ASTPrinter printer)
    {
        printer.Write("return ");
        Value.Print(printer);

        if (!printer.Context.Settings.UseSemicolon)
        {
            // Manually print semicolon for this specific statement, to prevent ambiguity
            printer.Write(';');
        }
    }

    public int BlockClean(ASTCleaner cleaner, BlockNode block, int i)
    {
        // Check for return temp variable (done on first pass)
        if (i > 0 && Value is VariableNode returnVariable &&
            returnVariable is { Variable.Name.Content: VMConstants.TempReturnVariable })
        {
            if (block.Children[i - 1] is AssignNode assign && 
                assign.Variable is VariableNode { Variable.Name.Content: VMConstants.TempReturnVariable })
            {
                // We found one - rewrite it as a normal return
                block.Children[i - 1] = new ReturnNode(assign.Value);
                block.Children.RemoveAt(i);
                return i - 1;
            }
        }

        // Remove duplicated finally statements (done on second pass)
        if (cleaner.TopFragmentContext.FinallyStatementCount.Count > 0)
        {
            int count = 0;
            foreach (int statementCount in cleaner.TopFragmentContext.FinallyStatementCount)
            {
                count += statementCount;
            }
            if (i - count >= 0)
            {
                block.Children.RemoveRange(i - count, count);

                // Additionally remove temporary variable, if it exists
                if (i - count - 1 >= 0 &&
                    block.Children[i - count - 1] is AssignNode assign &&
                    assign.Variable is VariableNode { Variable.Name.Content: VMConstants.TryCopyVariable, 
                                                      Variable.InstanceType: IGMInstruction.InstanceType.Local })
                {
                    block.Children[i - count - 1] = new ReturnNode(assign.Value);
                    block.Children.RemoveAt(i - count);
                    block.FragmentContext.RemoveLocal(VMConstants.TryCopyVariable);
                    return i - count - 1;
                }

                return i - count;
            }
        }

        return i;
    }
}
