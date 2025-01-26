/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

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
}
