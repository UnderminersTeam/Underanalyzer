using System.Text.Json;

namespace Underanalyzer.Decompiler.Macros.Json;

internal class MatchMacroTypeConverter
{
    public static MatchMacroType ReadContents(ref Utf8JsonReader reader, IMacroTypeConverter macroTypeConverter, JsonSerializerOptions options)
    {
        IMacroType innerType = null;
        string conditionalValue = null;
        string conditionalType = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (conditionalValue is null && conditionalType is null)
                {
                    throw new JsonException();
                }
                return new MatchMacroType(innerType, conditionalType, conditionalValue);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            
            // Read either value or type
            switch (reader.GetString())
            {
                case "ConditionalValue":
                    reader.Read();
                    if (conditionalValue is not null)
                    {
                        throw new JsonException();
                    }
                    conditionalValue = reader.GetString();
                    break;
                case "ConditionalType":
                    reader.Read();
                    if (conditionalType is not null)
                    {
                        throw new JsonException();
                    }
                    conditionalType = reader.GetString();
                    break;
                case "InnerMacro":
                    reader.Read();
                    if (innerType is not null)
                    {
                        throw new JsonException();
                    }
                    innerType = macroTypeConverter.Read(ref reader, null, options);
                    break;
                default:
                    throw new JsonException();
            }
        }

        throw new JsonException();
    }
}