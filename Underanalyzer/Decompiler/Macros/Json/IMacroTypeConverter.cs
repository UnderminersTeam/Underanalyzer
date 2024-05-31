using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Underanalyzer.Decompiler.Macros.Json;

internal class IMacroTypeConverter : JsonConverter<IMacroType>
{
    public MacroTypeRegistry Registry { get; }

    public IMacroTypeConverter(MacroTypeRegistry registry)
    {
        Registry = registry;
    }

    public override IMacroType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            // Valid token type is just nothing
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            // Read type name - access registry
            return Registry.FindType(reader.GetString());
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Read array of macro types as function arguments macro type
            List<IMacroType> subMacroTypes = new();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return new FunctionArgsMacroType(subMacroTypes);
                }

                subMacroTypes.Add(Read(ref reader, typeToConvert, options));
            }

            throw new JsonException();
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Read macro type! Ensure we start with type discriminator
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();
            if (propertyName != "MacroType")
            {
                throw new JsonException();
            }

            // Read data for relevant type
            reader.Read();
            switch (reader.GetString())
            {
                case "Enum":
                    return EnumMacroTypeConverter.ReadContents(ref reader);
                case "Constants":
                    return ConstantsMacroTypeConverter.ReadContents(ref reader);
                case "Union":
                    return UnionMacroTypeConverter.ReadContents(ref reader, this, options);
                case "Intersect":
                    return IntersectMacroTypeConverter.ReadContents(ref reader, this, options);
                case "ArrayInit":
                    return ArrayInitMacroTypeConverter.ReadContents(ref reader, this, options);
                case "Match":
                    return MatchMacroTypeConverter.ReadContents(ref reader, this, options);
                case "MatchNot":
                    return MatchNotMacroTypeConverter.ReadContents(ref reader, this, options);
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, IMacroType value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
