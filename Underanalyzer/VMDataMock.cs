using System;
using System.Collections.Generic;

namespace Underanalyzer.Mock;

public class GMCode : IGMCode<GMInstruction>
{
    public List<GMInstruction> Instructions { get; private set; }

    IList<GMInstruction> IGMCode<GMInstruction>.Instructions => Instructions;
}

public class GMInstruction : IGMInstruction
{
    public uint Address { get; set; }

    public IGMInstruction.Opcode Kind { get; set; }

    public IGMInstruction.ComparisonType ComparisonKind { get; set; }

    public IGMInstruction.DataType Type1 { get; set; }

    public IGMInstruction.DataType Type2 { get; set; }

    public IGMInstruction.InstanceType TypeInst { get; set; }

    public IGMVariable Variable { get; set; }

    public IGMFunction Function { get; set; }

    public IGMInstruction.VariableType ReferenceVarType { get; set; }

    public double ValueDouble { get; set; }

    public int ValueInt { get; set; }

    public long ValueLong { get; set; }

    public bool ValueBool { get; set; }

    public int BranchOffset { get; set; }

    public bool PopWithContextExit { get; set; }

    public byte DuplicationSize { get; set; }

    public byte DuplicationSize2 { get; set; }
}

public class GMString : IGMString
{
    public string Content { get; set; }
}

public class GMVariable : IGMVariable
{
    public IGMString Name { get; set; }

    public IGMInstruction.InstanceType VariableType { get; set; }

    public int VariableID { get; set; }
}

public class GMFunction : IGMFunction
{
    public IGMString Name { get; set; }
}