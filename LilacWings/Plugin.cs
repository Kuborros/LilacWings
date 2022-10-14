using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using Random = System.Random;

namespace LilacWings
{
    [BepInPlugin("com.kuborro.plugins.fp2.lilacwings", "LilacWingsRestorer", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<int> configYellPercent;
        private void Awake()
        {
            configYellPercent = Config.Bind("General", "YellChance", 10, new ConfigDescription("Set the percent of how often you want Lilac to scream when super boosting. 0 = Never, 100 = Every time", new AcceptableValueRange<int>(0, 100)));
            var harmony = new Harmony("com.kuborro.plugins.fp2.lilacwings");
            harmony.PatchAll(typeof(Patch));
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
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_PlaySoundUninterruptable), MethodType.Normal)]
        static bool Prefix( ref AudioClip sfxClip, bool ___hasSpecialItem, AudioClip ___sfxBigBoostLaunch)
        {
            if (___hasSpecialItem && sfxClip == ___sfxBigBoostLaunch)
            {
                Random rnd = new Random();
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
}
