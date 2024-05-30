﻿using Underanalyzer;
using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.Macros;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class DecompileContext_DecompileToString_Macros
{
    [Fact]
    public void TestUnknownEnum()
    {
        TestUtil.VerifyDecompileResult(
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

    [Fact]
    public void TestKnownEnum()
    {
        GameContextMock gameContext = new();
        EnumMacroType testEnum = new("TestEnum", new Dictionary<long, string>()
        {
            { 0, "TestValue0" },
            { 1, "TestValue1" },
            { 2, "TestValue2_Unused" },
            { 10, "TestValue10" },
            { 11, "TestValue11" }
        });
        NameMacroTypeResolver globalNames = gameContext.MacroTypeRegistry.Resolver.GlobalNames;
        globalNames.DefineVariableType("value0", testEnum);
        globalNames.DefineVariableType("value1", testEnum);
        globalNames.DefineVariableType("value10", testEnum);
        globalNames.DefineVariableType("value11", testEnum);

        TestUtil.VerifyDecompileResult(
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
            enum TestEnum
            {
                TestValue0,
                TestValue1,
                TestValue2_Unused,
                TestValue10 = 10,
                TestValue11
            }
            value0 = TestEnum.TestValue0;
            value1 = TestEnum.TestValue1;
            value11 = TestEnum.TestValue11;
            value10 = TestEnum.TestValue10;
            """,
            gameContext
        );
    }

    [Fact]
    public void TestAssetsWithAssign()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test", AssetType.Sprite);
        gameContext.DefineMockAsset(123, "obj_test", AssetType.Object);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineVariableType("sprite_index", registry.FindType("Asset.Sprite"));

        TestUtil.VerifyDecompileResult(
            """
            :[0]
            pushi.e 123
            pushenv [2]

            :[1]
            pushi.e 0
            pop.v.i self.sprite_index

            :[2]
            popenv [1]
            """,
            """
            with (obj_test)
            {
                sprite_index = spr_test;
            }
            """,
            gameContext
        );
    }

    [Fact]
    public void TestFunctionCall()
    {
        GameContextMock gameContext = new();

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        var mockColorMacro = new ConstantsMacroType(new Dictionary<int, string>()
        {
            { 0, "c_black" }
        });
        globalNames.DefineFunctionArgumentsType("draw_set_color", new FunctionArgsMacroType([mockColorMacro]));
        globalNames.DefineFunctionArgumentsType("draw_rectangle_color", 
            new FunctionArgsMacroType([null, null, null, null, mockColorMacro, mockColorMacro, mockColorMacro, mockColorMacro, null]));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 0
            conv.i.v
            call.i draw_set_color 1
            popz.v
            pushi.e 1
            conv.b.v
            pushi.e 0
            conv.i.v
            pushi.e 0
            conv.i.v
            pushi.e 0
            conv.i.v
            pushi.e 0
            conv.i.v
            pushi.e 4
            conv.i.v
            pushi.e 3
            conv.i.v
            pushi.e 2
            conv.i.v
            pushi.e 1
            conv.i.v
            call.i draw_rectangle_color 9
            popz.v
            """,
            """
            draw_set_color(c_black);
            draw_rectangle_color(1, 2, 3, 4, c_black, c_black, c_black, c_black, true);
            """,
            gameContext
        );
    }

    [Fact]
    public void TestBoolean()
    {
        GameContextMock gameContext = new()
        {
            UsingTypedBooleans = false
        };

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineVariableType("test", registry.FindType("Bool"));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 1
            pop.v.i self.test
            pushi.e 0
            pop.v.i self.test
            """,
            """
            test = true;
            test = false;
            """,
            gameContext
        );
    }

    [Fact]
    public void TestReturnValueBinary()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test", AssetType.Sprite);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineFunctionReturnType("scr_test", registry.FindType("Asset.Sprite"));

        TestUtil.VerifyDecompileResult(
            """
            :[0]
            call.i scr_test 0
            pushi.e 0
            cmp.i.v EQ
            bf [1]

            :[1]
            """,
            """
            if (scr_test() == spr_test)
            {
            }
            """,
            gameContext
        );
    }

    [Fact]
    public void TestArrayInit()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test0", AssetType.Sprite);
        gameContext.DefineMockAsset(1, "spr_test1", AssetType.Sprite);
        gameContext.DefineMockAsset(2, "spr_test2", AssetType.Sprite);
        gameContext.DefineMockAsset(3, "spr_test3", AssetType.Sprite);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineVariableType("test_array", new ArrayInitMacroType(registry.FindType("Asset.Sprite")));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 3
            conv.i.v
            pushi.e 2
            conv.i.v
            pushi.e 1
            conv.i.v
            pushi.e 0
            conv.i.v
            call.i @@NewGMLArray@@ 4
            pop.v.v self.test_array
            """,
            """
            test_array = [spr_test0, spr_test1, spr_test2, spr_test3];
            """,
            gameContext
        );
    }

    [Fact]
    public void TestChoose()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test0", AssetType.Sprite);
        gameContext.DefineMockAsset(1, "spr_test1", AssetType.Sprite);
        gameContext.DefineMockAsset(2, "spr_test2", AssetType.Sprite);
        gameContext.DefineMockAsset(3, "spr_test3", AssetType.Sprite);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineVariableType("random_sprite", registry.FindType("Asset.Sprite"));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 3
            conv.i.v
            pushi.e 2
            conv.i.v
            pushi.e 1
            conv.i.v
            pushi.e 0
            conv.i.v
            call.i choose 4
            pop.v.v self.random_sprite
            """,
            """
            random_sprite = choose(spr_test0, spr_test1, spr_test2, spr_test3);
            """,
            gameContext
        );
    }

    [Fact]
    public void TestUnionArrayInitAndAsset()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test0", AssetType.Sprite);
        gameContext.DefineMockAsset(1, "spr_test1", AssetType.Sprite);
        gameContext.DefineMockAsset(2, "spr_test2", AssetType.Sprite);
        gameContext.DefineMockAsset(3, "spr_test3", AssetType.Sprite);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();
        IMacroType spriteType = registry.FindType("Asset.Sprite");

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineVariableType("test", new UnionMacroType([new ArrayInitMacroType(spriteType), spriteType]));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 3
            conv.i.v
            pushi.e 2
            conv.i.v
            pushi.e 1
            conv.i.v
            pushi.e 0
            conv.i.v
            call.i @@NewGMLArray@@ 4
            pop.v.v self.test
            pushi.e 1
            pop.v.i self.test
            """,
            """
            test = [spr_test0, spr_test1, spr_test2, spr_test3];
            test = spr_test1;
            """,
            gameContext
        );
    }

    [Fact]
    public void TestConditionalMatchCall()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test", AssetType.Sprite);
        gameContext.DefineMockAsset(0, "obj_test", AssetType.Object);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();
        IMacroType spriteType = registry.FindType("Asset.Sprite");
        IMacroType objectType = registry.FindType("Asset.Object");

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineFunctionArgumentsType("scr_test", new UnionMacroType(
            [
                new FunctionArgsMacroType([new MatchMacroType(null, "String", "set_sprite"), spriteType]),
                new FunctionArgsMacroType([new MatchMacroType(null, "String", "set_object"), objectType])
            ]
        ));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 123
            conv.i.v
            push.s "random_string"
            conv.s.v
            call.i scr_test 2
            popz.v
            pushi.e 0
            conv.i.v
            push.s "set_object"
            conv.s.v
            call.i scr_test 2
            popz.v
            pushi.e 0
            conv.i.v
            push.s "set_sprite"
            conv.s.v
            call.i scr_test 2
            popz.v
            pushi.e 0
            conv.i.v
            push.s "set_number"
            conv.s.v
            call.i scr_test 2
            popz.v
            """,
            """
            scr_test("random_string", 123);
            scr_test("set_object", obj_test);
            scr_test("set_sprite", spr_test);
            scr_test("set_number", 0);
            """,
            gameContext
        );
    }

    [Fact]
    public void TestConditionalMatchUnionCall()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test", AssetType.Sprite);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();
        IMacroType spriteType = registry.FindType("Asset.Sprite");

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineFunctionArgumentsType("scr_test", new UnionMacroType(
            [
                new FunctionArgsMacroType([new UnionMacroType(
                    [
                        new MatchMacroType(null, "String", "set_sprite"),
                        new MatchMacroType(null, "String", "another_sprite")
                    ]
                ), spriteType]),
            ]
        ));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 0
            conv.i.v
            push.s "random_string"
            conv.s.v
            call.i scr_test 2
            popz.v
            pushi.e 0
            conv.i.v
            push.s "set_sprite"
            conv.s.v
            call.i scr_test 2
            popz.v
            pushi.e 0
            conv.i.v
            push.s "another_sprite"
            conv.s.v
            call.i scr_test 2
            popz.v
            """,
            """
            scr_test("random_string", 0);
            scr_test("set_sprite", spr_test);
            scr_test("another_sprite", spr_test);
            """,
            gameContext
        );
    }

    [Fact]
    public void TestConditionalMatchNotIntersectCall()
    {
        GameContextMock gameContext = new()
        {
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(0, "spr_test", AssetType.Sprite);

        MacroTypeRegistry registry = gameContext.MacroTypeRegistry;
        registry.RegisterBasic();
        IMacroType spriteType = registry.FindType("Asset.Sprite");

        NameMacroTypeResolver globalNames = registry.Resolver.GlobalNames;
        globalNames.DefineFunctionArgumentsType("scr_test", new UnionMacroType(
            [
                new FunctionArgsMacroType([new IntersectMacroType(
                    [
                        new MatchNotMacroType(null, "String", "this_one_is_not_a_sprite"),
                        new MatchNotMacroType(null, "String", "this_one_is_neither_a_sprite")
                    ]
                ), spriteType]),
            ]
        ));

        TestUtil.VerifyDecompileResult(
            """
            pushi.e 0
            conv.i.v
            push.s "random_string"
            conv.s.v
            call.i scr_test 2
            popz.v
            pushi.e 0
            conv.i.v
            push.s "this_one_is_not_a_sprite"
            conv.s.v
            call.i scr_test 2
            popz.v
            pushi.e 0
            conv.i.v
            push.s "this_one_is_neither_a_sprite"
            conv.s.v
            call.i scr_test 2
            popz.v
            """,
            """
            scr_test("random_string", spr_test);
            scr_test("this_one_is_not_a_sprite", 0);
            scr_test("this_one_is_neither_a_sprite", 0);
            """,
            gameContext
        );
    }
}
