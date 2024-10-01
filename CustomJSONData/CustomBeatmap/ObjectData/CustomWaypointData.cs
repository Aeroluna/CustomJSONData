using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomWaypointData : WaypointData, ICustomData, IVersionable
    {
        public CustomWaypointData(
            float time,
#if LATEST
            float beat,
#endif
            int lineIndex,
            NoteLineLayer noteLineLayer,
            OffsetDirection offsetDirection,
            CustomData customData,
            Version version)
            : base(
                time,
#if LATEST
                beat,
#endif
                lineIndex,
                noteLineLayer,
                offsetDirection)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomWaypointData(
                time,
#if LATEST
                beat,
#endif
                lineIndex,
                lineLayer,
                offsetDirection,
                customData.Copy(),
                version);
        }
    }
}
