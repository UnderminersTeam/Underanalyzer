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

    [Fact]
    public void TestCalculator()
    {
        ParseContext context = TestUtil.ParseAndPostProcess(
            """
            a = 2 + 2;
            b = 1 + 2 + 3;
            c = 1 + 2 + 3 + 4;
            d = (5 * 4) + (6 / 2) + (5 % 4.5);
            e = (true && 1) || false;
            f = (((123 << 32) | 456) >> 32) & 0xFFFFFFFF;
            g = -(5);
            h = !true;
            i = ~0x0FF0 & 0xFFFF;
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) => Assert.Equal(4, ((NumberNode)((AssignNode)node).Expression).Value),
            (node) => Assert.Equal(6, ((NumberNode)((AssignNode)node).Expression).Value),
            (node) => Assert.Equal(10, ((NumberNode)((AssignNode)node).Expression).Value),
            (node) => Assert.Equal((5.0 * 4.0) + (6.0 / 2.0) + (5.0 % 4.5), ((NumberNode)((AssignNode)node).Expression).Value),
            (node) => Assert.Equal(1, ((NumberNode)((AssignNode)node).Expression).Value),
            (node) => Assert.Equal(123, ((Int64Node)((AssignNode)node).Expression).Value),
            (node) => Assert.Equal(-5, ((NumberNode)((AssignNode)node).Expression).Value),
            (node) => Assert.False(((BooleanNode)((AssignNode)node).Expression).Value),
            (node) => Assert.Equal(0xF00F, ((NumberNode)((AssignNode)node).Expression).Value)
        );
    }

    [Fact]
    public void TestCalculatorEnums()
    {
        ParseContext context = TestUtil.ParseAndPostProcess(
            """
            enum A
            {
                a = 123,
                b = A.a + 10,
                c = B.a,
                d
            }
            enum B
            {
                a = A.b + 10
            }
            
            test1 = A.a;
            test2 = A.b;
            test3 = A.c;
            test4 = A.d;
            test5 = B.a;
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) => Assert.IsType<EmptyNode>(node),
            (node) => Assert.IsType<EmptyNode>(node),
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test1", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(123, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test2", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(133, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test3", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(143, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test4", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(144, ((Int64Node)assign.Expression!).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal("test5", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(143, ((Int64Node)assign.Expression!).Value);
            }
        );
    }
}
