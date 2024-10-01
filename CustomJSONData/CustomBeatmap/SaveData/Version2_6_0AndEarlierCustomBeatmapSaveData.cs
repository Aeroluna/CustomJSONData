using System.Collections.Generic;
using System.IO;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using Newtonsoft.Json;
#if !PRE_V1_37_1
using _BeatmapEventType = BeatmapSaveDataCommon.BeatmapEventType;
using _ColorType = BeatmapSaveDataVersion2_6_0AndEarlier.ColorType;
using _EventData = BeatmapSaveDataVersion2_6_0AndEarlier.EventData;
using _NoteCutDirection = BeatmapSaveDataCommon.NoteCutDirection;
using _NoteData = BeatmapSaveDataVersion2_6_0AndEarlier.NoteData;
using _NoteLineLayer = BeatmapSaveDataCommon.NoteLineLayer;
using _NoteType = BeatmapSaveDataVersion2_6_0AndEarlier.NoteType;
using _ObstacleData = BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData;
using _ObstacleType = BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleType;
using _OffsetDirection = BeatmapSaveDataCommon.OffsetDirection;
using _SliderData = BeatmapSaveDataVersion2_6_0AndEarlier.SliderData;
using _SliderMidAnchorMode = BeatmapSaveDataCommon.SliderMidAnchorMode;
using _SpecialEventKeywordFiltersData = BeatmapSaveDataVersion2_6_0AndEarlier.SpecialEventKeywordFiltersData;
using _SpecialEventsForKeyword = BeatmapSaveDataVersion2_6_0AndEarlier.SpecialEventsForKeyword;
using _WaypointData = BeatmapSaveDataVersion2_6_0AndEarlier.WaypointData;
#else
using _BeatmapEventType = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType;
using _ColorType = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.ColorType;
using _EventData = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.EventData;
using _NoteCutDirection = NoteCutDirection;
using _NoteData = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.NoteData;
using _NoteLineLayer = NoteLineLayer;
using _NoteType = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.NoteType;
using _ObstacleData = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.ObstacleData;
using _ObstacleType = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.ObstacleType;
using _OffsetDirection = OffsetDirection;
using _SliderData = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.SliderData;
using _SliderMidAnchorMode = SliderMidAnchorMode;
using _SpecialEventKeywordFiltersData = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.SpecialEventKeywordFiltersData;
using _SpecialEventsForKeyword = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.SpecialEventsForKeyword;
using _WaypointData = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.WaypointData;
#endif

namespace CustomJSONData.CustomBeatmap
{
    public class Version2_6_0AndEarlierCustomBeatmapSaveData : BeatmapSaveData
    {
        private Version2_6_0AndEarlierCustomBeatmapSaveData(
            string version,
            List<_EventData> events,
            List<_NoteData> notes,
            List<_SliderData> sliders,
            List<_WaypointData> waypoints,
            List<_ObstacleData> obstacles,
            _SpecialEventKeywordFiltersData specialEventsKeywordFilters,
            CustomData customData,
            List<CustomEventSaveData> customEvents)
            : base(
                events,
                notes,
                sliders,
                waypoints,
                obstacles,
                specialEventsKeywordFilters)
        {
            _version = version;
            this.customData = customData;
            this.customEvents = customEvents;
        }

        public List<CustomEventSaveData> customEvents { get; }

        public CustomData customData { get; }

        public static Version2_6_0AndEarlierCustomBeatmapSaveData Deserialize(string path)
        {
            string version = string.Empty;
            CustomData customData = new();
            List<CustomEventSaveData> customEvents = new();
            List<_EventData> events = new();
            List<_NoteData> notes = new();
            List<_SliderData> sliders = new();
            List<_WaypointData> waypoints = new();
            List<_ObstacleData> obstacles = new();
            List<_SpecialEventsForKeyword> keywords = new();

#if !PRE_V1_37_1
            using JsonTextReader reader = new(new StringReader(path));
#else
            using JsonTextReader reader = new(new StreamReader(path));
#endif

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value)
                    {
                        default:
                            reader.Skip();
                            break;

                        case "_version":
                            version = reader.ReadAsString() ?? version;
                            break;

                        case "_events":
                            DeserializeEventArray(reader, events);
                            break;

                        case "_notes":
                            DeserializeNoteArray(reader, notes);
                            break;

                        case "_sliders":
                            DeserializeSliderArray(reader, sliders);
                            break;

                        case "_waypoints":
                            DeserializeWaypointArray(reader, waypoints);
                            break;

                        case "_obstacles":
                            DeserializeObstacleArray(reader, obstacles);
                            break;

                        case "_specialEventsKeywordFilters":
                            reader.ReadObject(propertyName =>
                            {
                                if (propertyName.Equals("_keywords"))
                                {
                                    DeserializeKeywordArray(reader, keywords);
                                }
                                else
                                {
                                    reader.Skip();
                                }
                            });

                            break;

                        case "_customData":
                            reader.ReadToDictionary(customData, propertyName =>
                            {
                                if (!propertyName.Equals("_customEvents"))
                                {
                                    return true;
                                }

                                DeserializeCustomEventArray(reader, customEvents);
                                return false;
                            });

                            break;
                    }
                }
            }

            Version2_6_0AndEarlierCustomBeatmapSaveData beatmapSaveData = new(
                version,
                events,
                notes,
                sliders,
                waypoints,
                obstacles,
                new _SpecialEventKeywordFiltersData(keywords),
                customData,
                customEvents);

            return beatmapSaveData;
        }

        public static void DeserializeEventArray(JsonReader reader, List<_EventData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                _BeatmapEventType type = default;
                int value = default;
                float floatValue = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_type":
                            type = (_BeatmapEventType?)reader.ReadAsInt32Safe() ?? type;
                            break;

                        case "_value":
                            value = reader.ReadAsInt32Safe() ?? value;
                            break;

                        case "_floatValue":
                            floatValue = (float?)reader.ReadAsDouble() ?? floatValue;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new EventSaveData(time, type, value, floatValue, data)));
            });
        }

        public static void DeserializeNoteArray(JsonReader reader, List<_NoteData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                int lineIndex = default;
                _NoteLineLayer lineLayer = default;
                _NoteType type = default;
                _NoteCutDirection cutDirection = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_lineIndex":
                            lineIndex = reader.ReadAsInt32Safe() ?? lineIndex;
                            break;

                        case "_lineLayer":
                            lineLayer = (_NoteLineLayer?)reader.ReadAsInt32Safe() ?? lineLayer;
                            break;

                        case "_type":
                            type = (_NoteType?)reader.ReadAsInt32Safe() ?? type;
                            break;

                        case "_cutDirection":
                            cutDirection = (_NoteCutDirection?)reader.ReadAsInt32Safe() ?? cutDirection;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new NoteSaveData(time, lineIndex, lineLayer, type, cutDirection, data)));
            });
        }

        public static void DeserializeSliderArray(JsonReader reader, List<_SliderData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                _ColorType colorType = default;
                int headLineIndex = default;
                _NoteLineLayer noteLineLayer = default;
                float headControlPointLengthMultiplier = default;
                _NoteCutDirection noteCutDirection = default;
                float tailTime = default;
                int tailLineIndex = default;
                _NoteLineLayer tailLineLayer = default;
                float tailControlPointLengthMultiplier = default;
                _NoteCutDirection tailCutDirection = default;
                _SliderMidAnchorMode sliderMidAnchorMode = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_colorType":
                            colorType = (_ColorType?)reader.ReadAsInt32Safe() ?? colorType;
                            break;

                        case "_headLineIndex":
                            headLineIndex = reader.ReadAsInt32Safe() ?? headLineIndex;
                            break;

                        case "_noteLineLayer":
                            noteLineLayer = (_NoteLineLayer?)reader.ReadAsInt32Safe() ?? noteLineLayer;
                            break;

                        case "_headControlPointLengthMultiplier":
                            headControlPointLengthMultiplier =
                                (float?)reader.ReadAsDouble() ?? headControlPointLengthMultiplier;
                            break;

                        case "_noteCutDirection":
                            noteCutDirection = (_NoteCutDirection?)reader.ReadAsInt32Safe() ?? noteCutDirection;
                            break;

                        case "_tailTime":
                            tailTime = (float?)reader.ReadAsDouble() ?? tailTime;
                            break;

                        case "_tailLineIndex":
                            tailLineIndex = reader.ReadAsInt32Safe() ?? tailLineIndex;
                            break;

                        case "_tailLineLayer":
                            tailLineLayer = (_NoteLineLayer?)reader.ReadAsInt32Safe() ?? tailLineLayer;
                            break;

                        case "_tailControlPointLengthMultiplier":
                            tailControlPointLengthMultiplier =
                                (float?)reader.ReadAsDouble() ?? tailControlPointLengthMultiplier;
                            break;

                        case "_tailCutDirection":
                            tailCutDirection = (_NoteCutDirection?)reader.ReadAsInt32Safe() ?? tailCutDirection;
                            break;

                        case "_sliderMidAnchorMode":
                            sliderMidAnchorMode = (_SliderMidAnchorMode?)reader.ReadAsInt32Safe() ?? sliderMidAnchorMode;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new SliderSaveData(
                    colorType,
                    time,
                    headLineIndex,
                    noteLineLayer,
                    headControlPointLengthMultiplier,
                    noteCutDirection,
                    tailTime,
                    tailLineIndex,
                    tailLineLayer,
                    tailControlPointLengthMultiplier,
                    tailCutDirection,
                    sliderMidAnchorMode,
                    data)));
            });
        }

        public static void DeserializeWaypointArray(JsonReader reader, List<_WaypointData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                int lineIndex = default;
                _NoteLineLayer lineLayer = default;
                _OffsetDirection offsetDirection = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_lineIndex":
                            lineIndex = reader.ReadAsInt32Safe() ?? lineIndex;
                            break;

                        case "_lineLayer":
                            lineLayer = (_NoteLineLayer?)reader.ReadAsInt32Safe() ?? lineLayer;
                            break;

                        case "_offsetDirection":
                            offsetDirection = (_OffsetDirection?)reader.ReadAsInt32Safe() ?? offsetDirection;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new WaypointSaveData(time, lineIndex, lineLayer, offsetDirection, data)));
            });
        }

        public static void DeserializeObstacleArray(JsonReader reader, List<_ObstacleData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                int lineIndex = default;
                _ObstacleType type = default;
                float duration = default;
                int width = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_lineIndex":
                            lineIndex = reader.ReadAsInt32Safe() ?? lineIndex;
                            break;

                        case "_type":
                            type = (_ObstacleType?)reader.ReadAsInt32Safe() ?? type;
                            break;

                        case "_duration":
                            duration = (float?)reader.ReadAsDouble() ?? duration;
                            break;

                        case "_width":
                            width = reader.ReadAsInt32Safe() ?? width;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new ObstacleSaveData(time, lineIndex, type, duration, width, data)));
            });
        }

        public static void DeserializeKeywordArray(JsonReader reader, List<_SpecialEventsForKeyword> list)
        {
            reader.ReadArray(() =>
            {
                string keyword = string.Empty;
                List<_BeatmapEventType> specialEvents = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_keyword":
                            keyword = reader.ReadAsString() ?? keyword;
                            break;

                        case "_specialEvents":
                            reader.Read();
                            if (reader.TokenType != JsonToken.StartArray)
                            {
                                throw new JsonSerializationException("_specialEvents was not array.");
                            }

                            while (true)
                            {
                                int? specialEvent = reader.ReadAsInt32Safe();
                                if (specialEvent.HasValue)
                                {
                                    specialEvents.Add((_BeatmapEventType)specialEvent);
                                }
                                else
                                {
                                    if (reader.TokenType == JsonToken.EndArray)
                                    {
                                        break;
                                    }

                                    throw new JsonSerializationException("Value in _specialEvents was not int.");
                                }
                            }

                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new _SpecialEventsForKeyword(keyword, specialEvents)));
            });
        }

        public static void DeserializeCustomEventArray(JsonReader reader, List<CustomEventSaveData> list)
        {
            reader.ReadArray(
                () =>
            {
                float time = default;
                string type = string.Empty;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_type":
                            type = reader.ReadAsString() ?? type;
                            break;

                        case "_data":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new CustomEventSaveData(time, type, data)));
            },
                false);
        }

        internal void ConvertBeatmapSaveDataPreV2_5_0()
        {
            List<_EventData> originalEvents = events;
            List<_EventData> list = new(originalEvents.Count);
            foreach (_EventData origEventData in originalEvents)
            {
                if (origEventData is not EventSaveData eventData)
                {
                    continue;
                }

                // breaks more maps than fixes
                /*if (eventData.type == _BeatmapEventType.Event10)
                {
                    eventData = new EventSaveData(eventData.time, _BeatmapEventType.BpmChange, eventData.value, eventData.floatValue, eventData.customData);
                }*/

                if (eventData.type == _BeatmapEventType.BpmChange)
                {
                    if (eventData.value != 0)
                    {
                        eventData = new EventSaveData(eventData.time, eventData.type, eventData.value, eventData.value, eventData.customData);
                    }
                }
                else
                {
                    eventData = new EventSaveData(eventData.time, eventData.type, eventData.value, 1f, eventData.customData);
                }

                list.Add(eventData);
            }

            _events = list;
        }

        public class EventSaveData : _EventData, ICustomData
        {
            internal EventSaveData(float time, _BeatmapEventType type, int value, float floatValue, CustomData customData)
                : base(time, type, value, floatValue)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class CustomEventSaveData : BeatmapSaveDataItem, ICustomData
        {
            internal CustomEventSaveData(float time, string type, CustomData data)
            {
                this.time = time;
                this.type = type;
                customData = data;
            }

            public override float time { get; }

            public string type { get; }

            public CustomData customData { get; }
        }

        public class NoteSaveData : _NoteData, ICustomData
        {
            internal NoteSaveData(float time, int lineIndex, _NoteLineLayer lineLayer, _NoteType type, _NoteCutDirection cutDirection, CustomData customData)
                : base(time, lineIndex, lineLayer, type, cutDirection)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class SliderSaveData : _SliderData, ICustomData
        {
            internal SliderSaveData(
                _ColorType colorType,
                float headTime,
                int headLineIndex,
                _NoteLineLayer headLineLayer,
                float headControlPointLengthMultiplier,
                _NoteCutDirection headCutDirection,
                float tailTime,
                int tailLineIndex,
                _NoteLineLayer tailLineLayer,
                float tailControlPointLengthMultiplier,
                _NoteCutDirection tailCutDirection,
                _SliderMidAnchorMode sliderMidAnchorMode,
                CustomData customData)
                : base(
                    colorType,
                    headTime,
                    headLineIndex,
                    headLineLayer,
                    headControlPointLengthMultiplier,
                    headCutDirection,
                    tailTime,
                    tailLineIndex,
                    tailLineLayer,
                    tailControlPointLengthMultiplier,
                    tailCutDirection,
                    sliderMidAnchorMode)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class WaypointSaveData : _WaypointData, ICustomData
        {
            public WaypointSaveData(float time, int lineIndex, _NoteLineLayer lineLayer, _OffsetDirection offsetDirection, CustomData customData)
                : base(time, lineIndex, lineLayer, offsetDirection)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class ObstacleSaveData : _ObstacleData, ICustomData
        {
            public ObstacleSaveData(float time, int lineIndex, _ObstacleType type, float duration, int width, CustomData customData)
                : base(time, lineIndex, type, duration, width)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }
    }
}
