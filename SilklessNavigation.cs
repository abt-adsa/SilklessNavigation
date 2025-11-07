using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;

namespace SilklessNavigation
{
    [BepInPlugin("com.abt-adsa.silklessnavigation", "SilklessNavigation", "1.0.0")]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class SilklessNavigation : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public static ConfigEntry<bool> EnableAnkletsNoCost;
        public static ConfigEntry<bool> EnableClawlineNoCost;
        public static ConfigEntry<bool> EnableNeedolinNoCost;

        private static bool _isHarpoonActive;
        private static bool _isNeedolinPlaying;

        private void Awake()
        {
            Log = Logger;

            EnableAnkletsNoCost = Config.Bind(
                "General",
                "AnkletsNoCost",
                true,
                "Prevents silk consumption when using Silkspeed Anklets.");

            EnableClawlineNoCost = Config.Bind(
                "General",
                "ClawlineNoCost",
                true,
                "Prevents silk consumption when using the Clawline/Harpoon.");

            EnableNeedolinNoCost = Config.Bind(
                "General",
                "NeedolinNoCost",
                true,
                "Prevents silk consumption when playing the Needolin.");

            Log.LogInfo("SilklessNavigation mod is loaded!");

            Harmony.CreateAndPatchAll(typeof(SilklessNavigation));
        }


        [HarmonyPatch(typeof(PlayerData), "TakeSilk")]
        [HarmonyPrefix]
        public static bool PlayerData_TakeSilk_Prefix(ref int amount)
        {

            if (EnableAnkletsNoCost.Value)
            {

                HeroController hero = UnityEngine.Object.FindFirstObjectByType<HeroController>();
                if (hero != null && hero.IsSprintMasterActive && amount == 1)
                {
                    Log.LogInfo("SilklessNavigation: Silkspeed Anklets cost prevented.");
                    return false;
                }
            }

            if (EnableClawlineNoCost.Value && _isHarpoonActive && amount == 1)
            {
                _isHarpoonActive = false;
                Log.LogInfo("SilklessNavigation: Clawline/Harpoon cost prevented.");
                return false;
            }

            if (EnableNeedolinNoCost.Value && _isNeedolinPlaying)
            {

                Log.LogInfo("SilklessNavigation: Needolin cost prevented.");
                return false;
            }

            return true;
        }


        [HarmonyPatch(typeof(HeroControllerConfig), "get_CanHarpoonDash")]
        [HarmonyPrefix]
        private static void SetHarpoonFlag_Prefix()
        {
            _isHarpoonActive = true;
        }

        [HarmonyPatch(typeof(ControlNeedolin), "OnEnter")]
        [HarmonyPrefix]
        private static void SetNeedolinPlaying_OnEnter_Prefix(ControlNeedolin __instance)
        {
            if (__instance.isPlaying != null)
            {
                _isNeedolinPlaying = __instance.isPlaying.Value;
            }
        }

    }
}
