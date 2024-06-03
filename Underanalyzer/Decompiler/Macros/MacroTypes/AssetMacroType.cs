using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Macro type for GameMaker asset references.
/// </summary>
public class AssetMacroType : IMacroTypeInt32
{
    public AssetType Type { get; }

    public AssetMacroType(AssetType type)
    {
        Type = type;
    }

    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, int data)
    {
        // Ensure we don't resolve this on newer GameMaker versions where this is unnecessary
        if (cleaner.Context.GameContext.UsingAssetReferences)
        {
            if (cleaner.Context.GameContext.UsingRoomInstanceReferences || Type != AssetType.RoomInstance)
            {
                return null;
            }
        }

        // Check for asset name with the given type
        string assetName = cleaner.Context.GameContext.GetAssetName(Type, data);
        if (assetName is not null)
        {
            return new MacroValueNode(assetName);
        }

        return null;
    }
}
