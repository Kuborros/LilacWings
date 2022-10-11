using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace LilacWings
{
    [BepInPlugin("com.kuborro.plugins.fp2.lilacwings", "LilacWingsRestorer", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("com.kuborro.plugins.fp2.lilacwings");
            harmony.PatchAll(typeof(Patch));
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
}
