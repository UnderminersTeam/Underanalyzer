/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a "try" statement in the AST.
/// </summary>
internal sealed class TryCatchNode : IASTNode
{
    /// <summary>
    /// Statement/block to be executed under the "try" part of the statement.
    /// </summary>
    public IASTNode Try { get; private set; }

    /// <summary>
    /// Statement/block to be executed under the "catch" part of the statement, if one exists.
    /// </summary>
    public IASTNode? Catch { get; private set; }

    /// <summary>
    /// Local variable name used for the value caught by the "catch" part of the statement.
    /// </summary>
    public string? CatchVariableName { get; private set; }

    /// <summary>
    /// Statement/block to be executed under the "finally" part of the statement, if one exists.
    /// </summary>
    public IASTNode? Finally { get; private set; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; }

    private TryCatchNode(IToken nearbyToken, IASTNode @try, IASTNode? @catch, string? catchVariableName, IASTNode? @finally)
    {
        NearbyToken = nearbyToken;
        Try = @try;
        Catch = @catch;
        CatchVariableName = catchVariableName;
        Finally = @finally;
    }

    /// <summary>
    /// Creates a try statement node, parsing from the given context's current position.
    /// </summary>
    public static IASTNode? Parse(ParseContext context)
    {
        // Parse "try" keyword
        if (context.EnsureToken(KeywordKind.Try) is not TokenKeyword tokenKeyword)
        {
            return null;
        }

        // Parse "try" part of the statement
        if (Statements.ParseStatement(context) is not IASTNode @try)
        {
            return null;
        }

        // Parse "catch" part of the statement, if it exists
        IASTNode? @catch = null;
        string? catchVariableName = null;
        if (context.IsCurrentToken(KeywordKind.Catch))
        {
            context.Position++;

            // Parse the catch variable name
            context.EnsureToken(SeparatorKind.GroupOpen);
            if (!context.EndOfCode && context.Tokens[context.Position] is TokenVariable tokenVariable)
            {
                context.Position++;
                catchVariableName = tokenVariable.Text;

                // Add to this scope's local list
                // TODO: check for duplicates and conflicts with named arguments/statics?
                context.CurrentScope.DeclareLocal(tokenVariable.Text);
            }
            context.EnsureToken(SeparatorKind.GroupClose);

            // Parse the actual statement/body
            @catch = Statements.ParseStatement(context);
        }

        // Parse "finally" part of the statement, if it exists
        IASTNode? @finally = null;
        if (context.IsCurrentToken(KeywordKind.Finally))
        {
            context.Position++;
            @finally = Statements.ParseStatement(context);
        }

        // Create final statement
        if (@catch is null && @finally is null)
        {
            // Apparently it's valid to just have "try," which effectively does nothing
            return @try;
        }
        return new TryCatchNode(tokenKeyword, @try, @catch, catchVariableName, @finally);
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        Try = Try.PostProcess(context);
        Catch = Catch?.PostProcess(context);
        Finally = Finally?.PostProcess(context);

        // TODO: rewriting here, possibly?

        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        throw new System.NotImplementedException();
    }
}
