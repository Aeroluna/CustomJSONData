using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomSpawnRotationBeatmapEventdata : SpawnRotationBeatmapEventData, ICustomData, IVersionable
    {
        public CustomSpawnRotationBeatmapEventdata(
            float time,
            SpawnRotationEventType spawnRotationEventType,
            float deltaRotation,
            CustomData customData,
            Version version)
            : base(time, spawnRotationEventType, deltaRotation)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomSpawnRotationBeatmapEventdata(
                time,
                spawnRotationEventType,
                _deltaRotation,
                customData.Copy(),
                version);
        }
    }
}
