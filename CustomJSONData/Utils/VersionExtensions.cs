using System;
using JetBrains.Annotations;

namespace CustomJSONData
{
    public static class VersionExtensions
    {
        [PublicAPI]
        public static Version noVersion { get; } = new(0, 0, 0);

        [PublicAPI]
        public static Version version2 { get; } = new("2.0.0");

        [PublicAPI]
        public static Version version3 { get; } = new("3.0.0");

        [PublicAPI]
        public static Version version4 { get; } = new("4.0.0");

        [PublicAPI]
        public static bool IsVersion2(this Version version)
        {
            return version < version3;
        }
    }
}
