/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Xml.Linq;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a new object node in the AST.
/// </summary>
internal sealed class NewObjectNode : IMaybeStatementASTNode
{
    /// <summary>
    /// Expression being instantiated.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Arguments being used in constructor call.
    /// </summary>
    public List<IASTNode> Arguments { get; private set; }

    /// <inheritdoc/>
    public bool IsStatement { get; set; } = false;

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private NewObjectNode(TokenKeyword nearbyToken, IASTNode expression, List<IASTNode> arguments)
    {
        Expression = expression;
        Arguments = arguments;
        NearbyToken = nearbyToken;
    }

    /// <summary>
    /// Creates a new object node, parsing from the given context's current position.
    /// </summary>
    public static NewObjectNode? Parse(ParseContext context)
    {
        // Parse "new" keyword
        if (context.EnsureToken(KeywordKind.New) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse function/variable/value being instantiated
        IASTNode expression;
        if (!context.EndOfCode && context.Tokens[context.Position] is TokenFunction tokenFunction)
        {
            // Convert function to simple variable node
            expression = new SimpleVariableNode(tokenFunction.Text, null);
            context.Position++;
        }
        else
        {
            // Parse general chain expression
            if (Expressions.ParseChainExpression(context, true) is IASTNode chainExpression)
            {
                expression = chainExpression;
            }
            else
            {
                return null;
            }
        }

        // Parse arguments being used in constructor call
        if (Functions.ParseCallArguments(context, 65534 /* TODO: is this limit correct? */) is not List<IASTNode> arguments)
        {
            return null;
        }

        // Create final expression node
        return new NewObjectNode(tokenKeyword, expression, arguments);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Expression = Expression.PostProcess(context);
        for (int i = 0; i < Arguments.Count; i++)
        {
            Arguments[i] = Arguments[i].PostProcess(context);
        }
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        throw new System.NotImplementedException();
    }
}
