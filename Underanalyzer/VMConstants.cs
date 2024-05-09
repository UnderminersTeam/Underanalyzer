namespace Underanalyzer;

/// <summary>
/// Contains constant values used by the GameMaker VM.
/// </summary>
internal static class VMConstants
{
    // Function names used for try..catch..finally statements
    public const string TryHookFunction = "@@try_hook@@";
    public const string TryUnhookFunction = "@@try_unhook@@";
    public const string FinishCatchFunction = "@@finish_catch@@";
    public const string FinishFinallyFunction = "@@finish_finally@@";

    // Function name for creating methods/structs
    public const string MethodFunction = "method";
    public const string NullObjectFunction = "@@NullObject@@";
    public const string NewObjectFunction = "@@NewGMLObject@@";
}
