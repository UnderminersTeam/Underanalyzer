using System.Text.Json;

namespace Underanalyzer.Decompiler.GameSpecific.Json;

internal class ArrayInitMacroTypeConverter
{
    public static ArrayInitMacroType ReadContents(ref Utf8JsonReader reader, IMacroTypeConverter macroTypeConverter, JsonSerializerOptions options)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }
        if (reader.GetString() != "Macro")
        {
            throw new JsonException();
        }

        reader.Read();
        ArrayInitMacroType res = new(macroTypeConverter.Read(ref reader, null, options));

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException();
        }

        return res;
    }
}