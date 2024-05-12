namespace Underanalyzer.Mock;

public class GameContextMock : IGameContext
{
    public bool UsingGMS2OrLater { get; set; } = true;
    public bool UsingGMLv2 { get; set; } = true;
    public bool Bytecode14OrLower { get; set; } = false;
    public string GetAssetName(int assetIndex, AssetType assetType) => null;
}
