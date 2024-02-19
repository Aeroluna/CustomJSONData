using HarmonyLib;
using IPA;
using JetBrains.Annotations;
using Logger = IPA.Logging.Logger;

namespace CustomJSONData
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        private readonly Harmony _harmonyInstance = new("aeroluna.CustomJSONData");

#pragma warning disable CA1822
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger)
        {
            Log = pluginLogger;
        }

        internal static Logger Log { get; private set; } = null!;

        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            _harmonyInstance.PatchAll(typeof(Plugin).Assembly);
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            _harmonyInstance.UnpatchSelf();
        }
#pragma warning restore CA1822
    }
}
