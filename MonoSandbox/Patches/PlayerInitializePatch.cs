using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoSandbox.Patches
{
    [HarmonyPatch(typeof(GorillaTagger), "Start")]
    public class PlayerInitializePatch
    {
        public static void Postfix(GorillaTagger __instance)
        {
            if (__instance.offlineVRRig != null)
            {
                if (__instance.offlineVRRig.leftHandTransform != null)
                {
                    var leftPalm = __instance.offlineVRRig.leftHandTransform.parent?.Find("palm.01.L");
                    if (leftPalm != null)
                        RefCache.LHand = leftPalm.gameObject;
                }
                if (__instance.offlineVRRig.rightHandTransform != null)
                {
                    var rightPalm = __instance.offlineVRRig.rightHandTransform.parent?.Find("palm.01.R");
                    if (rightPalm != null)
                        RefCache.RHand = rightPalm.gameObject;
                }
            }
        }
    }
}
