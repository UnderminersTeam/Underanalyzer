using Underanalyzer.Decompiler;
using Underanalyzer.Mock;
using static System.Reflection.Metadata.BlobBuilder;

namespace UnderanalyzerTest;

public class Loop_FindLoops
{
    [Fact]
    public void TestNone()
    {
        GMCode code = TestUtil.GetCode(
            """
            pushi.e 123
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Empty(loops);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }

    [Fact]
    public void TestSingleWhile()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.i
            pushi.e 10
            cmp.i.v LT
            bf [end]

            :[1]
            push.v self.i
            push.e 1
            add.i.v
            pop.v.v self.i
            b [0]

            :[end]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Single(loops);
        Assert.IsType<WhileLoop>(loops[0]);
        WhileLoop loop0 = (WhileLoop)loops[0];
        Assert.Equal(loop0, fragments[0].Children[0]);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(blocks[1], loop0.Tail);
        Assert.NotEqual(blocks[2], loop0.After);
        Assert.Empty(loop0.After.Successors);
        Assert.Equal(blocks[2].Predecessors[0], loop0);
        Assert.Empty(loop0.Predecessors);
        Assert.Equal(fragments[0], loop0.Parent);
        Assert.Equal(blocks[1], loop0.Body);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }

    [Fact]
    public void TestNestedWhile()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 0
            pop.v.i self.i

            :[1]
            push.v self.i
            pushi.e 10
            cmp.i.v LT
            bf [end]

            :[2]
            push.v self.j
            pushi.e 10
            cmp.i.v LT
            bf [4]

            :[3]
            push.v self.j
            push.e 1
            add.i.v
            pop.v.v self.j
            b [2]

            :[4]
            push.v self.i
            push.e 1
            add.i.v
            pop.v.v self.i
            b [1]

            :[end]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Equal(2, loops.Count);
        Assert.IsType<WhileLoop>(loops[0]);
        Assert.IsType<WhileLoop>(loops[1]);
        WhileLoop loop0 = (WhileLoop)loops[0];
        WhileLoop loop1 = (WhileLoop)loops[1];
        Assert.Equal(loop0, fragments[0].Children[0].Successors[0]);
        Assert.Equal(loop1, loop0.Body);
        Assert.Equal(loop0, loop1.Parent);
        Assert.Equal(blocks[3], loop1.Body);
        Assert.Equal(blocks[1], loop0.Head);
        Assert.Equal(blocks[2], loop1.Head);
        Assert.Empty(loop1.Predecessors);
        Assert.Equal(blocks[4], loop1.Successors[0]);
        Assert.Empty(blocks[4].Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }

    [Fact]
    public void TestSequentialWhile()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 0
            pop.v.i self.i

            :[1]
            push.v self.i
            pushi.e 10
            cmp.i.v LT
            bf [3]

            :[2]
            push.v self.i
            push.e 1
            add.i.v
            pop.v.v self.i
            b [1]

            :[3]
            push.v self.i
            pushi.e 20
            cmp.i.v LT
            bf [end]

            :[4]
            push.v self.i
            push.e 1
            add.i.v
            pop.v.v self.i
            b [3]

            :[end]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Equal(2, loops.Count);
        Assert.IsType<WhileLoop>(loops[0]);
        Assert.IsType<WhileLoop>(loops[1]);
        WhileLoop loop0 = (WhileLoop)loops[0];
        WhileLoop loop1 = (WhileLoop)loops[1];
        Assert.Equal(loop0, blocks[0].Successors[0]);
        Assert.Equal(blocks[2], loop0.Body);
        Assert.Equal(loop1, loop0.Successors[0]);
        Assert.Equal(loop0, loop1.Predecessors[0]);
        Assert.Empty(blocks[3].Predecessors);
        Assert.Equal(blocks[2], loop0.Tail);
        Assert.Equal(blocks[4], loop1.Tail);
        Assert.Empty(loop0.Tail.Successors);
        Assert.Empty(loop1.Tail.Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }

    [Fact]
    public void TestSingleDoUntil()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.a
            push.e 1
            add.i.v
            pop.v.v self.a
            push.v self.a
            pushi.e 10
            cmp.i.v GTE
            bf [0]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Single(loops);
        Assert.IsType<DoUntilLoop>(loops[0]);
        DoUntilLoop loop0 = (DoUntilLoop)loops[0];
        Assert.Equal(fragments[0], loop0.Parent);
        Assert.Equal(loop0, fragments[0].Children[0]);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(blocks[0], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);
        Assert.Empty(loop0.Predecessors);
        Assert.Equal(blocks[1], loop0.Successors[0]);
        Assert.Empty(blocks[0].Predecessors);
        Assert.Empty(blocks[0].Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }

    [Fact]
    public void TestNestedDoUntil()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.b
            push.e 1
            add.i.v
            pop.v.v self.b
            push.v self.b
            pushi.e 10
            cmp.i.v GTE
            bf [0]

            :[1]
            push.v self.a
            pushi.e 10
            cmp.i.v GTE
            bf [0]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Equal(2, loops.Count);
        Assert.IsType<DoUntilLoop>(loops[0]);
        Assert.IsType<DoUntilLoop>(loops[1]);
        DoUntilLoop loop0 = (DoUntilLoop)loops[0];
        DoUntilLoop loop1 = (DoUntilLoop)loops[1];
        Assert.Equal(fragments[0], loop0.Parent);
        Assert.Equal(loop0, loop1.Parent);
        Assert.Equal(blocks[0], loop1.Head);
        Assert.Equal(blocks[0], loop1.Tail);
        Assert.IsType<EmptyNode>(loop1.After);
        Assert.Empty(loop1.Predecessors);
        Assert.Equal(blocks[1], loop1.Successors[0]);
        Assert.Empty(blocks[1].Successors);
        Assert.Equal(loop1, loop0.Head);
        Assert.Equal(blocks[1], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);
        Assert.Empty(loop0.Predecessors);
        Assert.Equal(blocks[2], loop0.Successors[0]);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }

    [Fact]
    public void TestNestedDoUntil2()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.a
            push.e 1
            add.i.v
            pop.v.v self.a

            :[1]
            push.v self.b
            push.e 1
            add.i.v
            pop.v.v self.b
            push.v self.b
            pushi.e 10
            cmp.i.v GTE
            bf [1]

            :[2]
            push.v self.a
            pushi.e 10
            cmp.i.v GTE
            bf [0]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Equal(2, loops.Count);
        Assert.IsType<DoUntilLoop>(loops[0]);
        Assert.IsType<DoUntilLoop>(loops[1]);
        DoUntilLoop loop0 = (DoUntilLoop)loops[0];
        DoUntilLoop loop1 = (DoUntilLoop)loops[1];
        Assert.Equal(fragments[0], loop0.Parent);
        Assert.Equal(loop1, blocks[1].Parent);
        Assert.Equal(blocks[1], loop1.Head);
        Assert.Equal(blocks[1], loop1.Tail);
        Assert.IsType<EmptyNode>(loop1.After);
        Assert.Equal([blocks[0]], loop1.Predecessors);
        Assert.Equal(blocks[2], loop1.Successors[0]);
        Assert.Empty(blocks[2].Successors);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(blocks[2], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);
        Assert.Empty(loop0.Predecessors);
        Assert.Equal(blocks[3], loop0.Successors[0]);
        Assert.Equal(loop0, blocks[0].Parent);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }

    [Fact]
    public void TestSequentialDoUntil()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.a
            pushi.e 10
            cmp.i.v GTE
            bf [0]

            :[1]
            push.v self.b
            pushi.e 10
            cmp.i.v GTE
            bf [1]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Loop> loops = Loop.FindLoops(blocks);

        Assert.Equal(2, loops.Count);
        Assert.IsType<DoUntilLoop>(loops[0]);
        Assert.IsType<DoUntilLoop>(loops[1]);
        DoUntilLoop loop0 = (DoUntilLoop)loops[0];
        DoUntilLoop loop1 = (DoUntilLoop)loops[1];
        Assert.Empty(loop0.Predecessors);
        Assert.Equal([loop1], loop0.Successors);
        Assert.Equal([blocks[2]], loop1.Successors);
        Assert.Equal(blocks[0], loop0.Head);
        Assert.Equal(blocks[0], loop0.Tail);
        Assert.IsType<EmptyNode>(loop0.After);
        Assert.Equal(blocks[1], loop1.Head);
        Assert.Equal(blocks[1], loop1.Tail);
        Assert.IsType<EmptyNode>(loop1.After);
        Assert.Empty(loop0.Head.Predecessors);
        Assert.Empty(loop0.Tail.Successors);
        Assert.Empty(loop1.Head.Predecessors);
        Assert.Empty(loop1.Tail.Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }
}
