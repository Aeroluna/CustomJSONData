using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomJSONData
{
    public static class JsonExtensions
    {
        public static bool? ReadIntAsBoolean(this JsonReader reader)
        {
            int? result = reader.ReadAsInt32Safe();
            return result.HasValue ? result.Value > 0 : null;
        }

        // Allows reading "-31.0" as "-31"
        public static int? ReadAsInt32Safe(this JsonReader reader)
        {
            double? result = reader.ReadAsDouble();
            if (!result.HasValue)
            {
                return null;
            }

            if (result.Value % 1 == 0)
            {
                return Convert.ToInt32(result.Value);
            }

            throw new JsonReaderException(reader.FormatMessage($"Input string [{result}] is not a valid integer."));
        }

        [PublicAPI]
        public static Color ReadAsColor(this JsonReader reader)
        {
            float r = default;
            float g = default;
            float b = default;
            float a = default;
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case "r":
                        r = (float?)reader.ReadAsDecimal() ?? r;
                        break;

                    case "g":
                        g = (float?)reader.ReadAsDecimal() ?? g;
                        break;

                    case "b":
                        b = (float?)reader.ReadAsDecimal() ?? b;
                        break;

                    case "a":
                        a = (float?)reader.ReadAsDecimal() ?? a;
                        break;
                }
            });

            return new Color(r, g, b, a);
        }

        public static void ReadToDictionary(
            this JsonReader reader,
            CustomData dictionary,
            [InstantHandle] Func<string, bool>? specialCase = null)
        {
            reader.Read();
            if (!reader.AssertToken("dictionary", JsonToken.StartObject, false))
            {
                return;
            }

            ObjectReadObject(reader, dictionary, specialCase);
        }

        public static bool ReadObject(this JsonReader reader, [InstantHandle] Action<string> action)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartObject)
            {
                return false;
            }

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                action((string?)reader.Value ?? string.Empty);

                reader.Read();
            }

            return true;
        }

        public static bool Finish(this bool result, [InstantHandle] Action action)
        {
            if (!result)
            {
                return false;
            }

            action();
            return true;
        }

        [PublicAPI]
        public static string[] ReadAsStringArray(this JsonReader reader, bool doThrow = true)
        {
            List<string> result = new();
            reader.Read(); // StartArray
            if (!reader.AssertToken("string array", JsonToken.StartArray, doThrow))
            {
                return result.ToArray();
            }

            while (reader.TokenType != JsonToken.EndArray)
            {
                string? cur = reader.ReadAsString();
                if (cur != null)
                {
                    result.Add(cur);
                }
            }

            return result.ToArray();
        }

        [PublicAPI]
        public static int[] ReadAsIntArray(this JsonReader reader, bool doThrow = true)
        {
            List<int> result = new();
            reader.Read(); // StartArray
            if (!reader.AssertToken("int array", JsonToken.StartArray, doThrow))
            {
                return result.ToArray();
            }

            while (reader.TokenType != JsonToken.EndArray)
            {
                int? cur = reader.ReadAsInt32Safe();
                if (cur != null)
                {
                    result.Add(cur.Value);
                }
            }

            return result.ToArray();
        }

        public static void ReadArray(this JsonReader reader, [InstantHandle] Func<bool> action, bool doThrow = true)
        {
            reader.Read(); // StartArray
            if (!reader.AssertToken("array", JsonToken.StartArray, doThrow))
            {
                return;
            }

            while (action())
            {
            }

            reader.AssertToken("array", JsonToken.EndArray);
        }

        public static string FormatMessage(this JsonReader reader, string message)
        {
            IJsonLineInfo? lineInfo = reader as IJsonLineInfo;
            message += $" Path [{reader.Path}]";

            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                message += $", line [{lineInfo.LineNumber}], position [{lineInfo.LinePosition}]";
            }

            message += ".";

            return message;
        }

        public static bool AssertToken(this JsonReader reader, string readMessage, JsonToken expectedToken, bool doThrow = true)
        {
            JsonToken tokenType = reader.TokenType;
            if (tokenType == expectedToken)
            {
                return true;
            }

            string message = reader
                .FormatMessage($"Unexpected token when reading {readMessage}: [{tokenType}], expected: [{expectedToken}].");
            if (doThrow && tokenType != JsonToken.Null)
            {
                throw new JsonSerializationException(message);
            }

            Plugin.Log.Error(message + " Error while reading customData, exception skipped");
            reader.Skip();
            return false;
        }

        // this is internal in json.net for some reason.
        public static JsonSerializationException CreateException(this JsonReader reader, string message)
        {
            return new JsonSerializationException(reader.FormatMessage(message));
        }

        internal static object ObjectReadObject(JsonReader reader, CustomData? dictionary = null, Func<string, bool>? specialCase = null)
        {
            dictionary ??= new CustomData();

            reader.AssertToken("dictionary", JsonToken.StartObject);

            while (reader.Read())
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = (string?)reader.Value ?? string.Empty;

                        if (specialCase != null && !specialCase(propertyName))
                        {
                            break;
                        }

                        if (!reader.Read())
                        {
                            throw reader.CreateException("Unexpected end when reading dictionary.");
                        }

                        dictionary[propertyName] = ObjectReadValue(reader);

                        break;

                    case JsonToken.Comment:
                        break;

                    case JsonToken.EndObject:
                        return dictionary;
                }
            }

            throw reader.CreateException("Unexpected end when reading dictionary.");
        }

        private static object? ObjectReadValue(JsonReader reader)
        {
            return reader.TokenType switch
            {
                JsonToken.StartObject => ObjectReadObject(reader),
                JsonToken.StartArray => ObjectReadList(reader),
                _ => reader.Value
            };
        }

        private static IList<object?> ObjectReadList(JsonReader reader)
        {
            IList<object?> list = new List<object?>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;

                    default:
                        list.Add(ObjectReadValue(reader));
                        break;

                    case JsonToken.EndArray:
                        return list;
                }
            }

            throw reader.CreateException("Unexpected end when reading dictionary.");
        }
    }
}
