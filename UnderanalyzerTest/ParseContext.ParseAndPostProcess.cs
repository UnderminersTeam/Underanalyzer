/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Nodes;
using Underanalyzer.Compiler.Parser;

namespace UnderanalyzerTest;

public class ParseContext_ParseAndPostProcess
{
    [Fact]
    public void TestEnums()
    {
        ParseContext context = TestUtil.ParseAndPostProcess(
            """
            test1 = A.a;
            test2 = A.b;
            test3 = A.c;
            test4 = B.a;
            test5 = A.e;

            enum A
            {
                a,
                b = 2,
                c,
                d,
                e = B.a
            }
            enum B
            {
                a = A.d
            }
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test1", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(0, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test2", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(2, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test3", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(3, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test4", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(4, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test5", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(4, ((Int64Node)assign.Expression!).Value);
            },
            (node) => Assert.IsType<EmptyNode>(node),
            (node) => Assert.IsType<EmptyNode>(node)
        );
    }
}
