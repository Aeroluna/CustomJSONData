using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomBPMChangeBeatmapEventData : BPMChangeBeatmapEventData, ICustomData, IVersionable
    {
        public CustomBPMChangeBeatmapEventData(
            float time,
            float bpm,
            CustomData customData,
            Version version)
            : base(time, bpm)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomBPMChangeBeatmapEventData(time, bpm, customData.Copy(), version);
        }
    }
}
