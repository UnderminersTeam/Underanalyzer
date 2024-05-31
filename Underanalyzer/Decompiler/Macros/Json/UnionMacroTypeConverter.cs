using System.Collections.Generic;
using System.Text.Json;

namespace Underanalyzer.Decompiler.Macros.Json;

internal class UnionMacroTypeConverter
{
    public static UnionMacroType ReadContents(ref Utf8JsonReader reader, IMacroTypeConverter macroTypeConverter, JsonSerializerOptions options)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }
        if (reader.GetString() != "Macros")
        {
            throw new JsonException();
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        List<IMacroType> types = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return new UnionMacroType(types);
            }

            types.Add(macroTypeConverter.Read(ref reader, null, options));
        }

        throw new JsonException();
    }
}