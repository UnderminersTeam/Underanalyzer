using Underanalyzer;
using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class BinaryBranch_FindBinaryBranches_Loops
{
    [Fact]
    public void TestWhileIfElse()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [5]

            :[1]
            push.v self.a
            conv.v.b
            bf [3]

            :[2]
            pushi.e 1
            pop.v.i self.b
            b [4]

            :[3]
            pushi.e 1
            pop.v.i self.c

            :[4]
            b [0]

            :[5]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Single(loops);
        WhileLoop loop = (WhileLoop)loops[0];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop.Predecessors);
        Assert.Equal([blocks[5]], loop.Successors);
        Assert.Equal(blocks[0], loop.Head);
        Assert.Equal(b, loop.Body);
        Assert.Equal(blocks[4], loop.Tail);
        Assert.IsType<EmptyNode>(loop.After);

        Assert.Equal(loop, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([blocks[4]], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Successors);
        Assert.Equal(blocks[3], b.False);
        Assert.Equal(blocks[3], b.Else);
        Assert.Empty(blocks[3].Successors);
        Assert.Empty(b.True.Predecessors);
        Assert.Empty(b.Else.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestWhileIfEmpty()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [3]

            :[1]
            push.v self.a
            conv.v.b
            bf [2]

            :[2]
            b [0]

            :[3]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Single(loops);
        WhileLoop loop = (WhileLoop)loops[0];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop.Predecessors);
        Assert.Equal([blocks[3]], loop.Successors);
        Assert.Equal(blocks[0], loop.Head);
        Assert.Equal(b, loop.Body);
        Assert.Equal(blocks[2], loop.Tail);
        Assert.IsType<EmptyNode>(loop.After);

        Assert.Equal(loop, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([blocks[2]], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.IsType<EmptyNode>(b.True);
        Assert.Empty(b.True.Successors);
        Assert.Equal(blocks[2], b.False);
        Assert.Null(b.Else);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestWhileIfElseEmpty()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [4]

            :[1]
            push.v self.a
            conv.v.b
            bf [3]

            :[2]
            b [3]

            :[3]
            b [0]

            :[4]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Single(loops);
        WhileLoop loop = (WhileLoop)loops[0];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop.Predecessors);
        Assert.Equal([blocks[4]], loop.Successors);
        Assert.Equal(blocks[0], loop.Head);
        Assert.Equal(b, loop.Body);
        Assert.Equal(blocks[3], loop.Tail);
        Assert.IsType<EmptyNode>(loop.After);

        Assert.Equal(loop, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([blocks[3]], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Successors);
        Assert.Equal(blocks[3], b.False);
        Assert.IsType<EmptyNode>(b.Else);
        Assert.Empty(b.True.Predecessors);
        Assert.Empty(b.Else.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestWhileIfContinue()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [4]

            :[1]
            push.v self.a
            conv.v.b
            bf [3]

            :[2]
            b [0]

            :[3]
            b [0]

            :[4]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Single(loops);
        WhileLoop loop = (WhileLoop)loops[0];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop.Predecessors);
        Assert.Equal([blocks[4]], loop.Successors);
        Assert.Equal(blocks[0], loop.Head);
        Assert.Equal(b, loop.Body);
        Assert.Equal(blocks[3], loop.Tail);
        Assert.IsType<EmptyNode>(loop.After);

        Assert.Equal(loop, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([blocks[3]], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<ContinueNode>(blocks[2].Successors[0]);
        ContinueNode c = (ContinueNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Equal(blocks[3], b.False);
        Assert.Null(b.Else);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestNestedWhileIfContinue()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [6]

            :[1]
            push.v self.a
            conv.v.b
            bf [3]

            :[2]
            b [0]

            :[3]
            pushi.e 1
            conv.i.b
            bf [5]

            :[4]
            b [3]

            :[5]
            b [0]

            :[6]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Equal(2, loops.Count);
        WhileLoop loop0 = (WhileLoop)loops[0];
        WhileLoop loop1 = (WhileLoop)loops[1];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop0.Predecessors);
        Assert.Equal([blocks[6]], loop0.Successors);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(b, loop0.Body);
        Assert.Equal(blocks[5], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);

        Assert.Equal(loop0, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([loop1], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<ContinueNode>(blocks[2].Successors[0]);
        ContinueNode c = (ContinueNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Equal(loop1, b.False);
        Assert.Null(b.Else);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestWhileIfElseContinue()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [6]

            :[1]
            push.v self.a
            conv.v.b
            bf [4]

            :[2]
            b [0]

            :[3]
            b [5]

            :[4]
            b [0]

            :[5]
            b [0]

            :[6]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Single(loops);
        WhileLoop loop = (WhileLoop)loops[0];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop.Predecessors);
        Assert.Equal([blocks[6]], loop.Successors);
        Assert.Equal(blocks[0], loop.Head);
        Assert.Equal(b, loop.Body);
        Assert.Equal(blocks[5], loop.Tail);
        Assert.IsType<EmptyNode>(loop.After);

        Assert.Equal(loop, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([blocks[5]], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<ContinueNode>(blocks[2].Successors[0]);
        ContinueNode c = (ContinueNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([blocks[3]], c.Successors);
        Assert.Equal([], blocks[3].Successors);
        Assert.Equal(blocks[4], b.False);
        Assert.Equal(blocks[4], b.Else);
        Assert.IsType<ContinueNode>(blocks[4].Successors[0]);
        c = (ContinueNode)blocks[4].Successors[0];
        Assert.Equal([blocks[4]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Empty(b.Else.Predecessors);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestNestedWhileIfElseContinue()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [8]

            :[1]
            push.v self.a
            conv.v.b
            bf [4]

            :[2]
            b [0]

            :[3]
            b [5]

            :[4]
            b [0]

            :[5]
            pushi.e 1
            conv.i.b
            bf [7]

            :[6]
            b [5]

            :[7]
            b [0]

            :[8]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Equal(2, loops.Count);
        WhileLoop loop0 = (WhileLoop)loops[0];
        WhileLoop loop1 = (WhileLoop)loops[1];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop0.Predecessors);
        Assert.Equal([blocks[8]], loop0.Successors);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(b, loop0.Body);
        Assert.Equal(blocks[7], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);

        Assert.Equal(loop0, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([loop1], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<ContinueNode>(blocks[2].Successors[0]);
        ContinueNode c = (ContinueNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([blocks[3]], c.Successors);
        Assert.Equal([], blocks[3].Successors);
        Assert.Equal(blocks[4], b.False);
        Assert.Equal(blocks[4], b.Else);
        Assert.IsType<ContinueNode>(blocks[4].Successors[0]);
        c = (ContinueNode)blocks[4].Successors[0];
        Assert.Equal([blocks[4]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Empty(b.Else.Predecessors);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestWhileIfBreak()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [4]

            :[1]
            push.v self.a
            conv.v.b
            bf [3]

            :[2]
            b [4]

            :[3]
            b [0]

            :[4]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Single(loops);
        WhileLoop loop = (WhileLoop)loops[0];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop.Predecessors);
        Assert.Equal([blocks[4]], loop.Successors);
        Assert.Equal(blocks[0], loop.Head);
        Assert.Equal(b, loop.Body);
        Assert.Equal(blocks[3], loop.Tail);
        Assert.IsType<EmptyNode>(loop.After);

        Assert.Equal(loop, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([blocks[3]], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<BreakNode>(blocks[2].Successors[0]);
        BreakNode c = (BreakNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Equal(blocks[3], b.False);
        Assert.Null(b.Else);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestNestedWhileIfBreak()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [6]

            :[1]
            push.v self.a
            conv.v.b
            bf [3]

            :[2]
            b [6]

            :[3]
            pushi.e 1
            conv.i.b
            bf [5]

            :[4]
            b [3]

            :[5]
            b [0]

            :[6]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Equal(2, loops.Count);
        WhileLoop loop0 = (WhileLoop)loops[0];
        WhileLoop loop1 = (WhileLoop)loops[1];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop0.Predecessors);
        Assert.Equal([blocks[6]], loop0.Successors);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(b, loop0.Body);
        Assert.Equal(blocks[5], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);

        Assert.Equal(loop0, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([loop1], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<BreakNode>(blocks[2].Successors[0]);
        BreakNode c = (BreakNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Equal(loop1, b.False);
        Assert.Null(b.Else);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestWhileIfElseBreak()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [6]

            :[1]
            push.v self.a
            conv.v.b
            bf [4]

            :[2]
            b [6]

            :[3]
            b [5]

            :[4]
            b [6]

            :[5]
            b [0]

            :[6]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Single(loops);
        WhileLoop loop = (WhileLoop)loops[0];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop.Predecessors);
        Assert.Equal([blocks[6]], loop.Successors);
        Assert.Equal(blocks[0], loop.Head);
        Assert.Equal(b, loop.Body);
        Assert.Equal(blocks[5], loop.Tail);
        Assert.IsType<EmptyNode>(loop.After);

        Assert.Equal(loop, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([blocks[5]], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<BreakNode>(blocks[2].Successors[0]);
        BreakNode c = (BreakNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([blocks[3]], c.Successors);
        Assert.Equal([], blocks[3].Successors);
        Assert.Equal(blocks[4], b.False);
        Assert.Equal(blocks[4], b.Else);
        Assert.IsType<BreakNode>(blocks[4].Successors[0]);
        c = (BreakNode)blocks[4].Successors[0];
        Assert.Equal([blocks[4]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Empty(b.Else.Predecessors);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }

    [Fact]
    public void TestNestedWhileIfElseBreak()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 1
            conv.i.b
            bf [8]

            :[1]
            push.v self.a
            conv.v.b
            bf [4]

            :[2]
            b [8]

            :[3]
            b [5]

            :[4]
            b [8]

            :[5]
            pushi.e 1
            conv.i.b
            bf [7]

            :[6]
            b [5]

            :[7]
            b [0]

            :[8]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);
        List<BinaryBranch> branches = BinaryBranch.FindBinaryBranches(blocks, loops);

        Assert.Equal(2, loops.Count);
        WhileLoop loop0 = (WhileLoop)loops[0];
        WhileLoop loop1 = (WhileLoop)loops[1];

        Assert.Single(branches);
        BinaryBranch b = branches[0];

        Assert.Equal([], loop0.Predecessors);
        Assert.Equal([blocks[8]], loop0.Successors);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(b, loop0.Body);
        Assert.Equal(blocks[7], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);

        Assert.Equal(loop0, b.Parent);
        Assert.Equal([], b.Predecessors);
        Assert.Equal([loop1], b.Successors);
        Assert.Equal(blocks[1], b.Condition);
        Assert.Equal(blocks[2], b.True);
        Assert.Empty(blocks[2].Instructions);
        Assert.Single(blocks[2].Successors);
        Assert.IsType<BreakNode>(blocks[2].Successors[0]);
        BreakNode c = (BreakNode)blocks[2].Successors[0];
        Assert.Equal([blocks[2]], c.Predecessors);
        Assert.Equal([blocks[3]], c.Successors);
        Assert.Equal([], blocks[3].Successors);
        Assert.Equal(blocks[4], b.False);
        Assert.Equal(blocks[4], b.Else);
        Assert.IsType<BreakNode>(blocks[4].Successors[0]);
        c = (BreakNode)blocks[4].Successors[0];
        Assert.Equal([blocks[4]], c.Predecessors);
        Assert.Equal([], c.Successors);
        Assert.Empty(b.Else.Predecessors);
        Assert.Empty(b.True.Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
        TestUtil.VerifyFlowDirections(branches);
    }
}
