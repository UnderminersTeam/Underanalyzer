using Underanalyzer.Decompiler.AST;

namespace Underanalyzer.Decompiler.Macros;

/// <summary>
/// Macro type for instance type references.
/// </summary>
public class InstanceMacroType : IMacroTypeInt32
{
    public IExpressionNode Resolve(ASTCleaner cleaner, IMacroResolvableNode node, int data)
    {
        return data switch
        {
            (int)IGMInstruction.InstanceType.Self => new InstanceTypeNode(IGMInstruction.InstanceType.Self),
            (int)IGMInstruction.InstanceType.Other => new InstanceTypeNode(IGMInstruction.InstanceType.Other),
            (int)IGMInstruction.InstanceType.All => new InstanceTypeNode(IGMInstruction.InstanceType.All),
            (int)IGMInstruction.InstanceType.Noone => new InstanceTypeNode(IGMInstruction.InstanceType.Noone),
            (int)IGMInstruction.InstanceType.Global => new InstanceTypeNode(IGMInstruction.InstanceType.Global),
            _ => null
        };
    }
}
