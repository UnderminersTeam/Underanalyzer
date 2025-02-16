﻿/*
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
            UsingAssetReferences = true
        };
        ((Underanalyzer.Mock.BuiltinsMock)gameContext.Builtins)
            .BuiltinFunctions["show_debug_message"] = new("show_debug_message", 1, 1);
        ((Underanalyzer.Mock.CodeBuilderMock)gameContext.CodeBuilder).SelfToBuiltin = true;
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
}