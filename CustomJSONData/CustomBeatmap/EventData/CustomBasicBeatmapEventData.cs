using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomBasicBeatmapEventData : BasicBeatmapEventData, ICustomData, IVersionable
    {
        public CustomBasicBeatmapEventData(
            float time,
            BasicBeatmapEventType basicBeatmapEventType,
            int value,
            float floatValue,
            CustomData customData,
            Version version)
            : base(time, basicBeatmapEventType, value, floatValue)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomBasicBeatmapEventData(time, basicBeatmapEventType, value, floatValue, customData.Copy(), version);
        }
    }
}
