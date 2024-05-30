namespace Underanalyzer.Decompiler.AST;

/// <summary>
/// Represents an asset reference in the AST.
/// </summary>
public class AssetReferenceNode : IExpressionNode
{
    /// <summary>
    /// The ID of the asset being referenced.
    /// </summary>
    public int AssetId { get; }

    /// <summary>
    /// The type of the asset being referenced.
    /// </summary>
    public AssetType AssetType { get; }

    public bool Duplicated { get; set; } = false;
    public bool Group { get; set; } = false;
    public IGMInstruction.DataType StackType { get; set; } = IGMInstruction.DataType.Variable;

    public string ConditionalTypeName => "AssetReference";
    public string ConditionalValue => $"{AssetType}:{AssetId}";

    public AssetReferenceNode(int assetId, AssetType assetType)
    {
        AssetId = assetId;
        AssetType = assetType;
    }

    public IExpressionNode Clean(ASTCleaner cleaner)
    {
        return this;
    }

    public void Print(ASTPrinter printer)
    {
        string assetName = printer.Context.GameContext.GetAssetName(AssetId, AssetType);
        if (assetName is not null)
        {
            printer.Write(assetName);
        }
        else
        {
            // Unknown asset ID
            if (Group)
            {
                printer.Write('(');
            }
            printer.Write(AssetId);
            if (Group)
            {
                printer.Write(')');
            }
        }
    }
}
