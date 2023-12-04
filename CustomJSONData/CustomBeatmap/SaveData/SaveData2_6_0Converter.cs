using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapSaveDataVersion3;
using static BeatmapSaveDataVersion3.BeatmapSaveData;

namespace CustomJSONData.CustomBeatmap
{
    // Why are both classes named BeatmapSaveData ????? totally not confusing
    // TODO: Deserialize JSON -> V3 rather than using a converter.
    public static class SaveData2_6_0Converter
    {
        public static CustomBeatmapSaveData Convert2_6_0AndEarlier(
            Version version,
            string path,
            CustomData beatmapData,
            CustomData levelData)
        {
            Custom2_6_0AndEarlierBeatmapSaveData oldSaveData = Custom2_6_0AndEarlierBeatmapSaveData.Deserialize(version, path);

            // notes
            ILookup<bool, Custom2_6_0AndEarlierBeatmapSaveData.NoteData> notesSplit = oldSaveData.notes
                .OrderBy(n => n)
                .ToLookup(n => n.type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.NoteType.Bomb);
            List<BeatmapSaveData.ColorNoteData> colorNotes = notesSplit[false]
                .Select(n => new CustomBeatmapSaveData.ColorNoteData(
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
                .Select(n => new CustomBeatmapSaveData.BombNoteData(
                    n.time,
                    n.lineIndex,
                    (int)n.lineLayer,
                    n.customData))
                .Cast<BeatmapSaveData.BombNoteData>()
                .ToList();

            // obstacles
            List<BeatmapSaveData.ObstacleData> obstacles = oldSaveData.obstacles
                .OrderBy(n => n)
                .Select(n => new CustomBeatmapSaveData.ObstacleData(
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
                .OrderBy(n => n)
                .Select(n => new CustomBeatmapSaveData.SliderData(
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
                .OrderBy(n => n)
                .Select(n => new CustomBeatmapSaveData.WaypointData(
                    n.time,
                    n.lineIndex,
                    (int)n.lineLayer,
                    n.offsetDirection,
                    n.customData))
                .Cast<BeatmapSaveData.WaypointData>()
                .ToList();

            // events
            ILookup<int, Custom2_6_0AndEarlierBeatmapSaveData.EventData> eventsSplit = oldSaveData.events
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
                    .Select(n => new CustomBeatmapSaveData.ColorBoostEventData(n.time, n.value == 1, n.customData))
                    .Cast<BeatmapSaveData.ColorBoostEventData>()
                    .ToList();
            List<BeatmapSaveData.RotationEventData> rotationEvents =
                eventsSplit[1]
                    .Select(n => new CustomBeatmapSaveData.RotationEventData(
                        n.time,
                        n.type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event14 ? BeatmapSaveData.ExecutionTime.Early : BeatmapSaveData.ExecutionTime.Late,
                        BeatmapSaveData.SpawnRotationForEventValue(n.value),
                        n.customData))
                    .Cast<BeatmapSaveData.RotationEventData>()
                    .ToList();
            List<BeatmapSaveData.BpmChangeEventData> bpmChanges =
                eventsSplit[2]
                    .Select(n => new CustomBeatmapSaveData.BpmChangeEventData(n.time, n.floatValue, n.customData))
                    .Cast<BeatmapSaveData.BpmChangeEventData>()
                    .ToList();
            List<BeatmapSaveData.BasicEventData> basicEvents =
                eventsSplit[3]
                    .Select(n => new CustomBeatmapSaveData.BasicEventData(n.time, n.type, n.value, n.floatValue, n.customData))
                    .Cast<BeatmapSaveData.BasicEventData>()
                    .ToList();

            // specialeventkeywordfiltersdata
            BeatmapSaveData.BasicEventTypesWithKeywords basicEventTypesWithKeywords =
                new(oldSaveData.specialEventsKeywordFilters.keywords
                    .Select(n => new BeatmapSaveData.BasicEventTypesWithKeywords.BasicEventTypesForKeyword(n.keyword, n.specialEvents))
                    .ToList());

            // custom events
            List<CustomBeatmapSaveData.CustomEventData> customEvents = oldSaveData.customEvents
                .Select(n => new CustomBeatmapSaveData.CustomEventData(n.time, n.type, n.data))
                .ToList();

            // yay we're done
            return new CustomBeatmapSaveData(
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
                basicEventTypesWithKeywords,
                new List<FxEventBoxGroup>(),
                new FxEventsCollection(),
                true,
                true,
                customEvents,
                oldSaveData.customData,
                beatmapData,
                levelData);
        }
    }
}
