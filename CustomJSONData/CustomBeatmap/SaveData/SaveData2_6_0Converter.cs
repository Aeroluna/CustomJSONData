#if PRE_V1_37_1
using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapSaveDataVersion3;

namespace CustomJSONData.CustomBeatmap
{
    // Why are both classes named BeatmapSaveData ????? totally not confusing
    // TODO: Deserialize JSON -> V3 rather than using a converter.
    public static class SaveData2_6_0Converter
    {
        public static Version3CustomBeatmapSaveData Convert2_6_0AndEarlier(string path, CustomData beatmapData, CustomData levelData)
        {
            Version2_6_0AndEarlierCustomBeatmapSaveData oldSaveData = Version2_6_0AndEarlierCustomBeatmapSaveData.Deserialize(path);

            // wtf
            if (new Version(oldSaveData.version).CompareTo(new Version("2.5.0")) < 0)
            {
                oldSaveData.ConvertBeatmapSaveDataPreV2_5_0();
            }

            // notes
            ILookup<bool, Version2_6_0AndEarlierCustomBeatmapSaveData.NoteSaveData> notesSplit = oldSaveData.notes
                .Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.NoteSaveData>()
                .OrderBy(n => n)
                .ToLookup(n => n.type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.NoteType.Bomb);
            List<BeatmapSaveData.ColorNoteData> colorNotes = notesSplit[false]
                .Select(n => new Version3CustomBeatmapSaveData.ColorNoteSaveData(
                    n.time,
                    n.lineIndex,
                    (int)n.lineLayer,
                    BeatmapSaveData.GetNoteColorType(n.type),
                    n.cutDirection,
                    0,
                    n.customData))
                .Cast<BeatmapSaveData.ColorNoteData>()
                .ToList();
            List<BeatmapSaveData.BombNoteData> bombNotes = notesSplit[true]
                .Select(n => new Version3CustomBeatmapSaveData.BombNoteSaveData(
                    n.time,
                    n.lineIndex,
                    (int)n.lineLayer,
                    n.customData))
                .Cast<BeatmapSaveData.BombNoteData>()
                .ToList();

            // obstacles
            List<BeatmapSaveData.ObstacleData> obstacles = oldSaveData.obstacles
                .Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.ObstacleSaveData>()
                .OrderBy(n => n)
                .Select(n => new Version3CustomBeatmapSaveData.ObstacleSaveData(
                    n.time,
                    n.lineIndex,
                    BeatmapSaveData.GetLayerForObstacleType(n.type),
                    n.duration,
                    n.width,
                    BeatmapSaveData.GetHeightForObstacleType(n.type),
                    n.customData))
                .Cast<BeatmapSaveData.ObstacleData>()
                .ToList();

            // sliders
            List<BeatmapSaveData.SliderData> sliders = oldSaveData.sliders
                .Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.SliderSaveData>()
                .OrderBy(n => n)
                .Select(n => new Version3CustomBeatmapSaveData.SliderSaveData(
                    BeatmapSaveData.GetNoteColorType(n.colorType),
                    n.time,
                    n.headLineIndex,
                    (int)n.headLineLayer,
                    n.headControlPointLengthMultiplier,
                    n.headCutDirection,
                    n.tailTime,
                    n.tailLineIndex,
                    (int)n.tailLineLayer,
                    n.tailControlPointLengthMultiplier,
                    n.tailCutDirection,
                    n.sliderMidAnchorMode,
                    n.customData))
                .Cast<BeatmapSaveData.SliderData>()
                .ToList();

            // waypoints
            List<BeatmapSaveData.WaypointData> waypoints = oldSaveData.waypoints
                .Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.WaypointSaveData>()
                .OrderBy(n => n)
                .Select(n => new Version3CustomBeatmapSaveData.WaypointSaveData(
                    n.time,
                    n.lineIndex,
                    (int)n.lineLayer,
                    n.offsetDirection,
                    n.customData))
                .Cast<BeatmapSaveData.WaypointData>()
                .ToList();

            // events
            ILookup<int, Version2_6_0AndEarlierCustomBeatmapSaveData.EventSaveData> eventsSplit = oldSaveData.events
                .Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.EventSaveData>()
                .OrderBy(n => n)
                .ToLookup(n => n.type switch
                {
                    BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event5 => 0,
                    BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event14 => 1,
                    BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event15 => 1,
                    BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.BpmChange => 2,
                    _ => 3
                });
            List<BeatmapSaveData.ColorBoostEventData> colorBoosts =
                eventsSplit[0]
                    .Select(n => new Version3CustomBeatmapSaveData.ColorBoostEventSaveData(n.time, n.value == 1, n.customData))
                    .Cast<BeatmapSaveData.ColorBoostEventData>()
                    .ToList();
            List<BeatmapSaveData.RotationEventData> rotationEvents =
                eventsSplit[1]
                    .Select(n => new Version3CustomBeatmapSaveData.RotationEventSaveData(
                        n.time,
                        n.type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event14 ? BeatmapSaveData.ExecutionTime.Early : BeatmapSaveData.ExecutionTime.Late,
                        BeatmapSaveData.SpawnRotationForEventValue(n.value),
                        n.customData))
                    .Cast<BeatmapSaveData.RotationEventData>()
                    .ToList();
            List<BeatmapSaveData.BpmChangeEventData> bpmChanges =
                eventsSplit[2]
                    .Select(n => new Version3CustomBeatmapSaveData.BpmChangeEventSaveData(n.time, n.floatValue, n.customData))
                    .Cast<BeatmapSaveData.BpmChangeEventData>()
                    .ToList();
            List<BeatmapSaveData.BasicEventData> basicEvents =
                eventsSplit[3]
                    .Select(n => new Version3CustomBeatmapSaveData.BasicEventSaveData(n.time, n.type, n.value, n.floatValue, n.customData))
                    .Cast<BeatmapSaveData.BasicEventData>()
                    .ToList();

            // specialeventkeywordfiltersdata
            BeatmapSaveData.BasicEventTypesWithKeywords basicEventTypesWithKeywords =
                new(oldSaveData.specialEventsKeywordFilters.keywords
                    .Select(n => new BeatmapSaveData.BasicEventTypesWithKeywords.BasicEventTypesForKeyword(n.keyword, n.specialEvents))
                    .ToList());

            // custom events
            List<Version3CustomBeatmapSaveData.CustomEventSaveData> customEvents = oldSaveData.customEvents
                .Select(n => new Version3CustomBeatmapSaveData.CustomEventSaveData(n.time, n.type, n.customData))
                .ToList();

            // yay we're done
            return new Version3CustomBeatmapSaveData(
                oldSaveData.version,
                bpmChanges,
                rotationEvents,
                colorNotes,
                bombNotes,
                obstacles,
                sliders,
                new List<BeatmapSaveData.BurstSliderData>(),
                waypoints,
                basicEvents,
                colorBoosts,
                new List<BeatmapSaveData.LightColorEventBoxGroup>(),
                new List<BeatmapSaveData.LightRotationEventBoxGroup>(),
                new List<BeatmapSaveData.LightTranslationEventBoxGroup>(),
#if V1_34_2
                new List<BeatmapSaveData.FxEventBoxGroup>(),
                new BeatmapSaveData.FxEventsCollection(),
#endif
                basicEventTypesWithKeywords,
                true,
                customEvents,
                beatmapData,
                levelData,
                oldSaveData.customData);
        }
    }
}
#endif
