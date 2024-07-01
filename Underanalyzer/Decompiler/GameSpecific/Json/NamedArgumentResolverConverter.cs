/*
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Text.Json;

namespace Underanalyzer.Decompiler.GameSpecific.Json;

internal class NamedArgumentResolverConverter
{
    public static void ReadContents(ref Utf8JsonReader reader, JsonSerializerOptions options, NamedArgumentResolver existing)
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
            string codeEntryName = reader.GetString();

            // Read name array
            List<string> names = new();
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }
                names.Add(reader.GetString());
            }

            // Define the code entry with these names
            existing.DefineCodeEntry(codeEntryName, names);
        }

        throw new JsonException();
    }
}
