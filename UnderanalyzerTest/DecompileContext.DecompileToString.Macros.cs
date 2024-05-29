using Underanalyzer.Decompiler;

namespace UnderanalyzerTest;

public class DecompileContext_DecompileToString_Macros
{
    [Fact]
    public void TestUnknownEnum()
    {
        DecompileContext context = TestUtil.VerifyDecompileResult(
            """
            push.l 0
            pop.v.l builtin.value0
            push.l 1
            pop.v.l builtin.value1
            push.l 11
            pop.v.l builtin.value11
            push.l 10
            pop.v.l builtin.value10
            """,
            """
            enum UnknownEnum
            {
                Value_0,
                Value_1,
                Value_10 = 10,
                Value_11
            }
            value0 = UnknownEnum.Value_0;
            value1 = UnknownEnum.Value_1;
            value11 = UnknownEnum.Value_11;
            value10 = UnknownEnum.Value_10;
            """
        );
    }
}
