using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomColorBoostBeatmapEventData : ColorBoostBeatmapEventData, ICustomData, IVersionable
    {
        public CustomColorBoostBeatmapEventData(
            float time,
            bool boostColorsAreOn,
            CustomData customData,
            Version version)
            : base(time, boostColorsAreOn)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomColorBoostBeatmapEventData(time, boostColorsAreOn, customData.Copy(), version);
        }
    }
}
