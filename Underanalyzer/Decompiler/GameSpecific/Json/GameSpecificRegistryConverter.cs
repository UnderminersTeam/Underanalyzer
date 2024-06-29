using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Underanalyzer.Decompiler.GameSpecific.Json;

internal class GameSpecificRegistryConverter : JsonConverter<GameSpecificRegistry>
{
    public GameSpecificRegistry Registry { get; }

    public GameSpecificRegistryConverter(GameSpecificRegistry existing)
    {
        Registry = existing;
    }

    public override GameSpecificRegistry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return Registry;
            }

            // Read property name
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();

            // Depending on property name, deserialize that component
            switch (propertyName)
            {
                case "Types":
                    reader.Read();
                    ReadTypes(ref reader, options);
                    break;
                case "GlobalNames":
                    reader.Read();
                    NameMacroTypeResolverConverter.ReadContents(ref reader, options, Registry.MacroResolver.GlobalNames);
                    break;
                case "CodeEntryNames":
                    reader.Read();
                    ReadCodeEntryNames(ref reader, options);
                    break;
                case "NamedArguments":
                    reader.Read();
                    NamedArgumentResolverConverter.ReadContents(ref reader, options, Registry.NamedArgumentResolver);
                    break;
                default:
                    throw new JsonException($"Unknown field {propertyName}");
            }
        }

        throw new JsonException();
    }

    private void ReadTypes(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return;
            }

            // Read property name
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();

            // Depending on property name, deserialize that component
            switch (propertyName)
            {
                case "RegisterBasic":
                    if (reader.Read() && reader.GetBoolean())
                    {
                        Registry.RegisterBasic();
                    }
                    break;
                case "Enums":
                    reader.Read();
                    ReadMacroTypeList<EnumMacroType>(ref reader, options);
                    break;
                case "Constants":
                    reader.Read();
                    ReadMacroTypeList<ConstantsMacroType>(ref reader, options);
                    break;
                case "Other":
                case "Custom":
                case "General":
                    reader.Read();
                    ReadGeneralTypes(ref reader, options);
                    break;
                default:
                    throw new JsonException($"Unknown field {propertyName}");
            }
        }

        throw new JsonException();
    }

    private void ReadMacroTypeList<T>(ref Utf8JsonReader reader, JsonSerializerOptions options) where T : IMacroType
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        JsonConverter<T> converter = (JsonConverter<T>)options.GetConverter(typeof(T));

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return;
            }

            // Read property name
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();
            if (propertyName is null)
            {
                throw new JsonException();
            }

            // Deserialize macro type
            reader.Read();
            T macroType = converter.Read(ref reader, typeof(T), options);

            // Register macro type under name
            Registry.RegisterType(propertyName, macroType);
        }

        throw new JsonException();
    }

    private void ReadGeneralTypes(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        JsonConverter<IMacroType> converter = (JsonConverter<IMacroType>)options.GetConverter(typeof(IMacroType));

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return;
            }

            // Read property name
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();
            if (propertyName is null)
            {
                throw new JsonException();
            }

            // Deserialize macro type
            reader.Read();
            IMacroType macroType = converter.Read(ref reader, typeof(IMacroType), options);

            // Register macro type under name
            Registry.RegisterType(propertyName, macroType);
        }

        throw new JsonException();
    }

    private void ReadCodeEntryNames(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();
            if (propertyName is null)
            {
                throw new JsonException();
            }

            // Read contents and register under code entry name
            reader.Read();
            NameMacroTypeResolver newResolver = new();
            NameMacroTypeResolverConverter.ReadContents(ref reader, options, newResolver);
            Registry.MacroResolver.DefineCodeEntry(propertyName, newResolver);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, GameSpecificRegistry value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
