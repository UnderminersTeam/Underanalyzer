using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

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

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(loops);
    }
}
