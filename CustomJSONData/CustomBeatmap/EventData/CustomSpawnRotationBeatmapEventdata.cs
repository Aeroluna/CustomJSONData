namespace CustomJSONData.CustomBeatmap
{
    public class CustomSpawnRotationBeatmapEventdata : SpawnRotationBeatmapEventData, ICustomData
    {
        public CustomSpawnRotationBeatmapEventdata(
            float time,
            SpawnRotationEventType spawnRotationEventType,
            float deltaRotation,
            CustomData customData)
            : base(time, spawnRotationEventType, deltaRotation)
        {
            this.customData = customData;
        }

        public CustomData customData { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomSpawnRotationBeatmapEventdata(
                time,
                spawnRotationEventType,
                _deltaRotation,
                customData.Copy());
        }
    }
}
