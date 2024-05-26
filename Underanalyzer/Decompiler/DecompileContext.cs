using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.ControlFlow;

namespace Underanalyzer.Decompiler;

/// <summary>
/// A decompilation context belonging to a single code entry in a game.
/// </summary>
public class DecompileContext
{
    public IGameContext GameContext { get; }
    public IGMCode Code { get; private set; }
    public IDecompileSettings Settings { get; private set; }

    // Helpers to refer to data on game context
    internal bool OlderThanBytecode15 { get => GameContext.Bytecode14OrLower; }
    internal bool GMLv2 { get => GameContext.UsingGMLv2; }

    // Data structures used (and re-used) for decompilation, as well as tests
    internal List<Block> Blocks { get; set; }
    internal Dictionary<int, Block> BlocksByAddress { get; set; }
    internal List<Fragment> FragmentNodes { get; set; }
    internal List<Loop> LoopNodes { get; set; }
    internal List<ShortCircuit> ShortCircuitNodes { get; set; }
    internal List<StaticInit> StaticInitNodes { get; set; }
    internal List<TryCatch> TryCatchNodes { get; set; }
    internal List<Nullish> NullishNodes { get; set; }
    internal List<BinaryBranch> BinaryBranchNodes { get; set; }
    internal HashSet<IControlFlowNode> SwitchEndNodes { get; set; }
    internal List<Switch.SwitchDetectionData> SwitchData { get; set; }
    internal HashSet<Block> SwitchContinueBlocks { get; set; }
    internal HashSet<Block> SwitchIgnoreJumpBlocks { get; set; }
    internal List<Switch> SwitchNodes { get; set; }
    internal Dictionary<Block, Loop> BlockSurroundingLoops { get; set; }
    internal Dictionary<Block, int> BlockAfterLimits { get; set; }

    public DecompileContext(IGameContext gameContext, IGMCode code, IDecompileSettings settings = null)
    {
        GameContext = gameContext;
        Code = code;
        Settings = settings ?? new DecompileSettings();
    }

    // Constructor used for control flow tests
    internal DecompileContext(IGMCode code) 
    {
        Code = code;
        GameContext = new Mock.GameContextMock();
    }

    // Solely decompiles control flow from the code entry
    private void DecompileControlFlow()
    {
        try
        {
            Block.FindBlocks(this);
            Fragment.FindFragments(this);
            StaticInit.FindStaticInits(this);
            Nullish.FindNullish(this);
            Loop.FindLoops(this);
            ShortCircuit.FindShortCircuits(this);
            TryCatch.FindTryCatch(this);
            Switch.FindSwitchStatements(this);
            BinaryBranch.FindBinaryBranches(this);
            Switch.InsertSwitchStatements(this);
        }
        catch (DecompilerException ex)
        {
            throw new DecompilerException($"Decompiler error during control flow analysis: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new DecompilerException($"Unexpected exception thrown in decompiler during control flow analysis: {ex.Message}", ex);
        }
    }

    // Decompiles the AST from the code entry4
    private AST.IStatementNode DecompileAST()
    {
        try
        {
            return new AST.ASTBuilder(this).Build();
        }
        catch (DecompilerException ex)
        {
            throw new DecompilerException($"Decompiler error during AST building: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new DecompilerException($"Unexpected exception thrown in decompiler during AST building: {ex.Message}", ex);
        }
    }

    // Decompiles the AST from the code entry
    private AST.IStatementNode CleanupAST(AST.IStatementNode ast)
    {
        try
        {
            return ast.Clean(new AST.ASTCleaner(this));
        }
        catch (DecompilerException ex)
        {
            throw new DecompilerException($"Decompiler error during AST cleanup: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new DecompilerException($"Unexpected exception thrown in decompiler during AST cleanup: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Decompiles the code entry, and returns the AST output.
    /// </summary>
    public AST.IStatementNode DecompileToAST()
    {
        DecompileControlFlow();
        AST.IStatementNode ast = DecompileAST();
        return CleanupAST(ast);
    }

    /// <summary>
    /// Decompiles the code entry, and returns the string output.
    /// </summary>
    public string DecompileToString()
    {
        AST.IStatementNode ast = DecompileToAST();
        try
        {
            AST.ASTPrinter printer = new(this);
            ast.Print(printer);
            return printer.OutputString;
        }
        catch (DecompilerException ex)
        {
            throw new DecompilerException($"Decompiler error during AST printing: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new DecompilerException($"Unexpected exception thrown in decompiler during AST printing: {ex.Message}", ex);
        }
    }
}
