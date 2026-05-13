using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TroflineMod
{
    [HarmonyPatch(typeof(NewMovement), "GetHurt")]
    public class NoDamagePatch
    {
        static bool Prefix()
        {
            if (MainHandling.noDamage) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(NewMovement), "Update")]
    public class StaminaPatch
    {
        static void Postfix(NewMovement __instance)
        {
            if (MainHandling.infStamina)
            {
                __instance.boostCharge = 300f; // Unendlicher Dash!
            }
        }
    }
    [HarmonyPatch(typeof(SceneManager), "LoadScene", new System.Type[] { typeof(string) })]
    public class SkipIntroPatch
    {
        static void Prefix(ref string sceneName)
        {
            if (sceneName == "Intro") sceneName = "Main Menu";
        }
    }
    [HarmonyPatch(typeof(LevelStats), "Update")]
    public class TimerColorWatermarkPatch
    {
        static void Postfix(LevelStats __instance)
        {
            if (__instance.time != null)
            {
                Color32 c = __instance.time.color;
                if (c.r == 255 && c.g == 255 && c.b == 255)
                {
                    c.b = 254; // Trofline Black Stealth Watermark
                    __instance.time.color = c;
                }
            }
        }
    }
}