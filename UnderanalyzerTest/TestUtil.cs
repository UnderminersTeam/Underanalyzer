/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer;
using Underanalyzer.Compiler;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.ControlFlow;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

internal static class TestUtil
{
    /// <summary>
    /// Utility function to reduce having to split lines in tests.
    /// </summary>
    public static GMCode GetCode(string assembly, IGameContext? context = null)
    {
        string[] lines = assembly.Split('\n');
        return VMAssembly.ParseAssemblyFromLines(lines, context);
    }

    /// <summary>
    /// Asserts that for every predecessor, there is a corresponding successor, and vice versa.
    /// Additionally asserts that for every parent, there is a child (and NOT the other way around).
    /// </summary>
    public static void VerifyFlowDirections(IEnumerable<IControlFlowNode> nodes)
    {
        foreach (var node in nodes)
        {
            foreach (var pred in node.Predecessors)
            {
                Assert.Contains(node, pred.Successors);
            }
            foreach (var succ in node.Successors)
            {
                Assert.Contains(node, succ.Predecessors);
            }
            if (node.Parent is not null)
            {
                Assert.Contains(node, node.Parent.Children);
            }
        }
    }

    /// <summary>
    /// Throws an exception if there's any detected continue/break statements that have yet to be
    /// processed. This indicates continue/break detection and/or processing is broken.
    /// </summary>
    public static void EnsureNoRemainingJumps(DecompileContext ctx)
    {
        List<Block> blocks = ctx.Blocks!;
        List<BinaryBranch> branches = ctx.BinaryBranchNodes!;

        foreach (BinaryBranch bb in branches)
        {
            int startIndex = ((Block)bb.Condition).BlockIndex;
            int endAddress = bb.EndAddress;
            for (int i = startIndex + 1; i < blocks.Count && blocks[i].StartAddress < endAddress; i++)
            {
                Block block = blocks[i];
                if (block.Instructions is [.., { Kind: IGMInstruction.Opcode.Branch }] &&
                    block.Successors.Count >= 1 && block.Successors[0].StartAddress >= endAddress)
                {
                    throw new Exception("Found unprocessed break/continue");
                }
            }
        }
    }

    /// <summary>
    /// Asserts that the decompilation result of the assembly equals the provided GML, as a string.
    /// </summary>
    public static DecompileContext VerifyDecompileResult(string asm, string gml, GameContextMock? gameContext = null, DecompileSettings? decompileSettings = null)
    {
        gameContext ??= new();
        DecompileContext decompilerContext = new(gameContext, GetCode(asm, gameContext), decompileSettings);
        string decompileResult = decompilerContext.DecompileToString().Trim();
        Assert.Equal(gml.Trim().ReplaceLineEndings("\n"), decompileResult);
        return decompilerContext;
    }

    /// <summary>
    /// Utility function to lex GML code with the compiler, for testing.
    /// </summary>
    public static LexContext Lex(string code, GameContextMock? gameContext = null)
    {
        CompileContext compileContext = new(gameContext ?? new());
        LexContext rootLexContext = new(compileContext, code);
        rootLexContext.Tokenize();
        rootLexContext.PostProcessTokens();
        return rootLexContext;
    }

    /// <summary>
    /// Asserts that a list of tokens match a list of text and type pairs, corresponding to each expected token.
    /// </summary>
    public static void AssertTokens((string Text, Type Type)[] expected, List<IToken> tokens)
    {
        Assert.Equal(expected.Length, tokens.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Text.ReplaceLineEndings("\n"), tokens[i].ToString()?.ReplaceLineEndings("\n"));
            Assert.IsType(expected[i].Type, tokens[i]);
        }
    }

    /// <summary>
    /// Utility function to parse GML code with the compiler, for testing.
    /// </summary>
    public static ParseContext Parse(string code, GameContextMock? gameContext = null)
    {
        LexContext lexContext = Lex(code, gameContext);
        ParseContext parseContext = new(lexContext.CompileContext, lexContext.Tokens);
        parseContext.Parse();
        return parseContext;
    }

    /// <summary>
    /// Utility function to parse and post-process GML code with the compiler, for testing.
    /// </summary>
    public static ParseContext ParseAndPostProcess(string code, GameContextMock? gameContext = null)
    {
        ParseContext parseContext = Parse(code, gameContext);
        parseContext.PostProcessTree();
        return parseContext;
    }
}
