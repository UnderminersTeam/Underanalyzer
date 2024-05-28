using Underanalyzer.Decompiler;

namespace UnderanalyzerTest;

public class DecompileContext_DecompileToString_Settings
{
    [Fact]
    public void TestDataLeftoverOnStack()
    {
        DecompileContext context = TestUtil.VerifyDecompileResult(
            """
            pushi.e 0
            """,
            "",
            null,
            new DecompileSettings()
            {
                AllowLeftoverDataOnStack = true
            }
        );
        Assert.Single(context.Warnings);
        Assert.IsType<DecompileDataLeftoverWarning>(context.Warnings[0]);
        Assert.Equal("root", context.Warnings[0].CodeEntryName);
        Assert.Equal(1, ((DecompileDataLeftoverWarning)context.Warnings[0]).NumberOfElements);
    }
}
