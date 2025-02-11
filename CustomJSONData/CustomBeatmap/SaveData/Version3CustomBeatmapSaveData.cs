using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapSaveDataVersion3;
using JetBrains.Annotations;
using Newtonsoft.Json;
#if !PRE_V1_37_1
using _Axis = BeatmapSaveDataCommon.Axis;
using _BasicEventData = BeatmapSaveDataVersion3.BasicEventData;
using _BasicEventTypesWithKeywords = BeatmapSaveDataCommon.BasicEventTypesWithKeywords;
using _BeatmapEventType = BeatmapSaveDataCommon.BeatmapEventType;
using _BombNoteData = BeatmapSaveDataVersion3.BombNoteData;
using _BpmChangeEventData = BeatmapSaveDataVersion3.BpmChangeEventData;
using _BurstSliderData = BeatmapSaveDataVersion3.BurstSliderData;
using _ColorBoostEventData = BeatmapSaveDataVersion3.ColorBoostEventData;
using _ColorNoteData = BeatmapSaveDataVersion3.ColorNoteData;
using _DistributionParamType = BeatmapSaveDataCommon.DistributionParamType;
using _EaseType = BeatmapSaveDataCommon.EaseType;
using _EnvironmentColorType = BeatmapSaveDataCommon.EnvironmentColorType;
using _ExecutionTime = BeatmapSaveDataCommon.ExecutionTime;
using _IndexFilter = BeatmapSaveDataVersion3.IndexFilter;
using _IndexFilterLimitAlsoAffectsType = BeatmapSaveDataCommon.IndexFilterLimitAlsoAffectsType;
using _IndexFilterRandomType = BeatmapSaveDataCommon.IndexFilterRandomType;
using _IndexFilterType = BeatmapSaveDataCommon.IndexFilterType;
using _LightColorBaseData = BeatmapSaveDataVersion3.LightColorBaseData;
using _LightColorEventBoxGroup = BeatmapSaveDataVersion3.LightColorEventBoxGroup;
using _LightRotationBaseData = BeatmapSaveDataVersion3.LightRotationBaseData;
using _LightRotationEventBoxGroup = BeatmapSaveDataVersion3.LightRotationEventBoxGroup;
using _LightTranslationBaseData = BeatmapSaveDataVersion3.LightTranslationBaseData;
using _NoteColorType = BeatmapSaveDataCommon.NoteColorType;
using _NoteCutDirection = BeatmapSaveDataCommon.NoteCutDirection;
using _ObstacleData = BeatmapSaveDataVersion3.ObstacleData;
using _OffsetDirection = BeatmapSaveDataCommon.OffsetDirection;
using _RotationDirection = BeatmapSaveDataCommon.RotationDirection;
using _RotationEventData = BeatmapSaveDataVersion3.RotationEventData;
using _SliderData = BeatmapSaveDataVersion3.SliderData;
using _SliderMidAnchorMode = BeatmapSaveDataCommon.SliderMidAnchorMode;
using _WaypointData = BeatmapSaveDataVersion3.WaypointData;
#else
using _Axis = BeatmapSaveDataVersion3.BeatmapSaveData.Axis;
using _BasicEventData = BeatmapSaveDataVersion3.BeatmapSaveData.BasicEventData;
using _BasicEventTypesWithKeywords = BeatmapSaveDataVersion3.BeatmapSaveData.BasicEventTypesWithKeywords;
using _BeatmapEventType = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType;
using _BombNoteData = BeatmapSaveDataVersion3.BeatmapSaveData.BombNoteData;
using _BpmChangeEventData = BeatmapSaveDataVersion3.BeatmapSaveData.BpmChangeEventData;
using _BurstSliderData = BeatmapSaveDataVersion3.BeatmapSaveData.BurstSliderData;
using _ColorBoostEventData = BeatmapSaveDataVersion3.BeatmapSaveData.ColorBoostEventData;
using _ColorNoteData = BeatmapSaveDataVersion3.BeatmapSaveData.ColorNoteData;
using _DistributionParamType = BeatmapSaveDataVersion3.BeatmapSaveData.EventBox.DistributionParamType;
using _EaseType = BeatmapSaveDataVersion3.BeatmapSaveData.EaseType;
using _EnvironmentColorType = BeatmapSaveDataVersion3.BeatmapSaveData.EnvironmentColorType;
using _ExecutionTime = BeatmapSaveDataVersion3.BeatmapSaveData.ExecutionTime;
using _IndexFilter = BeatmapSaveDataVersion3.BeatmapSaveData.IndexFilter;
using _IndexFilterLimitAlsoAffectsType = BeatmapSaveDataVersion3.BeatmapSaveData.IndexFilterLimitAlsoAffectsType;
using _IndexFilterRandomType = BeatmapSaveDataVersion3.BeatmapSaveData.IndexFilterRandomType;
using _IndexFilterType = BeatmapSaveDataVersion3.BeatmapSaveData.IndexFilter.IndexFilterType;
using _LightColorBaseData = BeatmapSaveDataVersion3.BeatmapSaveData.LightColorBaseData;
using _LightColorEventBoxGroup = BeatmapSaveDataVersion3.BeatmapSaveData.LightColorEventBoxGroup;
using _LightRotationBaseData = BeatmapSaveDataVersion3.BeatmapSaveData.LightRotationBaseData;
using _LightRotationEventBoxGroup = BeatmapSaveDataVersion3.BeatmapSaveData.LightRotationEventBoxGroup;
using _LightTranslationBaseData = BeatmapSaveDataVersion3.BeatmapSaveData.LightTranslationBaseData;
using _NoteColorType = BeatmapSaveDataVersion3.BeatmapSaveData.NoteColorType;
using _NoteCutDirection = NoteCutDirection;
using _ObstacleData = BeatmapSaveDataVersion3.BeatmapSaveData.ObstacleData;
using _OffsetDirection = OffsetDirection;
using _RotationDirection = BeatmapSaveDataVersion3.BeatmapSaveData.LightRotationBaseData.RotationDirection;
using _RotationEventData = BeatmapSaveDataVersion3.BeatmapSaveData.RotationEventData;
using _SliderData = BeatmapSaveDataVersion3.BeatmapSaveData.SliderData;
using _SliderMidAnchorMode = SliderMidAnchorMode;
using _WaypointData = BeatmapSaveDataVersion3.BeatmapSaveData.WaypointData;
#endif

namespace CustomJSONData.CustomBeatmap
{
    public partial class Version3CustomBeatmapSaveData : BeatmapSaveData
    {
        // worst naming scheme ever
        private const string _beat = "b";
        private const string _colorType = "c";
        private const string _line = "x";
        private const string _layer = "y";
        private const string _cutDirection = "d";
        private const string _tailBeat = "tb";
        private const string _tailLine = "tx";
        private const string _tailLayer = "ty";
        private const string _eventBoxes = "e";
        private const string _groupId = "g";
        private const string _indexFilter = "f";
        private const string _beatDistributionParam = "w";
        private const string _beatDistributionParamType = "d";

        private const string _customData = "customData";

        public Version3CustomBeatmapSaveData(
            string version,
            List<_BpmChangeEventData> bpmEvents,
            List<_RotationEventData> rotationEvents,
            List<_ColorNoteData> colorNotes,
            List<_BombNoteData> bombNotes,
            List<_ObstacleData> obstacles,
            List<_SliderData> sliders,
            List<_BurstSliderData> burstSliders,
            List<_WaypointData> waypoints,
            List<_BasicEventData> basicBeatmapEvents,
            List<_ColorBoostEventData> colorBoostBeatmapEvents,
            List<_LightColorEventBoxGroup> lightColorEventBoxGroups,
            List<_LightRotationEventBoxGroup> lightRotationEventBoxGroups,
            List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups,
#if !V1_29_1
            List<FxEventBoxGroup> vfxEventBoxGroup,
            FxEventsCollection fxEventsCollection,
#endif
            _BasicEventTypesWithKeywords basicEventTypesWithKeywords,
            bool useNormalEventsAsCompatibleEvents,
            List<CustomEventSaveData> customEvents,
#if PRE_V1_37_1
            CustomData beatmapCustomData,
            CustomData levelCustomData,
#endif
            CustomData customData)
            : base(
                bpmEvents,
                rotationEvents,
                colorNotes,
                bombNotes,
                obstacles,
                sliders,
                burstSliders,
                waypoints,
                basicBeatmapEvents,
                colorBoostBeatmapEvents,
                lightColorEventBoxGroups,
                lightRotationEventBoxGroups,
                lightTranslationEventBoxGroups,
#if !V1_29_1
                vfxEventBoxGroup,
                fxEventsCollection,
#endif
                basicEventTypesWithKeywords,
                useNormalEventsAsCompatibleEvents)
        {
            this.version = version;
#if PRE_V1_37_1
            beatmapVersion = string.IsNullOrEmpty(version) ? VersionExtensions.noVersion : new Version(version);
            this.beatmapCustomData = beatmapCustomData;
            this.levelCustomData = levelCustomData;
#endif
            this.customEvents = customEvents;
            this.customData = customData;
        }

        public List<CustomEventSaveData> customEvents { get; }

        public CustomData customData { get; }

#if PRE_V1_37_1
        public Version beatmapVersion { get; }

        public CustomData beatmapCustomData { get; }

        public CustomData levelCustomData { get; }
#endif

#if !PRE_V1_37_1
        public static Version3CustomBeatmapSaveData Deserialize(string path)
#else
        public static Version3CustomBeatmapSaveData Deserialize(string path, CustomData beatmapCustomData, CustomData levelCustomData)
#endif
        {
#if !PRE_V1_37_1
            string version = string.Empty;
#else
            string version = GetVersionFromPath(path);

            if (string.IsNullOrEmpty(version) ||
                new Version(version).CompareTo(version2_6_0) <= 0)
            {
                return SaveData2_6_0Converter.Convert2_6_0AndEarlier(path, beatmapCustomData, levelCustomData);
            }
#endif

            // lets do this
            List<_BpmChangeEventData> bpmEvents = new();
            List<_RotationEventData> rotationEvents = new();
            List<_ColorNoteData> colorNotes = new();
            List<_BombNoteData> bombNotes = new();
            List<_ObstacleData> obstacles = new();
            List<_SliderData> sliders = new();
            List<_BurstSliderData> burstSliders = new();
            List<_WaypointData> waypoints = new();
            List<_BasicEventData> basicBeatmapEvents = new();
            List<_ColorBoostEventData> colorBoostBeatmapEvents = new();
            List<_LightColorEventBoxGroup> lightColorEventBoxGroups = new();
            List<_LightRotationEventBoxGroup> lightRotationEventBoxGroups = new();
            List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups = new();
#if !V1_29_1
            List<FxEventBoxGroup> vfxEventBoxGroups = new();
            FxEventsCollection? fxEventsCollection = null;
#endif
            List<_BasicEventTypesWithKeywords.BasicEventTypesForKeyword> basicEventTypesForKeyword = new();
            bool useNormalEventsAsCompatibleEvents = default;
            CustomData data = new();
            List<CustomEventSaveData> customEvents = new();

#if !PRE_V1_37_1
            using JsonTextReader reader = new(new StringReader(path));
#else
            using JsonTextReader reader = new(new StreamReader(path));
#endif

            object[] inputs =
            {
                reader,
                bpmEvents,
                rotationEvents,
                colorNotes,
                bombNotes,
                obstacles,
                sliders,
                burstSliders,
                waypoints,
                basicBeatmapEvents,
                colorBoostBeatmapEvents,
                lightColorEventBoxGroups,
                lightRotationEventBoxGroups,
                lightTranslationEventBoxGroups,
                basicEventTypesForKeyword,
                useNormalEventsAsCompatibleEvents,
                customEvents,
#if PRE_V1_37_1
                new SaveDataCustomDatas(beatmapCustomData, levelCustomData, data)
#endif
            };

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value)
                    {
                        default:
                            reader.Skip();
                            break;

#if !PRE_V1_37_1
                        case "_version":
                            version = reader.ReadAsString() ?? version;
                            break;
#endif

                        case "bpmEvents":
                            DeserializeBpmChangeArray(reader, bpmEvents);
                            break;

                        case "rotationEvents":
                            DeserializeRotationArray(reader, rotationEvents);
                            break;

                        case "colorNotes":
                            DeserializeColorNoteArray(reader, colorNotes);
                            break;

                        case "bombNotes":
                            DeserializeBombNoteArray(reader, bombNotes);
                            break;

                        case "obstacles":
                            DeserializeObstacleArray(reader, obstacles);
                            break;

                        case "sliders":
                            DeserializeSliderArray(reader, sliders);
                            break;

                        case "burstSliders":
                            DeserializeBurstSliderArray(reader, burstSliders);
                            break;

                        case "waypoints":
                            DeserializeWaypointArray(reader, waypoints);
                            break;

                        case "basicBeatmapEvents":
                            DeserializeBasicEventArray(reader, basicBeatmapEvents);
                            break;

                        case "colorBoostBeatmapEvents":
                            DeserializeColorBoostArray(reader, colorBoostBeatmapEvents);
                            break;

                        case "lightColorEventBoxGroups":
                            DeserializeLightColorEventBoxGroupArray(reader, lightColorEventBoxGroups);
                            break;

                        case "lightRotationEventBoxGroups":
                            DeserializeLightRotationEventBoxGroupArray(reader, lightRotationEventBoxGroups);
                            break;

                        case "lightTranslationEventBoxGroups":
                            DeserializeLightTranslationEventBoxGroupArray(reader, lightTranslationEventBoxGroups);
                            break;

#if !V1_29_1
                        case "vfxEventBoxGroups":
                            DeserializeFxEventBoxGroupArray(reader, vfxEventBoxGroups);
                            break;

                        case "_fxEventsCollection":
                            fxEventsCollection = DeserializeFxEventCollection(reader);
                            break;
#endif

                        case "basicEventTypesWithKeywords":
                            reader.ReadObject(objectName =>
                            {
                                switch (objectName)
                                {
                                    case "d":
                                        DeserializeBasicEventTypesForKeywordArray(reader, basicEventTypesForKeyword);

                                        break;

                                    default:
                                        reader.Skip();
                                        break;
                                }
                            });
                            break;

                        case "useNormalEventsAsCompatibleEvents":
                            useNormalEventsAsCompatibleEvents = reader.ReadAsBoolean() ?? useNormalEventsAsCompatibleEvents;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data, propertyName =>
                            {
                                if (!propertyName.Equals("customEvents"))
                                {
                                    return CustomJSONDataDeserializer.Activate(inputs, propertyName);
                                }

                                DeserializeCustomEventArray(reader, customEvents);
                                return false;
                            });

                            break;
                    }
                }
            }

            return new Version3CustomBeatmapSaveData(
                version,
                bpmEvents.OrderBy(n => n).ToList(),
                rotationEvents.OrderBy(n => n).ToList(),
                colorNotes.OrderBy(n => n).ToList(),
                bombNotes.OrderBy(n => n).ToList(),
                obstacles.OrderBy(n => n).ToList(),
                sliders.OrderBy(n => n).ToList(),
                burstSliders.OrderBy(n => n).ToList(),
                waypoints.OrderBy(n => n).ToList(),
                basicBeatmapEvents.OrderBy(n => n).ToList(),
                colorBoostBeatmapEvents.OrderBy(n => n).ToList(),
                lightColorEventBoxGroups.OrderBy(n => n).ToList(),
                lightRotationEventBoxGroups.OrderBy(n => n).ToList(),
                lightTranslationEventBoxGroups.OrderBy(n => n).ToList(),
#if !V1_29_1
                vfxEventBoxGroups.OrderBy(n => n).ToList(),
                fxEventsCollection ?? new FxEventsCollection(),
#endif
                new _BasicEventTypesWithKeywords(basicEventTypesForKeyword),
                useNormalEventsAsCompatibleEvents,
                customEvents.OrderBy(n => n).ToList(),
#if PRE_V1_37_1
                beatmapCustomData,
                levelCustomData,
#endif
                data);
        }

#if PRE_V1_37_1
        public static string GetVersionFromPath(string path)
        {
            // SongCore has a fallback so i guess i do too
            // cant think of a better way than opening a streamreader
            using JsonTextReader reader = new(new StreamReader(path));
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value)
                    {
                        default:
                            reader.Skip();
                            break;

                        case "version":
                        case "_version":
                            return reader.ReadAsString()!; // ctor has null check
                    }
                }
            }

            Plugin.Log.Debug($"[{path}] does not contain a version");
            return string.Empty;
        }
#endif

        public static void DeserializeCustomEventArray(JsonReader reader, List<CustomEventSaveData> list)
        {
            reader.ReadArray(
                () =>
            {
                float beat = default;
                string type = string.Empty;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case "t":
                            type = reader.ReadAsString() ?? type;
                            break;

                        case "d":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new CustomEventSaveData(beat, type, data)));
            },
                false);
        }

        public static void DeserializeBpmChangeArray(JsonReader reader, List<_BpmChangeEventData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                float bpm = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case "m":
                            bpm = (float?)reader.ReadAsDouble() ?? bpm;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new BpmChangeEventSaveData(beat, bpm, data)));
            });
        }

        public static void DeserializeRotationArray(JsonReader reader, List<_RotationEventData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                _ExecutionTime executionTime = default;
                float rotation = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case "e":
                            executionTime = (_ExecutionTime?)reader.ReadAsInt32Safe() ?? executionTime;
                            break;

                        case "r":
                            rotation = (float?)reader.ReadAsDouble() ?? rotation;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new RotationEventSaveData(beat, executionTime, rotation, data)));
            });
        }

        public static void DeserializeColorNoteArray(JsonReader reader, List<_ColorNoteData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                int line = default;
                int layer = default;
                _NoteColorType color = default;
                _NoteCutDirection cutDirection = default;
                int angleOffset = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _line:
                            line = reader.ReadAsInt32Safe() ?? line;
                            break;

                        case _layer:
                            layer = reader.ReadAsInt32Safe() ?? layer;
                            break;

                        case _colorType:
                            color = (_NoteColorType?)reader.ReadAsInt32Safe() ?? color;
                            break;

                        case _cutDirection:
                            cutDirection = (_NoteCutDirection?)reader.ReadAsInt32Safe() ?? cutDirection;
                            break;

                        case "a":
                            angleOffset = reader.ReadAsInt32Safe() ?? angleOffset;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new ColorNoteSaveData(beat, line, layer, color, cutDirection, angleOffset, data)));
            });
        }

        public static void DeserializeBombNoteArray(JsonReader reader, List<_BombNoteData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                int line = default;
                int layer = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _line:
                            line = reader.ReadAsInt32Safe() ?? line;
                            break;

                        case _layer:
                            layer = reader.ReadAsInt32Safe() ?? layer;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new BombNoteSaveData(beat, line, layer, data)));
            });
        }

        public static void DeserializeObstacleArray(JsonReader reader, List<_ObstacleData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                int line = default;
                int layer = default;
                float duration = default;
                int width = default;
                int height = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _line:
                            line = reader.ReadAsInt32Safe() ?? line;
                            break;

                        case _layer:
                            layer = reader.ReadAsInt32Safe() ?? layer;
                            break;

                        case "d":
                            duration = (float?)reader.ReadAsDouble() ?? duration;
                            break;

                        case "w":
                            width = reader.ReadAsInt32Safe() ?? width;
                            break;

                        case "h":
                            height = reader.ReadAsInt32Safe() ?? height;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new ObstacleSaveData(beat, line, layer, duration, width, height, data)));
            });
        }

        public static void DeserializeSliderArray(JsonReader reader, List<_SliderData> list)
        {
            reader.ReadArray(() =>
            {
                _NoteColorType color = default;
                float headBeat = default;
                int headLine = default;
                int headLayer = default;
                float headControlPointLengthMultiplier = default;
                _NoteCutDirection headCutDirection = default;
                float tailBeat = default;
                int tailLine = default;
                int tailLayer = default;
                float tailControlPointLengthMultiplier = default;
                _NoteCutDirection tailCutDirection = default;
                _SliderMidAnchorMode sliderMidAnchorMode = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _colorType:
                            color = (_NoteColorType?)reader.ReadAsInt32Safe() ?? color;
                            break;

                        case _beat:
                            headBeat = (float?)reader.ReadAsDouble() ?? headBeat;
                            break;

                        case _line:
                            headLine = reader.ReadAsInt32Safe() ?? headLine;
                            break;

                        case _layer:
                            headLayer = reader.ReadAsInt32Safe() ?? headLayer;
                            break;

                        case "mu":
                            headControlPointLengthMultiplier =
                                (float?)reader.ReadAsDouble() ?? headControlPointLengthMultiplier;
                            break;

                        case _cutDirection:
                            headCutDirection = (_NoteCutDirection?)reader.ReadAsInt32Safe() ?? headCutDirection;
                            break;

                        case _tailBeat:
                            tailBeat = (float?)reader.ReadAsDouble() ?? tailBeat;
                            break;

                        case _tailLine:
                            tailLine = reader.ReadAsInt32Safe() ?? tailLine;
                            break;

                        case _tailLayer:
                            tailLayer = reader.ReadAsInt32Safe() ?? tailLayer;
                            break;

                        case "tmu":
                            tailControlPointLengthMultiplier =
                                (float?)reader.ReadAsDouble() ?? tailControlPointLengthMultiplier;
                            break;

                        case "tc":
                            tailCutDirection = (_NoteCutDirection?)reader.ReadAsInt32Safe() ?? tailCutDirection;
                            break;

                        case "m":
                            sliderMidAnchorMode = (_SliderMidAnchorMode?)reader.ReadAsInt32Safe() ?? sliderMidAnchorMode;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new SliderSaveData(
                    color,
                    headBeat,
                    headLine,
                    headLayer,
                    headControlPointLengthMultiplier,
                    headCutDirection,
                    tailBeat,
                    tailLine,
                    tailLayer,
                    tailControlPointLengthMultiplier,
                    tailCutDirection,
                    sliderMidAnchorMode,
                    data)));
            });
        }

        public static void DeserializeBurstSliderArray(JsonReader reader, List<_BurstSliderData> list)
        {
            reader.ReadArray(() =>
            {
                _NoteColorType color = default;
                float headBeat = default;
                int headLine = default;
                int headLayer = default;
                _NoteCutDirection headCutDirection = default;
                float tailBeat = default;
                int tailLine = default;
                int tailLayer = default;
                int sliceCount = default;
                float squishAmount = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _colorType:
                            color = (_NoteColorType?)reader.ReadAsInt32Safe() ?? color;
                            break;

                        case _beat:
                            headBeat = (float?)reader.ReadAsDouble() ?? headBeat;
                            break;

                        case _line:
                            headLine = reader.ReadAsInt32Safe() ?? headLine;
                            break;

                        case _layer:
                            headLayer = reader.ReadAsInt32Safe() ?? headLayer;
                            break;

                        case _cutDirection:
                            headCutDirection = (_NoteCutDirection?)reader.ReadAsInt32Safe() ?? headCutDirection;
                            break;

                        case _tailBeat:
                            tailBeat = (float?)reader.ReadAsDouble() ?? tailBeat;
                            break;

                        case _tailLine:
                            tailLine = reader.ReadAsInt32Safe() ?? tailLine;
                            break;

                        case _tailLayer:
                            tailLayer = reader.ReadAsInt32Safe() ?? tailLayer;
                            break;

                        case "sc":
                            sliceCount = reader.ReadAsInt32Safe() ?? sliceCount;
                            break;

                        case "s":
                            squishAmount = (float?)reader.ReadAsDouble() ?? squishAmount;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new BurstSliderSaveData(
                    color,
                    headBeat,
                    headLine,
                    headLayer,
                    headCutDirection,
                    tailBeat,
                    tailLine,
                    tailLayer,
                    sliceCount,
                    squishAmount,
                    data)));
            });
        }

        public static void DeserializeWaypointArray(JsonReader reader, List<_WaypointData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                int line = default;
                int layer = default;
                _OffsetDirection offsetDirection = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _line:
                            line = reader.ReadAsInt32Safe() ?? line;
                            break;

                        case _layer:
                            layer = reader.ReadAsInt32Safe() ?? layer;
                            break;

                        case _cutDirection:
                            offsetDirection = (_OffsetDirection?)reader.ReadAsInt32Safe() ?? offsetDirection;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new WaypointSaveData(beat, line, layer, offsetDirection, data)));
            });
        }

        public static void DeserializeBasicEventArray(JsonReader reader, List<_BasicEventData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                _BeatmapEventType eventType = default;
                int value = default;
                float floatValue = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case "et":
                            eventType = (_BeatmapEventType?)reader.ReadAsInt32Safe() ?? eventType;
                            break;

                        case "i":
                            value = reader.ReadAsInt32Safe() ?? value;
                            break;

                        case "f":
                            floatValue = (float?)reader.ReadAsDouble() ?? floatValue;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new BasicEventSaveData(beat, eventType, value, floatValue, data)));
            });
        }

        public static void DeserializeColorBoostArray(JsonReader reader, List<_ColorBoostEventData> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                bool boost = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case "o":
                            boost = reader.ReadAsBoolean() ?? boost;
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new ColorBoostEventSaveData(beat, boost, data)));
            });
        }

        public static _IndexFilter DeserializeIndexFilter(JsonReader reader)
        {
            _IndexFilterType type = default;
            int param0 = default;
            int param1 = default;
            bool reversed = default;
            _IndexFilterRandomType random = default;
            int seed = default;
            int chunks = default;
            float limit = default;
            _IndexFilterLimitAlsoAffectsType limitAlsoAffectsType = default;
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case "f":
                        type = (_IndexFilterType?)reader.ReadAsInt32Safe() ?? type;
                        break;

                    case "p":
                        param0 = reader.ReadAsInt32Safe() ?? param0;
                        break;

                    case "t":
                        param1 = reader.ReadAsInt32Safe() ?? param1;
                        break;

                    case "r":
                        reversed = reader.ReadIntAsBoolean() ?? reversed;
                        break;

                    case "c":
                        chunks = reader.ReadAsInt32Safe() ?? chunks;
                        break;

                    case "l":
                        limit = (float?)reader.ReadAsDouble() ?? limit;
                        break;

                    case "d":
                        limitAlsoAffectsType = (_IndexFilterLimitAlsoAffectsType?)reader.ReadAsInt32Safe() ??
                                               limitAlsoAffectsType;
                        break;

                    case "n":
                        random = (_IndexFilterRandomType?)reader.ReadAsInt32Safe() ?? random;
                        break;

                    case "s":
                        seed = reader.ReadAsInt32Safe() ?? seed;
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return new _IndexFilter(type, param0, param1, reversed, random, seed, chunks, limit, limitAlsoAffectsType);
        }

        public static void DeserializeLightColorEventBoxGroupArray(JsonReader reader, List<_LightColorEventBoxGroup> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                List<LightColorEventBox> eventBoxes = new();
                int groupId = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _groupId:
                            groupId = reader.ReadAsInt32Safe() ?? groupId;
                            break;

                        case _eventBoxes:
                            reader.ReadArray(() =>
                            {
                                _IndexFilter? indexFilter = default;
                                float beatDistributionParam = default;
                                _DistributionParamType beatDistributionParamType = default;
                                float brightnessDistributionParam = default;
                                bool brightnessDistributionShouldAffectFirstBaseEvent = default;
                                _DistributionParamType brightnessDistributionParamType = default;
                                _EaseType brightnessDistributionEaseType = default;
                                List<_LightColorBaseData> lightColorBaseDataList = new();
                                return reader.ReadObject(eventName =>
                                {
                                    switch (eventName)
                                    {
                                        case _indexFilter:
                                            indexFilter = DeserializeIndexFilter(reader);
                                            break;

                                        case _beatDistributionParam:
                                            beatDistributionParam =
                                                (float?)reader.ReadAsDouble() ?? beatDistributionParam;
                                            break;

                                        case _beatDistributionParamType:
                                            beatDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                beatDistributionParamType;
                                            break;

                                        case "r":
                                            brightnessDistributionParam = (float?)reader.ReadAsDouble() ??
                                                                          brightnessDistributionParam;
                                            break;

                                        case "b":
                                            brightnessDistributionShouldAffectFirstBaseEvent =
                                                reader.ReadIntAsBoolean() ??
                                                brightnessDistributionShouldAffectFirstBaseEvent;
                                            break;

                                        case "t":
                                            brightnessDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                brightnessDistributionParamType;
                                            break;

                                        case "i":
                                            brightnessDistributionEaseType = (_EaseType?)reader.ReadAsInt32Safe() ??
                                                                             brightnessDistributionEaseType;
                                            break;

                                        case "e":
                                            reader.ReadArray(() =>
                                            {
                                                float lightBeat = default;
                                                TransitionType transitionType = default;
                                                _EnvironmentColorType colorType = default;
                                                float brightness = default;
                                                int strobeFrequency = default;
#if !V1_29_1
                                                float strobeBrightness = default;
                                                bool strobeFade = default;
#endif
                                                return reader.ReadObject(lightName =>
                                                {
                                                    switch (lightName)
                                                    {
                                                        case _beat:
                                                            lightBeat = (float?)reader.ReadAsDouble() ?? lightBeat;
                                                            break;

                                                        case "i":
                                                            transitionType =
                                                                (TransitionType?)reader.ReadAsInt32Safe() ??
                                                                transitionType;
                                                            break;

                                                        case _colorType:
                                                            colorType =
                                                                (_EnvironmentColorType?)reader.ReadAsInt32Safe() ??
                                                                colorType;
                                                            break;

                                                        case "s":
                                                            brightness = (float?)reader.ReadAsDouble() ?? brightness;
                                                            break;

                                                        case "f":
                                                            strobeFrequency = reader.ReadAsInt32Safe() ??
                                                                              strobeFrequency;
                                                            break;
#if !V1_29_1
                                                        case "sb":
                                                            strobeBrightness = (float?)reader.ReadAsDouble() ??
                                                                               strobeBrightness;
                                                            break;

                                                        case "sf":
                                                            strobeFade = reader.ReadIntAsBoolean() ??
                                                                         strobeFade;
                                                            break;
#endif

                                                        default:
                                                            reader.Skip();
                                                            break;
                                                    }
                                                }).Finish(() => lightColorBaseDataList.Add(new _LightColorBaseData(
                                                    lightBeat,
                                                    transitionType,
                                                    colorType,
                                                    brightness,
#if !V1_29_1
                                                    strobeFrequency,
                                                    strobeBrightness,
                                                    strobeFade)));
#else
                                                    strobeFrequency)));
#endif
                                            });
                                            break;

                                        default:
                                            reader.Skip();
                                            break;
                                    }
                                }).Finish(() => eventBoxes.Add(new LightColorEventBox(
                                    indexFilter,
                                    beatDistributionParam,
                                    beatDistributionParamType,
                                    brightnessDistributionParam,
                                    brightnessDistributionShouldAffectFirstBaseEvent,
                                    brightnessDistributionParamType,
                                    brightnessDistributionEaseType,
                                    lightColorBaseDataList)));
                            });
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new LightColorEventBoxGroupSaveData(beat, groupId, eventBoxes, data)));
            });
        }

        public static void DeserializeLightRotationEventBoxGroupArray(JsonReader reader, List<_LightRotationEventBoxGroup> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                List<LightRotationEventBox> eventBoxes = new();
                int groupId = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _groupId:
                            groupId = reader.ReadAsInt32Safe() ?? groupId;
                            break;

                        case _eventBoxes:
                            reader.ReadArray(() =>
                            {
                                _IndexFilter? indexFilter = default;
                                float beatDistributionParam = default;
                                _DistributionParamType beatDistributionParamType = default;
                                float rotationDistributionParam = default;
                                _DistributionParamType rotationDistributionParamType = default;
                                bool rotationDistributionShouldAffectFirstBaseEvent = default;
                                _EaseType rotationDistributionEaseType = default;
                                _Axis axis = default;
                                bool flipRotation = default;
                                List<_LightRotationBaseData> lightRotationBaseDataList = new();
                                return reader.ReadObject(eventName =>
                                {
                                    switch (eventName)
                                    {
                                        case _indexFilter:
                                            indexFilter = DeserializeIndexFilter(reader);
                                            break;

                                        case _beatDistributionParam:
                                            beatDistributionParam =
                                                (float?)reader.ReadAsDouble() ?? beatDistributionParam;
                                            break;

                                        case _beatDistributionParamType:
                                            beatDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                beatDistributionParamType;
                                            break;

                                        case "s":
                                            rotationDistributionParam = (float?)reader.ReadAsDouble() ??
                                                                        rotationDistributionParam;
                                            break;

                                        case "t":
                                            rotationDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                rotationDistributionParamType;
                                            break;

                                        case "b":
                                            rotationDistributionShouldAffectFirstBaseEvent =
                                                reader.ReadIntAsBoolean() ??
                                                rotationDistributionShouldAffectFirstBaseEvent;
                                            break;

                                        case "a":
                                            axis = (_Axis?)reader.ReadAsInt32Safe() ?? axis;
                                            break;

                                        case "r":
                                            flipRotation = reader.ReadIntAsBoolean() ?? flipRotation;
                                            break;

                                        case "i":
                                            rotationDistributionEaseType = (_EaseType?)reader.ReadAsInt32() ??
                                                                           rotationDistributionEaseType;
                                            break;

                                        case "l":
                                            reader.ReadArray(() =>
                                            {
                                                float lightBeat = default;
                                                bool usePreviousEventRotationValue = default;
                                                _EaseType easeType = default;
                                                int loopsCount = default;
                                                float rotation = default;
                                                _RotationDirection rotationDirection = default;
                                                return reader.ReadObject(lightName =>
                                                {
                                                    switch (lightName)
                                                    {
                                                        case _beat:
                                                            lightBeat = (float?)reader.ReadAsDouble() ?? lightBeat;
                                                            break;

                                                        case "p":
                                                            usePreviousEventRotationValue = reader.ReadIntAsBoolean() ??
                                                                usePreviousEventRotationValue;
                                                            break;

                                                        case "e":
                                                            easeType = (_EaseType?)reader.ReadAsInt32Safe() ?? easeType;
                                                            break;

                                                        case "l":
                                                            loopsCount = reader.ReadAsInt32Safe() ?? loopsCount;
                                                            break;

                                                        case "r":
                                                            rotation = (float?)reader.ReadAsDouble() ?? rotation;
                                                            break;

                                                        case "o":
                                                            rotationDirection =
                                                                (_RotationDirection?)reader
                                                                    .ReadAsInt32Safe() ?? rotationDirection;
                                                            break;

                                                        default:
                                                            reader.Skip();
                                                            break;
                                                    }
                                                }).Finish(() => lightRotationBaseDataList.Add(new _LightRotationBaseData(
                                                    lightBeat,
                                                    usePreviousEventRotationValue,
                                                    easeType,
                                                    loopsCount,
                                                    rotation,
                                                    rotationDirection)));
                                            });
                                            break;

                                        default:
                                            reader.Skip();
                                            break;
                                    }
                                }).Finish(() => eventBoxes.Add(new LightRotationEventBox(
                                    indexFilter,
                                    beatDistributionParam,
                                    beatDistributionParamType,
                                    rotationDistributionParam,
                                    rotationDistributionParamType,
                                    rotationDistributionShouldAffectFirstBaseEvent,
                                    rotationDistributionEaseType,
                                    axis,
                                    flipRotation,
                                    lightRotationBaseDataList)));
                            });
                            break;

                        case _customData:
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new LightRotationEventBoxGroupSaveData(beat, groupId, eventBoxes, data)));
            });
        }

        // TODO: figure out custom data for event boxes and co.
        public static void DeserializeLightTranslationEventBoxGroupArray(JsonReader reader, List<LightTranslationEventBoxGroup> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                List<LightTranslationEventBox> eventBoxes = new();
                int groupId = default;
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _groupId:
                            groupId = reader.ReadAsInt32Safe() ?? groupId;
                            break;

                        case _eventBoxes:
                            reader.ReadArray(() =>
                            {
                                _IndexFilter? indexFilter = default;
                                float beatDistributionParam = default;
                                _DistributionParamType beatDistributionParamType = default;
                                float gapDistributionParam = default;
                                _DistributionParamType gapDistributionParamType = default;
                                bool gapDistributionShouldAffectFirstBaseEvent = default;
                                _EaseType gapDistributionEaseType = default;
                                _Axis axis = default;
                                bool flipRotation = default;
                                List<_LightTranslationBaseData> lightTranslationBaseDataList = new();
                                return reader.ReadObject(eventName =>
                                {
                                    switch (eventName)
                                    {
                                        case _indexFilter:
                                            indexFilter = DeserializeIndexFilter(reader);
                                            break;

                                        case _beatDistributionParam:
                                            beatDistributionParam =
                                                (float?)reader.ReadAsDouble() ?? beatDistributionParam;
                                            break;

                                        case _beatDistributionParamType:
                                            beatDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                beatDistributionParamType;
                                            break;

                                        case "s":
                                            gapDistributionParam =
                                                (float?)reader.ReadAsDouble() ?? gapDistributionParam;
                                            break;

                                        case "t":
                                            gapDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                gapDistributionParamType;
                                            break;

                                        case "b":
                                            gapDistributionShouldAffectFirstBaseEvent = reader.ReadIntAsBoolean() ??
                                                gapDistributionShouldAffectFirstBaseEvent;
                                            break;

                                        case "a":
                                            axis = (_Axis?)reader.ReadAsInt32Safe() ?? axis;
                                            break;

                                        case "r":
                                            flipRotation = reader.ReadIntAsBoolean() ?? flipRotation;
                                            break;

                                        case "i":
                                            gapDistributionEaseType = (_EaseType?)reader.ReadAsInt32() ??
                                                                      gapDistributionEaseType;
                                            break;

                                        case "l":
                                            reader.ReadArray(() =>
                                            {
                                                float lightBeat = default;
                                                bool usePreviousEventTransitionValue = default;
                                                _EaseType easeType = default;
                                                float translation = default;
                                                return reader.ReadObject(lightName =>
                                                {
                                                    switch (lightName)
                                                    {
                                                        case _beat:
                                                            lightBeat = (float?)reader.ReadAsDouble() ?? lightBeat;
                                                            break;

                                                        case "p":
                                                            usePreviousEventTransitionValue =
                                                                reader.ReadIntAsBoolean() ??
                                                                usePreviousEventTransitionValue;
                                                            break;

                                                        case "e":
                                                            easeType = (_EaseType?)reader.ReadAsInt32Safe() ?? easeType;
                                                            break;

                                                        case "t":
                                                            translation = (float?)reader.ReadAsDouble() ?? translation;
                                                            break;

                                                        default:
                                                            reader.Skip();
                                                            break;
                                                    }
                                                }).Finish(() => lightTranslationBaseDataList.Add(new _LightTranslationBaseData(
                                                    lightBeat,
                                                    usePreviousEventTransitionValue,
                                                    easeType,
                                                    translation)));
                                            });
                                            break;

                                        default:
                                            reader.Skip();
                                            break;
                                    }
                                }).Finish(() => eventBoxes.Add(new LightTranslationEventBox(
                                    indexFilter,
                                    beatDistributionParam,
                                    beatDistributionParamType,
                                    gapDistributionParam,
                                    gapDistributionParamType,
                                    gapDistributionShouldAffectFirstBaseEvent,
                                    gapDistributionEaseType,
                                    axis,
                                    flipRotation,
                                    lightTranslationBaseDataList)));
                            });
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new LightTranslationEventBoxGroup(beat, groupId, eventBoxes)));
            });
        }

        // TODO: Make this file not 1 billion lines long
#if !V1_29_1
        public static void DeserializeFxEventBoxGroupArray(JsonReader reader, List<FxEventBoxGroup> list)
        {
            reader.ReadArray(() =>
            {
                float beat = default;
                List<FxEventBox> eventBoxes = new();
                int groupId = default;
                FxEventType type = default;
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case _beat:
                            beat = (float?)reader.ReadAsDouble() ?? beat;
                            break;

                        case _groupId:
                            groupId = reader.ReadAsInt32Safe() ?? groupId;
                            break;

                        case _eventBoxes:
                            reader.ReadArray(() =>
                            {
                                _IndexFilter? indexFilter = default;
                                float beatDistributionParam = default;
                                _DistributionParamType beatDistributionParamType = default;
                                float vfxDistributionParam = default;
                                _DistributionParamType vfxDistributionParamType = default;
                                bool vfxDistributionShouldAffectFirstBaseEvent = default;
                                _EaseType vfxDistributionEaseType = default;
                                int[] vfxBaseDataList = Array.Empty<int>();
                                return reader.ReadObject(eventName =>
                                {
                                    switch (eventName)
                                    {
                                        case _indexFilter:
                                            indexFilter = DeserializeIndexFilter(reader);
                                            break;

                                        case _beatDistributionParam:
                                            beatDistributionParam =
                                                (float?)reader.ReadAsDouble() ?? beatDistributionParam;
                                            break;

                                        case _beatDistributionParamType:
                                            beatDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                beatDistributionParamType;
                                            break;

                                        case "s":
                                            vfxDistributionParam =
                                                (float?)reader.ReadAsDouble() ?? vfxDistributionParam;
                                            break;

                                        case "t":
                                            vfxDistributionParamType =
                                                (_DistributionParamType?)reader.ReadAsInt32Safe() ??
                                                vfxDistributionParamType;
                                            break;

                                        case "b":
                                            vfxDistributionShouldAffectFirstBaseEvent = reader.ReadIntAsBoolean() ??
                                                vfxDistributionShouldAffectFirstBaseEvent;
                                            break;

                                        case "i":
                                            vfxDistributionEaseType = (_EaseType?)reader.ReadAsInt32() ??
                                                                      vfxDistributionEaseType;
                                            break;

                                        case "l":
                                            vfxBaseDataList = reader.ReadAsIntArray();
                                            break;

                                        default:
                                            reader.Skip();
                                            break;
                                    }
                                }).Finish(() => eventBoxes.Add(new FxEventBox(
                                    indexFilter,
                                    beatDistributionParam,
                                    beatDistributionParamType,
                                    vfxDistributionParam,
                                    vfxDistributionParamType,
                                    vfxDistributionEaseType,
                                    vfxDistributionShouldAffectFirstBaseEvent,
                                    vfxBaseDataList.ToList())));
                            });
                            break;

                        case "t":
                            type = (FxEventType?)reader.ReadAsInt32Safe() ?? type;
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new FxEventBoxGroup(beat, groupId, type, eventBoxes)));
            });
        }

        public static FxEventsCollection DeserializeFxEventCollection(JsonReader reader)
        {
            FxEventsCollection result = new();
            List<IntFxEventBaseData> intEventsList = result._il;
            List<FloatFxEventBaseData> floatEventsList = result._fl;
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case "_il":
                        reader.ReadArray(() =>
                        {
                            float beat = default;
                            bool usePreviousEventValue = default;
                            int value = default;
                            return reader.ReadObject(keywordName =>
                            {
                                switch (keywordName)
                                {
                                    case _beat:
                                        beat = (float?)reader.ReadAsDouble() ?? beat;
                                        break;

                                    case "p":
                                        usePreviousEventValue = reader.ReadIntAsBoolean() ?? usePreviousEventValue;
                                        break;

                                    case "v":
                                        value = reader.ReadAsInt32Safe() ?? value;
                                        break;

                                    default:
                                        reader.Skip();
                                        break;
                                }
                            }).Finish(() => intEventsList.Add(new IntFxEventBaseData(beat, value)
                                {
                                    p = usePreviousEventValue ? 1 : 0 // missing in constructor
                                }));
                        });

                        break;

                    case "_fl":
                        reader.ReadArray(() =>
                        {
                            float beat = default;
                            bool usePreviousEventValue = default;
                            float value = default;
                            _EaseType easeType = default;
                            return reader.ReadObject(keywordName =>
                            {
                                switch (keywordName)
                                {
                                    case _beat:
                                        beat = (float?)reader.ReadAsDouble() ?? beat;
                                        break;

                                    case "p":
                                        usePreviousEventValue = reader.ReadIntAsBoolean() ?? usePreviousEventValue;
                                        break;

                                    case "v":
                                        value = (float?)reader.ReadAsDouble() ?? value;
                                        break;

                                    case "i":
                                        easeType = (_EaseType?)reader.ReadAsInt32() ??
                                                   easeType;
                                        break;

                                    default:
                                        reader.Skip();
                                        break;
                                }
                            }).Finish(() => floatEventsList.Add(new FloatFxEventBaseData(beat, usePreviousEventValue, value, easeType)));
                        });

                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return result;
        }
#endif

        public static void DeserializeBasicEventTypesForKeywordArray(JsonReader reader, List<_BasicEventTypesWithKeywords.BasicEventTypesForKeyword> list)
        {
            reader.ReadArray(() =>
            {
                string keyword = string.Empty;
                List<_BeatmapEventType> eventTypes = new();
                return reader.ReadObject(keywordName =>
                {
                    switch (keywordName)
                    {
                        case "k":
                            keyword = reader.ReadAsString() ?? keyword;
                            break;

                        case "e":
                            reader.Read();
                            if (reader.TokenType != JsonToken.StartArray)
                            {
                                throw new JsonSerializationException("[e] was not array.");
                            }

                            while (true)
                            {
                                int? specialEvent = reader.ReadAsInt32Safe();
                                if (specialEvent.HasValue)
                                {
                                    eventTypes.Add((_BeatmapEventType)specialEvent);
                                }
                                else
                                {
                                    if (reader.TokenType == JsonToken.EndArray)
                                    {
                                        break;
                                    }

                                    throw new JsonSerializationException("Value in [e] was not int.");
                                }
                            }

                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new _BasicEventTypesWithKeywords.BasicEventTypesForKeyword(keyword, eventTypes)));
            });
        }

        public readonly struct SaveDataCustomDatas
        {
            internal SaveDataCustomDatas(
                CustomData beatmapCustomData,
                CustomData levelCustomData,
                CustomData customData)
            {
                this.beatmapCustomData = beatmapCustomData;
                this.levelCustomData = levelCustomData;
                this.customData = customData;
            }

            [PublicAPI]
            public CustomData beatmapCustomData { get; }

            [PublicAPI]
            public CustomData levelCustomData { get; }

            [PublicAPI]
            public CustomData customData { get; }
        }
    }
}
