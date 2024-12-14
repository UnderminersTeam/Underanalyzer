/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer;
using Underanalyzer.Compiler.Nodes;
using Underanalyzer.Compiler.Parser;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class ParseContext_Parse
{
    [Fact]
    public void TestSimpleFunctionCalls()
    {
        ParseContext context = TestUtil.Parse(
            """
            show_message("Hello, world!");
            a(b(), c(456, 789), true, 0.5, , false);
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) =>
            {
                SimpleFunctionCallNode funcCall = (SimpleFunctionCallNode)node;
                Assert.Equal("show_message", funcCall.FunctionName);
                Assert.True(funcCall.IsStatement);
                Assert.Single(funcCall.Arguments);
                Assert.Equal("Hello, world!", ((StringNode)funcCall.Arguments[0]).Value);
            },
            (node) =>
            {
                SimpleFunctionCallNode funcCall = (SimpleFunctionCallNode)node;
                Assert.Equal("a", funcCall.FunctionName);
                Assert.True(funcCall.IsStatement);
                Assert.Collection(funcCall.Arguments,
                    (node) =>
                    {
                        SimpleFunctionCallNode funcCall = (SimpleFunctionCallNode)node;
                        Assert.Equal("b", funcCall.FunctionName);
                        Assert.False(funcCall.IsStatement);
                        Assert.Empty(funcCall.Arguments);
                    },
                    (node) =>
                    {
                        SimpleFunctionCallNode funcCall = (SimpleFunctionCallNode)node;
                        Assert.Equal("c", funcCall.FunctionName);
                        Assert.False(funcCall.IsStatement);
                        Assert.Collection(funcCall.Arguments,
                            (node) => Assert.Equal(456, ((NumberNode)node).Value),
                            (node) => Assert.Equal(789, ((NumberNode)node).Value)
                        );
                    },
                    (node) => Assert.True(((BooleanNode)node).Value),
                    (node) => Assert.Equal(0.5, ((NumberNode)node).Value),
                    (node) => Assert.Equal("undefined", ((SimpleVariableNode)node).VariableName),
                    (node) => Assert.False(((BooleanNode)node).Value)
                );
            }
        );
    }

    [Fact]
    public void TestIf()
    {
        ParseContext context = TestUtil.Parse(
            """
            if (1)
            {
            }
            else
            {
            }

            if (2) call();

            if 3 then begin end else {}
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) =>
            {
                IfNode ifNode = (IfNode)node;
                Assert.Equal(1, ((NumberNode)ifNode.Condition!).Value);
                Assert.Empty(((BlockNode)ifNode.TrueStatement!).Children);
                Assert.Empty(((BlockNode)ifNode.FalseStatement!).Children);
            },
            (node) =>
            {
                IfNode ifNode = (IfNode)node;
                Assert.Equal(2, ((NumberNode)ifNode.Condition!).Value);
                Assert.Empty(((SimpleFunctionCallNode)ifNode.TrueStatement!).Arguments);
                Assert.Null(ifNode.FalseStatement);
            },
            (node) =>
            {
                IfNode ifNode = (IfNode)node;
                Assert.Equal(3, ((NumberNode)ifNode.Condition!).Value);
                Assert.Empty(((BlockNode)ifNode.TrueStatement!).Children);
                Assert.Empty(((BlockNode)ifNode.FalseStatement!).Children);
            }
        );
    }

    [Fact]
    public void TestAssignsAndFunctions()
    {
        ParseContext context = TestUtil.Parse(
            """
            a = 123;
            ++b;
            if (++c) {}
            d();
            if (e()) {}
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("a", ((SimpleVariableNode)assign.Destination!).VariableName);
                Assert.Equal(123, ((NumberNode)assign.Expression!).Value);
            },
            (node) =>
            {
                PrefixNode prefix = (PrefixNode)node;
                Assert.Equal("b", ((SimpleVariableNode)prefix.Expression!).VariableName);
                Assert.True(prefix.IsIncrement);
                Assert.True(prefix.IsStatement);
            },
            (node) =>
            {
                IfNode ifNode = (IfNode)node;
                PrefixNode prefix = (PrefixNode)ifNode.Condition!;
                Assert.Equal("c", ((SimpleVariableNode)prefix.Expression!).VariableName);
                Assert.True(prefix.IsIncrement);
                Assert.False(prefix.IsStatement);
            },
            (node) =>
            {
                SimpleFunctionCallNode funcCall = (SimpleFunctionCallNode)node;
                Assert.Equal("d", funcCall.FunctionName);
                Assert.True(funcCall.IsStatement);
                Assert.Empty(funcCall.Arguments);
            },
            (node) =>
            {
                IfNode ifNode = (IfNode)node;
                SimpleFunctionCallNode funcCall = (SimpleFunctionCallNode)ifNode.Condition!;
                Assert.Equal("e", funcCall.FunctionName);
                Assert.False(funcCall.IsStatement);
                Assert.Empty(funcCall.Arguments);
            }
        );
    }

    [Fact]
    public void TestAssignTypes()
    {
        // TODO: postfix here as well
        ParseContext context = TestUtil.Parse(
            """
            a = 1;
            a := 1;
            a += 1;
            a -= 1;
            a *= 1;
            a /= 1;
            a %= 1;
            a &= 1;
            a |= 1;
            a ^= 1;
            a ??= 1;
            ++a;
            --a;
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) => Assert.Equal(AssignNode.AssignKind.Normal, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.Normal, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundPlus, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundMinus, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundTimes, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundDivide, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundMod, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundBitwiseAnd, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundBitwiseOr, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundBitwiseXor, ((AssignNode)node).Kind),
            (node) => Assert.Equal(AssignNode.AssignKind.CompoundNullishCoalesce, ((AssignNode)node).Kind),
            (node) => Assert.True(((PrefixNode)node).IsIncrement),
            (node) => Assert.False(((PrefixNode)node).IsIncrement)
        );
    }

    [Fact]
    public void TestUnary()
    {
        ParseContext context = TestUtil.Parse(
            """
            a = !b;
            a = ~b;
            a = +b;
            a = -b;
            a = not b;
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("a", ((SimpleVariableNode)assign.Destination!).VariableName);
                UnaryNode unary = (UnaryNode)assign.Expression!;
                Assert.Equal(UnaryNode.UnaryKind.BooleanNot, unary.Kind);
                Assert.Equal("b", ((SimpleVariableNode)unary.Expression!).VariableName);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("a", ((SimpleVariableNode)assign.Destination!).VariableName);
                UnaryNode unary = (UnaryNode)assign.Expression!;
                Assert.Equal(UnaryNode.UnaryKind.BitwiseNegate, unary.Kind);
                Assert.Equal("b", ((SimpleVariableNode)unary.Expression!).VariableName);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("a", ((SimpleVariableNode)assign.Destination!).VariableName);
                UnaryNode unary = (UnaryNode)assign.Expression!;
                Assert.Equal(UnaryNode.UnaryKind.Positive, unary.Kind);
                Assert.Equal("b", ((SimpleVariableNode)unary.Expression!).VariableName);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("a", ((SimpleVariableNode)assign.Destination!).VariableName);
                UnaryNode unary = (UnaryNode)assign.Expression!;
                Assert.Equal(UnaryNode.UnaryKind.Negative, unary.Kind);
                Assert.Equal("b", ((SimpleVariableNode)unary.Expression!).VariableName);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("a", ((SimpleVariableNode)assign.Destination!).VariableName);
                UnaryNode unary = (UnaryNode)assign.Expression!;
                Assert.Equal(UnaryNode.UnaryKind.BooleanNot, unary.Kind);
                Assert.Equal("b", ((SimpleVariableNode)unary.Expression!).VariableName);
            }
        );
    }

    [Fact]
    public void TestArrayLiteral()
    {
        ParseContext context = TestUtil.Parse(
            """
            a = [];
            b = [1];
            c = [1, 2, 3];
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("a", ((SimpleVariableNode)assign.Destination!).VariableName);
                SimpleFunctionCallNode func = (SimpleFunctionCallNode)assign.Expression!;
                Assert.Equal(VMConstants.NewArrayFunction, func.FunctionName);
                Assert.Empty(func.Arguments);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("b", ((SimpleVariableNode)assign.Destination!).VariableName);
                SimpleFunctionCallNode func = (SimpleFunctionCallNode)assign.Expression!;
                Assert.Equal(VMConstants.NewArrayFunction, func.FunctionName);
                Assert.Single(func.Arguments);
                Assert.Equal(1, ((NumberNode)func.Arguments[0]).Value);
            },
            (node) =>
            {
                AssignNode assign = (AssignNode)node;
                Assert.Equal(AssignNode.AssignKind.Normal, assign.Kind);
                Assert.Equal("c", ((SimpleVariableNode)assign.Destination!).VariableName);
                SimpleFunctionCallNode func = (SimpleFunctionCallNode)assign.Expression!;
                Assert.Equal(VMConstants.NewArrayFunction, func.FunctionName);
                Assert.Equal(3, func.Arguments.Count);
                Assert.Equal(1, ((NumberNode)func.Arguments[0]).Value);
                Assert.Equal(2, ((NumberNode)func.Arguments[1]).Value);
                Assert.Equal(3, ((NumberNode)func.Arguments[2]).Value);
            }
        );
    }

    [Fact]
    public void TestExitReturnBreakContinue()
    {
        ParseContext context = TestUtil.Parse(
            """
            exit;
            return;
            return 123;
            break;
            continue;
            """
        );

        Assert.Empty(context.CompileContext.Errors);
        Assert.Collection(((BlockNode)context.Root!).Children,
            (node) => Assert.IsType<ExitNode>(node),
            (node) => Assert.IsType<ExitNode>(node),
            (node) =>
            {
                Assert.IsType<ReturnNode>(node);
                ReturnNode ret = (ReturnNode)node;
                Assert.Equal(123, ((NumberNode)ret.ReturnValue).Value);
            },
            (node) => Assert.IsType<BreakNode>(node),
            (node) => Assert.IsType<ContinueNode>(node)
        );
    }

    [Fact]
    public void TestErrorFreeFloating()
    {
        ParseContext context = TestUtil.Parse(
            """
            1
            """
        );
        Assert.Equal(2, context.CompileContext.Errors.Count);
    }

    [Fact]
    public void TestErrorEndOfCode1()
    {
        ParseContext context = TestUtil.Parse(
            """
            a =
            """
        );
        Assert.Equal(2, context.CompileContext.Errors.Count);
    }

    [Fact]
    public void TestErrorEndOfCode2()
    {
        ParseContext context = TestUtil.Parse(
            """
            {
            """
        );
        Assert.Single(context.CompileContext.Errors);
    }

    [Fact]
    public void TestErrorInvalidExpression()
    {
        ParseContext context = TestUtil.Parse(
            """
            a = ;
            """
        );
        Assert.Equal(2, context.CompileContext.Errors.Count);
    }

    [Fact]
    public void TestErrorUnclosedCall()
    {
        ParseContext context = TestUtil.Parse(
            """
            a(1, 2
            """
        );
        Assert.Single(context.CompileContext.Errors);
    }
}