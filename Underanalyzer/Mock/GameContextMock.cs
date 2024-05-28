using Underanalyzer.Decompiler;

namespace Underanalyzer.Mock;

public class GameContextMock : IGameContext
{
    public bool UsingGMS2OrLater { get; set; } = true;
    public bool UsingGMLv2 { get; set; } = true;
    public bool Bytecode14OrLower { get; set; } = false;
    public bool UsingFinallyBeforeThrow { get; set; } = false;
    public IGlobalFunctions GlobalFunctions { get; } = new GlobalFunctions();
    public string GetAssetName(int assetIndex, AssetType assetType)
    {
        return assetType switch
        {
            AssetType.RoomInstance => $"inst_id_{assetIndex}",
            _ => null
        };
    }
}
