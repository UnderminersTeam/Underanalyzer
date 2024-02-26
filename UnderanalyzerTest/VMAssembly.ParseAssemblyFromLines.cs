using Underanalyzer;
using Underanalyzer.Mock;

namespace UnderanalyzerTest;

public class VMAssembly_ParseInstructions
{
    [Fact]
    public void TestBasic()
    {
        string text =
        """
        :[0]

        # Conv as well as data types
        conv.i.v
        conv.v.i
        conv.b.v
        conv.s.v
        conv.l.v
        conv.d.i
        conv.i.d

        # Misc. instructions
        mul.i.i
        div.i.i
        rem.i.i
        mod.i.i
        add.i.i
        sub.i.i
        and.b.b
        or.b.b
        xor.b.b
        neg.b
        not.b
        shl.i.i
        shr.i.i

        # Comparison
        cmp.i.i lt
        cmp.i.i leq
        cmp.i.i eq
        cmp.i.i neq
        cmp.i.i geq
        cmp.i.i gt

        # Pop instruction
        pop.i.v self.a
        pop.i.v local.a
        pop.v.v [array]self.a
        pop.v.v [stacktop]self.a
        pop.v.v [stacktop]self.a
        pop.v.v [instance]self.a

        # Duplication
        dup.i 0
        dup.l 0
        dup.i 1

        # Duplication (swap mode)
        dup.i 4 5
        
        # Return/exit
        ret.v
        ret.i
        exit.i

        # Discard pop
        popz.v
        popz.i

        # Branching
        b [0]
        bt [end]
        bf [0]
        pushenv [0]
        popenv [0]
        popenv <drop>

        # Push instructions
        push.d 3.1415926535
        push.i 123456
        push.l 5000000000
        push.b true
        push.v self.a
        push.v local.a
        push.v [array]self.a
        push.v [stacktop]self.a
        push.v [instance]self.a
        push.s "Test string!"
        push.s "\"Test\nescaped\nstring!\""
        push.e 123
        pushi.e 123

        # Call instruction
        call.i test_function 5
        
        # Extended instructions
        chkindex.e
        pushaf.e
        popaf.e
        pushac.e
        setowner.e
        isstaticok.e
        setstatic.e
        savearef.e
        restorearef.e
        isnullish.e
        pushref.i 1234
        
        :[end]
        """;
        string[] lines = text.Split('\n');

        GMCode code = VMAssembly.ParseAssemblyFromLines(lines);
        List<GMInstruction> list = code.Instructions;

        Assert.True(list.Count == 72);

        Assert.True(list[0].Kind == IGMInstruction.Opcode.Convert);
        Assert.True(list[0].Type1 == IGMInstruction.DataType.Int32);
        Assert.True(list[0].Type2 == IGMInstruction.DataType.Variable);

        Assert.True(list[1].Kind == IGMInstruction.Opcode.Convert);
        Assert.True(list[1].Type1 == IGMInstruction.DataType.Variable);
        Assert.True(list[1].Type2 == IGMInstruction.DataType.Int32);

        Assert.True(list[26].Kind == IGMInstruction.Opcode.Pop);
        Assert.True(list[26].Type1 == IGMInstruction.DataType.Int32);
        Assert.True(list[26].Type2 == IGMInstruction.DataType.Variable);
        Assert.True(list[26].InstType == IGMInstruction.InstanceType.Self);
        Assert.True(list[26].ReferenceVarType == IGMInstruction.VariableType.Normal);
        Assert.True(list[26].Variable.Name.Content == "a");

        Assert.True(list[29].Kind == IGMInstruction.Opcode.Pop);
        Assert.True(list[29].Type1 == IGMInstruction.DataType.Variable);
        Assert.True(list[29].Type2 == IGMInstruction.DataType.Variable);
        Assert.True(list[29].InstType == IGMInstruction.InstanceType.Self);
        Assert.True(list[29].ReferenceVarType == IGMInstruction.VariableType.StackTop);
        Assert.True(list[29].Variable.Name.Content == "a");

        Assert.True(list[41].Kind == IGMInstruction.Opcode.Branch);
        Assert.True(list[41].BranchOffset == -list[41].Address);

        Assert.True(list[42].Kind == IGMInstruction.Opcode.BranchTrue);
        Assert.True(list[42].BranchOffset == 8 + (list[^1].Address - list[42].Address));

        Assert.True(list[56].Kind == IGMInstruction.Opcode.Push);
        Assert.True(list[56].Type1 == IGMInstruction.DataType.String);
        Assert.True(list[56].ValueString.Content == "Test string!");

        Assert.True(list[57].Kind == IGMInstruction.Opcode.Push);
        Assert.True(list[57].Type1 == IGMInstruction.DataType.String);
        Assert.True(list[57].ValueString.Content == "\"Test\nescaped\nstring!\"");
    }

    [Fact]
    public void TestSubEntries()
    {
        string text =
        """
        > test_root (locals=5)
        pushi.e 0

        > test_sub_entry_1 (locals=20, args=5)
        :[0]
        pushi.e 1

        :[1]
        pushi.e 2

        > test_sub_entry_2 (args=15, locals=10)
        :[2]
        pushi.e 3

        """;
        string[] lines = text.Split('\n');

        GMCode code = VMAssembly.ParseAssemblyFromLines(lines, "test_root");
        List<GMInstruction> list = code.Instructions;

        Assert.True(code.Name.Content == "test_root");
        Assert.True(code.Children.Count == 2);
        Assert.True(code.ArgumentCount == 1);
        Assert.True(code.LocalCount == 5);
        Assert.True(code.Children[0].Name.Content == "test_sub_entry_1");
        Assert.True(code.Children[0].Parent == code);
        Assert.True(code.Children[0].StartOffset == list[1].Address);
        Assert.True(code.Children[0].ArgumentCount == 5);
        Assert.True(code.Children[0].LocalCount == 20);
        Assert.True(code.Children[1].Name.Content == "test_sub_entry_2");
        Assert.True(code.Children[1].Parent == code);
        Assert.True(code.Children[1].StartOffset == list[3].Address);
        Assert.True(code.Children[1].ArgumentCount == 15);
        Assert.True(code.Children[1].LocalCount == 10);
    }
}