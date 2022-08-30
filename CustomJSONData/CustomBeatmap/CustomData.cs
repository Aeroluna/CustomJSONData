﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IPA.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomJSONData.CustomBeatmap
{
    [JsonConverter(typeof(CustomDataConverter))]
    public class CustomData : ConcurrentDictionary<string, object?>
    {
        public CustomData()
        {
        }

        public CustomData(IEnumerable<KeyValuePair<string, object?>> collection)
            : base(collection)
        {
        }

        [PublicAPI]
        public static CustomData FromJSON(string jsonString)
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(jsonString));
            using JsonTextReader reader = new(new StreamReader(jsonString));
            return FromJSON(reader);
        }

        public static CustomData FromJSON(JsonReader reader, Func<string, bool>? specialCase = null)
        {
            CustomData dictioanry = new();
            JsonExtensions.ObjectReadObject(reader, dictioanry, specialCase);
            return dictioanry;
        }

        // TODO: remove all dictionary copying and make it immutable
        public CustomData Copy()
        {
            return new CustomData(this);
        }

        [PublicAPI]
        public T GetRequired<T>(string key)
        {
            return Get<T>(key) ?? throw new JsonNotDefinedException(key);
        }

        public T? Get<T>(string key)
        {
            static bool IsNumericType(object o)
            {
                switch (Type.GetTypeCode(o.GetType()))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        return true;
                    default:
                        return false;
                }
            }

            // trygetvalue missing [notnullwhen] attribute :(
            if (!TryGetValue(key, out object? value) || value == null)
            {
                return default;
            }

            Type resultType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (IsNumericType(value))
            {
                return (T)Convert.ChangeType(value, resultType);
            }

            return (T)value;
        }

        [PublicAPI]
        public Vector3? GetVector3(string key)
        {
            List<float>? data = Get<List<object>>(key)?.Select(Convert.ToSingle).ToList();
            Vector3? final = null;
            if (data != null)
            {
                final = new Vector3(data[0], data[1], data[2]);
            }

            return final;
        }

        [PublicAPI]
        public Quaternion? GetQuaternion(string key)
        {
            Vector3? final = GetVector3(key);
            if (final.HasValue)
            {
                return Quaternion.Euler(final.Value);
            }

            return null;
        }

        [PublicAPI]
        public T GetStringToEnumRequired<T>(string key)
        {
            return GetStringToEnum<T?>(key) ?? throw new JsonNotDefinedException(key);
        }

        [PublicAPI]
        public T? GetStringToEnum<T>(string key)
        {
            if (!TryGetValue(key, out object? value) || value == null)
            {
                return default;
            }

            Type? underlyingType = Nullable.GetUnderlyingType(typeof(T));
            if (underlyingType != null)
            {
                return (T)Enum.Parse(underlyingType, (string)value);
            }

            return (T)Enum.Parse(typeof(T), (string)value);
        }

        public override string ToString()
        {
            return FormatDictionary(this);
        }

        private static string FormatDictionary(CustomData dictionary, int indent = 0)
        {
            string prefix = new('\t', indent);
            StringBuilder builder = new();
            builder.AppendLine(prefix + "{");
            foreach ((string key, object? value) in dictionary)
            {
                builder.AppendLine($"{prefix}\t\"{key}\": {FormatObject(value, indent + 1)}");
            }

            builder.Append(prefix + "}");
            return builder.ToString();
        }

        private static string FormatList(IEnumerable<object?> list, int indent = 0)
        {
            return "[" + string.Join(", ", list.Select(n => FormatObject(n, indent))) + "]";
        }

        private static string FormatObject(object? obj, int indent = 0)
        {
            return obj switch
            {
                List<object?> recursiveList => FormatList(recursiveList, indent),
                CustomData dictionary => FormatDictionary(dictionary, indent),
                _ => obj?.ToString() ?? "NULL"
            };
        }
    }
}
