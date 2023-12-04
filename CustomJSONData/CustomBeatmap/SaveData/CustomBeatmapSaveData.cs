using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapSaveDataVersion3;
using HarmonyLib;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomJSONData.CustomBeatmap
{
    public partial class CustomBeatmapSaveData : BeatmapSaveData
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

        private static readonly Version _version2_6_0 = (Version)AccessTools.Field(typeof(BeatmapSaveData), "version2_6_0").GetValue(null);

        public CustomBeatmapSaveData(
            List<BeatmapSaveData.BpmChangeEventData> bpmEvents,
            List<BeatmapSaveData.RotationEventData> rotationEvents,
            List<BeatmapSaveData.ColorNoteData> colorNotes,
            List<BeatmapSaveData.BombNoteData> bombNotes,
            List<BeatmapSaveData.ObstacleData> obstacles,
            List<BeatmapSaveData.SliderData> sliders,
            List<BeatmapSaveData.BurstSliderData> burstSliders,
            List<BeatmapSaveData.WaypointData> waypoints,
            List<BeatmapSaveData.BasicEventData> basicBeatmapEvents,
            List<BeatmapSaveData.ColorBoostEventData> colorBoostBeatmapEvents,
            List<BeatmapSaveData.LightColorEventBoxGroup> lightColorEventBoxGroups,
            List<BeatmapSaveData.LightRotationEventBoxGroup> lightRotationEventBoxGroups,
            List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups,
            BasicEventTypesWithKeywords basicEventTypesWithKeywords,
            List<FxEventBoxGroup> vfxEventBoxGroups,
            FxEventsCollection fxEventsCollection,
            bool useNormalEventsAsCompatibleEvents,
            bool version2_6_0AndEarlier,
            List<CustomEventData> customEvents,
            CustomData customData,
            CustomData beatmapCustomData,
            CustomData levelCustomData)
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
                vfxEventBoxGroups,
                fxEventsCollection,
                basicEventTypesWithKeywords,
                useNormalEventsAsCompatibleEvents)
        {
            this.version2_6_0AndEarlier = version2_6_0AndEarlier;
            this.customEvents = customEvents;
            this.customData = customData;
            this.beatmapCustomData = beatmapCustomData;
            this.levelCustomData = levelCustomData;
        }

        public bool version2_6_0AndEarlier { get; }

        public List<CustomEventData> customEvents { get; }

        public CustomData customData { get; }

        public CustomData beatmapCustomData { get; }

        public CustomData levelCustomData { get; }

        public static CustomBeatmapSaveData Deserialize(
            string path,
            CustomData beatmapData,
            CustomData levelData)
        {
            Version version = GetVersionFromPath(path);

            if (version.CompareTo(_version2_6_0) <= 0)
            {
                return SaveData2_6_0Converter.Convert2_6_0AndEarlier(
                    version, path, beatmapData, levelData);
            }

            // lets do this
            List<BeatmapSaveData.BpmChangeEventData> bpmEvents = new();
            List<BeatmapSaveData.RotationEventData> rotationEvents = new();
            List<BeatmapSaveData.ColorNoteData> colorNotes = new();
            List<BeatmapSaveData.BombNoteData> bombNotes = new();
            List<BeatmapSaveData.ObstacleData> obstacles = new();
            List<BeatmapSaveData.SliderData> sliders = new();
            List<BeatmapSaveData.BurstSliderData> burstSliders = new();
            List<BeatmapSaveData.WaypointData> waypoints = new();
            List<BeatmapSaveData.BasicEventData> basicBeatmapEvents = new();
            List<BeatmapSaveData.ColorBoostEventData> colorBoostBeatmapEvents = new();
            List<BeatmapSaveData.LightColorEventBoxGroup> lightColorEventBoxGroups = new();
            List<BeatmapSaveData.LightRotationEventBoxGroup> lightRotationEventBoxGroups = new();
            List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups = new();
            List<BasicEventTypesWithKeywords.BasicEventTypesForKeyword> basicEventTypesForKeyword = new();
            List<FxEventBoxGroup> vfxEventBoxGroups = new();
            FxEventsCollection fxEventsCollection = new();
            bool useNormalEventsAsCompatibleEvents = default;
            CustomData data = new();
            List<CustomEventData> customEvents = new();

            using JsonTextReader reader = new(new StreamReader(path));

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
                vfxEventBoxGroups,
                fxEventsCollection,
                useNormalEventsAsCompatibleEvents,
                customEvents,
                new SaveDataCustomDatas(data, beatmapData, levelData)
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

                        case "bpmEvents":
                            reader.ReadObjectArray(() => bpmEvents.Add(DeserializeBpmChange(reader)));
                            break;

                        case "rotationEvents":
                            reader.ReadObjectArray(() => rotationEvents.Add(DeserializeRotation(reader)));
                            break;

                        case "colorNotes":
                            reader.ReadObjectArray(() => colorNotes.Add(DeserializeColorNote(reader)));
                            break;

                        case "bombNotes":
                            reader.ReadObjectArray(() => bombNotes.Add(DeserializeBombNote(reader)));
                            break;

                        case "obstacles":
                            reader.ReadObjectArray(() => obstacles.Add(DeserializeObstacle(reader)));
                            break;

                        case "sliders":
                            reader.ReadObjectArray(() => sliders.Add(DeserializeSlider(reader)));
                            break;

                        case "burstSliders":
                            reader.ReadObjectArray(() => burstSliders.Add(DeserializeBurstSlider(reader)));
                            break;

                        case "waypoints":
                            reader.ReadObjectArray(() => waypoints.Add(DeserializeWaypoint(reader)));
                            break;

                        case "basicBeatmapEvents":
                            reader.ReadObjectArray(() => basicBeatmapEvents.Add(DeserializeBasicEvent(reader)));
                            break;

                        case "colorBoostBeatmapEvents":
                            reader.ReadObjectArray(() => colorBoostBeatmapEvents.Add(DeserializeColorBoost(reader)));
                            break;

                        case "lightColorEventBoxGroups":
                            reader.ReadObjectArray(() => lightColorEventBoxGroups.Add(DeserializeLightColorEventBoxGroup(reader)));
                            break;

                        case "lightRotationEventBoxGroups":
                            reader.ReadObjectArray(() => lightRotationEventBoxGroups.Add(DeserializeLightRotationEventBoxGroup(reader)));
                            break;

                        case "lightTranslationEventBoxGroups":
                            reader.ReadObjectArray(() => lightTranslationEventBoxGroups.Add(DeserializeLightTranslationEventBoxGroup(reader)));
                            break;

                        case "basicEventTypesWithKeywords":
                            reader.Read();
                            reader.ReadObject(objectName =>
                            {
                                switch (objectName)
                                {
                                    case "d":
                                        reader.ReadObjectArray(() => basicEventTypesForKeyword.Add(DeserializeBasicEventTypesForKeyword(reader)));

                                        break;

                                    default:
                                        reader.Skip();
                                        break;
                                }
                            });
                            break;

                        // TODO: Need to add code to actually handle the vfxEventBoxGroups and fxEventsCollection
                        case "vfxEventBoxGroups":
                            throw new NotImplementedException();
                            break;

                        // TODO: Need to add code to actually handle the vfxEventBoxGroups and fxEventsCollection
                        case "fxEventsCollection":
                            throw new NotImplementedException();
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

                                reader.ReadObjectArray(() => customEvents.Add(DeserializeCustomEvent(reader)), false);
                                return false;
                            });

                            break;
                    }
                }
            }

            return new CustomBeatmapSaveData(
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
                new BasicEventTypesWithKeywords(basicEventTypesForKeyword),
                vfxEventBoxGroups.OrderBy(n => n).ToList(),
                fxEventsCollection,
                useNormalEventsAsCompatibleEvents,
                false,
                customEvents.OrderBy(n => n).ToList(),
                data,
                beatmapData,
                levelData);
        }

        public static Version GetVersionFromPath(string path)
        {
            // SongCore has a fallback so i guess i do too
            const string fallback = "2.0.0";

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
                            return new Version(reader.ReadAsString()!); // ctor has null check
                    }
                }
            }

            Logger.Log($"[{path}] does not contain a version, falling back to [{fallback}].");
            return new Version(fallback);
        }

        public static CustomEventData DeserializeCustomEvent([InstantHandle] JsonReader reader)
        {
            float beat = default;
            string type = string.Empty;
            CustomData data = new();
            reader.ReadObject(
                objectName =>
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
            },
                false);

            return new CustomEventData(beat, type, data);
        }

        public static BpmChangeEventData DeserializeBpmChange([InstantHandle] JsonReader reader)
        {
            float beat = default;
            float bpm = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
            });

            return new BpmChangeEventData(beat, bpm, data);
        }

        public static RotationEventData DeserializeRotation([InstantHandle] JsonReader reader)
        {
            float beat = default;
            ExecutionTime executionTime = default;
            float rotation = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case _beat:
                        beat = (float?)reader.ReadAsDouble() ?? beat;
                        break;

                    case "e":
                        executionTime = (ExecutionTime?)reader.ReadAsInt32Safe() ?? executionTime;
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
            });

            return new RotationEventData(beat, executionTime, rotation, data);
        }

        public static ColorNoteData DeserializeColorNote([InstantHandle] JsonReader reader)
        {
            float beat = default;
            int line = default;
            int layer = default;
            NoteColorType color = default;
            NoteCutDirection cutDirection = default;
            int angleOffset = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
                        color = (NoteColorType?)reader.ReadAsInt32Safe() ?? color;
                        break;

                    case _cutDirection:
                        cutDirection = (NoteCutDirection?)reader.ReadAsInt32Safe() ?? cutDirection;
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
            });

            return new ColorNoteData(beat, line, layer, color, cutDirection, angleOffset, data);
        }

        public static BombNoteData DeserializeBombNote([InstantHandle] JsonReader reader)
        {
            float beat = default;
            int line = default;
            int layer = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
            });

            return new BombNoteData(beat, line, layer, data);
        }

        public static ObstacleData DeserializeObstacle([InstantHandle] JsonReader reader)
        {
            float beat = default;
            int line = default;
            int layer = default;
            float duration = default;
            int width = default;
            int height = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
            });

            return new ObstacleData(beat, line, layer, duration, width, height, data);
        }

        public static SliderData DeserializeSlider([InstantHandle] JsonReader reader)
        {
            NoteColorType color = default;
            float headBeat = default;
            int headLine = default;
            int headLayer = default;
            float headControlPointLengthMultiplier = default;
            NoteCutDirection headCutDirection = default;
            float tailBeat = default;
            int tailLine = default;
            int tailLayer = default;
            float tailControlPointLengthMultiplier = default;
            NoteCutDirection tailCutDirection = default;
            SliderMidAnchorMode sliderMidAnchorMode = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case _colorType:
                        color = (NoteColorType?)reader.ReadAsInt32Safe() ?? color;
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
                        headControlPointLengthMultiplier = (float?)reader.ReadAsDouble() ?? headControlPointLengthMultiplier;
                        break;

                    case _cutDirection:
                        headCutDirection = (NoteCutDirection?)reader.ReadAsInt32Safe() ?? headCutDirection;
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
                        tailControlPointLengthMultiplier = (float?)reader.ReadAsDouble() ?? tailControlPointLengthMultiplier;
                        break;

                    case "tc":
                        tailCutDirection = (NoteCutDirection?)reader.ReadAsInt32Safe() ?? tailCutDirection;
                        break;

                    case "m":
                        sliderMidAnchorMode = (SliderMidAnchorMode?)reader.ReadAsInt32Safe() ?? sliderMidAnchorMode;
                        break;

                    case _customData:
                        reader.ReadToDictionary(data);
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return new SliderData(
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
                data);
        }

        public static BurstSliderData DeserializeBurstSlider([InstantHandle] JsonReader reader)
        {
            NoteColorType color = default;
            float headBeat = default;
            int headLine = default;
            int headLayer = default;
            NoteCutDirection headCutDirection = default;
            float tailBeat = default;
            int tailLine = default;
            int tailLayer = default;
            int sliceCount = default;
            float squishAmount = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case _colorType:
                        color = (NoteColorType?)reader.ReadAsInt32Safe() ?? color;
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
                        headCutDirection = (NoteCutDirection?)reader.ReadAsInt32Safe() ?? headCutDirection;
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
            });

            return new BurstSliderData(
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
                data);
        }

        public static WaypointData DeserializeWaypoint([InstantHandle] JsonReader reader)
        {
            float beat = default;
            int line = default;
            int layer = default;
            OffsetDirection offsetDirection = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
                        offsetDirection = (OffsetDirection?)reader.ReadAsInt32Safe() ?? offsetDirection;
                        break;

                    case _customData:
                        reader.ReadToDictionary(data);
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return new WaypointData(beat, line, layer, offsetDirection, data);
        }

        public static BasicEventData DeserializeBasicEvent([InstantHandle] JsonReader reader)
        {
            float beat = default;
            BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType eventType = default;
            int value = default;
            float floatValue = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case _beat:
                        beat = (float?)reader.ReadAsDouble() ?? beat;
                        break;

                    case "et":
                        eventType = (BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType?)reader.ReadAsInt32Safe() ?? eventType;
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
            });

            return new BasicEventData(beat, eventType, value, floatValue, data);
        }

        public static ColorBoostEventData DeserializeColorBoost([InstantHandle] JsonReader reader)
        {
            float beat = default;
            bool boost = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
            });

            return new ColorBoostEventData(beat, boost, data);
        }

        public static IndexFilter DeserializeIndexFilter([InstantHandle] JsonReader reader)
        {
            IndexFilter.IndexFilterType type = default;
            int param0 = default;
            int param1 = default;
            bool reversed = default;
            IndexFilterRandomType random = default;
            int seed = default;
            int chunks = default;
            float limit = default;
            IndexFilterLimitAlsoAffectsType limitAlsoAffectsType = default;
            reader.Read();
            reader.ReadObject(objectName =>
            {
                switch (objectName)
                {
                    case "f":
                        type = (IndexFilter.IndexFilterType?)reader.ReadAsInt32Safe() ?? type;
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
                        limitAlsoAffectsType = (IndexFilterLimitAlsoAffectsType?)reader.ReadAsInt32Safe() ?? limitAlsoAffectsType;
                        break;

                    case "n":
                        random = (IndexFilterRandomType?)reader.ReadAsInt32Safe() ?? random;
                        break;

                    case "s":
                        seed = reader.ReadAsInt32Safe() ?? seed;
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return new IndexFilter(type, param0, param1, reversed, random, seed, chunks, limit, limitAlsoAffectsType);
        }

        public static LightColorEventBoxGroup DeserializeLightColorEventBoxGroup([InstantHandle] JsonReader reader)
        {
            float beat = default;
            List<LightColorEventBox> eventBoxes = new();
            int groupId = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
                        reader.ReadObjectArray(() =>
                        {
                            IndexFilter? indexFilter = default;
                            float beatDistributionParam = default;
                            EventBox.DistributionParamType beatDistributionParamType = default;
                            float brightnessDistributionParam = default;
                            bool brightnessDistributionShouldAffectFirstBaseEvent = default;
                            EventBox.DistributionParamType brightnessDistributionParamType = default;
                            EaseType brightnessDistributionEaseType = default;
                            List<LightColorBaseData> lightColorBaseDataList = new();
                            reader.ReadObject(eventName =>
                            {
                                switch (eventName)
                                {
                                    case _indexFilter:
                                        indexFilter = DeserializeIndexFilter(reader);
                                        break;

                                    case _beatDistributionParam:
                                        beatDistributionParam = (float?)reader.ReadAsDouble() ?? beatDistributionParam;
                                        break;

                                    case _beatDistributionParamType:
                                        beatDistributionParamType = (EventBox.DistributionParamType?)reader.ReadAsInt32Safe() ?? beatDistributionParamType;
                                        break;

                                    case "r":
                                        brightnessDistributionParam = (float?)reader.ReadAsDouble() ?? brightnessDistributionParam;
                                        break;

                                    case "b":
                                        brightnessDistributionShouldAffectFirstBaseEvent = reader.ReadIntAsBoolean() ?? brightnessDistributionShouldAffectFirstBaseEvent;
                                        break;

                                    case "t":
                                        brightnessDistributionParamType = (EventBox.DistributionParamType?)reader.ReadAsInt32Safe() ?? brightnessDistributionParamType;
                                        break;

                                    case "i":
                                        brightnessDistributionEaseType = (EaseType?)reader.ReadAsInt32Safe() ?? brightnessDistributionEaseType;
                                        break;

                                    case "e":
                                        reader.ReadObjectArray(() =>
                                        {
                                            float lightBeat = default;
                                            TransitionType transitionType = default;
                                            EnvironmentColorType colorType = default;
                                            float brightness = default;
                                            int strobeFrequency = default;
                                            reader.ReadObject(lightName =>
                                            {
                                                switch (lightName)
                                                {
                                                    case _beat:
                                                        lightBeat = (float?)reader.ReadAsDouble() ?? lightBeat;
                                                        break;

                                                    case "i":
                                                        transitionType = (TransitionType?)reader.ReadAsInt32Safe() ?? transitionType;
                                                        break;

                                                    case _colorType:
                                                        colorType = (EnvironmentColorType?)reader.ReadAsInt32Safe() ?? colorType;
                                                        break;

                                                    case "s":
                                                        brightness = (float?)reader.ReadAsDouble() ?? brightness;
                                                        break;

                                                    case "f":
                                                        strobeFrequency = reader.ReadAsInt32Safe() ?? strobeFrequency;
                                                        break;

                                                    default:
                                                        reader.Skip();
                                                        break;
                                                }
                                            });

                                            lightColorBaseDataList.Add(new LightColorBaseData(
                                                lightBeat,
                                                transitionType,
                                                colorType,
                                                brightness,
                                                strobeFrequency));
                                        });
                                        break;

                                    default:
                                        reader.Skip();
                                        break;
                                }
                            });

                            eventBoxes.Add(new LightColorEventBox(
                                indexFilter,
                                beatDistributionParam,
                                beatDistributionParamType,
                                brightnessDistributionParam,
                                brightnessDistributionShouldAffectFirstBaseEvent,
                                brightnessDistributionParamType,
                                brightnessDistributionEaseType,
                                lightColorBaseDataList));
                        });
                        break;

                    case _customData:
                        reader.ReadToDictionary(data);
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return new LightColorEventBoxGroup(beat, groupId, eventBoxes, data);
        }

        public static LightRotationEventBoxGroup DeserializeLightRotationEventBoxGroup([InstantHandle] JsonReader reader)
        {
            float beat = default;
            List<LightRotationEventBox> eventBoxes = new();
            int groupId = default;
            CustomData data = new();
            reader.ReadObject(objectName =>
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
                        reader.ReadObjectArray(() =>
                        {
                            IndexFilter? indexFilter = default;
                            float beatDistributionParam = default;
                            EventBox.DistributionParamType beatDistributionParamType = default;
                            float rotationDistributionParam = default;
                            EventBox.DistributionParamType rotationDistributionParamType = default;
                            bool rotationDistributionShouldAffectFirstBaseEvent = default;
                            EaseType rotationDistributionEaseType = default;
                            Axis axis = default;
                            bool flipRotation = default;
                            List<LightRotationBaseData> lightRotationBaseDataList = new();
                            reader.ReadObject(eventName =>
                            {
                                switch (eventName)
                                {
                                    case _indexFilter:
                                        indexFilter = DeserializeIndexFilter(reader);
                                        break;

                                    case _beatDistributionParam:
                                        beatDistributionParam = (float?)reader.ReadAsDouble() ?? beatDistributionParam;
                                        break;

                                    case _beatDistributionParamType:
                                        beatDistributionParamType = (EventBox.DistributionParamType?)reader.ReadAsInt32Safe() ?? beatDistributionParamType;
                                        break;

                                    case "s":
                                        rotationDistributionParam = (float?)reader.ReadAsDouble() ?? rotationDistributionParam;
                                        break;

                                    case "t":
                                        rotationDistributionParamType = (EventBox.DistributionParamType?)reader.ReadAsInt32Safe() ?? rotationDistributionParamType;
                                        break;

                                    case "b":
                                        rotationDistributionShouldAffectFirstBaseEvent = reader.ReadIntAsBoolean() ?? rotationDistributionShouldAffectFirstBaseEvent;
                                        break;

                                    case "a":
                                        axis = (Axis?)reader.ReadAsInt32Safe() ?? axis;
                                        break;

                                    case "r":
                                        flipRotation = reader.ReadIntAsBoolean() ?? flipRotation;
                                        break;

                                    case "i":
                                        rotationDistributionEaseType = (EaseType?)reader.ReadAsInt32() ?? rotationDistributionEaseType;
                                        break;

                                    case "l":
                                        reader.ReadObjectArray(() =>
                                        {
                                            float lightBeat = default;
                                            bool usePreviousEventRotationValue = default;
                                            EaseType easeType = default;
                                            int loopsCount = default;
                                            float rotation = default;
                                            LightRotationBaseData.RotationDirection rotationDirection = default;
                                            reader.ReadObject(lightName =>
                                            {
                                                switch (lightName)
                                                {
                                                    case _beat:
                                                        lightBeat = (float?)reader.ReadAsDouble() ?? lightBeat;
                                                        break;

                                                    case "p":
                                                        usePreviousEventRotationValue = reader.ReadIntAsBoolean() ?? usePreviousEventRotationValue;
                                                        break;

                                                    case "e":
                                                        easeType = (EaseType?)reader.ReadAsInt32Safe() ?? easeType;
                                                        break;

                                                    case "l":
                                                        loopsCount = reader.ReadAsInt32Safe() ?? loopsCount;
                                                        break;

                                                    case "r":
                                                        rotation = (float?)reader.ReadAsDouble() ?? rotation;
                                                        break;

                                                    case "o":
                                                        rotationDirection = (LightRotationBaseData.RotationDirection?)reader.ReadAsInt32Safe() ?? rotationDirection;
                                                        break;

                                                    default:
                                                        reader.Skip();
                                                        break;
                                                }
                                            });

                                            lightRotationBaseDataList.Add(new LightRotationBaseData(
                                                lightBeat,
                                                usePreviousEventRotationValue,
                                                easeType,
                                                loopsCount,
                                                rotation,
                                                rotationDirection));
                                        });
                                        break;

                                    default:
                                        reader.Skip();
                                        break;
                                }
                            });

                            eventBoxes.Add(new LightRotationEventBox(
                                indexFilter,
                                beatDistributionParam,
                                beatDistributionParamType,
                                rotationDistributionParam,
                                rotationDistributionParamType,
                                rotationDistributionShouldAffectFirstBaseEvent,
                                rotationDistributionEaseType,
                                axis,
                                flipRotation,
                                lightRotationBaseDataList));
                        });
                        break;

                    case _customData:
                        reader.ReadToDictionary(data);
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return new LightRotationEventBoxGroup(beat, groupId, eventBoxes, data);
        }

        // TODO: figure out custom data for event boxes and co.
        public static LightTranslationEventBoxGroup DeserializeLightTranslationEventBoxGroup([InstantHandle] JsonReader reader)
        {
            float beat = default;
            List<LightTranslationEventBox> eventBoxes = new();
            int groupId = default;
            reader.ReadObject(objectName =>
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
                        reader.ReadObjectArray(() =>
                        {
                            IndexFilter? indexFilter = default;
                            float beatDistributionParam = default;
                            EventBox.DistributionParamType beatDistributionParamType = default;
                            float gapDistributionParam = default;
                            EventBox.DistributionParamType gapDistributionParamType = default;
                            bool gapDistributionShouldAffectFirstBaseEvent = default;
                            EaseType gapDistributionEaseType = default;
                            Axis axis = default;
                            bool flipRotation = default;
                            List<LightTranslationBaseData> lightTranslationBaseDataList = new();
                            reader.ReadObject(eventName =>
                            {
                                switch (eventName)
                                {
                                    case _indexFilter:
                                        indexFilter = DeserializeIndexFilter(reader);
                                        break;

                                    case _beatDistributionParam:
                                        beatDistributionParam = (float?)reader.ReadAsDouble() ?? beatDistributionParam;
                                        break;

                                    case _beatDistributionParamType:
                                        beatDistributionParamType = (EventBox.DistributionParamType?)reader.ReadAsInt32Safe() ?? beatDistributionParamType;
                                        break;

                                    case "s":
                                        gapDistributionParam = (float?)reader.ReadAsDouble() ?? gapDistributionParam;
                                        break;

                                    case "t":
                                        gapDistributionParamType = (EventBox.DistributionParamType?)reader.ReadAsInt32Safe() ?? gapDistributionParamType;
                                        break;

                                    case "b":
                                        gapDistributionShouldAffectFirstBaseEvent = reader.ReadIntAsBoolean() ?? gapDistributionShouldAffectFirstBaseEvent;
                                        break;

                                    case "a":
                                        axis = (Axis?)reader.ReadAsInt32Safe() ?? axis;
                                        break;

                                    case "r":
                                        flipRotation = reader.ReadIntAsBoolean() ?? flipRotation;
                                        break;

                                    case "i":
                                        gapDistributionEaseType = (EaseType?)reader.ReadAsInt32() ?? gapDistributionEaseType;
                                        break;

                                    case "l":
                                        reader.ReadObjectArray(() =>
                                        {
                                            float lightBeat = default;
                                            bool usePreviousEventTransitionValue = default;
                                            EaseType easeType = default;
                                            float translation = default;
                                            reader.ReadObject(lightName =>
                                            {
                                                switch (lightName)
                                                {
                                                    case _beat:
                                                        lightBeat = (float?)reader.ReadAsDouble() ?? lightBeat;
                                                        break;

                                                    case "p":
                                                        usePreviousEventTransitionValue = reader.ReadIntAsBoolean() ?? usePreviousEventTransitionValue;
                                                        break;

                                                    case "e":
                                                        easeType = (EaseType?)reader.ReadAsInt32Safe() ?? easeType;
                                                        break;

                                                    case "t":
                                                        translation = (float?)reader.ReadAsDouble() ?? translation;
                                                        break;

                                                    default:
                                                        reader.Skip();
                                                        break;
                                                }
                                            });

                                            lightTranslationBaseDataList.Add(new LightTranslationBaseData(
                                                lightBeat,
                                                usePreviousEventTransitionValue,
                                                easeType,
                                                translation));
                                        });
                                        break;

                                    default:
                                        reader.Skip();
                                        break;
                                }
                            });

                            eventBoxes.Add(new LightTranslationEventBox(
                                indexFilter,
                                beatDistributionParam,
                                beatDistributionParamType,
                                gapDistributionParam,
                                gapDistributionParamType,
                                gapDistributionShouldAffectFirstBaseEvent,
                                gapDistributionEaseType,
                                axis,
                                flipRotation,
                                lightTranslationBaseDataList));
                        });
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            });

            return new LightTranslationEventBoxGroup(beat, groupId, eventBoxes);
        }

        public static BasicEventTypesWithKeywords.BasicEventTypesForKeyword DeserializeBasicEventTypesForKeyword(JsonReader reader)
        {
            string keyword = string.Empty;
            List<BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType> eventTypes = new();
            reader.ReadObject(keywordName =>
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
                                eventTypes.Add((BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType)specialEvent);
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
            });

            return new BasicEventTypesWithKeywords.BasicEventTypesForKeyword(keyword, eventTypes);
        }

        public readonly struct SaveDataCustomDatas
        {
            internal SaveDataCustomDatas(
                CustomData customData,
                CustomData beatmapCustomData,
                CustomData levelCustomData)
            {
                this.customData = customData;
                this.beatmapCustomData = beatmapCustomData;
                this.levelCustomData = levelCustomData;
            }

            [PublicAPI]
            public CustomData customData { get; }

            [PublicAPI]
            public CustomData beatmapCustomData { get; }

            [PublicAPI]
            public CustomData levelCustomData { get; }
        }
    }
}
