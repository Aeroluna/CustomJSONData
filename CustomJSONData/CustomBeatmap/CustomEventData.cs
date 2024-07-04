using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomEventData : BeatmapDataItem, ICustomData, IVersionable
    {
        public CustomEventData(float time, string type, CustomData data, Version version)
            : base(time, 0, 0, (BeatmapDataItemType)2)
        {
            eventType = type;
            customData = data;
            this.version = version;
        }

        public string eventType { get; }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomEventData(time, eventType, customData.Copy(), version);
        }
    }
}
