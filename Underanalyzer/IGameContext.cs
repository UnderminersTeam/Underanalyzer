using Underanalyzer.Decompiler;

namespace Underanalyzer;

/// <summary>
/// All types of assets used in GML code. Exact value must be adapted depending on GameMaker version.
/// </summary>
public enum AssetType
{
    Object,
    Sprite,
    Sound,
    Room,
    Background,
    Path,
    Script,
    Font,
    Timeline,
    Shader,
    Sequence,
    AnimCurve,
    ParticleSystem
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
    /// True if this game uses the old behavior of "throw" statements with finally blocks; false otherwise.
    /// As of writing, this is true before GameMaker version 2024.6, and false otherwise.
    /// </summary>
    public bool UsingFinallyBeforeThrow { get; }

    /// <summary>
    /// Interface for getting global functions.
    /// Can be custom, or can use the provided implementation of <see cref="Decompiler.GlobalFunctions"/>.
    /// </summary>
    public IGlobalFunctions GlobalFunctions { get; }

    /// <summary>
    /// Returns the string name of an asset, or null if no such asset exists.
    /// </summary>
    public string GetAssetName(int assetIndex, AssetType assetType);
}
