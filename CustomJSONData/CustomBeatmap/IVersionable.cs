using System;
using JetBrains.Annotations;

namespace CustomJSONData.CustomBeatmap
{
    public interface IVersionable
    {
        [PublicAPI]
        public Version version { get; }
    }
}
