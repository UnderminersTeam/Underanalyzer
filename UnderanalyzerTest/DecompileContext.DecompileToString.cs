using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class DecompileContext_DecompileToString
{
    [Fact]
    public void BasicTest()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            pushi.e 123
            pop.v.i self.a
            push.v self.b
            conv.v.b
            bf [2]

            :[1]
            push.s "B is true"
            pop.v.s self.msg
            b [7]

            :[2]
            push.v self.c
            conv.v.b
            bf [4]

            :[3]
            push.v self.d
            conv.v.b
            b [5]

            :[4]
            push.e 0

            :[5]
            bf [7]

            :[6]
            push.s "C and D are both true"
            pop.v.s self.msg

            :[7]
            """,
            """
            a = 123;
            if (b)
            {
                msg = "B is true";
            }
            else if (c && d)
            {
                msg = "C and D are both true";
            }
            """
            );
    }
}
