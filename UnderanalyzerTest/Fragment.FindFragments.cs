﻿using Underanalyzer;
using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class Fragment_FindFragments
{
    [Fact]
    public void TestSingle()
    {
        GMCode code = TestUtil.GetCode(
            """
            pushi.e 123
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);

        Assert.Single(fragments);
        Assert.Equal(2, fragments[0].Blocks.Count);
        Assert.Equal(blocks[0], fragments[0].Blocks[0]);
        Assert.Equal(blocks[1], fragments[0].Blocks[1]);
        Assert.Empty(fragments[0].Predecessors);
        Assert.Empty(fragments[0].Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
    }

    [Fact]
    public void TestDouble()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 0
            b [2]

            > child_entry
            :[1]
            pushi.e 1
            exit.i

            :[2]
            pushi.e 2

            :[3]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);

        Assert.Equal(2, fragments.Count);

        Assert.Equal(3, fragments[0].Blocks.Count);
        Assert.Equal(blocks[0], fragments[0].Blocks[0]);
        Assert.Equal(blocks[2], fragments[0].Blocks[1]);
        Assert.Equal(blocks[3], fragments[0].Blocks[2]);
        Assert.Equal(fragments[1], blocks[0].Successors[0]);
        Assert.Equal(fragments[1], blocks[2].Predecessors[0]);
        Assert.Empty(fragments[0].Predecessors);
        Assert.Empty(fragments[0].Successors);

        Assert.Single(fragments[1].Blocks);
        Assert.Equal(blocks[1], fragments[1].Blocks[0]);
        Assert.Single(blocks[1].Instructions);
        Assert.Equal(1, blocks[1].Instructions[0].ValueShort);
        Assert.Equal(blocks[0], fragments[1].Predecessors[0]);
        Assert.Equal(blocks[2], fragments[1].Successors[0]);
        Assert.Empty(blocks[1].Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
    }

    [Fact]
    public void TestNested()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            pushi.e 0
            b [6]

            > child_entry
            :[1]
            pushi.e 1
            b [3]

            > child_child_entry_1
            :[2]
            pushi.e 2
            exit.i

            :[3]
            pushi.e 3
            b [5]

            > child_child_entry_2
            :[4]
            pushi.e 4
            exit.i

            :[5]
            # Test an empty block here
            exit.i

            :[6]
            pushi.e 6

            :[7]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);

        Assert.Equal(4, fragments.Count);

        Assert.Equal("root", fragments[0].CodeEntry.Name.Content);
        Assert.Equal([blocks[0], blocks[6], blocks[7]], fragments[0].Blocks);
        Assert.Equal([], fragments[0].Predecessors);
        Assert.Equal([], fragments[0].Successors);

        Assert.Equal("child_entry", fragments[1].CodeEntry.Name.Content);
        Assert.Equal([blocks[1], blocks[3], blocks[5]], fragments[1].Blocks);
        Assert.Empty(blocks[5].Instructions);
        Assert.Equal([blocks[0]], fragments[1].Predecessors);
        Assert.Equal([blocks[6]], fragments[1].Successors);
        Assert.Empty(blocks[1].Predecessors);
        Assert.Empty(blocks[5].Successors);

        Assert.Equal("child_child_entry_1", fragments[2].CodeEntry.Name.Content);
        Assert.Equal([blocks[2]], fragments[2].Blocks);
        Assert.Equal([blocks[1]], fragments[2].Predecessors);
        Assert.Equal([blocks[3]], fragments[2].Successors);
        Assert.Empty(blocks[2].Predecessors);
        Assert.Empty(blocks[2].Successors);

        Assert.Equal("child_child_entry_2", fragments[3].CodeEntry.Name.Content);
        Assert.Equal([blocks[4]], fragments[3].Blocks);
        Assert.Equal([blocks[3]], fragments[3].Predecessors);
        Assert.Equal([blocks[5]], fragments[3].Successors);
        Assert.Empty(blocks[4].Predecessors);
        Assert.Empty(blocks[4].Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
    }
}