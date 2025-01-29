﻿/*
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
            for (var i = 0; i < 10; i = i + 1)
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
        TestUtil.VerifyRoundTrip(
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
            """
        );
    }

    [Fact]
    public void TestAssignments()
    {
        TestUtil.VerifyRoundTrip(
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
            """
        );
    }

    // TODO: test for @@Global@@ rewriting for things like global.a[0] = 1 becoming @@Global@@().a[0] = 1
}
