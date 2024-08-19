using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomJSONDataDeserializer
    {
        private static readonly List<CustomJSONDataDeserializer> _deserializers = new();

        private readonly Dictionary<string, MethodInfo> _methods = new();

#pragma warning disable 8618
        private CustomJSONDataDeserializer(Type type)
#pragma warning restore 8618
        {
            foreach (MethodInfo method in type.GetMethods(AccessTools.allDeclared))
            {
                JSONDeserializer attribute = method.GetCustomAttribute<JSONDeserializer>();
                if (attribute == null)
                {
                    continue;
                }

                _methods.Add(attribute.PropertyName, method);
            }

            if (_methods.Count == 0)
            {
                throw new ArgumentException($"[{type.FullName}] does not contain a method marked with [{nameof(JSONDeserializer)}].", nameof(type));
            }
        }

        [PublicAPI]
        public bool Enabled { get; set; } = true;

        [PublicAPI]
        public static CustomJSONDataDeserializer Register<T>()
        {
            CustomJSONDataDeserializer deserializer = new(typeof(T));
            _deserializers.Add(deserializer);
            return deserializer;
        }

        public static bool Activate(object[] inputs, string field)
        {
            foreach (CustomJSONDataDeserializer deserializer in _deserializers.Where(n => n.Enabled))
            {
                if (deserializer._methods.TryGetValue(field, out MethodInfo method))
                {
                    return (bool)method.Invoke(null, method.ActualParameters(inputs));
                }
            }

            return true;
        }

        [PublicAPI]
        [MeansImplicitUse]
        [AttributeUsage(AttributeTargets.Method)]
        public class JSONDeserializer : Attribute
        {
            public JSONDeserializer(string propertyName)
            {
                PropertyName = propertyName;
            }

            internal string PropertyName { get; }
        }
    }
}
