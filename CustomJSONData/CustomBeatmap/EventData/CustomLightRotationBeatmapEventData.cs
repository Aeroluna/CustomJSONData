namespace CustomJSONData.CustomBeatmap
{
    public class CustomLightRotationBeatmapEventData : LightRotationBeatmapEventData, ICustomData
    {
        public CustomLightRotationBeatmapEventData(
            float time,
            int groupId,
            int elementId,
            bool usePreviousEventValue,
            EaseType easeType,
            LightAxis axis,
            float rotation,
            int loopCount,
            LightRotationDirection rotationDirection,
            CustomData customData)
            : base(time, groupId, elementId, usePreviousEventValue, easeType, axis, rotation, loopCount, rotationDirection)
        {
            this.customData = customData;
        }

        public CustomData customData { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomLightRotationBeatmapEventData(
                time,
                groupId,
                elementId,
                usePreviousEventValue,
                easeType,
                axis,
                rotation,
                loopCount,
                rotationDirection,
                customData.Copy());
        }
    }
}
