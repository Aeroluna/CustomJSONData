﻿using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomWaypointData : WaypointData, ICustomData, IVersionable
    {
        public CustomWaypointData(
            float time,
#if !PRE_V1_39_1
            float beat,
            int rotation,
#endif
            int lineIndex,
            NoteLineLayer noteLineLayer,
            OffsetDirection offsetDirection,
            CustomData customData,
            Version version)
            : base(
                time,
#if !PRE_V1_39_1
                beat,
                rotation,
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
#if !PRE_V1_39_1
                beat,
                rotation,
#endif
                lineIndex,
                lineLayer,
                offsetDirection,
                customData.Copy(),
                version);
        }
    }
}
