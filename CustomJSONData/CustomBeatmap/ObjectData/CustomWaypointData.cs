using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomWaypointData : WaypointData, ICustomData, IVersionable
    {
        public CustomWaypointData(float time, int lineIndex, NoteLineLayer noteLineLayer, OffsetDirection offsetDirection, CustomData customData, Version version)
            : base(time, lineIndex, noteLineLayer, offsetDirection)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomWaypointData(time, lineIndex, lineLayer, offsetDirection, customData.Copy(), version);
        }
    }
}
