namespace Underanalyzer;

/// <summary>
/// Contains constant values used by the GameMaker VM.
/// </summary>
public class VMConstants
{
    // Function names used for try..catch..finally statements
    public static string TryHookFunction { get; } = "@@try_hook@@";
    public static string TryUnhookFunction { get; } = "@@try_unhook@@";
    public static string FinishCatchFunction { get; } = "@@finish_catch@@";
    public static string FinishFinallyFunction { get; } = "@@finish_finally@@";
}
