using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Random = System.Random;

namespace LilacWings
{
    [BepInPlugin("com.kuborro.plugins.fp2.lilacwings", "LilacWingsRestorer", "1.3.1")]
    [BepInProcess("FP2.exe")]
    [BepInIncompatibility("com.micg.plugins.fp2.rebalance")]
    public class Plugin : BaseUnityPlugin
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
                harmony.PatchAll(typeof(Patch));
            }
            else
            {
                harmony.PatchAll(typeof(Patch3));
                harmony.PatchAll(typeof(Patch4));
            }
            harmony.PatchAll(typeof(Patch2));
        }
    }


    class Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_Init), MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_InAir), MethodType.Normal)]
        static void Postfix(ref bool ___hasSpecialItem)
        {
            if (GameObject.Find("Player 1").GetComponent<FPPlayer>().characterID.ToString() == "LILAC")
            {
                ___hasSpecialItem = true;
            }
        }

    }

    class Patch2
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_PlayVoice), MethodType.Normal)]
        static bool Prefix(ref AudioClip voiceClip, bool ___hasSpecialItem, AudioClip[] ___vaExtra)
        {
            if (___hasSpecialItem && voiceClip == ___vaExtra[0])
            {
                Random rnd = new();
                int yell = rnd.Next(0, 100);
                if (yell > Plugin.configYellPercent.Value)
                {
                    return false;
                    //Simply not run the method at all
                }
            }
            return true;


        }

    }
    public class Patch3
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemFuel), "CollisionCheck")]
        static void Postfix(ItemFuel __instance, FPHitBox ___hbItem)
        {
            FPPlayer player = GameObject.Find("Player 1").GetComponent<FPPlayer>();
            if (player != null) { 
                if (player.characterID.ToString() == "LILAC" && FPCollision.CheckOOBB(__instance, ___hbItem, player, player.hbTouch, false, false, false))
                {
                    player.hasSpecialItem = true;
                    player.powerupTimer = 0;
                    player.flashTime = 0;
                }
            }
        }
    }
    public class Patch4
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_CrushKO), MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_KO), MethodType.Normal)]
        static void Postfix()
        {
            FPPlayer player = GameObject.Find("Player 1").GetComponent<FPPlayer>();
            if (player.characterID.ToString() == "LILAC")
            {
                player.hasSpecialItem = false;
            }
        }
    }
}
