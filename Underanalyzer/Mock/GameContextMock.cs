/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.Macros;

namespace Underanalyzer.Mock;

/// <summary>
/// A default implementation of <see cref="IGameContext"/>.
/// </summary>
public class GameContextMock : IGameContext
{
    /// <inheritdoc/>
    public bool UsingGMS2OrLater { get; set; } = true;
    /// <inheritdoc/>
    public bool UsingGMLv2 { get; set; } = true;
    /// <inheritdoc/>
    public bool Bytecode14OrLower { get; set; } = false;
    /// <inheritdoc/>
    public bool UsingFinallyBeforeThrow { get; set; } = false;
    /// <inheritdoc/>
    public bool UsingTypedBooleans { get; set; } = true;
    /// <inheritdoc/>
    public bool UsingAssetReferences { get; set; } = true;
    /// <inheritdoc/>
    public bool UsingRoomInstanceReferences { get; set; } = true;
    /// <inheritdoc/>
    public IGlobalFunctions GlobalFunctions { get; } = new GlobalFunctions();
    /// <inheritdoc/>
    public MacroTypeRegistry MacroTypeRegistry { get; set; } = new();

    /// <summary>
    /// A Dictionary that mocks asset chunks and their contents.
    /// </summary>
    public Dictionary<AssetType, Dictionary<int, string>> MockAssets { get; set; } = new();

    /// <summary>
    /// Define a new mock asset that gets added to <see cref="MockAssets"/>.
    /// </summary>
    /// <param name="assetType">The type of the asset.</param>
    /// <param name="assetIndex">The index of the asset.</param>
    /// <param name="assetName">The name of the asset.</param>
    public void DefineMockAsset(AssetType assetType, int assetIndex, string assetName)
    {
        Dictionary<int, string> assets;
        if (!MockAssets.TryGetValue(assetType, out assets))
        {
            assets = new();
            MockAssets.Add(assetType, assets);
        }
        assets[assetIndex] = assetName;
    }
    
    /// <summary>
    /// Fetches an asset from <see cref="MockAssets"/>.
    /// </summary>
    /// <param name="assetType">The asset type of the asset that should be fetched.</param>
    /// <param name="assetIndex">The index of the asset that should be fetched.</param>
    /// <returns>The name of the asset, or <see langword="null"/> if it does not exist.</returns>
    public string GetMockAsset(AssetType assetType, int assetIndex)
    {
        if (!MockAssets.TryGetValue(assetType, out var dict)) return null;

        if (dict.TryGetValue(assetIndex, out string name))
        {
            return name;
        }
        return null;
    }
    
    /// <inheritdoc/>
    public string GetAssetName(AssetType assetType, int assetIndex)
    {
        return assetType switch
        {
            AssetType.RoomInstance => assetIndex >= 100000 ? $"inst_id_{assetIndex}" : null,
            _ => GetMockAsset(assetType, assetIndex)
        };
    }
}
