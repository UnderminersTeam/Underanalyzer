namespace Underanalyzer.Decompiler.AST;

// TODO: need to design this, and *possibly* make it an interface...
public class ASTPrinter
{
    public DecompileContext Context { get; private set; }

    public ASTPrinter(DecompileContext context)
    {
        Context = context;
    }
}
