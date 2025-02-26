/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer;
using Underanalyzer.Compiler;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class CompileContext_Compile
{
    [Fact]
    public void TestLocalsList()
    {
        CompileContext context = new(
            """
            var first = 1;
            
            function Test()
            {
                var second = 2;
            }
            
            function Test2()
            {
                // Duplicate, but shouldn't result in a new list entry
                var second = 3;

                // Temp return variable should end up here in terms of order, 
                // even though it's generated at the very end
                repeat (4)
                {
                    return 5;
                }
            }

            var third = 6;
            """, 
            CompileScriptKind.GlobalScript, 
            "Example", 
            new GameContextMock()
        );
        context.Compile();

        Assert.Empty(context.Errors);
        Assert.Equal(["first", "second", VMConstants.TempReturnVariable, "third"], context.OutputLocalsOrder);
    }
}
