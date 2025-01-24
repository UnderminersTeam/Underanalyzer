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
}
