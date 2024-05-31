using System.Text.Json;

namespace Underanalyzer.Decompiler.Macros.Json;

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
        return new ArrayInitMacroType(macroTypeConverter.Read(ref reader, null, options));
    }
}