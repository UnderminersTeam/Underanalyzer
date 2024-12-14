/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents an assignment statement in the AST.
/// </summary>
internal sealed class AssignNode : IASTNode
{
    /// <summary>
    /// Expression being assigned to.
    /// </summary>
    public IAssignableASTNode Destination { get; private set; }

    /// <summary>
    /// The expression being evaluted and assigned to the destination.
    /// </summary>
    public IASTNode Expression { get; private set; }

    /// <summary>
    /// Kind of assignment being performed.
    /// </summary>
    public AssignKind Kind { get; }

    /// <inheritdoc/>
    public IToken? NearbyToken => Destination.NearbyToken;

    public enum AssignKind
    {
        Normal,
        CompoundPlus,
        CompoundMinus,
        CompoundTimes,
        CompoundDivide,
        CompoundMod,
        CompoundBitwiseAnd,
        CompoundBitwiseOr,
        CompoundBitwiseXor,
        CompoundNullishCoalesce
    }

    /// <summary>
    /// Creates an assignment node from the given destination and expression.
    /// </summary>
    public AssignNode(AssignKind kind, IAssignableASTNode destination, IASTNode expression)
    {
        Kind = kind;
        Destination = destination;
        Expression = expression;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        // TODO
        Destination = Destination.PostProcess(context) as IAssignableASTNode ?? throw new Exception("Destination no longer assignable");
        Expression = Expression.PostProcess(context);
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // TODO
    }
}
