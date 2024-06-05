/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using Underanalyzer.Decompiler.ControlFlow;
using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Decompiler;

/// <summary>
/// A decompilation context belonging to a single code entry in a game.
/// </summary>
public class DecompileContext
{
    /// <summary>
    /// The game context this <see cref="DecompileContext"/> belongs to.
    /// </summary>
    public IGameContext GameContext { get; }

    /// <summary>
    /// The specific code entry within the game this <see cref="DecompileContext"/> belongs to.
    /// </summary>
    public IGMCode Code { get; private set; }

    /// <summary>
    /// The decompilation settings to be used for this <see cref="DecompileContext"/> in its operation.
    /// </summary>/
    public IDecompileSettings Settings { get; private set; }

    /// <summary>
    /// A list of warnings produced throughout the decompilation process.
    /// </summary>
    public List<IDecompileWarning> Warnings { get; } = new();

    // Helpers to refer to data on game context
    internal bool OlderThanBytecode15 { get => GameContext.Bytecode14OrLower; }
    internal bool GMLv2 { get => GameContext.UsingGMLv2; }

    // Data structures used (and re-used) for decompilation, as well as tests
    // See about changing these to not be nullable?
    internal List<Block>? Blocks { get; set; }
    internal Dictionary<int, Block>? BlocksByAddress { get; set; }
    internal List<Fragment>? FragmentNodes { get; set; }
    internal List<Loop>? LoopNodes { get; set; }
    internal List<Block>? ShortCircuitBlocks { get; set; }
    internal List<ShortCircuit>? ShortCircuitNodes { get; set; }
    internal List<StaticInit>? StaticInitNodes { get; set; }
    internal List<TryCatch>? TryCatchNodes { get; set; }
    internal List<Nullish>? NullishNodes { get; set; }
    internal List<BinaryBranch>? BinaryBranchNodes { get; set; }
    internal HashSet<IControlFlowNode>? SwitchEndNodes { get; set; }
    internal List<Switch.SwitchDetectionData>? SwitchData { get; set; }
    internal HashSet<Block>? SwitchContinueBlocks { get; set; }
    internal HashSet<Block>? SwitchIgnoreJumpBlocks { get; set; }
    internal List<Switch>? SwitchNodes { get; set; }
    internal Dictionary<Block, Loop>? BlockSurroundingLoops { get; set; }
    internal Dictionary<Block, int>? BlockAfterLimits { get; set; }
    internal List<GMEnum>? EnumDeclarations { get; set; } = new();
    internal Dictionary<string, GMEnum>? NameToEnumDeclaration { get; set; } = new();
    internal GMEnum? UnknownEnumDeclaration { get; set; } = null;
    internal int UnknownEnumReferenceCount { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecompileContext"/> class.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="code">The code entry.</param>
    /// <param name="settings">The decompilation settings that should be used.</param>
    public DecompileContext(IGameContext gameContext, IGMCode code, IDecompileSettings settings)
    {
        GameContext = gameContext;
        Code = code;
        Settings = settings;
    }

    /// <summary>
    /// <inheritdoc cref="DecompileContext(IGameContext, IGMCode, IDecompileSettings)"/>
    /// </summary>
    /// <param name="gameContext"><inheritdoc cref="DecompileContext(IGameContext, IGMCode, IDecompileSettings)"/></param>
    /// <param name="code"><see cref="DecompileContext(IGameContext, IGMCode, IDecompileSettings)"/></param>
    public DecompileContext(IGameContext gameContext, IGMCode code) : this(gameContext, code, new DecompileSettings()) 
    { }
    

    // Constructor used for control flow tests
    internal DecompileContext(IGMCode code) 
    {
        Code = code;
        GameContext = new Mock.GameContextMock();
        Settings = new DecompileSettings();
    }

    /// <summary>
    /// Solely decompiles control flow from the code entry .
    /// </summary>
    /// <exception cref="DecompilerException">When a decompiler error occured.</exception>
    private void DecompileControlFlow()
    {
        try
        {
            Block.FindBlocks(this);
            Fragment.FindFragments(this);
            StaticInit.FindStaticInits(this);
            Nullish.FindNullish(this);
            ShortCircuit.FindShortCircuits(this);
            Loop.FindLoops(this);
            ShortCircuit.InsertShortCircuits(this);
            TryCatch.FindTryCatch(this);
            Switch.FindSwitchStatements(this);
            BinaryBranch.FindBinaryBranches(this);
            Switch.InsertSwitchStatements(this);
            TryCatch.CleanTryEndBranches(this);
        }
        catch (DecompilerException ex)
        {
            throw new DecompilerException($"Decompiler error during control flow analysis: {ex.Message}", ex);
        }
        // Should probably throw something else, 'cause this should basically never happen.
        catch (Exception ex)
        {
            throw new DecompilerException($"Unexpected exception thrown in decompiler during control flow analysis: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Decompiles the AST from the code entry.
    /// </summary>
    /// <returns>The AST</returns>
    /// <exception cref="DecompilerException">When a decompiler error occured.</exception>
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
        // See in DecompileControlFlow
        catch (Exception ex)
        {
            throw new DecompilerException($"Unexpected exception thrown in decompiler during AST building: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Cleans up a given AST.
    /// </summary>
    /// <param name="ast">The AST that should be cleaned up.</param>
    /// <returns>A new cleaned AST.</returns>
    /// <exception cref="DecompilerException">When a decompiler error occured.</exception>
    private AST.IStatementNode CleanupAST(AST.IStatementNode ast)
    {
        try
        {
            AST.ASTCleaner cleaner = new(this);
            AST.IStatementNode cleaned = ast.Clean(cleaner);
            if (Settings.CreateEnumDeclarations)
            {
                AST.EnumDeclNode.GenerateDeclarations(cleaner, cleaned);
            }
            return cleaned;
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
    /// <returns>The AST.</returns>
    public AST.IStatementNode DecompileToAST()
    {
        DecompileControlFlow();
        AST.IStatementNode ast = DecompileAST();
        return CleanupAST(ast);
    }

    /// <summary>
    /// Decompiles the code entry, and returns the string output.
    /// </summary>
    /// <returns>The decompiled code.</returns>
    public string DecompileToString()
    {
        AST.IStatementNode ast = DecompileToAST();
        try
        {
            AST.ASTPrinter printer = new(this);
            if (Settings.PrintWarnings)
            {
                printer.PrintRemainingWarnings(true);
            }
            ast.Print(printer);
            if (Settings.PrintWarnings)
            {
                printer.PrintRemainingWarnings(false);
            }
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
