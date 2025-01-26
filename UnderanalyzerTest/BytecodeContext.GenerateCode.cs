/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace UnderanalyzerTest;

public class BytecodeContext_GenerateCode
{
    [Fact]
    public void TestReturn()
    {
        TestUtil.AssertBytecode(
            """
            return 123;
            """,
            """
            pushi.e 123
            conv.i.v
            ret.v
            """
        );
    }

    [Fact]
    public void TestSimpleAssigns()
    {
        TestUtil.AssertBytecode(
            """
            a = 1;
            var b = 2;
            self.c = 3;
            global.d = 4;
            b = 5;
            """,
            """
            pushi.e 1
            pop.v.i self.a
            pushi.e 2
            pop.v.i local.b
            pushi.e 3
            pop.v.i self.c
            pushi.e 4
            pop.v.i global.d
            pushi.e 5
            pop.v.i local.b
            """
        );
    }

    [Fact]
    public void TestInstanceConstants1()
    {
        TestUtil.AssertBytecode(
            """
            a = self;
            b = other;
            c = global;
            d = noone;
            """,
            """
            pushi.e -1
            pop.v.i self.a
            pushi.e -2
            pop.v.i self.b
            pushi.e -5
            pop.v.i self.c
            pushi.e -4
            pop.v.i self.d
            """,
            false,
            new Underanalyzer.Mock.GameContextMock()
            {
                UsingGMLv2 = false
            }
        );
    }

    [Fact]
    public void TestInstanceConstants2()
    {
        TestUtil.AssertBytecode(
            """
            a = self;
            b = other;
            c = global;
            d = noone;
            """,
            """
            call.i @@This@@ 0
            pop.v.v self.a
            call.i @@Other@@ 0
            pop.v.v self.b
            call.i @@Global@@ 0
            pop.v.v self.c
            pushi.e -4
            pop.v.i self.d
            """,
            false,
            new Underanalyzer.Mock.GameContextMock()
            {
                UsingGMLv2 = true
            }
        );
    }
}