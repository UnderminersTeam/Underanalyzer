using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Underanalyzer.Decompiler.Macros.Json;

internal class NameMacroTypeResolverConverter : JsonConverter<NameMacroTypeResolver>
{
    public NameMacroTypeResolver Resolver { get; }

    public NameMacroTypeResolverConverter(NameMacroTypeResolver existing)
    {
        Resolver = existing;
    }

    public override NameMacroTypeResolver Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return Resolver;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();

            switch (propertyName)
            {
                case "Variables":
                    reader.Read();
                    ReadMacroNameList(ref reader, options, Resolver.DefineVariableType);
                    break;
                case "FunctionArguments":
                    reader.Read();
                    ReadMacroNameList(ref reader, options, Resolver.DefineFunctionArgumentsType);
                    break;
                case "FunctionReturn":
                    reader.Read();
                    ReadMacroNameList(ref reader, options, Resolver.DefineFunctionReturnType);
                    break;
                default:
                    throw new JsonException($"Unknown property name {propertyName}");
            }
        }

        throw new JsonException();
    }

    private void ReadMacroNameList(ref Utf8JsonReader reader, JsonSerializerOptions options, Action<string, IMacroType> define)
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

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            string propertyName = reader.GetString();

            // Read and define macro type
            reader.Read();
            define(propertyName, converter.Read(ref reader, typeof(IMacroType), options));
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, NameMacroTypeResolver value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
