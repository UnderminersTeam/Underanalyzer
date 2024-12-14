/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a simple function call in the AST.
/// </summary>
internal sealed class SimpleFunctionCallNode : IMaybeStatementASTNode
{
    /// <summary>
    /// Function name (or variable name) being called.
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    /// Builtin function corresponding to the function name, or null if none.
    /// </summary>
    public IBuiltinFunction? BuiltinFunction { get; }

    /// <summary>
    /// Arguments being used for this function call, in order.
    /// </summary>
    public List<IASTNode> Arguments { get; }

    /// <inheritdoc/>
    public bool IsStatement { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    /// <summary>
    /// Creates a simple function call node, parsing from the given context's current position,
    /// and given the provided function token.
    /// </summary>
    public SimpleFunctionCallNode(ParseContext context, TokenFunction token)
    {
        NearbyToken = token;
        FunctionName = token.Text;
        BuiltinFunction = token.BuiltinFunction;
        Arguments = Functions.ParseCallArguments(context, 65535 /* TODO: change based on gamemaker version? */);
    }

    /// <summary>
    /// Creates an expression function call directly, without any parsing.
    /// </summary>
    private SimpleFunctionCallNode(string functionName, IBuiltinFunction? builtinFunction, List<IASTNode> arguments)
    {
        FunctionName = functionName;
        BuiltinFunction = builtinFunction;
        Arguments = arguments;
    }

    /// <summary>
    /// Parses an array literal from the given context's current position, and returns
    /// a corresponding function call node to create that array.
    /// </summary>
    public static SimpleFunctionCallNode ParseArrayLiteral(ParseContext context)
    {
        List<IASTNode> arguments = new(16);
        SimpleFunctionCallNode result = new(VMConstants.NewArrayFunction, 
                                            context.CompileContext.GameContext.Builtins.LookupBuiltinFunction(VMConstants.NewArrayFunction), 
                                            arguments);

        while (!context.EndOfCode && !context.IsCurrentToken(SeparatorKind.ArrayClose))
        {
            // Parse current expression in array
            if (Expressions.ParseExpression(context) is IASTNode expr)
            {
                arguments.Add(expr);
            }
            else
            {
                // Failed to parse expression; stop parsing array literal
                break;
            }

            // If at end of code, stop here
            if (context.EndOfCode)
            {
                break;
            }

            // We expect either a comma (separating the expressions), or an array close
            if (context.IsCurrentToken(SeparatorKind.Comma))
            {
                context.Position++;
                continue;
            }

            // Should be an array close at this point
            if (!context.IsCurrentToken(SeparatorKind.ArrayClose))
            {
                // Failed to find group end, so give error and stop parsing
                IToken currentToken = context.Tokens[context.Position];
                context.CompileContext.PushError(
                    $"Expected '{TokenSeparator.KindToString(SeparatorKind.Comma)}' or " +
                    $"'{TokenSeparator.KindToString(SeparatorKind.ArrayClose)}', " +
                    $"got {currentToken}", currentToken);
                break;
            }
        }
        context.EnsureToken(SeparatorKind.ArrayClose);

        return result;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        // TODO
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
