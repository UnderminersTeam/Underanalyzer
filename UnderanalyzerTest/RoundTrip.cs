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
}
