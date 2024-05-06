using System;
using System.Collections.Generic;
using System.Text;
using Underanalyzer.Decompiler.ControlFlow;

namespace Underanalyzer.Decompiler;

public class DecompileContext
{
    public IGMCode Code { get; private set; }

    // TODO: probably refer to global data here
    internal bool OlderThanBytecode15 { get => false; }

    internal List<Block> Blocks { get; set; }
    internal List<Fragment> FragmentNodes { get; set; }
    internal List<Loop> LoopNodes { get; set; }
    internal List<ShortCircuit> ShortCircuitNodes { get; set; }
    internal List<StaticInit> StaticInitNodes { get; set; }
    internal List<TryCatch> TryCatchNodes { get; set; }
    internal List<Nullish> NullishNodes { get; set; }
    internal List<BinaryBranch> BinaryBranchNodes { get; set; }
    internal HashSet<Block> SwitchEndBlocks { get; set; }
    internal HashSet<Block> SwitchContinueBlocks { get; set; }

    // Constructor used for control flow tests
    internal DecompileContext(IGMCode code) 
    {
        Code = code;
    }
}
