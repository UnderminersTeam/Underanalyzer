/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler.Bytecode;
using Underanalyzer.Compiler.Lexer;
using Underanalyzer.Compiler.Parser;
using static Underanalyzer.IGMInstruction;

namespace Underanalyzer.Compiler.Nodes;

/// <summary>
/// Represents a constant asset reference (by ID) in the AST.
/// </summary>
internal sealed class AssetReferenceNode : IConstantASTNode
{
    /// <summary>
    /// Asset ID for this node.
    /// </summary>
    public int AssetId { get; init; }

    /// <inheritdoc/>
    public IToken? NearbyToken { get; init; }

    public AssetReferenceNode(TokenAssetReference token)
    {
        AssetId = token.AssetId;
        NearbyToken = token;
    }

    /// <inheritdoc/>
    public IASTNode PostProcess(ParseContext context)
    {
        // If not using typed asset references, just convert to numeric form
        if (!context.CompileContext.GameContext.UsingAssetReferences)
        {
            return new NumberNode(AssetId, NearbyToken);
        }

        return this;
    }

    /// <inheritdoc/>
    public IASTNode Duplicate(ParseContext context)
    {
        return this;
    }

    /// <inheritdoc/>
    public void GenerateCode(BytecodeContext context)
    {
        // Assume that typed asset references are being used by this point
        context.Emit(ExtendedOpcode.PushReference, AssetId);
        context.PushDataType(DataType.Variable);
    }
}
