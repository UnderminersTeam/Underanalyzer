/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer;

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
            pushi.e -5
            pop.v.i self.c
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

    [Fact]
    public void TestInstanceConstants3()
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
                UsingGMLv2 = true,
                UsingGlobalConstantFunction = true
            }
        );
    }

    [Fact]
    public void TestVariableCalls()
    {
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false
        };
        ((Underanalyzer.Mock.BuiltinsMock)gameContext.Builtins)
            .BuiltinFunctions["show_debug_message"] = new("show_debug_message", 1, 1);
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.AssertBytecode(
            """
            var d;
            show_debug_message("a");
            test("a");
            self.test("a");
            a.b("a");
            d("a");
            obj_test.a("a");
            obj_test.a.b("a");
            """,
            """
            push.s "a"
            conv.s.v
            call.i show_debug_message 1
            popz.v
            push.s "a"
            conv.s.v
            call.i @@This@@ 0
            push.v builtin.test
            callv.v 1
            popz.v
            call.i @@This@@ 0
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.test
            callv.v 1
            popz.v
            push.v self.a
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            push.s "a"
            conv.s.v
            call.i @@This@@ 0
            pushloc.v local.d
            callv.v 1
            popz.v
            pushi.e 123
            conv.i.v
            call.i @@GetInstance@@ 1
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.a
            callv.v 1
            popz.v
            push.v 123.a
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestVariableCallsAssetRefs()
    {
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = true,
            UsingSelfToBuiltin = true
        };
        ((Underanalyzer.Mock.BuiltinsMock)gameContext.Builtins)
            .BuiltinFunctions["show_debug_message"] = new("show_debug_message", 1, 1);
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.AssertBytecode(
            """
            var d;
            show_debug_message("a");
            test("a");
            self.test("a");
            a.b("a");
            d("a");
            obj_test.a("a");
            obj_test.a.b("a");
            """,
            """
            push.s "a"
            conv.s.v
            call.i show_debug_message 1
            popz.v
            push.s "a"
            conv.s.v
            call.i @@This@@ 0
            push.v builtin.test
            callv.v 1
            popz.v
            call.i @@This@@ 0
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.test
            callv.v 1
            popz.v
            push.v builtin.a
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            push.s "a"
            conv.s.v
            call.i @@This@@ 0
            pushloc.v local.d
            callv.v 1
            popz.v
            pushref.i 123 Object
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.a
            callv.v 1
            popz.v
            pushref.i 123 Object
            pushi.e -9
            push.v [stacktop]self.a
            push.s "a"
            conv.s.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestRepeatVariableCalls()
    {
        // NOTE: as of writing, "a.b(1)(2)(3);" does not compile correctly in GameMaker, but does here
        TestUtil.AssertBytecode(
            """
            a(1)(2)(3)(4)(5);
            a.b(1)(2)(3);
            a(1).b(2);
            a(1).b(2)(3)(4);
            a(1).b(2)(3).c(4);
            """,
            """
            pushi.e 5
            conv.i.v
            call.i @@This@@ 0
            pushi.e 4
            conv.i.v
            call.i @@This@@ 0
            pushi.e 3
            conv.i.v
            call.i @@This@@ 0
            pushi.e 2
            conv.i.v
            call.i @@This@@ 0
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            push.v builtin.a
            callv.v 1
            callv.v 1
            callv.v 1
            callv.v 1
            callv.v 1
            popz.v
            pushi.e 3
            conv.i.v
            call.i @@This@@ 0
            pushi.e 2
            conv.i.v
            call.i @@This@@ 0
            push.v self.a
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            callv.v 1
            callv.v 1
            popz.v
            pushi.e 2
            conv.i.v
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            push.v builtin.a
            callv.v 1
            dup.v 0
            pushi.e -9
            push.v [stacktop]self.b
            callv.v 1
            popz.v
            pushi.e 4
            conv.i.v
            call.i @@This@@ 0
            pushi.e 3
            conv.i.v
            call.i @@This@@ 0
            pushi.e 2
            conv.i.v
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            push.v builtin.a
            callv.v 1
            dup.v 0
            pushi.e -9
            push.v [stacktop]self.b
            callv.v 1
            callv.v 1
            callv.v 1
            popz.v
            pushi.e 4
            conv.i.v
            pushi.e 3
            conv.i.v
            call.i @@This@@ 0
            pushi.e 2
            conv.i.v
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            push.v builtin.a
            callv.v 1
            dup.v 0
            pushi.e -9
            push.v [stacktop]self.b
            callv.v 1
            callv.v 1
            dup.v 0
            pushi.e -9
            push.v [stacktop]self.c
            callv.v 1
            popz.v
            """
        );
    }

    [Fact]
    public void TestRepeatVariableCalls2()
    {
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.AssertBytecode(
            """
            obj_test.a(1).b(2);
            """,
            """
            pushi.e 123
            conv.i.v
            call.i @@GetInstance@@ 1
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.a
            callv.v 1
            pushi.e 2
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestRepeatVariableCalls2AssetRefs()
    {
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = true
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.AssertBytecode(
            """
            obj_test.a(1).b(2);
            """,
            """
            pushref.i 123 Object
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.a
            callv.v 1
            pushi.e 2
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestRepeatVariableCalls3()
    {
        TestUtil.AssertBytecode(
            """
            self.a(1).b(2);
            """,
            """
            call.i @@This@@ 0
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.a
            callv.v 1
            pushi.e 2
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            """
        );
    }

    [Fact]
    public void TestVariableCallsArrays()
    {
        // NOTE: These compile somewhat differently depending on GameMaker version, which this currently ignores.
        //       As of writing this test, the goal is to mainly get compatible code compilation for all versions.
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.AssertBytecode(
            """
            a[0](1);
            self.a[0](1);
            global.a[0](1);
            obj_test.a[0](1);
            a[0].b(1);
            a[0].b[1](2);
            a[0](1).b[2](3);
            a.b[0](1).c.d[2](3);
            a.b[0].c(1).d.e[2].f(3);
            """,
            """
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            callv.v 1
            popz.v
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            push.v [array]self.a
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 1
            conv.i.v
            pushi.e -5
            pushi.e 0
            push.v [array]self.a
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 1
            conv.i.v
            pushi.e 123
            pushi.e 0
            push.v [array]self.a
            dup.v 0
            callv.v 1
            popz.v
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            pushi.e 2
            conv.i.v
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            pushi.e -9
            pushi.e 1
            push.v [array]self.b
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 3
            conv.i.v
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            callv.v 1
            pushi.e -9
            pushi.e 2
            push.v [array]self.b
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 3
            conv.i.v
            pushi.e 1
            conv.i.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            dup.v 0
            callv.v 1
            pushi.e -9
            push.v [stacktop]self.c
            pushi.e -9
            pushi.e 2
            push.v [array]self.d
            dup.v 0
            callv.v 1
            popz.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.c
            callv.v 1
            pushi.e -9
            push.v [stacktop]self.d
            pushi.e -9
            pushi.e 2
            push.v [array]self.e
            pushi.e 3
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.f
            callv.v 1
            popz.v
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestVariableCallsArraysAssetRefs()
    {
        // NOTE: These compile somewhat differently depending on GameMaker version, which this currently ignores.
        //       As of writing this test, the goal is to mainly get compatible code compilation for all versions.
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = true
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.AssertBytecode(
            """
            a[0](1);
            self.a[0](1);
            global.a[0](1);
            obj_test.a[0](1);
            a[0].b(1);
            a[0].b[1](2);
            a[0](1).b[2](3);
            a.b[0](1).c.d[2](3);
            a.b[0].c(1).d.e[2].f(3);
            """,
            """
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            callv.v 1
            popz.v
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            push.v [array]self.a
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 1
            conv.i.v
            pushi.e -5
            pushi.e 0
            push.v [array]self.a
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 1
            conv.i.v
            pushref.i 123 Object
            pushi.e -9
            pushi.e 0
            push.v [array]self.a
            dup.v 0
            callv.v 1
            popz.v
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            pushi.e 2
            conv.i.v
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            pushi.e -9
            pushi.e 1
            push.v [array]self.b
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 3
            conv.i.v
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            callv.v 1
            pushi.e -9
            pushi.e 2
            push.v [array]self.b
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 3
            conv.i.v
            pushi.e 1
            conv.i.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            dup.v 0
            callv.v 1
            pushi.e -9
            push.v [stacktop]self.c
            pushi.e -9
            pushi.e 2
            push.v [array]self.d
            dup.v 0
            callv.v 1
            popz.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pushi.e 1
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.c
            callv.v 1
            pushi.e -9
            push.v [stacktop]self.d
            pushi.e -9
            pushi.e 2
            push.v [array]self.e
            pushi.e 3
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.f
            callv.v 1
            popz.v
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestVariableCallsArraysMulti()
    {
        // NOTE: These compile somewhat differently depending on GameMaker version, which this currently ignores.
        //       As of writing this test, the goal is to mainly get compatible code compilation for all versions.
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.AssertBytecode(
            """
            a[0][1](2);
            self.a[0][1](2);
            global.a[0][1](2);
            obj_test.a[0][1](2);
            a.b[0][1](2);
            a[0][1].b(2);
            a[0][1].b[1][2](3);
            a[0][1](2).b[2][3](4);
            a.b[0][1](2).c.d[3][4](5);
            a.b[0][1].c(2).d.e[3][4].f(5);
            """,
            """
            pushi.e 2
            conv.i.v
            call.i @@This@@ 0
            pushi.e -1
            pushi.e 0
            push.v [multipush]self.a
            pushi.e 1
            pushaf.e
            callv.v 1
            popz.v
            pushi.e 2
            conv.i.v
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            push.v [multipush]self.a
            pushi.e 1
            pushaf.e
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 2
            conv.i.v
            pushi.e -5
            pushi.e 0
            push.v [multipush]self.a
            pushi.e 1
            pushaf.e
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 2
            conv.i.v
            pushi.e 123
            pushi.e 0
            push.v [multipush]self.a
            pushi.e 1
            pushaf.e
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 2
            conv.i.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [multipush]self.b
            pushi.e 1
            pushaf.e
            dup.v 0
            callv.v 1
            popz.v
            pushi.e -1
            pushi.e 0
            push.v [multipush]self.a
            pushi.e 1
            pushaf.e
            pushi.e 2
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.b
            callv.v 1
            popz.v
            pushi.e 3
            conv.i.v
            pushi.e -1
            pushi.e 0
            push.v [multipush]self.a
            pushi.e 1
            pushaf.e
            pushi.e -9
            pushi.e 1
            push.v [multipush]self.b
            pushi.e 2
            pushaf.e
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 4
            conv.i.v
            pushi.e 2
            conv.i.v
            call.i @@This@@ 0
            pushi.e -1
            pushi.e 0
            push.v [multipush]self.a
            pushi.e 1
            pushaf.e
            callv.v 1
            pushi.e -9
            pushi.e 2
            push.v [multipush]self.b
            pushi.e 3
            pushaf.e
            dup.v 0
            callv.v 1
            popz.v
            pushi.e 5
            conv.i.v
            pushi.e 2
            conv.i.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [multipush]self.b
            pushi.e 1
            pushaf.e
            dup.v 0
            callv.v 1
            pushi.e -9
            push.v [stacktop]self.c
            pushi.e -9
            pushi.e 3
            push.v [multipush]self.d
            pushi.e 4
            pushaf.e
            dup.v 0
            callv.v 1
            popz.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [multipush]self.b
            pushi.e 1
            pushaf.e
            pushi.e 2
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.c
            callv.v 1
            pushi.e -9
            push.v [stacktop]self.d
            pushi.e -9
            pushi.e 3
            push.v [multipush]self.e
            pushi.e 4
            pushaf.e
            pushi.e 5
            conv.i.v
            dup.v 1 1
            dup.v 0
            push.v stacktop.f
            callv.v 1
            popz.v
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestFunctionDeclNames()
    {
        TestUtil.AssertBytecode(
            """
            function GlobalScriptMockName()
            {
            }

            function RandomOtherName()
            {
            }
            """,
            """
            :[0]
            b [2]

            > global_func_GlobalScriptMockName (locals=0, args=0)
            :[1]
            exit.i

            :[2]
            push.i [function]global_func_GlobalScriptMockName
            conv.i.v
            pushi.e -1
            conv.i.v
            call.i method 2
            dup.v 0
            pushi.e -1
            pop.v.v [stacktop]self.GlobalScriptMockName
            popz.v
            b [4]

            > global_func_RandomOtherName (locals=0, args=0)
            :[3]
            exit.i

            :[4]
            push.i [function]global_func_RandomOtherName
            conv.i.v
            pushi.e -1
            conv.i.v
            call.i method 2
            dup.v 0
            pushi.e -6
            pop.v.v [stacktop]self.RandomOtherName
            popz.v
            """,
            true
        );
    }

    [Fact]
    public void TestNewObject()
    {
        TestUtil.AssertBytecode(
            """
            function Test() constructor 
            {
            }

            new Test();
            new self.Test();
            new global.Test();
            a = new Test();
            b = new VariableCall();
            b = new self.VariableCall();
            b = new global.VariableCall();
            new Complex.Variable.Call(123, 456);
            """,
            """
            :[0]
            b [2]

            > regular_func_Test (locals=0, args=0)
            :[1]
            exit.i

            :[2]
            push.i [function]regular_func_Test
            conv.i.v
            call.i @@NullObject@@ 0
            call.i method 2
            dup.v 0
            pushi.e -6
            pop.v.v [stacktop]self.Test
            popz.v
            push.i [function]regular_func_Test
            conv.i.v
            call.i @@NewGMLObject@@ 1
            popz.v
            push.i [function]regular_func_Test
            conv.i.v
            call.i @@NewGMLObject@@ 1
            popz.v
            push.i [function]regular_func_Test
            conv.i.v
            call.i @@NewGMLObject@@ 1
            popz.v
            push.i [function]regular_func_Test
            conv.i.v
            call.i @@NewGMLObject@@ 1
            pop.v.v self.a
            push.v builtin.VariableCall
            call.i @@NewGMLObject@@ 1
            pop.v.v self.b
            push.v self.VariableCall
            call.i @@NewGMLObject@@ 1
            pop.v.v self.b
            pushglb.v global.VariableCall
            call.i @@NewGMLObject@@ 1
            pop.v.v self.b
            pushi.e 456
            conv.i.v
            pushi.e 123
            conv.i.v
            push.v self.Complex
            pushi.e -9
            push.v [stacktop]self.Variable
            pushi.e -9
            push.v [stacktop]self.Call
            call.i @@NewGMLObject@@ 3
            popz.v
            """
        );
    }

    [Fact]
    public void TestNonSelfToBuiltin()
    {
        TestUtil.AssertBytecode(
            """
            a = 0;
            self.a = 0;
            a = b;
            a = self.b;
            a += 1;
            self.a += 1;
            a++;
            self.a++;
            a.b = 0;
            a[0] = 1;
            self.a[0] = 1;
            a.b[0] = 1;
            a[0] += 1;
            self.a[0] += 1;
            a.b[0] += 1;
            a = a[0];
            a = self.a[0];
            a = a.b[0];
            global.a = 0;
            global.a[0] = 0;
            """,
            """
            pushi.e 0
            pop.v.i self.a
            pushi.e 0
            pop.v.i self.a
            push.v self.b
            pop.v.v self.a
            push.v self.b
            pop.v.v self.a
            push.v self.a
            pushi.e 1
            add.i.v
            pop.v.v self.a
            push.v self.a
            pushi.e 1
            add.i.v
            pop.v.v self.a
            push.v self.a
            push.e 1
            add.i.v
            pop.v.v self.a
            push.v self.a
            push.e 1
            add.i.v
            pop.v.v self.a
            pushi.e 0
            push.v self.a
            pushi.e -9
            pop.v.i [stacktop]self.b
            pushi.e 1
            conv.i.v
            pushi.e -1
            pushi.e 0
            pop.v.v [array]self.a
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            pop.v.v [array]self.a
            pushi.e 1
            conv.i.v
            push.v self.a
            pushi.e -9
            pushi.e 0
            pop.v.v [array]self.b
            pushi.e -1
            pushi.e 0
            dup.i 1
            push.v [array]self.a
            pushi.e 1
            add.i.v
            pop.i.v [array]self.a
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.a
            pushi.e 1
            add.i.v
            pop.i.v [array]self.a
            push.v self.a
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.b
            pushi.e 1
            add.i.v
            pop.i.v [array]self.b
            pushi.e -1
            pushi.e 0
            push.v [array]self.a
            pop.v.v self.a
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            push.v [array]self.a
            pop.v.v self.a
            push.v self.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pop.v.v self.a
            pushi.e 0
            pop.v.i global.a
            pushi.e 0
            conv.i.v
            pushi.e -5
            pushi.e 0
            pop.v.v [array]self.a
            """
        );
    }

    [Fact]
    public void TestSelfToBuiltin()
    {
        Underanalyzer.Mock.GameContextMock gameContext = new()
        {
            UsingSelfToBuiltin = true,
            UsingGlobalConstantFunction = true
        };
        TestUtil.AssertBytecode(
            """
            a = 0;
            self.a = 0;
            a = b;
            a = self.b;
            a += 1;
            self.a += 1;
            a++;
            self.a++;
            a.b = 0;
            a[0] = 1;
            self.a[0] = 1;
            a.b[0] = 1;
            a[0] += 1;
            self.a[0] += 1;
            a.b[0] += 1;
            a = a[0];
            a = self.a[0];
            a = a.b[0];
            global.a = 0;
            global.a[0] = 0;
            """,
            """
            pushi.e 0
            pop.v.i builtin.a
            pushi.e 0
            pop.v.i self.a
            push.v builtin.b
            pop.v.v builtin.a
            push.v self.b
            pop.v.v builtin.a
            push.v builtin.a
            pushi.e 1
            add.i.v
            pop.v.v builtin.a
            push.v self.a
            pushi.e 1
            add.i.v
            pop.v.v self.a
            push.v builtin.a
            push.e 1
            add.i.v
            pop.v.v builtin.a
            push.v self.a
            push.e 1
            add.i.v
            pop.v.v self.a
            pushi.e 0
            push.v builtin.a
            pushi.e -9
            pop.v.i [stacktop]self.b
            pushi.e 1
            conv.i.v
            pushi.e -6
            pushi.e 0
            pop.v.v [array]self.a
            pushi.e 1
            conv.i.v
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            pop.v.v [array]self.a
            pushi.e 1
            conv.i.v
            push.v builtin.a
            pushi.e -9
            pushi.e 0
            pop.v.v [array]self.b
            pushi.e -6
            pushi.e 0
            dup.i 1
            push.v [array]self.a
            pushi.e 1
            add.i.v
            pop.i.v [array]self.a
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.a
            pushi.e 1
            add.i.v
            pop.i.v [array]self.a
            push.v builtin.a
            pushi.e -9
            pushi.e 0
            dup.i 5
            push.v [array]self.b
            pushi.e 1
            add.i.v
            pop.i.v [array]self.b
            pushi.e -6
            pushi.e 0
            push.v [array]self.a
            pop.v.v builtin.a
            call.i @@This@@ 0
            pushi.e -9
            pushi.e 0
            push.v [array]self.a
            pop.v.v builtin.a
            push.v builtin.a
            pushi.e -9
            pushi.e 0
            push.v [array]self.b
            pop.v.v builtin.a
            pushi.e 0
            pop.v.i global.a
            pushi.e 0
            conv.i.v
            call.i @@Global@@ 0
            pushi.e -9
            pushi.e 0
            pop.v.v [array]self.a
            """,
            false,
            gameContext
        );
    }
}