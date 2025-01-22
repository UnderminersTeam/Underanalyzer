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
}