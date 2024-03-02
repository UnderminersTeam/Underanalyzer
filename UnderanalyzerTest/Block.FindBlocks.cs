using Underanalyzer;
using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class Block_FindBlocks
{
    [Fact]
    public void TestEmpty()
    {
        GMCode code = TestUtil.GetCode("");
        List<Block> blocks = Block.FindBlocks(code);

        Assert.Single(blocks);
        Assert.Equal(0, blocks[0].StartAddress);
        Assert.Equal(0, blocks[0].EndAddress);
        Assert.Empty(blocks[0].Instructions);
    }

    [Fact]
    public void TestSingle()
    {
        GMCode code = TestUtil.GetCode(
            """
            pushi.e 123
            """
        );
        List<Block> blocks = Block.FindBlocks(code);

        Assert.Equal(2, blocks.Count);
        Assert.Equal(0, blocks[0].StartAddress);
        Assert.Equal(4, blocks[0].EndAddress);
        Assert.Equal(4, blocks[1].StartAddress);
        Assert.Equal(4, blocks[1].EndAddress);
        Assert.Single(blocks[0].Instructions);
        Assert.Equal(IGMInstruction.Opcode.PushImmediate, blocks[0].Instructions[0].Kind);
        Assert.Empty(blocks[1].Instructions);
        Assert.Equal(blocks[1], blocks[0].Successors[0]);
        Assert.Single(blocks[0].Successors);
        Assert.Empty(blocks[0].Predecessors);
        Assert.Equal(blocks[0], blocks[1].Predecessors[0]);
        Assert.Single(blocks[1].Predecessors);
        Assert.Empty(blocks[1].Successors);
    }

    [Fact]
    public void TestIfElse()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 0
            bf [2]

            :[1]
            pushi.e 1
            b [3]

            :[2]
            pushi.e 2

            :[3]
            pushi.e 3
            """
        );
        List<Block> blocks = Block.FindBlocks(code);

        Assert.Equal(5, blocks.Count);
        for (int i = 0; i <= 3; i++)
            Assert.Equal(i, blocks[i].Instructions[0].ValueShort);
        Assert.Empty(blocks[4].Instructions);

        Assert.Empty(blocks[0].Predecessors);
        Assert.Equal(2, blocks[0].Successors.Count);
        Assert.Equal(blocks[1], blocks[0].Successors[0]);
        Assert.Equal(blocks[2], blocks[0].Successors[1]);

        Assert.Single(blocks[1].Predecessors);
        Assert.Contains(blocks[0], blocks[1].Predecessors);
        Assert.Single(blocks[1].Successors);
        Assert.Contains(blocks[3], blocks[1].Successors);

        Assert.Single(blocks[2].Predecessors);
        Assert.Contains(blocks[0], blocks[2].Predecessors);
        Assert.Single(blocks[2].Successors);
        Assert.Contains(blocks[3], blocks[2].Successors);

        Assert.Equal(2, blocks[3].Predecessors.Count);
        Assert.Equal(blocks[1], blocks[3].Predecessors[0]);
        Assert.Equal(blocks[2], blocks[3].Predecessors[1]);
        Assert.Single(blocks[3].Successors);
        Assert.Contains(blocks[4], blocks[3].Successors);

        Assert.Single(blocks[4].Predecessors);
        Assert.Contains(blocks[3], blocks[4].Predecessors);
    }

    [Fact]
    public void TestLoop()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 0
            bf [end]

            :[1]
            pushi.e 1
            b [0]

            :[end]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);

        Assert.Equal(3, blocks.Count);
        for (int i = 0; i <= 1; i++)
            Assert.Equal(i, blocks[i].Instructions[0].ValueShort);
        Assert.Empty(blocks[2].Instructions);

        Assert.Single(blocks[0].Predecessors);
        Assert.Contains(blocks[1], blocks[0].Predecessors);
        Assert.Equal(2, blocks[0].Successors.Count);
        Assert.Equal(blocks[1], blocks[0].Successors[0]);
        Assert.Equal(blocks[2], blocks[0].Successors[1]);

        Assert.Single(blocks[1].Predecessors);
        Assert.Contains(blocks[0], blocks[1].Predecessors);
        Assert.Single(blocks[1].Successors);
        Assert.Contains(blocks[0], blocks[1].Successors);

        Assert.Single(blocks[2].Predecessors);
        Assert.Contains(blocks[0], blocks[2].Predecessors);
        Assert.Empty(blocks[2].Successors);

        TestUtil.VerifyFlowDirections(blocks);
    }

    [Fact]
    public void TestWith()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 0
            pushenv [2]

            :[1]
            pushi.e 1

            :[2]
            # Note: This handling is different than other pre-existing tooling.
            # We treat popenv as a branch instruction as well, when it has a destination.
            popenv [1]

            :[3]
            pushi.e 3

            :[end]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);

        Assert.Equal(5, blocks.Count);
        Assert.Equal(0, blocks[0].Instructions[0].ValueShort);
        Assert.Equal(1, blocks[1].Instructions[0].ValueShort);
        Assert.Equal(3, blocks[3].Instructions[0].ValueShort);
        Assert.Empty(blocks[4].Instructions);

        Assert.Empty(blocks[0].Predecessors);
        Assert.Equal(2, blocks[0].Successors.Count);
        Assert.Equal(blocks[1], blocks[0].Successors[0]);
        Assert.Equal(blocks[2], blocks[0].Successors[1]);

        Assert.Equal(2, blocks[1].Predecessors.Count);
        Assert.Equal(blocks[0], blocks[1].Predecessors[0]);
        Assert.Equal(blocks[2], blocks[1].Predecessors[1]);
        Assert.Single(blocks[1].Successors);
        Assert.Contains(blocks[2], blocks[1].Successors);

        Assert.Equal(2, blocks[2].Predecessors.Count);
        Assert.Equal(blocks[0], blocks[2].Predecessors[0]);
        Assert.Equal(blocks[1], blocks[2].Predecessors[1]);
        Assert.Equal(2, blocks[2].Successors.Count);
        Assert.Equal(blocks[3], blocks[2].Successors[0]);
        Assert.Equal(blocks[1], blocks[2].Successors[1]);

        Assert.Single(blocks[3].Predecessors);
        Assert.Contains(blocks[2], blocks[3].Predecessors);

        Assert.Single(blocks[4].Predecessors);
        Assert.Contains(blocks[3], blocks[4].Predecessors);

        TestUtil.VerifyFlowDirections(blocks);
    }
}
