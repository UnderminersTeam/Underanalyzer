namespace Underanalyzer;

/// <summary>
/// Interface for managing the data belonging to an individual GameMaker game.
/// </summary>
public interface IGameContext
{
    /// <summary>
    /// True if this game is using GMLv2 features (e.g., GameMaker Studio 2.3 and above); false otherwise.
    /// </summary>
    public bool UsingGMLv2 { get; }

    /// <summary>
    /// True if the game uses bytecode 14 or lower; false otherwise.
    /// </summary>
    public bool Bytecode14OrLower { get; }

    // TODO: ways of handling global asset, script, and macro information
}
