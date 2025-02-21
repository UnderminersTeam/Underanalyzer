/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using Underanalyzer.Compiler;
using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.GameSpecific;

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
    /// <see langword="true"/> if this game is using GMS2 or above; <see langword="false"/> otherwise.
    /// </summary>
    public bool UsingGMS2OrLater { get; }

    /// <summary>
    /// <see langword="true"/> if this game is using GMLv2 features (e.g., GameMaker Studio 2.3 and above); <see langword="false"/> otherwise.
    /// </summary>
    public bool UsingGMLv2 { get; }

    /// <summary>
    /// <see langword="true"/> if the game is using typed booleans in code; <see langword="false"/> otherwise.
    /// This should be <see langword="true"/> for GMS 2.3.7 and above.
    /// </summary>
    public bool UsingTypedBooleans { get; }

    /// <summary>
    /// <see langword="true"/> if the game is using the <see cref="IGMInstruction.ExtendedOpcode.PushReference"/> instruction to use asset references in code; <see langword="false"/> otherwise.
    /// This should be <see langword="true"/> for GameMaker 2023.8 and above.
    /// </summary>
    public bool UsingAssetReferences { get; }

    /// <summary>
    /// <see langword="true"/> if the game is using the <see cref="IGMInstruction.ExtendedOpcode.PushReference"/> instruction to reference room instances in code; <see langword="false"/> otherwise.
    /// This should be <see langword="true"/> for GameMaker 2024.2 and above.
    /// </summary>
    public bool UsingRoomInstanceReferences { get; }

    /// <summary>
    /// <see langword="true"/> if the game uses bytecode 14 or lower; <see langword="true"/> otherwise.
    /// </summary>
    public bool Bytecode14OrLower { get; }

    /// <summary>
    /// <see langword="true"/> if this game short-circuits logical AND and OR operations in GML; <see langword="false"/> otherwise.
    /// </summary>
    public bool UsingLogicalShortCircuit { get; }

    /// <summary>
    /// <see langword="true"/> if this game uses the older repeat loop code generation (as of GM 2022.11); <see langword="false"/> otherwise.
    /// </summary>
    public bool UsingExtraRepeatInstruction { get; }

    /// <summary>
    /// <see langword="true"/> if this game uses the old behavior of "throw" statements with finally blocks; <see langword="false"/> otherwise.
    /// As of writing, this is <see langword="true"/> before GameMaker version 2024.6, and <see langword="false"/> otherwise.
    /// </summary>
    public bool UsingFinallyBeforeThrow { get; }

    /// <summary>
    /// <see langword="true"/> if this game uses the new code generation for constructors, as introduced in GameMaker version 2024.11; <see langword="false"/> otherwise.
    /// </summary>
    public bool UsingConstructorSetStatic { get; }

    /// <summary>
    /// <see langword="true"/> if this game allows static code blocks to recursively re-enter themselves; <see langword="false"/> otherwise.
    /// </summary>
    /// <remarks>
    /// Before GameMaker 2024.11, this is guaranteed to be <see langword="true"/>. Afterwards, it is <see langword="false"/> by default, but can be changed by a game's developer.
    /// </remarks>
    public bool UsingReentrantStatic { get; }

    /// <summary>
    /// <see langword="true"/> if this game assigns function declarations using new code generation introduced in GameMaker 2024.2; <see langword="false"/> otherwise.
    /// </summary>
    public bool UsingNewFunctionVariables { get; }

    /// <summary>
    /// <see langword="true"/> if "self" should become "builtin" during code generation for simple variables (not on the right side of a dot); <see langword="false"/> otherwise.
    /// </summary>
    /// <remarks>
    /// Before GameMaker 2024.2, this is observed to be <see langword="false"/>. Afterwards, it is <see langword="true"/>.
    /// </remarks>
    public bool UsingSelfToBuiltin { get; set; }

    /// <summary>
    /// <see langword="true"/> if the "global" constant should become a function call during code generation; <see langword="false"/> otherwise.
    /// </summary>
    /// <remarks>
    /// Before GameMaker 2023.11, this is observed to be <see langword="false"/>. Afterwards, it is <see langword="true"/>.
    /// </remarks>
    public bool UsingGlobalConstantFunction { get; set; }

    /// <summary>
    /// <see langword="true"/> if the compiler is aware of functions before they are declared in the same object code event entry (this is 
    /// always true for global scripts & room creation code); <see langword="false"/> otherwise.
    /// </summary>
    /// <remarks>
    /// Before GameMaker 2024.11, this is observed to be <see langword="false"/>. Afterwards, it is <see langword="true"/>.
    /// </remarks>
    public bool UsingObjectFunctionForesight { get; set; }

    /// <summary>
    /// Interface for getting global functions.
    /// Can be custom, or can use the provided implementation of <see cref="Decompiler.GlobalFunctions"/>.
    /// This should not be modified during decompilation.
    /// </summary>
    public IGlobalFunctions GlobalFunctions { get; }

    /// <summary>
    /// Game-specific data registry used for resolving constant macros/enums, as well as other game-specific data, in decompiled code.
    /// The default constructor for <see cref="GameSpecificRegistry"/> results in an empty registry, which can be populated.
    /// This should not be modified during decompilation.
    /// </summary>
    public GameSpecificRegistry GameSpecificRegistry { get; }

    /// <summary>
    /// Interface representing an instance of builtin function/variable/etc. information to use for compilation.
    /// </summary>
    public IBuiltins Builtins { get; }

    /// <summary>
    /// Interface representing an implementation for building code entries and emitting instructions, during compilation.
    /// </summary>
    public ICodeBuilder CodeBuilder { get; }

    /// <summary>
    /// Returns the string name of an asset, or <see langword="null"/> if no such asset exists.
    /// </summary>
    public string? GetAssetName(AssetType assetType, int assetIndex);

    /// <summary>
    /// Returns the ID of an asset, if one exists. If <see cref="UsingAssetReferences"/> is <see langword="true"/>, 
    /// this ID encodes the correct asset type as well.
    /// </summary>
    /// <param name="assetName">Asset name to look up</param>
    /// <param name="assetId">Outputs the asset ID, or is undefined if this method returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if an asset ID was found; <see langword="false"/> otherwise.</returns>
    public bool GetAssetId(string assetName, out int assetId);

    /// <summary>
    /// Returns the asset ID of a script, if one exists. If <see cref="UsingAssetReferences"/> is <see langword="true"/>, 
    /// this ID encodes the correct script asset type as well.
    /// </summary>
    /// <param name="scriptName">Script name to look up</param>
    /// <param name="assetId">Outputs the asset ID, or is undefined if this method returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if an asset ID for the script name was found; <see langword="false"/> otherwise.</returns>
    public bool GetScriptId(string scriptName, out int assetId);

    /// <summary>
    /// Returns the asset ID of a script from its global function name, if one exists. If <see cref="UsingAssetReferences"/> is <see langword="true"/>, 
    /// this ID encodes the correct script asset type as well.
    /// </summary>
    /// <param name="functionName">Global function name to look up</param>
    /// <param name="assetId">Outputs the asset ID, or is undefined if this method returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if an asset ID for the script name was found; <see langword="false"/> otherwise.</returns>
    public bool GetScriptIdByFunctionName(string functionName, out int assetId);
}
