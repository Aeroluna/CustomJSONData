#if PRE_V1_37_1
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
#if !V1_29_1
using System.Diagnostics;
#endif

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    internal static class BeatmapDataLoader1_34_2AndEarlierCustomify
    {
        private static readonly ConstructorInfo _beatmapDataCtor = AccessTools.FirstConstructor(typeof(BeatmapData), _ => true);
        private static readonly ConstructorInfo _bpmChangeCtor = AccessTools.FirstConstructor(typeof(BPMChangeBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _bpmTimeProcessorCtor = AccessTools.FirstConstructor(typeof(BeatmapDataLoader.BpmTimeProcessor), _ => true);
        private static readonly MethodInfo _createCustomBeatmapData = AccessTools.Method(typeof(BeatmapDataLoader1_34_2AndEarlierCustomify), nameof(CreateCustomBeatmapData));
        private static readonly MethodInfo _createCustomBPMChangeData = AccessTools.Method(typeof(BeatmapDataLoader1_34_2AndEarlierCustomify), nameof(CreateCustomBPMChangeData));
        private static readonly MethodInfo _addCustomEvent = AccessTools.Method(typeof(BeatmapDataLoader1_34_2AndEarlierCustomify), nameof(AddCustomEvents));

        private static readonly MethodInfo _getBeatmapDataFromBeatmapSaveData = AccessTools.Method(typeof(BeatmapDataLoader), "GetBeatmapDataFromBeatmapSaveData");
        private static readonly MethodInfo _lockedMethod = AccessTools.Method(typeof(BeatmapDataLoader1_34_2AndEarlierCustomify), nameof(GetBeatmapDataLock));
        private static readonly object _lock = new();

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoader.GetBeatmapDataFromSaveData))]
        private static IEnumerable<CodeInstruction> LockTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                // ReSharper disable once InconsistentlySynchronizedField
                .MatchForward(false, new CodeMatch(OpCodes.Call, _getBeatmapDataFromBeatmapSaveData))
                .SetOperandAndAdvance(_lockedMethod)
                .InstructionEnumeration();
        }

        // TODO: figure out what causes a race condition in the first place.
        private static BeatmapData GetBeatmapDataLock(
            BeatmapSaveData beatmapSaveData,
            BeatmapDifficulty beatmapDifficulty,
            float startBpm,
            bool loadingForDesignatedEnvironment,
            EnvironmentKeywords environmentKeywords,
            EnvironmentLightGroups environmentLightGroups,
            DefaultEnvironmentEvents defaultEnvironmentEvents,
#if !V1_29_1
            BeatmapLightshowSaveData defaultLightshowEventsSaveData,
            PlayerSpecificSettings playerSpecificSettings,
            Stopwatch? stopwatch = null)
#else
            PlayerSpecificSettings playerSpecificSettings)
#endif
        {
            lock (_lock)
            {
                return (BeatmapData)_getBeatmapDataFromBeatmapSaveData.Invoke(
                    null,
                    new object?[]
                    {
                        beatmapSaveData, beatmapDifficulty, startBpm, loadingForDesignatedEnvironment, environmentKeywords, environmentLightGroups,
                        defaultEnvironmentEvents,
#if !V1_29_1
                        defaultLightshowEventsSaveData,
                        playerSpecificSettings,
                        stopwatch
#else
                        playerSpecificSettings
#endif
                    });
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoader.GetBeatmapDataFromBeatmapSaveData))]
        private static IEnumerable<CodeInstruction> Customify(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _beatmapDataCtor))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _createCustomBeatmapData)

                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _bpmChangeCtor))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _createCustomBPMChangeData)

                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _bpmTimeProcessorCtor))
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _addCustomEvent))

                // "convertor" is NOT the correct spelling
                .ReplaceConverter<DataConvertor<BeatmapObjectData>, Converters.CustomDataConverter<BeatmapObjectData>>()

                .ReplaceVersionableConverter<BeatmapDataLoader.ColorNoteConvertor, Converters.CustomColorNoteConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.BombNoteConvertor, Converters.CustomBombNoteConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.ObstacleConvertor, Converters.CustomObstacleConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.SliderConvertor, Converters.CustomSliderConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.BurstSliderConvertor, Converters.CustomBurstSliderConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.WaypointConvertor, Converters.CustomWaypointConverter>()

                .ReplaceVersionableConverter<DataConvertor<BeatmapEventData>, Converters.CustomDataConverter<BeatmapEventData>>()

                .ReplaceVersionableConverter<BeatmapDataLoader.BpmEventConvertor, Converters.CustomBpmEventConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.RotationEventConvertor, Converters.CustomRotationEventConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.BasicEventConvertor, Converters.CustomBasicEventConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.ColorBoostEventConvertor, Converters.CustomColorBoostEventConverter>()

                // for reasons beyond my understanding, the leave will still take you to the InsertDefaultEnvironmentEvents, but inserting a nop fixes it...
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Leave))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Nop))

                .InstructionEnumeration();
        }

        private static CodeMatcher ReplaceVersionableConverter<TOriginal, TCustom>(this CodeMatcher matcher)
        {
            ConstructorInfo original = AccessTools.FirstConstructor(typeof(TOriginal), _ => true);
            ConstructorInfo custom = AccessTools.FirstConstructor(typeof(TCustom), _ => true);

            return matcher.MatchForward(false, new CodeMatch(OpCodes.Newobj, original))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .SetOperandAndAdvance(custom);
        }

        private static CodeMatcher ReplaceConverter<TOriginal, TCustom>(this CodeMatcher matcher)
        {
            ConstructorInfo original = AccessTools.FirstConstructor(typeof(TOriginal), _ => true);
            ConstructorInfo custom = AccessTools.FirstConstructor(typeof(TCustom), _ => true);

            return matcher.MatchForward(false, new CodeMatch(OpCodes.Newobj, original))
                .SetOperandAndAdvance(custom);
        }

        private static BeatmapData CreateCustomBeatmapData(int numberOfLines, BeatmapSaveData beatmapSaveData)
        {
            if (beatmapSaveData is Version3CustomBeatmapSaveData customBeatmapSaveData)
            {
                return new CustomBeatmapData(
                    numberOfLines,
                    customBeatmapSaveData.beatmapCustomData,
                    customBeatmapSaveData.levelCustomData,
                    customBeatmapSaveData.customData,
                    customBeatmapSaveData.beatmapVersion);
            }

            return new CustomBeatmapData(
                4,
                new CustomData(),
                new CustomData(),
                new CustomData(),
                VersionExtensions.version3);
        }

        private static BPMChangeBeatmapEventData CreateCustomBPMChangeData(float time, float bpm, BeatmapSaveData beatmapSaveData)
        {
            return new CustomBPMChangeBeatmapEventData(
                time,
                bpm,
                new CustomData(),
                beatmapSaveData is Version3CustomBeatmapSaveData customBeatmapSaveData ? customBeatmapSaveData.beatmapVersion : VersionExtensions.version3);
        }

        private static void AddCustomEvents(
            BeatmapDataLoader.BpmTimeProcessor timeProcessor,
            CustomBeatmapData beatmapData,
            BeatmapSaveData saveData)
        {
            if (saveData is not Version3CustomBeatmapSaveData customSaveData)
            {
                return;
            }

            foreach (Version3CustomBeatmapSaveData.CustomEventSaveData customEventSaveData in customSaveData.customEvents.OrderBy(n => n))
            {
                beatmapData.InsertCustomEventData(new CustomEventData(
                    timeProcessor.ConvertBeatToTime(customEventSaveData.beat),
                    customEventSaveData.type,
                    customEventSaveData.customData,
                    customSaveData.beatmapVersion));
            }
        }
    }
}
#endif
