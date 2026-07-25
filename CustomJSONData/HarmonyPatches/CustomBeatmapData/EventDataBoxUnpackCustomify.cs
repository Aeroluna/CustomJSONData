using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(LightColorBeatmapEventDataBox))]
    internal static class EventDataBoxUnpackCustomify
    {
#if !PRE_V1_44_1
        private static readonly MethodInfo _original = AccessTools.Method(
            typeof(IBeatmapLightEventConverter),
            nameof(IBeatmapLightEventConverter.ConvertLightColorBeatmapEvent));

        private static readonly MethodInfo _custom = AccessTools.Method(
            typeof(EventDataBoxUnpackCustomify),
            nameof(ConvertLightColorBeatmapEvent));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(LightColorBeatmapEventDataBox.Unpack), MethodType.Enumerator)]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _original))
                .SetAndAdvance(OpCodes.Ldloc_3, null) // this instruction has a label, so we do a little finagling to preserve it
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, _custom))
                .InstructionEnumeration();
        }

        private static IEnumerable<BeatmapEventData> ConvertLightColorBeatmapEvent(
            IBeatmapLightEventConverter instance,
            int subtypeIdentifier,
            float time,
            int groupId,
            int elementId,
            bool usePreviousValue,
            EaseType easeType,
            EnvironmentColorType colorType,
            float brightness,
            int strobeBeatFrequency,
            float strobeBrightness,
            bool strobeFade,
            float? nextEventBrightness,
            LightColorBaseData lightColorBaseData)
        {
            if (instance is not BeatmapLightEventConverterNoConvert type || type._ignoreColorEvents)
            {
                yield break;
            }

            yield return new CustomLightColorBeatmapEventData(
                time,
                groupId,
                elementId,
                usePreviousValue,
                easeType,
                colorType,
                brightness,
                strobeBeatFrequency,
                strobeBrightness,
                strobeFade,
                lightColorBaseData is ICustomData customData ? customData.customData : new CustomData(),
                VersionExtensions.version3);
        }
#elif V1_40_8
        private static readonly MethodInfo _original = AccessTools.Method(
            typeof(IBeatmapLightEventConverter),
            nameof(IBeatmapLightEventConverter.ConvertLightColorBeatmapEvent));

        private static readonly MethodInfo _custom = AccessTools.Method(
            typeof(EventDataBoxUnpackCustomify),
            nameof(ConvertLightColorBeatmapEvent));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(LightColorBeatmapEventDataBox.Unpack))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _original))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0))
                .SetOperandAndAdvance(_custom)
                .InstructionEnumeration();
        }

        private static void ConvertLightColorBeatmapEvent(
            IBeatmapLightEventConverter instance,
            List<BeatmapEventData> output,
            int subtypeIdentifier,
            float time,
            int groupId,
            int elementId,
            bool usePreviousValue,
            EaseType easeType,
            EnvironmentColorType colorType,
            float brightness,
            int strobeBeatFrequency,
            float strobeBrightness,
            bool strobeFade,
            LightColorBaseData lightColorBaseData)
        {
            output.Add(new CustomLightColorBeatmapEventData(
                time,
                groupId,
                elementId,
                usePreviousValue,
                easeType,
                colorType,
                brightness,
                strobeBeatFrequency,
                strobeBrightness,
                strobeFade,
                lightColorBaseData is ICustomData customData ? customData.customData : new CustomData(),
                VersionExtensions.version3));
        }
#else
        private static readonly ConstructorInfo _lightColorBeatmapEventDataCtor = AccessTools.FirstConstructor(typeof(LightColorBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customLightColorBeatmapEventDataCtor = AccessTools.FirstConstructor(typeof(CustomLightColorBeatmapEventData), _ => true);

        private static readonly MethodInfo _getData = AccessTools.Method(typeof(EventDataBoxUnpackCustomify), nameof(GetData));
        private static readonly MethodInfo _version3 = AccessTools.PropertyGetter(typeof(VersionExtensions), nameof(VersionExtensions.version3));

        private static CustomData GetData(this LightColorBaseData dataItem)
        {
            return dataItem is ICustomData customData
                ? customData.customData : new CustomData();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(LightColorBeatmapEventDataBox.Unpack))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _lightColorBeatmapEventDataCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Call, _version3))
                .SetOperandAndAdvance(_customLightColorBeatmapEventDataCtor)
                .InstructionEnumeration();
        }
#endif
    }
}
