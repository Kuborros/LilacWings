using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LilacWings
{
    [BepInPlugin("com.kuborro.plugins.fp2.lilacwings", "LilacWings", "1.5.0")]
    [BepInProcess("FP2.exe")]
    [BepInIncompatibility("com.micg.plugins.fp2.rebalance")]
    public class LilacWings : BaseUnityPlugin
    {
        public static ConfigEntry<int> configYellPercent;
        public static ConfigEntry<bool> configWingAsPowerUp;
        private void Awake()
        {
            configYellPercent = Config.Bind("General", "YellChance", 10, new ConfigDescription("Set the percent of how often you want Lilac to scream when super boosting. 0 = Never, 100 = Every time", new AcceptableValueRange<int>(0, 100)));
            configWingAsPowerUp = Config.Bind("General", "WingsAsPowerUp", false, "Set if you want the wings to be a powerup you need to find on the stage.");

            var harmony = new Harmony("com.kuborro.plugins.fp2.lilacwings");
            if (!configWingAsPowerUp.Value)
            {
                harmony.PatchAll(typeof(PatchPlayerStart));
            }
            else
            {
                harmony.PatchAll(typeof(PatchItemFuel));
                harmony.PatchAll(typeof(PatchKOState));
            }
            harmony.PatchAll(typeof(PatchPlayerVoice));
        }
    }


    class PatchPlayerStart
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
        static void PatchFPPlayerStart(ref bool ___hasSpecialItem)
        {
            if (FPSaveManager.character == FPCharacterID.LILAC)
            {
                ___hasSpecialItem = true;
            }
        }
    }

    class PatchPlayerVoice
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_PlayVoice), MethodType.Normal)]
        static bool PatchFPPlayerPlayVoice(ref AudioClip voiceClip, bool ___hasSpecialItem, AudioClip[] ___vaExtra, FPPlayer __instance)
        {
            if (__instance.characterID == FPCharacterID.LILAC)
            {
                if (___vaExtra.Length > 0)
                {
                    if (___hasSpecialItem && voiceClip == ___vaExtra[0])
                    {
                        int yell = Random.Range(0, 101);
                        if (yell > LilacWings.configYellPercent.Value)
                        {
                            return false;
                            //Simply not run the method at all
                        }
                    }
                }
            }
            return true;
        }
    }
    public class PatchItemFuel
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemFuel), "CollisionCheck")]
        static void PatchItemFuelColCheck(ItemFuel __instance, FPHitBox ___hbItem)
        {
            FPPlayer player = GameObject.Find("Player 1").GetComponent<FPPlayer>();
            if (player != null) { 
                if (player.characterID == FPCharacterID.LILAC && FPCollision.CheckOOBB(__instance, ___hbItem, player, player.hbTouch, false, false, false))
                {
                    player.hasSpecialItem = true;
                    player.powerupTimer = 0;
                    player.flashTime = 0;
                }
            }
        }
    }
    public class PatchKOState
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_CrushKO), MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_KO), MethodType.Normal)]
        static void PatchFPPlayerKOStates(ref bool ___hasSpecialItem)
        {
            if (FPSaveManager.character == FPCharacterID.LILAC)
            {
                ___hasSpecialItem = false;
            }
        }
    }
}
