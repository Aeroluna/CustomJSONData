#if !PRE_V1_37_1
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch]
    internal static class ConvertersCustomify
    {
        private static readonly MethodInfo _getData = AccessTools.Method(typeof(ConvertersCustomify), nameof(GetData));

        private static readonly MethodInfo _createBasicNoteData = AccessTools.Method(typeof(NoteData), nameof(NoteData.CreateBasicNoteData));
        private static readonly MethodInfo _createCustomBasicNoteData = AccessTools.Method(typeof(CustomNoteData), nameof(CustomNoteData.CreateCustomBasicNoteData));

        private static readonly MethodInfo _createBombNoteData = AccessTools.Method(typeof(NoteData), nameof(NoteData.CreateBombNoteData));
        private static readonly MethodInfo _createCustomBombNoteData = AccessTools.Method(typeof(CustomNoteData), nameof(CustomNoteData.CreateCustomBombNoteData));

        private static readonly ConstructorInfo _obstacleDataCtor = AccessTools.FirstConstructor(typeof(ObstacleData), _ => true);
        private static readonly ConstructorInfo _customObstacleDataCtor = AccessTools.FirstConstructor(typeof(CustomObstacleData), _ => true);

        private static readonly MethodInfo _createSliderData = AccessTools.Method(typeof(SliderData), nameof(SliderData.CreateSliderData));
        private static readonly MethodInfo _createCustomSliderData = AccessTools.Method(typeof(CustomSliderData), nameof(CustomSliderData.CreateCustomSliderData));

        private static readonly MethodInfo _createBurstSliderData = AccessTools.Method(typeof(SliderData), nameof(SliderData.CreateBurstSliderData));
        private static readonly MethodInfo _createCustomBurstSliderData = AccessTools.Method(typeof(CustomSliderData), nameof(CustomSliderData.CreateCustomBurstSliderData));

        private static readonly ConstructorInfo _waypointCtor = AccessTools.FirstConstructor(typeof(WaypointData), _ => true);
        private static readonly ConstructorInfo _customWaypointCtor = AccessTools.FirstConstructor(typeof(CustomWaypointData), _ => true);

        private static readonly ConstructorInfo _bpmEventCtor = AccessTools.FirstConstructor(typeof(BPMChangeBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customBpmEventCtor = AccessTools.FirstConstructor(typeof(CustomBPMChangeBeatmapEventData), _ => true);

#if PRE_V1_39_1
        private static readonly ConstructorInfo _rotationEventCtor = AccessTools.FirstConstructor(typeof(SpawnRotationBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customRotationEventCtor = AccessTools.FirstConstructor(typeof(CustomSpawnRotationBeatmapEventdata), _ => true);
#endif

        private static readonly ConstructorInfo _basicEventCtor = AccessTools.FirstConstructor(typeof(BasicBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customBasicEventCtor = AccessTools.FirstConstructor(typeof(CustomBasicBeatmapEventData), _ => true);

        private static readonly ConstructorInfo _colorBoostEventCtor = AccessTools.FirstConstructor(typeof(ColorBoostBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customColorBoostEventCtor = AccessTools.FirstConstructor(typeof(CustomColorBoostBeatmapEventData), _ => true);

        private static readonly FieldInfo _version2 = AccessTools.Field(typeof(BeatmapSaveDataHelpers), nameof(BeatmapSaveDataHelpers.version2));
        private static readonly FieldInfo _version3 = AccessTools.Field(typeof(BeatmapSaveDataHelpers), nameof(BeatmapSaveDataHelpers.version3));

        // V3 GLS color custom data: why we can't use the standard ReplaceCtor transpiler pattern
        // -----------------------------------------------------------------------------------------
        // For simple converters (BasicEventConverter, ObstacleConverter, etc.) the standard
        // ReplaceCtor transpiler works because:
        //   - Convert(SaveDataItem saveData) takes the ICustomData item as Ldarg_1
        //   - The method directly calls `newobj XxxBeatmapEventData(...)` so we can insert
        //     Ldarg_1.GetData() + version before the newobj.
        //
        // For V3 GLS, LightColorBeatmapEventData instances are NOT created in
        // LightColorEventBoxConverter.Convert. Instead:
        //   1. LightColorEventBoxConverter.Convert(LightColorEventBox, ILightGroup)
        //      calls LightColoBaseDataConvertor.Convert on each ICustomData save-data item,
        //      which returns plain LightColorBaseData game-structs (custom data is dropped).
        //   2. The structs are stored in a LightColorBeatmapEventDataBox._lightColorBaseDataList.
        //   3. LightColorBeatmapEventDataBox.Unpack(...) is called much later (at play time, once
        //      per event during gameplay) and creates LightColorBeatmapEventData items from those
        //      plain structs via IBeatmapLightEventConverter. At that point Ldarg_1 is `groupBoxBeat`
        //      (a float), not the save data - so there is nothing carrying ICustomData.
        //
        // Solution: a two-postfix ConditionalWeakTable approach (same motivation as the
        // ObstacleConvertV2_6_0AndEarlier prefix override):
        //   - Postfix LightColorEventBoxConverter.Convert to read ICustomData from the original
        //     save data list (before the structs are stripped) and store per-index CustomData
        //     in a ConditionalWeakTable keyed by the returned LightColorBeatmapEventDataBox.
        //   - Postfix LightColorBeatmapEventDataBox.Unpack to look up the box in the table
        //     and replace output LightColorBeatmapEventData items with CustomLightColorBeatmapEventData.
        // Maps each LightColorBeatmapEventDataBox instance → ordered list of per-event CustomData
        // (null entry = no custom data for that event index).
        private static readonly ConditionalWeakTable<LightColorBeatmapEventDataBox, List<CustomData?>> _boxCustomData
            = new();

        private static CustomData GetData(this IBeat dataItem)
        {
            return dataItem is ICustomData customData
                ? customData.customData : new CustomData();
        }

        private static IEnumerable<CodeInstruction> ReplaceMethod(
            this IEnumerable<CodeInstruction> instructions,
            FieldInfo field,
            MethodInfo original,
            MethodInfo replace)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, original))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, field))
                .SetOperandAndAdvance(replace)
                .InstructionEnumeration();
        }

        private static IEnumerable<CodeInstruction> ReplaceCtor(
            this IEnumerable<CodeInstruction> instructions,
            FieldInfo field,
            ConstructorInfo original,
            ConstructorInfo replace)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, original))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, field))
                .SetOperandAndAdvance(replace)
                .InstructionEnumeration();
        }

        // VERSION 3
        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorNoteConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorNoteConverter.Convert))]
        private static IEnumerable<CodeInstruction> ColorNoteConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createBasicNoteData, _createCustomBasicNoteData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BombNoteConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BombNoteConverter.Convert))]
        private static IEnumerable<CodeInstruction> BombNoteConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createBombNoteData, _createCustomBombNoteData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ObstacleConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ObstacleConverter.Convert))]
        private static IEnumerable<CodeInstruction> ObstacleConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _obstacleDataCtor, _customObstacleDataCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.SliderConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.SliderConverter.Convert))]
        private static IEnumerable<CodeInstruction> SliderConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createSliderData, _createCustomSliderData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BurstSliderConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BurstSliderConverter.Convert))]
        private static IEnumerable<CodeInstruction> BurstSliderConverterV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createBurstSliderData, _createCustomBurstSliderData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.WaypointConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.WaypointConverter.Convert))]
        private static IEnumerable<CodeInstruction> WaypointConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _waypointCtor, _customWaypointCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BpmEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BpmEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> BpmEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _bpmEventCtor, _customBpmEventCtor);
        }

#if PRE_V1_39_1
        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.RotationEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.RotationEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> RotationEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _rotationEventCtor, _customRotationEventCtor);
        }
#endif

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.LightColorEventBoxConverter),
            "Convert")]
        private static void LightColorEventBoxConverterPostfix(
            BeatmapSaveDataVersion3.LightColorEventBox saveData,
            BeatmapEventDataBox __result)
        {
            if (__result is not LightColorBeatmapEventDataBox box)
            {
                return;
            }

            List<CustomData?> perEventData = new();
            bool anyCustom = false;
            foreach (BeatmapSaveDataVersion3.LightColorBaseData item in saveData.lightColorBaseDataList ?? new List<BeatmapSaveDataVersion3.LightColorBaseData>())
            {
                if (item is ICustomData cd && cd.customData.Count > 0)
                {
                    perEventData.Add(cd.customData);
                    anyCustom = true;
                }
                else
                {
                    perEventData.Add(null);
                }
            }

            if (anyCustom)
            {
                _boxCustomData.Add(box, perEventData);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LightColorBeatmapEventDataBox), "Unpack")]
        private static void LightColorBeatmapEventDataBoxUnpackPostfix(
            LightColorBeatmapEventDataBox __instance,
            List<BeatmapEventData> output)
        {
            if (!_boxCustomData.TryGetValue(__instance, out List<CustomData?> perEventData))
            {
                return;
            }

            // output may contain events from multiple boxes (appended); scan from the end
            // matching the count of items we know this box produced.
            int boxCount = perEventData!.Count;
            int outputStart = output.Count - boxCount;
            if (outputStart < 0)
            {
                return;
            }

            for (int i = 0; i < boxCount; i++)
            {
                CustomData? customData = perEventData[i];
                if (customData == null)
                {
                    continue;
                }

                int outIdx = outputStart + i;
                if (output[outIdx] is not LightColorBeatmapEventData ev || ev is CustomLightColorBeatmapEventData)
                {
                    continue;
                }

                output[outIdx] = new CustomLightColorBeatmapEventData(
                    ev.time,
                    ev.groupId,
                    ev.elementId,
                    ev.usePreviousValue,
                    ev.easeType,
                    ev.colorType,
                    ev.brightness,
                    ev.strobeBeatFrequency,
                    ev.strobeBrightness,
                    ev.strobeFade,
                    customData);
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BasicEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BasicEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> BasicEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _basicEventCtor, _customBasicEventCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorBoostEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorBoostEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> ColorBoostEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _colorBoostEventCtor, _customColorBoostEventCtor);
        }

        // VERSION 2_6_0AndEarlier
        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ColorNoteConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ColorNoteConverter.Convert))]
        private static IEnumerable<CodeInstruction> ColorNoteConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createBasicNoteData))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_createCustomBasicNoteData)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createBombNoteData))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_createCustomBombNoteData)
                .InstructionEnumeration();
        }

        // the transpiler dark magic strikes again...
        // this time our transpiler here causes patches on GetHeightForObstacleType to fail
        // specifically the height patch that mapping extensions needs
        // how a transpiler breaks the patch on a different method is beyond me!!!
        // transpiler giveth and transpiler taketh away
        // for this reason, we'll just replace this with a prefix and override the original method
        /*[HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter.Convert))]
        private static IEnumerable<CodeInstruction> ObstacleConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version2, _obstacleDataCtor, _customObstacleDataCtor);
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter.Convert))]
        private static bool ObstacleConvertV2_6_0AndEarlier(
            BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter __instance,
            BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData o,
            ref ObstacleData? __result)
        {
            float time = __instance.BeatToTime(o.time);
            float endBeat = o.time + o.duration;
            float duration = __instance.BeatToTime(endBeat) - time;

            __result = new CustomObstacleData(
                time,
#if !PRE_V1_39_1
                o.time,
                endBeat,
                __instance.BeatToRotation(o.time),
#endif
                o.lineIndex,
                BeatmapTypeConverters.ConvertNoteLineLayer(
                    BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter
                        .GetLayerForObstacleType(o.type)),
                duration,
                o.width,
                BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter
                    .GetHeightForObstacleType(o.type),
                o.GetData(),
                BeatmapSaveDataHelpers.version2);
            return false;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.SliderConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.SliderConverter.Convert))]
        private static IEnumerable<CodeInstruction> SliderConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version2, _createSliderData, _createCustomSliderData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.WaypointConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.WaypointConverter.Convert))]
        private static IEnumerable<CodeInstruction> WaypointConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version2, _waypointCtor, _customWaypointCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.BasicEventConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.BasicEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> BasicEventConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*.MatchForward(false, new CodeMatch(OpCodes.Newobj, _rotationEventCtor))
                .Repeat(n => n
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, _getData),
                        new CodeInstruction(OpCodes.Ldsfld, _version2))
                    .SetOperandAndAdvance(_customRotationEventCtor))*/
                .Start()
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _colorBoostEventCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_customColorBoostEventCtor)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _basicEventCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_customBasicEventCtor)
                .InstructionEnumeration();
        }
    }
}
#endif
