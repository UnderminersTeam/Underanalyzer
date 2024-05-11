using Underanalyzer.Decompiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class DecompileContext_DecompileToString
{
    [Fact]
    public void TestBasic()
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

    [Fact]
    public void TestWhileIfElseEmpty()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.a
            conv.v.b
            bf [4]

            :[1]
            push.v self.b
            conv.v.b
            bf [3]

            :[2]
            b [3]

            :[3]
            b [0]

            :[4]
            """,
            """
            while (a)
            {
                if (b)
                {
                }
                else
                {
                }
            }
            """
        );
    }

    [Fact]
    public void TestNestedDoUntil()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.c
            push.v self.d
            add.v.v
            pushi.e 2
            conv.i.d
            div.d.v
            pop.v.v self.b
            push.v self.b
            pushi.e 200
            cmp.i.v GT
            bf [0]

            :[1]
            push.v self.a
            pushi.e 1
            add.i.v
            pop.v.v self.a
            push.v self.a
            pushi.e 100
            cmp.i.v GT
            bf [0]
            """,
            """
            do
            {
                do
                {
                    b = (c + d) / 2;
                }
                until (b > 200);
                a = a + 1;
            }
            until (a > 100);
            """
        );
    }

    [Fact]
    public void TestBasicSwitch()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.a
            dup.v 0
            pushi.e 1
            cmp.i.v EQ
            bt [5]

            :[1]
            dup.v 0
            pushi.e 2
            cmp.i.v EQ
            bt [7]

            :[2]
            dup.v 0
            pushi.e 3
            cmp.i.v EQ
            bt [7]

            :[3]
            b [6]

            :[4]
            b [8]

            :[5]
            push.s "Case 1"
            pop.v.s self.msg
            b [8]

            :[6]
            push.s "Default"
            pop.v.s self.msg
            b [8]

            :[7]
            push.s "Case 2 and 3"
            pop.v.s self.msg
            b [8]

            :[8]
            popz.v
            """,
            """
            switch (a)
            {
                case 1:
                    msg = "Case 1";
                    break;
                default:
                    msg = "Default";
                    break;
                case 2:
                case 3:
                    msg = "Case 2 and 3";
                    break;
            }
            """
        );
    }
}
