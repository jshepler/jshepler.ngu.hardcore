using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.hardcore
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("jshepler.ngu.mods")]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log;
        internal static void LogInfo(string text) => Log.LogInfo(text);

        internal static event EventHandler OnSaveLoaded;
        internal static Character Character = null;
        internal static event EventHandler onGUI; // have to use onGUI instead of OnGUI because OnGUI is the method unity calls

        private void Awake()
        {
            // prevents the bepinex manager object (i.e. this plugin instance) from being destroyed after Awake()
            // https://github.com/aedenthorn/PlanetCrafterMods/issues/7
            // not needed for all games, but I'm not currently aware of anything that it would hurt
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            Log = base.Logger;

            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnGUI()
        {
            if (Character == null)
                return;

            onGUI?.Invoke(null, EventArgs.Empty);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_postfix(Character __instance)
        {
            Character = __instance;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_prefix()
        {
            OnSaveLoaded?.Invoke(null, EventArgs.Empty);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(MainMenuController), "updateMiscText")]
        private static bool MainMenuController_updateMiscText_prefix(MainMenuController __instance)
        {
            var build = __instance.character.getVersionAsString();
            __instance.buildText.text = $"<b>Build {build} <color=blue>[HC]</color></b> (jshepler hc {PluginInfo.PLUGIN_VERSION})";
            __instance.buildText.resizeTextForBestFit = true;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(VersionNumbering), "Start")]
        private static bool VersionNumbering_Start_prefix(VersionNumbering __instance)
        {
            var build = __instance.character.getVersionAsString();
            __instance.versionNumber.text = $"<b>Build {build} <color=blue>[HC]</color></b>\n(jshepler hc {PluginInfo.PLUGIN_VERSION})";
            __instance.versionNumber.resizeTextForBestFit = true;
            return false;
        }
    }
}
