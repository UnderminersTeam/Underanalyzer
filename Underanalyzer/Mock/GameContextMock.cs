namespace Underanalyzer.Mock;

public class GameContextMock : IGameContext
{
    public bool UsingGMLv2 { get; set; } = true;

    public bool Bytecode14OrLower { get; set; } = false;
}
