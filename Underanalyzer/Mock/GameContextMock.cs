using System.Collections.Generic;
using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Mock;

public class GameContextMock : IGameContext
{
    public bool UsingGMS2OrLater { get; set; } = true;
    public bool UsingGMLv2 { get; set; } = true;
    public bool Bytecode14OrLower { get; set; } = false;
    public bool UsingFinallyBeforeThrow { get; set; } = false;
    public bool UsingTypedBooleans { get; set; } = true;
    public bool UsingAssetReferences { get; set; } = true;
    public bool UsingRoomInstanceReferences { get; set; } = true;
    public IGlobalFunctions GlobalFunctions { get; } = new GlobalFunctions();
    public MacroTypeRegistry MacroTypeRegistry { get; set; } = new();

    public Dictionary<AssetType, Dictionary<int, string>> MockAssets { get; set; } = new();

    public void DefineMockAsset(int assetIndex, string assetName, AssetType assetType)
    {
        Dictionary<int, string> assets;
        if (!MockAssets.TryGetValue(assetType, out assets))
        {
            assets = new();
            MockAssets.Add(assetType, assets);
        }
        assets[assetIndex] = assetName;
    }
    public string GetMockAsset(int assetIndex, AssetType assetType)
    {
        if (MockAssets.TryGetValue(assetType, out var dict))
        {
            if (dict.TryGetValue(assetIndex, out string name))
            {
                return name;
            }
        }
        return null;
    }

    public string GetAssetName(int assetIndex, AssetType assetType)
    {
        return assetType switch
        {
            AssetType.RoomInstance => assetIndex >= 100000 ? $"inst_id_{assetIndex}" : null,
            _ => GetMockAsset(assetIndex, assetType)
        };
    }
}
