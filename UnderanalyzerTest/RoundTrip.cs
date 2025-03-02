/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class RoundTrip
{
    [Fact]
    public void TestEmpty()
    {
        TestUtil.VerifyRoundTrip("");
    }

    [Fact]
    public void TestSimpleReturns()
    {
        TestUtil.VerifyRoundTrip(
            """
            return 123;
            return true;
            return false;
            return "test string";
            return 123123123123123123;
            """
        );
    }

    [Fact]
    public void TestIf()
    {
        TestUtil.VerifyRoundTrip(
            """
            if (a)
            {
                if (d)
                {
                }
                else
                {
                }
            }
            else if (b)
            {
                if (c)
                {
                }
            }
            """
        );
    }

    [Fact]
    public void TestWhileIfContinueBreak()
    {
        TestUtil.VerifyRoundTrip(
            """
            top = 1;
            while (a)
            {
                if (b)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            bottom = 2;
            """
        );
    }

    [Fact]
    public void TestForIfContinueBreak()
    {
        TestUtil.VerifyRoundTrip(
            """
            top = 1;
            for (var i = 0; i < 10; i++)
            {
                if (a)
                {
                    break;
                }
                continue;
            }
            bottom = 2;
            """
        );
    }

    [Fact]
    public void TestDoUntilContinueBreak()
    {
        TestUtil.VerifyRoundTrip(
            """
            top = 1;
            do
            {
                if (a)
                {
                    break;
                }
                continue;
            }
            until (b);
            bottom = 2;
            """
        );
    }

    [Fact]
    public void TestConditional()
    {
        TestUtil.VerifyRoundTrip(
            """
            basic = a ? b : c;
            basic_cast = a ? 1 : 2;
            nested = (a ? b : c) ? d : (e ? f : g);
            """
        );
    }

    [Fact]
    public void TestBinaryAndUnary()
    {
        TestUtil.VerifyRoundTrip(
            """
            a = b + (c - d);
            a = (b + c) - d;
            a = (b * c) / d;
            a = (b div c) % d;
            a = b + c + d + e;
            a = b - c - d - 5;
            a = b * c * 5 * e;
            a = b && c && d;
            a = b || c || d;
            a = (b && c) || (d && e);
            a = b << (c >> d);
            a = b ^ (c & (d | e));
            a = !b + ~b + -b;
            """
        );
    }

    [Fact]
    public void TestTypesOld()
    {
        GameContextMock mock = new()
        {
            UsingAssetReferences = false,
            UsingTypedBooleans = false
        };
        mock.DefineMockAsset(AssetType.Sprite, 8, "spr_test");

        TestUtil.VerifyRoundTrip(
            """
            a = spr_test;
            b = true;
            """,
            """
            a = 8;
            b = 1;
            """,
            false, mock
        );
    }

    [Fact]
    public void TestTypesNew()
    {
        GameContextMock mock = new()
        {
            UsingAssetReferences = true,
            UsingTypedBooleans = true
        };
        mock.DefineMockAsset(AssetType.Sprite, 8, "spr_test");

        TestUtil.VerifyRoundTrip(
            """
            a = spr_test;
            b = true;
            """,
            false, mock
        );
    }

    [Fact]
    public void TestWithContinueBreakExit()
    {
        GameContextMock mock = new()
        {
            UsingAssetReferences = true,
            UsingTypedBooleans = true
        };
        mock.DefineMockAsset(AssetType.Object, 8, "obj_test");

        TestUtil.VerifyRoundTrip(
            """
            with (-3)
            {
                a = 123;
                return 0;
            }
            with (abc)
            {
                b = 456;
                exit;
            }
            with (obj_test)
            {
                if (c)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            """,
            false, mock
        );
    }

    [Fact]
    public void TestRepeatContinueBreakExit()
    {
        TestUtil.VerifyRoundTrip(
            """
            repeat (123)
            {
                if (a)
                {
                    continue;
                }
                else if (b)
                {
                    exit;
                }
                else
                {
                    break;
                }
            }
            """
        );
    }

    [Fact]
    public void TestSwitchContinueBreakExit()
    {
        string testCode =
            """
            repeat (a)
            {
                switch (b)
                {
                    case 123:
                        c = 0;
                        break;
                    case 456:
                        d = 0;
                        break;
                        return 123;
                    case 789:
                        continue;
                    default:
                        exit;
                }
            }
            """;
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingExtraRepeatInstruction = true
        });
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingExtraRepeatInstruction = false
        });
    }

    [Fact]
    public void TestAssignments()
    {
        string testCode =
            """
            a = 0;
            a.b = 0;
            a.b.c = 0;
            a[0] = 1;
            a.b[0] = 1;
            a.b.c[0] = 1;
            a[0].b[1] = 2;
            a[0].b[1].c = 2;
            global.a = 0;
            global.a.b = 0;
            global.a.b.c = 0;
            global.a[0] = 1;
            global.a.b[0] = 1;
            global.a.b.c[0] = 1;
            global.a[0].b[1] = 2;
            global.a[0].b[1].c = 2;
            """;
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = true
        });
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = false
        });
    }

    // TODO: test for @@Global@@ rewriting for things like global.a[0] = 1 becoming @@Global@@().a[0] = 1

    [Fact]
    public void TestAssignments2dArray()
    {
        TestUtil.VerifyRoundTrip(
            """
            a[0, 1] = 2;
            a.b[0, 1] = 2;
            a.b.c[0, 1] = 2;
            a[0, 1].b[0, 1] = 2;
            a[0, 1].b[0, 1].c = 2;
            global.a[0, 1] = 2;
            global.a.b[0, 1] = 2;
            global.a.b.c[0, 1] = 2;
            global.a[0, 1].b[0, 1] = 2;
            global.a[0, 1].b[0, 1].c = 2;
            """,
            false, new()
            {
                UsingGMLv2 = false
            }
        );
    }

    [Fact]
    public void TestCompoundAssignments()
    {
        string testCode =
            """
            a += 1;
            a.b += 1;
            a.b.c += 1;
            a[0] += 1;
            a.b[0] += 1;
            a.b.c[0] += 1;
            a[0].b[1] += 2;
            a[0].b[1].c += 2;
            global.a += 1;
            global.a.b += 1;
            global.a.b.c += 1;
            global.a[0] += 1;
            global.a.b[0] += 1;
            global.a.b.c[0] += 1;
            global.a[0].b[1] += 2;
            global.a[0].b[1].c += 2;
            a.b = a.b + 1;
            a.b.c = a.b.c + 1;
            a[0] = a[0] + 1;
            a.b[0] = a.b[0] + 1;
            a.b.c[0] = a.b.c[0] + 1;
            a[0].b[1] = a[0].b[1] + 2;
            a[0].b[1].c = a[0].b[1].c + 2;
            global.a = global.a + 1;
            global.a.b = global.a.b + 1;
            global.a.b.c = global.a.b.c + 1;
            global.a[0] = global.a[0] + 1;
            global.a.b[0] = global.a.b[0] + 1;
            global.a.b.c[0] = global.a.b.c[0] + 1;
            global.a[0].b[1] = global.a[0].b[1] + 2;
            global.a[0].b[1].c = global.a[0].b[1].c + 2;
            """;
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = true
        });
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = false
        });
    }

    [Fact]
    public void TestCompoundAssignments2dArray()
    {
        TestUtil.VerifyRoundTrip(
            """
            a[0, 1] += 2;
            a.b[0, 1] += 2;
            a.b.c[0, 1] += 2;
            a[0, 1].b[0, 1] += 2;
            a[0, 1].b[0, 1].c += 2;
            global.a[0, 1] += 2;
            global.a.b[0, 1] += 2;
            global.a.b.c[0, 1] += 2;
            global.a[0, 1].b[0, 1] += 2;
            global.a[0, 1].b[0, 1].c += 2;
            a[0, 1] = a[0, 1] + 2;
            a.b[0, 1] = a.b[0, 1] + 2;
            a.b.c[0, 1] = a.b.c[0, 1] + 2;
            a[0, 1].b[0, 1] = a[0, 1].b[0, 1] + 2;
            a[0, 1].b[0, 1].c = a[0, 1].b[0, 1].c + 2;
            global.a[0, 1] = global.a[0, 1] + 2;
            global.a.b[0, 1] = global.a.b[0, 1] + 2;
            global.a.b.c[0, 1] = global.a.b.c[0, 1] + 2;
            global.a[0, 1].b[0, 1] = global.a[0, 1].b[0, 1] + 2;
            global.a[0, 1].b[0, 1].c = global.a[0, 1].b[0, 1].c + 2;
            """,
            false, new()
            {
                UsingGMLv2 = false
            }
        );
    }

    [Fact]
    public void TestPrefixPostfix()
    {
        string testCode =
            """
            a++;
            a.b++;
            a.b.c++;
            a[0]++;
            a.b[0]++;
            a.b.c[0]++;
            a[0].b[1]++;
            a[0].b[1].c++;
            global.a++;
            global.a.b++;
            global.a.b.c++;
            global.a[0]++;
            global.a.b[0]++;
            global.a.b.c[0]++;
            global.a[0].b[1]++;
            global.a[0].b[1].c++;
            """;
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = true
        });
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = false
        });
    }

    [Fact]
    public void TestPrefixExpr()
    {
        string testCode =
            """
            d = ++a;
            d = ++a.b;
            d = ++a.b.c;
            d = ++a[0];
            d = ++a.b[0];
            d = ++a.b.c[0];
            d = ++a[0].b[1];
            d = ++a[0].b[1].c;
            d = ++global.a;
            d = ++global.a.b;
            d = ++global.a.b.c;
            d = ++global.a[0];
            d = ++global.a.b[0];
            d = ++global.a.b.c[0];
            d = ++global.a[0].b[1];
            d = ++global.a[0].b[1].c;
            """;
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = true
        });
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = false
        });
    }

    [Fact]
    public void TestPostfixExpr()
    {
        string testCode =
            """
            d = a++;
            d = a.b++;
            d = a.b.c++;
            d = a[0]++;
            d = a.b[0]++;
            d = a.b.c[0]++;
            d = a[0].b[1]++;
            d = a[0].b[1].c++;
            d = global.a++;
            d = global.a.b++;
            d = global.a.b.c++;
            d = global.a[0]++;
            d = global.a.b[0]++;
            d = global.a.b.c[0]++;
            d = global.a[0].b[1]++;
            d = global.a[0].b[1].c++;
            """;
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = true
        });
        TestUtil.VerifyRoundTrip(testCode, false, new()
        {
            UsingGMLv2 = false
        });
    }

    [Fact]
    public void TestPrefixExpr2dArray()
    {
        TestUtil.VerifyRoundTrip(
            """
            d = ++a[0, 1];
            d = ++a.b[0, 1];
            d = ++a.b.c[0, 1];
            d = ++a[0, 1].b[0, 1];
            d = ++a[0, 1].b[0, 1].c;
            d = ++global.a[0, 1];
            d = ++global.a.b[0, 1];
            d = ++global.a.b.c[0, 1];
            d = ++global.a[0, 1].b[0, 1];
            d = ++global.a[0, 1].b[0, 1].c;
            """,
            false, new()
            {
                UsingGMLv2 = false
            }
        );
    }

    [Fact]
    public void TestPostfixExpr2dArray()
    {
        TestUtil.VerifyRoundTrip(
            """
            d = a[0, 1]++;
            d = a.b[0, 1]++;
            d = a.b.c[0, 1]++;
            d = a[0, 1].b[0, 1]++;
            d = a[0, 1].b[0, 1].c++;
            d = global.a[0, 1]++;
            d = global.a.b[0, 1]++;
            d = global.a.b.c[0, 1]++;
            d = global.a[0, 1].b[0, 1]++;
            d = global.a[0, 1].b[0, 1].c++;
            """,
            false, new()
            {
                UsingGMLv2 = false
            }
        );
    }

    [Fact]
    public void TestMultiArrays()
    {
        TestUtil.VerifyRoundTrip(
            """
            a[0] = 1;
            a[0][1] = 2;
            a[0][1][2] = 3;
            a[0][1][2][3] = 4;
            a.b[0][1][2] = 3;
            a.b[0][1].c[2][3] = 4;
            a.b[0][1].c[2][3].d = 4;
            global.a[0] = 1;
            global.a[0][1] = 2;
            global.a[0][1][2] = 3;
            global.a[0][1][2][3] = 4;
            global.a.b[0][1][2] = 3;
            global.a.b[0][1].c[2][3] = 4;
            global.a.b[0][1].c[2][3].d = 4;
            e = a[0];
            e = a[0][1];
            e = a[0][1][2];
            e = a[0][1][2][3];
            e = a.b[0][1][2];
            e = a.b[0][1].c[2][3];
            e = a.b[0][1].c[2][3].d;
            """
        );
    }

    [Fact]
    public void TestMultiArraysPrePost()
    {
        TestUtil.VerifyRoundTrip(
            """
            a[0]++;
            a[0][1]++;
            a[0][1][2]++;
            a[0][1][2][3]++;
            a.b[0][1][2]++;
            a.b[0][1].c[2][3]++;
            a.b[0][1].c[2][3].d++;
            global.a[0]++;
            global.a[0][1]++;
            global.a[0][1][2]++;
            global.a[0][1][2][3]++;
            global.a.b[0][1][2]++;
            global.a.b[0][1].c[2][3]++;
            global.a.b[0][1].c[2][3].d++;
            e = a[0]++;
            e = a[0][1]++;
            e = a[0][1][2]++;
            e = a[0][1][2][3]++;
            e = a.b[0][1][2]++;
            e = a.b[0][1].c[2][3]++;
            e = a.b[0][1].c[2][3].d++;
            e = ++a[0];
            e = ++a[0][1];
            e = ++a[0][1][2];
            e = ++a[0][1][2][3];
            e = ++a.b[0][1][2];
            e = ++a.b[0][1].c[2][3];
            e = ++a.b[0][1].c[2][3].d;
            """
        );
    }

    [Fact]
    public void TestMultiArraysCompound()
    {
        TestUtil.VerifyRoundTrip(
            """
            a[0] += 1;
            a[0][1] += 2;
            a[0][1][2] += 3;
            a[0][1][2][3] += 4;
            a.b[0][1][2] += 3;
            a.b[0][1].c[2][3] += 4;
            a.b[0][1].c[2][3].d += 4;
            global.a[0] += 1;
            global.a[0][1] += 2;
            global.a[0][1][2] += 3;
            global.a[0][1][2][3] += 4;
            global.a.b[0][1][2] += 3;
            global.a.b[0][1].c[2][3] += 4;
            global.a.b[0][1].c[2][3].d += 4;
            """
        );
    }

    [Fact]
    public void TestVariableCalls()
    {
        GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false
        };
        ((BuiltinsMock)gameContext.Builtins).BuiltinFunctions["show_debug_message"] = 
            new("show_debug_message", 1, 1);
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.VerifyRoundTrip(
            """
            var d;
            show_debug_message("a");
            test("a");
            self.test("a");
            other.test("a");
            global.test("a");
            a.b("a");
            a.b.c("a");
            a[0].b("a");
            d("a");
            a.d("a");
            d.a("a");
            obj_test.a("a");
            obj_test.a.b("a");
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestVariableCallsAssetRefs()
    {
        GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = true,
            UsingSelfToBuiltin = true
        };
        ((BuiltinsMock)gameContext.Builtins).BuiltinFunctions["show_debug_message"] =
            new("show_debug_message", 1, 1);
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.VerifyRoundTrip(
            """
            var d;
            show_debug_message("a");
            test("a");
            self.test("a");
            other.test("a");
            global.test("a");
            a.b("a");
            a.b.c("a");
            a[0].b("a");
            d("a");
            a.d("a");
            d.a("a");
            obj_test.a("a");
            obj_test.a.b("a");
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestRepeatVariableCalls()
    {
        GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false,
            UsingSelfToBuiltin = true
        };
        ((BuiltinsMock)gameContext.Builtins).BuiltinFunctions["show_debug_message"] =
            new("show_debug_message", 1, 1);
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.VerifyRoundTrip(
            """
            a(1)(2)(3)(4)(5);
            a.b(1)(2)(3);
            a(1).b(2);
            self.a(1).b(2);
            a(1).b(2)(3)(4);
            a(1).b(2)(3).c(4);
            obj_test.b(1)(2)(3);
            obj_test.b.c(1)(2)(3);
            obj_test.a(1).b(2);
            obj_test.a(1).b(2)(3)(4);
            obj_test.a(1).b(2)(3).c(4);
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestRepeatVariableCallsAssetRefs()
    {
        GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = true,
            UsingSelfToBuiltin = true
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.VerifyRoundTrip(
            """
            a(1)(2)(3)(4)(5);
            a.b(1)(2)(3);
            a(1).b(2);
            self.a(1).b(2);
            a(1).b(2)(3)(4);
            a(1).b(2)(3).c(4);
            obj_test.b(1)(2)(3);
            obj_test.b.c(1)(2)(3);
            obj_test.a(1).b(2);
            obj_test.a(1).b(2)(3)(4);
            obj_test.a(1).b(2)(3).c(4);
            """,
            false,
            gameContext
        );
    }

    [Fact]
    public void TestVariableCallsArrays()
    {
        GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.VerifyRoundTrip(
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
            false,
            gameContext
        );
    }

    [Fact]
    public void TestVariableCallsArraysMulti()
    {
        GameContextMock gameContext = new()
        {
            UsingGMLv2 = true,
            UsingAssetReferences = false
        };
        gameContext.DefineMockAsset(AssetType.Object, 123, "obj_test");
        TestUtil.VerifyRoundTrip(
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
            false,
            gameContext
        );
    }

    [Fact]
    public void TestFunctionDecls()
    {
        TestUtil.VerifyRoundTrip(
            """
            abc();
            
            function abc()
            {
            }

            anon = function()
            {
            };

            struct = 
            {
                a: 123,
                
                b: function()
                {
                },
                
                c: [1, 2, 3, 4, 5],
                d: 
                {
                    e: 3.14
                }
            };

            function Parent(arg0) constructor
            {
                a = arg0;
            }

            function Child(arg0, arg1 = 789) : Parent(arg0) constructor
            {
                static testFunc = function()
                {
                };
                
                b = arg1;
            }
            """
        );
    }

    [Fact]
    public void TestNewObject()
    {
        TestUtil.VerifyRoundTrip(
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
            function Test() constructor
            {
            }
            
            new Test();
            new Test();
            new Test();
            a = new Test();
            b = new VariableCall();
            b = new self.VariableCall();
            b = new global.VariableCall();
            new Complex.Variable.Call(123, 456);
            """
        );
    }
    [Fact]
    public void TestNonSelfToBuiltin()
    {
        TestUtil.VerifyRoundTrip(
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
            a = 0;
            a = 0;
            a = b;
            a = b;
            a += 1;
            a += 1;
            a++;
            a++;
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
            """
        );
    }

    [Fact]
    public void TestSelfToBuiltin()
    {
        GameContextMock gameContext = new()
        {
            UsingSelfToBuiltin = true,
            UsingGlobalConstantFunction = true
        };
        TestUtil.VerifyRoundTrip(
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
            false,
            gameContext
        );
    }

    [Fact]
    public void TestThrow()
    {
        TestUtil.VerifyRoundTrip(
            """
            throw "test";
            throw new ExampleException();
            """
        );
    }

    [Fact]
    public void TestNullishCoalesce()
    {
        TestUtil.VerifyRoundTrip(
            """
            a = b ?? c;
            d = (e ?? f) ?? (g ?? h);
            j ??= k;
            l ??= m ?? n;
            """
        );
    }

    [Fact]
    public void TestBooleanLoops()
    {
        TestUtil.VerifyRoundTrip(
            """
            while (1)
            {
            }
            while (true)
            {
            }
            do
            {
            }
            until (1);
            do
            {
            }
            until (true);
            """
        );
    }

    [Fact]
    public void TestTryCatchBasic()
    {
        TestUtil.VerifyRoundTrip(
            """
            try
            {
                throw "Error!";
            }
            catch (ex)
            {
                a = ex;
            }
            """
        );
    }

    [Fact]
    public void TestTryCatchFinally1()
    {
        TestUtil.VerifyRoundTrip(
            """
            try
            {
                throw "Error!";
            }
            catch (ex)
            {
                a = ex;
            }
            finally
            {
                b = 123;
            }
            """
        );
    }

    [Fact]
    public void TestTryCatchFinally2()
    {
        TestUtil.VerifyRoundTrip(
            """
            try
            {
                throw "Error!";
                return 456;
            }
            catch (ex)
            {
                a = ex;
            }
            finally
            {
                b = 123;
            }
            """
        );
    }

    [Fact]
    public void TestTryFinally()
    {
        TestUtil.VerifyRoundTrip(
            """
            try
            {
                return 123;
            }
            finally
            {
                a = "finally code";
            }
            """
        );
    }

    [Fact]
    public void TestTryBreakContinue()
    {
        TestUtil.VerifyRoundTrip(
            """
            repeat (123)
            {
                try
                {
                    if (c)
                    {
                        continue;
                    }
                    if (d)
                    {
                        break;
                    }
                }
                catch (ex)
                {
                }
            }
            """
        );
    }

    [Fact]
    public void TestTryCatchBreakContinue()
    {
        TestUtil.VerifyRoundTrip(
            """
            repeat (123)
            {
                try
                {
                    continue;
                    break;
                }
                catch (ex)
                {
                    continue;
                    break;
                }
            }
            """
        );
    }

    [Fact]
    public void TestTrySwitchBreak()
    {
        TestUtil.VerifyRoundTrip(
            """
            try
            {
                switch (a)
                {
                    default:
                        break;
                }
            }
            catch (ex)
            {
            }
            """
        );
    }

    [Fact]
    public void TestArguments()
    {
        TestUtil.VerifyRoundTrip(
            """
            function args(arg0)
            {
                test = arg0;
                test2 = arg0[0];
                test3 = argument0;
                test4 = argument0[0];
            }
            """
        );
    }

    [Fact]
    public void TestStruct()
    {
        GameContextMock gameContext = new()
        {
            UsingSelfToBuiltin = true,
            UsingNewFunctionVariables = true
        };
        TestUtil.VerifyRoundTrip(
            """
            a = 
            {
                b: c + d
            };
            """,
            false,
            gameContext
        );
    }
}
