using Underanalyzer;
using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class Nullish_FindNullish
{
    [Fact]
    public void TestBasicExpression()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.a
            isnullish.e
            bf [2]

            :[1]
            popz.v
            push.v self.b

            :[2]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Nullish> nulls = Nullish.FindNullish(blocks);

        Assert.Single(nulls);
        Assert.Equal(Nullish.NullishType.Expression, nulls[0].NullishKind);
        Assert.Equal([blocks[0]], nulls[0].Predecessors);
        Assert.Equal([blocks[2]], nulls[0].Successors);
        Assert.Equal([nulls[0]], blocks[0].Successors);
        Assert.Equal([nulls[0]], blocks[2].Predecessors);
        Assert.Equal(blocks[1], nulls[0].IfNullish);
        Assert.Empty(blocks[1].Predecessors);
        Assert.Empty(blocks[1].Successors);
        Assert.True(blocks[0].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);
        Assert.True(blocks[1].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(nulls);
    }

    [Fact]
    public void TestBasicAssignment()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.a
            isnullish.e
            bf [2]

            :[1]
            popz.v
            push.v self.b
            pop.v.v self.a
            b [3]

            :[2]
            popz.v

            :[3]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Nullish> nulls = Nullish.FindNullish(blocks);

        Assert.Single(nulls);
        Assert.Equal(Nullish.NullishType.Assignment, nulls[0].NullishKind);
        Assert.Equal([blocks[0]], nulls[0].Predecessors);
        Assert.Equal([blocks[2]], nulls[0].Successors);
        Assert.Equal([nulls[0]], blocks[0].Successors);
        Assert.Equal([nulls[0]], blocks[2].Predecessors);
        Assert.Equal(blocks[1], nulls[0].IfNullish);
        Assert.Empty(blocks[1].Predecessors);
        Assert.Empty(blocks[1].Successors);
        Assert.True(blocks[0].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);
        Assert.True(blocks[1].Instructions is [{ Kind: IGMInstruction.Opcode.Push }, { Kind: IGMInstruction.Opcode.Pop }]);
        Assert.True(blocks[2].Instructions is []);
        Assert.Equal([blocks[3]], blocks[2].Successors);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(nulls);
    }

    [Fact]
    public void TestChainExpression()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.a
            isnullish.e
            bf [2]

            :[1]
            popz.v
            push.v self.b

            :[2]
            isnullish.e
            bf [4]

            :[3]
            popz.v
            push.v self.c

            :[4]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Nullish> nulls = Nullish.FindNullish(blocks);

        Assert.Equal(2, nulls.Count);
        Assert.Equal(Nullish.NullishType.Expression, nulls[0].NullishKind);
        Assert.Equal([blocks[0]], nulls[0].Predecessors);
        Assert.Equal([blocks[2]], nulls[0].Successors);
        Assert.Equal([nulls[0]], blocks[0].Successors);
        Assert.Equal([nulls[0]], blocks[2].Predecessors);
        Assert.Equal(blocks[1], nulls[0].IfNullish);
        Assert.Empty(blocks[1].Predecessors);
        Assert.Empty(blocks[1].Successors);
        Assert.True(blocks[0].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);
        Assert.True(blocks[1].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);

        Assert.Equal(Nullish.NullishType.Expression, nulls[1].NullishKind);
        Assert.Equal([blocks[2]], nulls[1].Predecessors);
        Assert.Equal([blocks[4]], nulls[1].Successors);
        Assert.Equal([nulls[1]], blocks[2].Successors);
        Assert.Equal([nulls[1]], blocks[4].Predecessors);
        Assert.Equal(blocks[3], nulls[1].IfNullish);
        Assert.Empty(blocks[3].Predecessors);
        Assert.Empty(blocks[3].Successors);
        Assert.Equal([nulls[1]], blocks[2].Successors);
        Assert.True(blocks[2].Instructions is []);
        Assert.True(blocks[3].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(nulls);
    }

    [Fact]
    public void TestCombined()
    {
        GMCode code = TestUtil.GetCode(
            """
            :[0]
            push.v self.a
            isnullish.e
            bf [4]

            :[1]
            popz.v
            push.v self.b
            isnullish.e
            bf [3]

            :[2]
            popz.v
            push.v self.c

            :[3]
            pop.v.v self.a
            b [5]

            :[4]
            popz.v

            :[5]
            """
        );
        List<Block> blocks = Block.FindBlocks(code);
        List<Fragment> fragments = Fragment.FindFragments(code, blocks);
        List<Nullish> nulls = Nullish.FindNullish(blocks);

        Assert.Equal(2, nulls.Count);
        Assert.Equal(Nullish.NullishType.Assignment, nulls[0].NullishKind);
        Assert.Equal([blocks[0]], nulls[0].Predecessors);
        Assert.Equal([blocks[4]], nulls[0].Successors);
        Assert.Equal([nulls[0]], blocks[0].Successors);
        Assert.Equal([nulls[0]], blocks[4].Predecessors);
        Assert.Equal(blocks[1], nulls[0].IfNullish);
        Assert.Empty(blocks[1].Predecessors);
        Assert.Empty(blocks[3].Successors);
        Assert.True(blocks[0].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);
        Assert.True(blocks[1].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);
        Assert.True(blocks[4].Instructions is []);

        Assert.Equal(Nullish.NullishType.Expression, nulls[1].NullishKind);
        Assert.Equal([blocks[1]], nulls[1].Predecessors);
        Assert.Equal([blocks[3]], nulls[1].Successors);
        Assert.Equal([nulls[1]], blocks[1].Successors);
        Assert.Equal([nulls[1]], blocks[3].Predecessors);
        Assert.Equal(blocks[2], nulls[1].IfNullish);
        Assert.Empty(blocks[2].Predecessors);
        Assert.Empty(blocks[2].Successors);
        Assert.True(blocks[2].Instructions is [{ Kind: IGMInstruction.Opcode.Push }]);
        Assert.True(blocks[3].Instructions is [{ Kind: IGMInstruction.Opcode.Pop }]);

        TestUtil.VerifyFlowDirections(blocks);
        TestUtil.VerifyFlowDirections(fragments);
        TestUtil.VerifyFlowDirections(nulls);
    }
}
