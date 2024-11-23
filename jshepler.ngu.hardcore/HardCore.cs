using System.Reflection;
using HarmonyLib;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.hardcore
{
    [HarmonyPatch]
    internal class HardCore
    {
        // Canvas/Player Panel Canvas/Player Panel/Standalone Load
        // Canvas/Box Canvas/Main Menu Screen/Autosave Pod Border/Autosave Button
        // Canvas/Box Canvas/Main Menu Screen/Load File Button

        internal static bool Enabled = true;
        private static bool _playerDied = false;

        private static void _saveCloud() => Plugin.Character.saveLoad.saveGamestateToSteamCloud();
        private static byte[] _emptySave = [0];

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "Awake")]
        private static void MainMenuController_Awake_postfix()
        {
            if (Enabled)
                GameObject.Find("Canvas/Box Canvas/Main Menu Screen/Load File Button").GetComponent<Button>().interactable = false;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                if (!Enabled)
                    ModSave.Data.HardCore = false;
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StandaloneFileBrowser), "OpenFilePanel", typeof(string), typeof(string), typeof(string), typeof(bool))]
        private static bool StandaloneFileBrowser_OpenFilePanel_prefix(ref string title)
        {
            return !Enabled;
        }

        // disable loading cloud save if HC is enabled and the save doesn't have the HC flag set to true
        [HarmonyPostfix, HarmonyPatch(typeof(SteamManager), "OnRemoteStorageFileReadAsyncComplete")]
        private static void SteamManager_OnRemoteStorageFileReadAsyncComplete_postfix()
        {
            if (!Enabled)
                return;

            var mm = Plugin.Character.mainMenu;
            var cloudPlayerDataField = typeof(MainMenuController).GetField("cloudPlayerData", BindingFlags.Instance | BindingFlags.NonPublic);
            var cloudPlayerData = cloudPlayerDataField.GetValue(mm) as mods.ModSave.ModPlayerData;

            // should never be null, even if loading vanilla save, but check anyway in case the cast fails for some reason
            // if null or is hardcore save, allow it
            if (cloudPlayerData == null || (cloudPlayerData.Data.ContainsKey("HardCore") && (bool)cloudPlayerData.Data["HardCore"]))
                return;

            cloudPlayerDataField.SetValue(mm, null);
            mm.setCloudSaveValidity(false);
            mm.cloudInfo.text = "<b><color=red>HARDCORE MODE</color></b>\n\nCloud save is not hardcore\n\n";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "updateAutosavePod")]
        private static void MainMenuController_updateAutosavePod_postfix(MainMenuController __instance)
        {
            if (Enabled)
            {
                __instance.loadAutosaveButton.interactable = false;
                __instance.autosaveInfo.text = "<b><color=red>HARDCORE MODE</color></b>\n\nLoading autosave is disabled";
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "startNewGame")]
        private static void MainMenuController_startNewGame_postfix()
        {
            if (Enabled)
                ModSave.Data.HardCore = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OpenFileDialog), "Start")]
        private static void OpenFileDialog_Start_postfix(OpenFileDialog __instance)
        {
            if (Enabled)
                __instance.standaloneLoad.interactable = false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OpenFileDialog), "showLoadAdvice")]
        private static bool OpenFileDialog_showLoadAdvice_prefix(OpenFileDialog __instance)
        {
            if (!Enabled)
                return true;

            __instance.tooltip.showTooltip("<b><color=red>HARDCORE MODE</color></b>\n\nCannot load saves in hardcore");
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OpenFileDialog), "saveGamestateToSteamCloud")]
        private static bool OpenFileDialog_saveGamestateToSteamCloud_prefix()
        {
            if (!Enabled || !_playerDied)
                return true;

            Plugin.Character.steamAPI.writeToSteamCloud(_emptySave);
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "fight")]
        private static void BossController_fight_postfix()
        {
            if (Enabled && Plugin.Character.curHP <= 0)
                died();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "playerDeath")]
        private static void AdventureController_playerDeath_postfix()
        {
            if (Enabled)
                died();
        }

        private static void died()
        {
            _playerDied = true;
            _saveCloud();
            GameOver.Show();
        }
    }
}
