using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.Macros;

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
    ParticleSystem,
    RoomInstance
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
    /// True if the game is using typed booleans in code; false otherwise.
    /// This should be true for GMS 2.3.7 and above.
    /// </summary>
    public bool UsingTypedBooleans { get; }

    /// <summary>
    /// True if the game is using the PushReference instruction to use asset references in code; false otherwise.
    /// This should be true for GameMaker 2023.8 and above.
    /// </summary>
    public bool UsingAssetReferences { get; }

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
    /// This should not be modified during decompilation.
    /// </summary>
    public IGlobalFunctions GlobalFunctions { get; }

    /// <summary>
    /// Macro type registry used for resolving constant macros/enums in decompiled code.
    /// The default constructor for <see cref="MacroTypeRegistry"/> results in an empty registry, which can be populated.
    /// This should not be modified during decompilation.
    /// </summary>
    public MacroTypeRegistry MacroTypeRegistry { get; }

    /// <summary>
    /// Returns the string name of an asset, or null if no such asset exists.
    /// </summary>
    public string GetAssetName(int assetIndex, AssetType assetType);
}
