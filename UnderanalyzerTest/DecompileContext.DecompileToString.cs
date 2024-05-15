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

    [Fact]
    public void TestPrePostfix()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.b
            dup.v 0
            push.e 1
            add.i.v
            pop.v.v self.b
            pop.v.v self.a
            push.v self.b
            push.e 1
            add.i.v
            dup.v 0
            pop.v.v self.b
            pop.v.v self.a
            push.v self.b
            conv.v.i
            push.v [stacktop]self.c
            conv.v.i
            dup.i 0
            push.v [stacktop]self.d
            dup.v 0
            pop.e.v 5
            push.e 1
            add.i.v
            pop.i.v [stacktop]self.d
            pop.v.v self.a
            push.v self.b
            conv.v.i
            push.v [stacktop]self.c
            conv.v.i
            dup.i 0
            push.v [stacktop]self.d
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 5
            pop.i.v [stacktop]self.d
            pop.v.v self.a
            pushi.e -1
            pushi.e 0
            dup.l 0
            push.v [array]self.b
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e 0
            dup.l 0
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.b
            pop.v.v self.a
            push.v self.a
            conv.v.i
            push.v [stacktop]self.b
            conv.v.i
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            pop.v.v self.a
            push.v self.a
            conv.v.i
            push.v [stacktop]self.b
            conv.v.i
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.c
            pop.v.v self.a
            push.v self.a
            conv.v.i
            pushi.e 0
            push.v [array]self.b
            conv.v.i
            dup.i 0
            push.v [stacktop]self.c
            dup.v 0
            pop.e.v 5
            push.e 1
            add.i.v
            pop.i.v [stacktop]self.c
            pop.v.v self.a
            push.v self.a
            conv.v.i
            pushi.e 0
            push.v [array]self.b
            conv.v.i
            dup.i 0
            push.v [stacktop]self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 5
            pop.i.v [stacktop]self.c
            pop.v.v self.a
            push.v self.a
            conv.v.i
            pushi.e 0
            push.v [array]self.b
            conv.v.i
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            pop.v.v self.a
            push.v self.a
            conv.v.i
            pushi.e 0
            push.v [array]self.b
            conv.v.i
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.c
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            dup.v 0
            push.e 1
            add.i.v
            pop.v.v self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.v.v self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.v.v self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            dup.v 0
            push.e 1
            add.i.v
            pop.v.v self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.l 0
            push.v [array]self.c
            dup.v 0
            pop.e.v 6
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            conv.v.i
            dup.l 0
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            pop.e.v 6
            pop.i.v [array]self.b
            pop.v.v self.a
            """,
            """
            a = b++;
            a = ++b;
            a = b.c.d++;
            a = ++b.c.d;
            a = b[0]++;
            a = ++b[0];
            a = a.b.c[0]++;
            a = ++a.b.c[0];
            a = a.b[0].c++;
            a = ++a.b[0].c;
            a = a.b[0].c[0]++;
            a = ++a.b[0].c[0];
            a = b[c++]++;
            a = b[++c]++;
            a = ++b[++c];
            a = ++b[c++];
            a = b[c[0]++]++;
            a = b[++c[0]]++;
            a = ++b[++c[0]];
            a = ++b[c[0]++];
            """
        );
    }

    [Fact]
    public void TestPrePostfix_GMLv2()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.b
            dup.v 0
            push.e 1
            add.i.v
            pop.v.v self.b
            pop.v.v self.a
            push.v self.b
            push.e 1
            add.i.v
            dup.v 0
            pop.v.v self.b
            pop.v.v self.a
            push.v self.b
            pushi.e -9
            push.v [stacktop]self.c
            pushi.e -9
            dup.i 4
            push.v [stacktop]self.d
            dup.v 0
            dup.i 4 9
            push.e 1
            add.i.v
            pop.i.v [stacktop]self.d
            pop.v.v self.a
            push.v self.b
            pushi.e -9
            push.v [stacktop]self.c
            pushi.e -9
            dup.i 4
            push.v [stacktop]self.d
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 9
            pop.i.v [stacktop]self.d
            pop.v.v self.a
            pushi.e -1
            pushi.e 0
            dup.i 1
            push.v [array]self.b
            dup.v 0
            dup.i 4 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e 0
            dup.i 1
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 6
            pop.i.v [array]self.b
            pop.v.v self.a
            push.v self.a
            pushi.e -9
            push.v [stacktop]self.b
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.c
            dup.v 0
            dup.i 4 10
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            pop.v.v self.a
            push.v self.a
            pushi.e -9
            push.v [stacktop]self.b
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 10
            pop.i.v [array]self.c
            pop.v.v self.a
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pushi.e -9
            dup.i 4
            push.v [stacktop]self.c
            dup.v 0
            dup.i 4 9
            push.e 1
            add.i.v
            pop.i.v [stacktop]self.c
            pop.v.v self.a
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pushi.e -9
            dup.i 4
            push.v [stacktop]self.c
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 9
            pop.i.v [stacktop]self.c
            pop.v.v self.a
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.c
            dup.v 0
            dup.i 4 10
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            pop.v.v self.a
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 10
            pop.i.v [array]self.c
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            dup.v 0
            push.e 1
            add.i.v
            pop.v.v self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            dup.v 0
            dup.i 4 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.v.v self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            dup.v 0
            dup.i 4 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            push.e 1
            add.i.v
            dup.v 0
            pop.v.v self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 6
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            push.v self.c
            dup.v 0
            push.e 1
            add.i.v
            pop.v.v self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 6
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.i 1
            push.v [array]self.c
            dup.v 0
            dup.i 4 6
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            dup.v 0
            dup.i 4 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.i 1
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 6
            pop.i.v [array]self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            dup.v 0
            dup.i 4 6
            push.e 1
            add.i.v
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.i 1
            push.v [array]self.c
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 6
            pop.i.v [array]self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 6
            pop.i.v [array]self.b
            pop.v.v self.a
            pushi.e -1
            pushi.e -1
            pushi.e 0
            dup.i 1
            push.v [array]self.c
            dup.v 0
            dup.i 4 6
            push.e 1
            add.i.v
            pop.i.v [array]self.c
            conv.v.i
            dup.i 1
            push.v [array]self.b
            push.e 1
            add.i.v
            dup.v 0
            dup.i 4 6
            pop.i.v [array]self.b
            pop.v.v self.a
            """,
            """
            a = b++;
            a = ++b;
            a = b.c.d++;
            a = ++b.c.d;
            a = b[0]++;
            a = ++b[0];
            a = a.b.c[0]++;
            a = ++a.b.c[0];
            a = a.b[0].c++;
            a = ++a.b[0].c;
            a = a.b[0].c[0]++;
            a = ++a.b[0].c[0];
            a = b[c++]++;
            a = b[++c]++;
            a = ++b[++c];
            a = ++b[c++];
            a = b[c[0]++]++;
            a = b[++c[0]]++;
            a = ++b[++c[0]];
            a = ++b[c[0]++];
            """
        );
    }

    [Fact]
    public void TestNullishTernary()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.a
            isnullish.e
            bf [5]
            
            :[1]
            popz.v
            push.v self.b
            conv.v.b
            bf [3]
            
            :[2]
            push.v self.c
            b [4]
            
            :[3]
            push.v self.d
            
            :[4]
            pop.v.v self.a
            b [6]
            
            :[5]
            popz.v
            
            :[6]
            """,
            """
            a ??= b ? c : d;
            """
        );
    }

    [Fact]
    public void TestMultiWithBreak()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.a
            conv.v.b
            bf [10]

            :[1]
            push.v self.b
            pushi.e -9
            pushenv [6]

            :[2]
            push.v self.c
            pushi.e -9
            pushenv [4]

            :[3]
            pushi.e 1
            pop.v.i self.d

            :[4]
            popenv [3]

            :[5]
            pushi.e 1
            pop.v.i self.e
            b [8]

            :[6]
            popenv [2]

            :[7]
            b [9]

            :[8]
            popenv <drop>

            :[9]
            b [12]

            :[10]
            push.v self.f
            pushi.e -9
            pushenv [11]

            :[11]
            popenv [11]

            :[12]
            """,
            """
            if (a)
            {
                with (b)
                {
                    with (c)
                    {
                        d = 1;
                    }
                    e = 1;
                    break;
                }
            }
            else
            {
                with (f)
                {
                }
            }
            """
        );
    }

    [Fact]
    public void TestMultiWithBreak2()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.a
            conv.v.b
            bf [10]

            :[1]
            push.v self.b
            pushi.e -9
            pushenv [6]

            :[2]
            push.v self.c
            pushi.e -9
            pushenv [4]

            :[3]
            pushi.e 1
            pop.v.i self.d

            :[4]
            popenv [3]

            :[5]
            pushi.e 1
            pop.v.i self.e
            b [8]

            :[6]
            popenv [2]
            
            :[7]
            b [9]

            :[8]
            popenv <drop>

            :[9]
            b [18]

            :[10]
            push.v self.b
            pushi.e -9
            pushenv [15]

            :[11]
            push.v self.c
            pushi.e -9
            pushenv [13]

            :[12]
            pushi.e 1
            pop.v.i self.d

            :[13]
            popenv [12]
            
            :[14]
            pushi.e 1
            pop.v.i self.e
            b [17]

            :[15]
            popenv [11]
            
            :[16]
            b [18]

            :[17]
            popenv <drop>

            :[18]
            """,
            """
            if (a)
            {
                with (b)
                {
                    with (c)
                    {
                        d = 1;
                    }
                    e = 1;
                    break;
                }
            }
            else
            {
                with (b)
                {
                    with (c)
                    {
                        d = 1;
                    }
                    e = 1;
                    break;
                }
            }
            """
        );
    }

    [Fact]
    public void TestIfElseWithBreak()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.a
            conv.v.b
            bf [2]

            :[1]
            b [7]

            :[2]
            push.v self.b
            pushi.e -9
            pushenv [4]

            :[3]
            b [6]

            :[4]
            popenv [3]

            :[5]
            b [7]

            :[6]
            popenv <drop>

            :[7]
            """,
            """
            if (a)
            {
            }
            else
            {
                with (b)
                {
                    break;
                }
            }
            """
        );
    }

    [Fact]
    public void TestSwitchIfShortCircuit()
    {
        TestUtil.VerifyDecompileResult(
            """
            :[0]
            push.v self.a
            dup.v 0
            pushi.e 1
            cmp.i.v EQ
            bt [2]

            :[1]
            b [3]

            :[2]
            b [3]

            :[3]
            popz.v
            push.v self.b
            conv.v.b
            bf [5]

            :[4]
            push.v self.c
            conv.v.b
            b [6]

            :[5]
            push.e 0

            :[6]
            bf [7]

            :[7]
            """,
            """
            switch (a)
            {
                case 1:
                    break;
            }
            if (b && c)
            {
            }
            """
        );
    }
}
