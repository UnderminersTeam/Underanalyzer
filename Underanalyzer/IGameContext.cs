namespace Underanalyzer;

/// <summary>
/// All types of assets used in 
/// </summary>
public enum AssetType
{
    Object = 0,
    Sprite = 1,
    Sound = 2,
    Room = 3,
    Background = 4,
    Path = 5,
    Script = 6,
    Font = 7,
    Timeline = 8,
    Shader = 10,
    Sequence = 11,
    AnimCurve = 12,
    ParticleSystem = 13
}

/// <summary>
/// Interface for managing the data belonging to an individual GameMaker game.
/// </summary>
public interface IGameContext
{
    /// <summary>
    /// True if this game is using GMS2 or above; false otherwise.
    /// </summary>
    public bool UsingGMS2OrLater { get; }

    /// <summary>
    /// True if this game is using GMLv2 features (e.g., GameMaker Studio 2.3 and above); false otherwise.
    /// </summary>
    public bool UsingGMLv2 { get; }

    /// <summary>
    /// True if the game uses bytecode 14 or lower; false otherwise.
    /// </summary>
    public bool Bytecode14OrLower { get; }

    /// <summary>
    /// Returns the string name of an asset, or null if no such asset exists.
    /// </summary>
    public string GetAssetName(int assetIndex, AssetType assetType);
}
